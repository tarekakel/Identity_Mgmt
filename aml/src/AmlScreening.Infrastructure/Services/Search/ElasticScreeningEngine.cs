using AmlScreening.Application.Interfaces;
using AmlScreening.Infrastructure.Options;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using System.Text.Json;
using HttpMethod = Elastic.Transport.HttpMethod;

namespace AmlScreening.Infrastructure.Services.Search;

/// <summary>
/// Elasticsearch-backed implementation of <see cref="IScreeningEngine"/>.
/// Builds a bool/function_score query (multi_match across fullName + aliases with both
/// edge-ngram and phonetic analyzers, plus DOB/nationality boosts), normalizes the raw
/// _score to a 0-100 scale and assigns Status from the request thresholds.
/// </summary>
public class ElasticScreeningEngine : IScreeningEngine
{
    private const string StatusClear = "Clear";
    private const string StatusPossibleMatch = "PossibleMatch";
    private const string StatusConfirmedMatch = "ConfirmedMatch";

    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly ISanctionEntryIndexer _indexer;
    private readonly ILogger<ElasticScreeningEngine> _logger;

    public ElasticScreeningEngine(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        ISanctionEntryIndexer indexer,
        ILogger<ElasticScreeningEngine> logger)
    {
        _client = client;
        _options = options.Value;
        _indexer = indexer;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ScreeningCandidate>> SearchAsync(
        ScreeningQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.FullName))
            return Array.Empty<ScreeningCandidate>();

        await _indexer.EnsureIndexAsync(cancellationToken);

        var top = query.Top > 0 ? query.Top : _options.MaxCandidates;
        var requestBody = BuildSearchJson(query, top);

        var resp = await _client.Transport.RequestAsync<StringResponse>(
            HttpMethod.POST,
            $"/{_options.IndexName}/_search",
            PostData.String(requestBody),
            cancellationToken);

        if (!resp.ApiCallDetails.HasSuccessfulStatusCode)
        {
            _logger.LogWarning("Elasticsearch search failed ({Status}): {Body}",
                resp.ApiCallDetails.HttpStatusCode, resp.Body);
            return Array.Empty<ScreeningCandidate>();
        }

        var possibleThreshold = Math.Clamp(query.Threshold0to100, 0, 100);
        var confirmedThreshold = Math.Clamp(possibleThreshold + 15, 0, 100);

