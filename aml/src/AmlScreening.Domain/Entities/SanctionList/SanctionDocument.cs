namespace AmlScreening.Domain.Entities.SanctionList;

/// <summary>
/// UN XML <c>INDIVIDUAL_DOCUMENT</c>. One identity document attached to a sanction entry
/// (passport, national-ID, etc.). Stored for completeness; not used for screening scoring.
/// </summary>
public class SanctionDocument
{
    public string? Type { get; set; }
    public string? Type2 { get; set; }
    public string? Number { get; set; }
    public string? IssuingCountry { get; set; }
    public DateTime? DateOfIssue { get; set; }
    public string? Note { get; set; }
}
