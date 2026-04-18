using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Options;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HttpMethod = Elastic.Transport.HttpMethod;

namespace AmlScreening.Infrastructure.Services.Search;

public class SanctionEntryIndexer : ISanctionEntryIndexer
{
    private const int BulkBatchSize = 1000;

    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly ILogger<SanctionEntryIndexer> _logger;

    public SanctionEntryIndexer(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        ILogger<SanctionEntryIndexer> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public string IndexName => _options.IndexName;

    public async Task EnsureIndexAsync(CancellationToken cancellationToken = default)
    {
        var exists = await _client.Indices.ExistsAsync(IndexName, cancellationToken);

        if (exists.ApiCallDetails.OriginalException is not null ||
            (!exists.ApiCallDetails.HasSuccessfulStatusCode && exists.ApiCallDetails.HttpStatusCode != 404))
        {
            throw BuildElasticException(
                $"Failed to check existence of Elasticsearch index '{IndexName}'",
                exists.ApiCallDetails,
                body: null);
        }

        if (exists.Exists) return;

        var body = BuildIndexJson();
        var response = await _client.Transport.RequestAsync<StringResponse>(
            HttpMethod.PUT,
            $"/{IndexName}",
            PostData.String(body),
            cancellationToken);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            throw BuildElasticException(
                $"Failed to create Elasticsearch index '{IndexName}'",
                response.ApiCallDetails,
                response.Body);
        }

        _logger.LogInformation("Created Elasticsearch index {IndexName}", IndexName);
    }

    public async Task IndexAsync(SanctionListEntry entry, CancellationToken cancellationToken = default)
    {
        var doc = SanctionEntryDocument.FromEntity(entry);
        var resp = await _client.IndexAsync(doc, IndexName, doc.Id.ToString(), cancellationToken);
        if (!resp.IsValidResponse)
        {
            var details = resp.ApiCallDetails;
            _logger.LogWarning(details?.OriginalException,
                "Failed to index entry {Id} (status={Status}): {Debug}",
                doc.Id,
                details?.HttpStatusCode?.ToString() ?? "no-response",
                resp.DebugInformation);
        }
    }

    public async Task IndexBulkAsync(IEnumerable<SanctionListEntry> entries, CancellationToken cancellationToken = default)
    {
        await EnsureIndexAsync(cancellationToken);

        var batch = new List<SanctionEntryDocument>(BulkBatchSize);
        foreach (var entry in entries)
        {
            batch.Add(SanctionEntryDocument.FromEntity(entry));
            if (batch.Count >= BulkBatchSize)
            {
                await FlushBulkAsync(batch, cancellationToken);
                batch.Clear();
            }
        }
        if (batch.Count > 0)
            await FlushBulkAsync(batch, cancellationToken);
    }

    private async Task FlushBulkAsync(List<SanctionEntryDocument> batch, CancellationToken cancellationToken)
    {
        var resp = await _client.BulkAsync(b => b
            .Index(IndexName)
            .IndexMany(batch, (op, doc) => op.Id(doc.Id.ToString())),
            cancellationToken);

        if (!resp.IsValidResponse || resp.Errors)
        {
            var details = resp.ApiCallDetails;
            _logger.LogWarning(details?.OriginalException,
                "Bulk index of {Count} docs reported errors (status={Status}): {Debug}",
                batch.Count,
                details?.HttpStatusCode?.ToString() ?? "no-response",
                resp.DebugInformation);
        }
    }

    public async Task DeleteAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        var resp = await _client.DeleteAsync<SanctionEntryDocument>(entryId.ToString(),
            d => d.Index(IndexName), cancellationToken);

