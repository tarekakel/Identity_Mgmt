namespace AmlScreening.Application.Interfaces;

/// <summary>
/// Single source of truth for sanction list matching across all flows
/// (individual, corporate, customer record, instant search). Backed by Elasticsearch.
/// </summary>
public interface IScreeningEngine
{
    Task<IReadOnlyList<ScreeningCandidate>> SearchAsync(
        ScreeningQuery query,
        CancellationToken cancellationToken = default);
}

public class ScreeningQuery
{
    public string FullName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Nationality { get; set; }
    public string? PlaceOfBirthCountry { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Tolerance window in years around DateOfBirth.</summary>
    public int? BirthYearRange { get; set; }

    /// <summary>List source filter (e.g. UN, UAE, OFAC). Empty = all.</summary>
    public IReadOnlyCollection<string> ListSources { get; set; } = Array.Empty<string>();

    /// <summary>Possible-match threshold on a 0-100 scale (confirmed = +15).</summary>
    public int Threshold0to100 { get; set; } = 70;

    /// <summary>Max candidates to return.</summary>
    public int Top { get; set; } = 50;
}

public class ScreeningCandidate
{
    public Guid EntryId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? MatchedAlias { get; set; }
    public string ListSource { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Raw Elasticsearch _score as returned by the query.</summary>
    public double RawScore { get; set; }

    /// <summary>Score normalized to 0-100 for UI/persistence.</summary>
    public double NormalizedScore0to100 { get; set; }

    /// <summary>Status bucket: "ConfirmedMatch", "PossibleMatch", or "Clear".</summary>
    public string Status { get; set; } = "Clear";

    /// <summary>"FullName", "Alias", "Nationality", "DateOfBirth", or "CorporateName".</summary>
    public string MatchType { get; set; } = "FullName";
}
