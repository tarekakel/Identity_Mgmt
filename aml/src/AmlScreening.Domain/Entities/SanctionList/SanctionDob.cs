namespace AmlScreening.Domain.Entities.SanctionList;

/// <summary>
/// One declared date of birth. Some UN entries declare a full <c>DATE</c>, others only a <c>YEAR</c>,
/// and a few use a <c>BETWEEN</c> range with <c>FROM_YEAR</c>/<c>TO_YEAR</c>.
/// </summary>
public class SanctionDob
{
    public DateTime? Date { get; set; }
    public int? Year { get; set; }
    public int? FromYear { get; set; }
    public int? ToYear { get; set; }
    /// <summary>UN <c>TYPE_OF_DATE</c>, e.g. EXACT, BETWEEN, APPROXIMATELY.</summary>
    public string? TypeOfDate { get; set; }
    public string? Note { get; set; }
}