        if (!resp.IsValidResponse && resp.ApiCallDetails?.HttpStatusCode != 404)
        {
            var details = resp.ApiCallDetails;
            _logger.LogWarning(details?.OriginalException,
                "Failed to delete entry {Id} (status={Status}): {Debug}",
                entryId,
                details?.HttpStatusCode?.ToString() ?? "no-response",
                resp.DebugInformation);
        }
    }

    public async Task DeleteByListSourceAsync(string listSource, CancellationToken cancellationToken = default)
    {
        var resp = await _client.DeleteByQueryAsync<SanctionEntryDocument>(IndexName,
            d => d.Query(q => q.Term(t => t.Field("listSource").Value(listSource))),
            cancellationToken);

        if (!resp.IsValidResponse)
        {
            var details = resp.ApiCallDetails;
            _logger.LogWarning(details?.OriginalException,
                "DeleteByQuery on listSource {ListSource} failed (status={Status}): {Debug}",
                listSource,
                details?.HttpStatusCode?.ToString() ?? "no-response",
                resp.DebugInformation);
        }
    }

    private static InvalidOperationException BuildElasticException(
        string message,
        Elastic.Transport.ApiCallDetails? details,
        string? body)
    {
        var statusCode = details?.HttpStatusCode?.ToString() ?? "no-response";
        var inner = details?.OriginalException;
        var bodyText = string.IsNullOrWhiteSpace(body) ? "<empty>" : body;
        var debug = details?.DebugInformation ?? "<no debug info>";
        return new InvalidOperationException(
            $"{message} (status={statusCode}): {bodyText}. Debug: {debug}",
            inner);
    }

    public async Task<long> ReindexAllAsync(IEnumerable<SanctionListEntry> entries, CancellationToken cancellationToken = default)
    {
        var existing = await _client.Indices.ExistsAsync(IndexName, cancellationToken);
        if (existing.Exists)
            await _client.Indices.DeleteAsync(IndexName, cancellationToken);

        await EnsureIndexAsync(cancellationToken);

        long total = 0;
        var batch = new List<SanctionEntryDocument>(BulkBatchSize);
        foreach (var entry in entries)
        {
            batch.Add(SanctionEntryDocument.FromEntity(entry));
            if (batch.Count >= BulkBatchSize)
            {
                await FlushBulkAsync(batch, cancellationToken);
                total += batch.Count;
                batch.Clear();
            }
        }
        if (batch.Count > 0)
        {
            await FlushBulkAsync(batch, cancellationToken);
            total += batch.Count;
        }

        await _client.Indices.RefreshAsync(IndexName, cancellationToken);
        _logger.LogInformation("Reindexed {Count} sanction entries into {IndexName}", total, IndexName);
        return total;
    }

    public async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var exists = await _client.Indices.ExistsAsync(IndexName, cancellationToken);
        if (!exists.Exists) return 0;

        var resp = await _client.CountAsync<SanctionEntryDocument>(c => c.Indices(IndexName), cancellationToken);
        return resp.IsValidResponse ? resp.Count : 0;
    }

    private static string BuildIndexJson()
    {
        // Custom analyzers:
        //   name_index   : indexing-time, builds edge-n-grams for partial matching
        //   name_search  : query-time, no n-grams, just folding + lowercase
        //   name_phonetic: double-metaphone for cross-language name matching
        return """
        {
          "settings": {
            "index": {
              "max_ngram_diff": 13,
              "analysis": {
                "filter": {
                  "name_edge_ngram": {
                    "type": "edge_ngram",
                    "min_gram": 2,
                    "max_gram": 15
                  },
                  "name_phonetic_filter": {
                    "type": "phonetic",
                    "encoder": "double_metaphone",
                    "replace": false
                  }
                },
                "normalizer": {
                  "lowercase_norm": {
                    "type": "custom",
                    "filter": ["lowercase", "asciifolding"]
                  }
                },
                "analyzer": {
                  "name_index": {
                    "type": "custom",
                    "tokenizer": "standard",
                    "filter": ["icu_folding", "lowercase", "asciifolding", "name_edge_ngram"]
                  },
                  "name_search": {
                    "type": "custom",
                    "tokenizer": "standard",
                    "filter": ["icu_folding", "lowercase", "asciifolding"]
                  },
                  "name_phonetic": {
                    "type": "custom",
                    "tokenizer": "standard",
                    "filter": ["icu_folding", "lowercase", "asciifolding", "name_phonetic_filter"]
                  }
                }
              }
            }
          },
          "mappings": {
            "properties": {
              "id":             { "type": "keyword" },
              "listSource":     { "type": "keyword" },
              "fullName": {
                "type": "text",
                "analyzer": "name_index",
                "search_analyzer": "name_search",
                "fields": {
                  "phonetic": { "type": "text", "analyzer": "name_phonetic" },
                  "keyword":  { "type": "keyword", "ignore_above": 512 }
                }
              },
              "aliases": {
                "type": "text",
                "analyzer": "name_index",
                "search_analyzer": "name_search",
                "fields": {
                  "phonetic": { "type": "text", "analyzer": "name_phonetic" }
                }
              },
              "aliasesGood": {
                "type": "text",
                "analyzer": "name_index",
                "search_analyzer": "name_search",
                "fields": {
                  "phonetic": { "type": "text", "analyzer": "name_phonetic" }
                }
              },
              "firstName":  { "type": "text", "analyzer": "name_index", "search_analyzer": "name_search" },
              "secondName": { "type": "text", "analyzer": "name_index", "search_analyzer": "name_search" },
              "nationalities":         { "type": "keyword", "normalizer": "lowercase_norm" },
              "addressCountries":      { "type": "keyword", "normalizer": "lowercase_norm" },
              "placeOfBirthCountries": { "type": "keyword", "normalizer": "lowercase_norm" },
              "gender":                { "type": "keyword", "normalizer": "lowercase_norm" },
              "birthDates": { "type": "date" },
              "birthYears": { "type": "integer" },
              "birthYearRanges": {
                "type": "nested",
                "properties": {
                  "from": { "type": "integer" },
                  "to":   { "type": "integer" }
                }
              },
              "referenceNumber": { "type": "keyword" },
              "fullNameArabic":  { "type": "text", "analyzer": "name_index", "search_analyzer": "name_search" },
              "designation":     { "type": "text" },
              "comments":        { "type": "text" }
            }
          }
        }
        """;
    }
}
