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
    public Guid? CorporateScreeningRequestId { get; set; }
    public string? MatchedName { get; set; }
    public string? SanctionList { get; set; }
    public decimal? MatchScore { get; set; }
    public string? MatchType { get; set; }
    public DateTime ScreeningDate { get; set; }
    public string Status { get; set; } = string.Empty; // Clear, PossibleMatch, ConfirmedMatch
    public string? ReviewStatus { get; set; } // PendingReview, Approved, Rejected
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
}

/// <summary>
/// Request to record a checker action (Approve/Reject) on a screening result.
/// </summary>
public class RecordSanctionScreeningActionRequest
{
    public string Action { get; set; } = string.Empty; // Approve, Reject
    public string? Notes { get; set; }
}

/// <summary>
/// Sanction action audit log entry (history).
/// </summary>
public class SanctionActionAuditLogDto
{
    public Guid Id { get; set; }
    public Guid SanctionsScreeningId { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? MatchedName { get; set; }
    public string? SanctionList { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Result of running sanctions screening for a customer.
/// </summary>
public class RunSanctionsScreeningResultDto
{
    public List<SanctionsScreeningResultItemDto> Results { get; set; } = new();
    public bool HasConfirmedMatch { get; set; }
}
