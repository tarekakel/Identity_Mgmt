using AmlScreening.Domain.Entities;
using AmlScreening.Domain.Entities.SanctionList;

namespace AmlScreening.Infrastructure.Services.Search;

/// <summary>
/// Elasticsearch projection of <see cref="SanctionListEntry"/>.
/// Multi-valued fields (aliases / DOBs / nationalities / POBs / address countries) are surfaced as arrays
/// so the engine can match against any-of-many values natively without nested queries on simple keywords.
/// Birth-year ranges remain nested because they need from..to overlap matching.
/// </summary>
public class SanctionEntryDocument
{
    public Guid Id { get; set; }
    public string ListSource { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? SecondName { get; set; }
    public string? Gender { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? FullNameArabic { get; set; }
    public string? Designation { get; set; }
    public string? Comments { get; set; }

    /// <summary>All alias names (any quality).</summary>
    public List<string> Aliases { get; set; } = new();
    /// <summary>Aliases the source list flagged as high-quality (UN <c>QUALITY=Good</c> or entity <c>a.k.a.</c>).</summary>
    public List<string> AliasesGood { get; set; } = new();

    public List<string> Nationalities { get; set; } = new();
    public List<string> AddressCountries { get; set; } = new();
    public List<string> PlaceOfBirthCountries { get; set; } = new();

    public List<DateTime> BirthDates { get; set; } = new();
    public List<int> BirthYears { get; set; } = new();
    public List<BirthYearRange> BirthYearRanges { get; set; } = new();

    public static SanctionEntryDocument FromEntity(SanctionListEntry e)
    {
        var doc = new SanctionEntryDocument
        {
            Id = e.Id,
            ListSource = e.ListSource,
            FullName = e.FullName,
            FirstName = e.FirstName,
            SecondName = e.SecondName,
            Gender = e.Gender,
            ReferenceNumber = e.ReferenceNumber,
            FullNameArabic = e.FullNameArabic,
            Designation = e.Designation,
            Comments = e.Comments
        };

        // Aliases: prefer the structured collection; fall back to the legacy semicolon string.
        if (e.AliasItems != null && e.AliasItems.Count > 0)
        {
            foreach (var a in e.AliasItems)
            {
                if (string.IsNullOrWhiteSpace(a.Name)) continue;
                doc.Aliases.Add(a.Name);
                if (IsGoodQuality(a.Quality))
                    doc.AliasesGood.Add(a.Name);
            }
        }
        else
        {
            foreach (var a in SplitLegacyAliases(e.Aliases))
                doc.Aliases.Add(a);
        }
        Distinct(doc.Aliases);
        Distinct(doc.AliasesGood);

        // Nationalities: collection or fall back to scalar.
        if (e.Nationalities != null && e.Nationalities.Count > 0)
            foreach (var n in e.Nationalities) AddIfNotEmpty(doc.Nationalities, n);
        else if (!string.IsNullOrWhiteSpace(e.Nationality))
            doc.Nationalities.Add(e.Nationality!);
        Distinct(doc.Nationalities);

        // Address countries: collection of distinct address.Country values, fallback to scalar.
        if (e.Addresses != null && e.Addresses.Count > 0)
            foreach (var a in e.Addresses) AddIfNotEmpty(doc.AddressCountries, a.Country);
        if (doc.AddressCountries.Count == 0 && !string.IsNullOrWhiteSpace(e.AddressCountry))
            doc.AddressCountries.Add(e.AddressCountry!);
        Distinct(doc.AddressCountries);

        // Places of birth: country values, fallback to scalar.
        if (e.PlacesOfBirth != null && e.PlacesOfBirth.Count > 0)
            foreach (var p in e.PlacesOfBirth) AddIfNotEmpty(doc.PlaceOfBirthCountries, p.Country);
        if (doc.PlaceOfBirthCountries.Count == 0 && !string.IsNullOrWhiteSpace(e.PlaceOfBirthCountry))
            doc.PlaceOfBirthCountries.Add(e.PlaceOfBirthCountry!);
        Distinct(doc.PlaceOfBirthCountries);

        // Dates of birth: collect every full date and every year (year-only entries become a year too).
        if (e.DatesOfBirth != null && e.DatesOfBirth.Count > 0)
        {
            foreach (var d in e.DatesOfBirth)
            {
                if (d.Date.HasValue)
                {
                    doc.BirthDates.Add(d.Date.Value);
                    doc.BirthYears.Add(d.Date.Value.Year);
                }
                else if (d.Year.HasValue)
                {
                    doc.BirthYears.Add(d.Year.Value);
                }

                if (d.FromYear.HasValue || d.ToYear.HasValue)
                {
                    var from = d.FromYear ?? d.ToYear ?? 0;
                    var to = d.ToYear ?? d.FromYear ?? 0;
                    if (from > 0 && to > 0 && to >= from)
                        doc.BirthYearRanges.Add(new BirthYearRange { From = from, To = to });
                }
            }
        }
        else if (e.DateOfBirth.HasValue)
        {
            doc.BirthDates.Add(e.DateOfBirth.Value);
            doc.BirthYears.Add(e.DateOfBirth.Value.Year);
        }
        DistinctValue(doc.BirthDates);
        DistinctValue(doc.BirthYears);

        return doc;
    }

    private static bool IsGoodQuality(string? quality)
    {
        if (string.IsNullOrWhiteSpace(quality)) return false;
        var q = quality.Trim().ToLowerInvariant();
        return q == "good" || q == "a.k.a." || q == "aka";
    }

    private static IEnumerable<string> SplitLegacyAliases(string? aliases)
    {
        if (string.IsNullOrWhiteSpace(aliases)) yield break;
        foreach (var part in aliases.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (part.Length > 0) yield return part;
    }

    private static void AddIfNotEmpty(List<string> list, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) list.Add(value!.Trim());
    }

    private static void Distinct(List<string> list)
    {
        if (list.Count <= 1) return;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        list.RemoveAll(item => !seen.Add(item));
    }

    private static void DistinctValue<T>(List<T> list) where T : struct
    {
        if (list.Count <= 1) return;
        var seen = new HashSet<T>();
        list.RemoveAll(item => !seen.Add(item));
    }
}

public class BirthYearRange
{
    public int From { get; set; }
    public int To { get; set; }
}
