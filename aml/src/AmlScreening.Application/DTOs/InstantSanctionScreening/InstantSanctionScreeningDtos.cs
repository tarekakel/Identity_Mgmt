namespace AmlScreening.Application.DTOs.InstantSanctionScreening;

public class InstantSanctionScreeningSearchRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public Guid? NationalityId { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class InstantSanctionScreeningResultItemDto
{
    public decimal MatchScore { get; set; }
    public string? CustomerId { get; set; }
    public string Uid { get; set; } = string.Empty;
    public string? EntryType { get; set; }
    public string? Name { get; set; }
    public string? NationalityOrCountry { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? IdNumber { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime? CreatedOn { get; set; }
    public string? Remarks { get; set; }
}