        return ParseHits(resp.Body, query, possibleThreshold, confirmedThreshold);
    }

    private string BuildSearchJson(ScreeningQuery q, int size)
    {
        var sb = new StringBuilder();
        sb.Append('{');
        sb.Append("\"size\":").Append(size).Append(',');
        sb.Append("\"query\":{");
        sb.Append("\"function_score\":{");

        // ----- Inner bool query -----
        sb.Append("\"query\":{\"bool\":{");

        // Filters (list source)
        var filters = new List<string>();
        if (q.ListSources != null && q.ListSources.Count > 0)
        {
            var arr = string.Join(",", q.ListSources.Select(s => JsonString(s)));
            filters.Add($"{{\"terms\":{{\"listSource\":[{arr}]}}}}");
        }
        if (filters.Count > 0)
            sb.Append("\"filter\":[").Append(string.Join(",", filters)).Append("],");

        // Should clauses: name + aliases on multiple analyzers.
        // aliasesGood (UN QUALITY=Good / entity a.k.a.) gets a higher weight than the generic alias bag
        // so verified alternate names dominate over weak transliterations the source list flagged "Low".
        var name = JsonString(q.FullName);
        var should = new List<string>
        {
            // Token / edge-ngram match on full name & aliases (highest weight)
            $"{{\"multi_match\":{{\"query\":{name},\"type\":\"best_fields\",\"fuzziness\":\"AUTO\",\"operator\":\"and\",\"fields\":[\"fullName^3\",\"aliasesGood^4\",\"aliases^2.5\",\"firstName^1\",\"secondName^1\",\"fullNameArabic^1\"]}}}}",
            // Phonetic (Double Metaphone) match - catches transliterations
            $"{{\"multi_match\":{{\"query\":{name},\"type\":\"best_fields\",\"fields\":[\"fullName.phonetic^1.5\",\"aliasesGood.phonetic^2\",\"aliases.phonetic^1.2\"]}}}}"
        };

        if (!string.IsNullOrWhiteSpace(q.FirstName))
        {
            should.Add($"{{\"match\":{{\"firstName\":{{\"query\":{JsonString(q.FirstName)},\"fuzziness\":\"AUTO\",\"boost\":0.7}}}}}}");
        }
        if (!string.IsNullOrWhiteSpace(q.LastName))
        {
            should.Add($"{{\"match\":{{\"secondName\":{{\"query\":{JsonString(q.LastName)},\"fuzziness\":\"AUTO\",\"boost\":0.7}}}}}}");
        }

        sb.Append("\"should\":[").Append(string.Join(",", should)).Append("],");
        sb.Append("\"minimum_should_match\":1");
        sb.Append("}}"); // close bool, query

        // ----- Function score boosts and penalties -----
        // Boosts fire when the entry agrees with the query.
        // Penalties only fire when the entry has a known value that contradicts the query
        // (uses bool { must: exists, must_not: term }) - entries with null on the field
        // are unaffected, preserving the AML-conservative posture for incomplete lists.
        // Boosts/penalties target the multi-valued array fields. ES `term`/`terms` against an array
        // matches if ANY value satisfies the predicate; `KnownDifferentPenalty` only fires when the
        // array has at least one value AND none of them match the query, so dual-citizen / multi-DOB
        // entries are scored fairly without false penalties on partially-known data.
        var functions = new List<string>();
        if (!string.IsNullOrWhiteSpace(q.Nationality))
        {
            var nat = JsonString(q.Nationality!.ToLowerInvariant());
            functions.Add($"{{\"filter\":{{\"term\":{{\"nationalities\":{nat}}}}},\"weight\":1.3}}");
            functions.Add(KnownDifferentPenalty("nationalities", nat, weight: 0.7));
        }
        if (!string.IsNullOrWhiteSpace(q.PlaceOfBirthCountry))
        {
            var pob = JsonString(q.PlaceOfBirthCountry!.ToLowerInvariant());
            functions.Add($"{{\"filter\":{{\"term\":{{\"placeOfBirthCountries\":{pob}}}}},\"weight\":1.2}}");
            functions.Add(KnownDifferentPenalty("placeOfBirthCountries", pob, weight: 0.85));
        }
        if (!string.IsNullOrWhiteSpace(q.Gender))
        {
            var gen = JsonString(q.Gender!.ToLowerInvariant());
            functions.Add($"{{\"filter\":{{\"term\":{{\"gender\":{gen}}}}},\"weight\":1.05}}");
            functions.Add(KnownDifferentPenalty("gender", gen, weight: 0.85));
        }
        if (q.DateOfBirth.HasValue)
        {
            var dobIso = JsonString(q.DateOfBirth.Value.ToString("yyyy-MM-dd"));
            // Exact DOB (any of the indexed birthDates equals the query DOB)
            functions.Add($"{{\"filter\":{{\"term\":{{\"birthDates\":{dobIso}}}}},\"weight\":1.5}}");

            var year = q.DateOfBirth.Value.Year;
            var range = Math.Max(0, q.BirthYearRange ?? 0);
            var minY = year - range;
            var maxY = year + range;
            // Year (or near-year) match against the indexed scalar years
            functions.Add($"{{\"filter\":{{\"range\":{{\"birthYears\":{{\"gte\":{minY},\"lte\":{maxY}}}}}}},\"weight\":1.2}}");
            // Range overlap against indexed BETWEEN ranges: entry.from <= query.maxY AND entry.to >= query.minY
            functions.Add($"{{\"filter\":{{\"nested\":{{\"path\":\"birthYearRanges\",\"score_mode\":\"max\",\"query\":{{\"bool\":{{\"must\":[{{\"range\":{{\"birthYearRanges.from\":{{\"lte\":{maxY}}}}}}},{{\"range\":{{\"birthYearRanges.to\":{{\"gte\":{minY}}}}}}}]}}}}}}}},\"weight\":1.1}}");
        }
        sb.Append(",\"functions\":[").Append(string.Join(",", functions)).Append("],");
        sb.Append("\"score_mode\":\"multiply\",");
        sb.Append("\"boost_mode\":\"multiply\"");
        sb.Append("}}"); // close function_score, query
        sb.Append('}');

        return sb.ToString();
    }

    private IReadOnlyList<ScreeningCandidate> ParseHits(string body, ScreeningQuery q,
        int possibleThreshold, int confirmedThreshold)
    {
        var results = new List<ScreeningCandidate>();
        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("hits", out var hitsObj)) return results;
        if (!hitsObj.TryGetProperty("hits", out var hits)) return results;

        foreach (var hit in hits.EnumerateArray())
        {
            var rawScore = hit.TryGetProperty("_score", out var scoreEl) && scoreEl.ValueKind == JsonValueKind.Number
                ? scoreEl.GetDouble() : 0d;
            if (rawScore <= 0) continue;

            if (!hit.TryGetProperty("_source", out var src)) continue;

            var normalized = Math.Clamp(Math.Round(rawScore / Math.Max(0.001, _options.TopScoreReference) * 100, 2), 0, 100);
            if (normalized < possibleThreshold) continue;

            var status = normalized >= confirmedThreshold
                ? StatusConfirmedMatch
                : (normalized >= possibleThreshold ? StatusPossibleMatch : StatusClear);

            var candidate = new ScreeningCandidate
            {
                EntryId = TryGetGuid(src, "id"),
                FullName = TryGetString(src, "fullName") ?? string.Empty,
                ListSource = TryGetString(src, "listSource") ?? string.Empty,
                Nationality = FirstString(src, "nationalities"),
                DateOfBirth = FirstDate(src, "birthDates"),
                RawScore = rawScore,
                NormalizedScore0to100 = normalized,
                Status = status,
                MatchType = ChooseMatchType(q, src)
            };

            // Detect alias match if customer name appears in the alias list closer than the full name
            if (src.TryGetProperty("aliases", out var aliasesEl) && aliasesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var a in aliasesEl.EnumerateArray())
                {
                    var aliasText = a.GetString();
                    if (string.IsNullOrWhiteSpace(aliasText)) continue;
                    if (aliasText.Contains(q.FullName, StringComparison.OrdinalIgnoreCase) ||
                        q.FullName.Contains(aliasText, StringComparison.OrdinalIgnoreCase))
                    {
                        candidate.MatchedAlias = aliasText;
                        candidate.MatchType = "Alias";
                        break;
                    }
                }
            }

            results.Add(candidate);
        }

        return results;
    }

    private static string ChooseMatchType(ScreeningQuery q, JsonElement src)
    {
        if (q.DateOfBirth.HasValue && AnyDate(src, "birthDates", q.DateOfBirth.Value.Date))
            return "DateOfBirth";
        if (!string.IsNullOrWhiteSpace(q.Nationality) &&
            AnyStringEquals(src, "nationalities", q.Nationality!))
            return "Nationality";
        return "FullName";
    }

    private static string? FirstString(JsonElement src, string prop)
    {
        if (!src.TryGetProperty(prop, out var el)) return null;
        if (el.ValueKind == JsonValueKind.String) return el.GetString();
        if (el.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in el.EnumerateArray())
                if (item.ValueKind == JsonValueKind.String) return item.GetString();
        }
        return null;
    }

    private static DateTime? FirstDate(JsonElement src, string prop)
    {
        if (!src.TryGetProperty(prop, out var el)) return null;
        if (el.ValueKind == JsonValueKind.String && DateTime.TryParse(el.GetString(), out var d))
            return d.Date;
        if (el.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in el.EnumerateArray())
                if (item.ValueKind == JsonValueKind.String && DateTime.TryParse(item.GetString(), out var dd))
                    return dd.Date;
        }
        return null;
    }

    private static bool AnyDate(JsonElement src, string prop, DateTime target)
    {
        if (!src.TryGetProperty(prop, out var el)) return false;
        if (el.ValueKind == JsonValueKind.String)
            return DateTime.TryParse(el.GetString(), out var d) && d.Date == target;
        if (el.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in el.EnumerateArray())
                if (item.ValueKind == JsonValueKind.String && DateTime.TryParse(item.GetString(), out var dd) && dd.Date == target)
                    return true;
        }
        return false;
    }

    private static bool AnyStringEquals(JsonElement src, string prop, string target)
    {
        if (!src.TryGetProperty(prop, out var el)) return false;
        if (el.ValueKind == JsonValueKind.String)
            return string.Equals(el.GetString(), target, StringComparison.OrdinalIgnoreCase);
        if (el.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in el.EnumerateArray())
                if (item.ValueKind == JsonValueKind.String && string.Equals(item.GetString(), target, StringComparison.OrdinalIgnoreCase))
                    return true;
        }
        return false;
    }

    private static string? TryGetString(JsonElement src, string prop)
        => src.TryGetProperty(prop, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString() : null;

    private static Guid TryGetGuid(JsonElement src, string prop)
    {
        var s = TryGetString(src, prop);
        return Guid.TryParse(s, out var g) ? g : Guid.Empty;
    }

    private static DateTime? TryGetDate(JsonElement src, string prop)
    {
        var s = TryGetString(src, prop);
        if (string.IsNullOrWhiteSpace(s)) return null;
        return DateTime.TryParse(s, out var d) ? d.Date : null;
    }

    private static string JsonString(string value)
    {
        return JsonSerializer.Serialize(value);
    }

    /// <summary>
    /// Builds a function_score function that fires only when the entry has a known value
    /// for <paramref name="field"/> that does NOT equal <paramref name="jsonValue"/>.
    /// Used to demote (weight &lt; 1) candidates with contradicting metadata while leaving
    /// candidates with null on that field untouched.
    /// </summary>
    private static string KnownDifferentPenalty(string field, string jsonValue, double weight)
    {
        var w = weight.ToString("0.###", CultureInfo.InvariantCulture);
        return $"{{\"filter\":{{\"bool\":{{\"must\":[{{\"exists\":{{\"field\":\"{field}\"}}}}],\"must_not\":[{{\"term\":{{\"{field}\":{jsonValue}}}}}]}}}},\"weight\":{w}}}";
    }
}
