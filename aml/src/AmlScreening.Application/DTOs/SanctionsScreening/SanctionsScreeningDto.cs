namespace AmlScreening.Application.DTOs.SanctionsScreening;

public class SanctionsScreeningDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? ScreeningList { get; set; }
    public string? Result { get; set; }
    public string? MatchedName { get; set; }
    public string? MatchType { get; set; }
    public decimal? Score { get; set; }
    public DateTime? ScreenedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

public class CreateSanctionsScreeningDto
{
    public Guid CustomerId { get; set; }
    public string ScreeningList { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? MatchedName { get; set; }
    public string? MatchType { get; set; }
    public decimal? Score { get; set; }
    public DateTime ScreenedAt { get; set; }
}

public class UpdateSanctionsScreeningDto
{
    public string ScreeningList { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? MatchedName { get; set; }
    public string? MatchType { get; set; }
    public decimal? Score { get; set; }
    public DateTime ScreenedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Single result item returned from run sanctions screening.
/// </summary>
public class SanctionsScreeningResultItemDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? MatchedName { get; set; }
    public string? SanctionList { get; set; }
    public decimal? MatchScore { get; set; }
    public string? MatchType { get; set; }
    public DateTime ScreeningDate { get; set; }
    public string Status { get; set; } = string.Empty; // Clear, PossibleMatch, ConfirmedMatch
}

/// <summary>
/// Result of running sanctions screening for a customer.
/// </summary>
public class RunSanctionsScreeningResultDto
{
    public List<SanctionsScreeningResultItemDto> Results { get; set; } = new();
    public bool HasConfirmedMatch { get; set; }
}
