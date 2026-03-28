using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class SanctionsScreening : IEntity, IAuditable, ISoftDelete
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    /// <summary>
    /// When set, ties this screening row to a corporate screening request (multi-entity per customer).
    /// </summary>
    public Guid? CorporateScreeningRequestId { get; set; }
    public string ScreeningList { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? MatchedName { get; set; }
    public string? MatchType { get; set; }
    public decimal? Score { get; set; }
    public DateTime ScreenedAt { get; set; }
    public string? ReviewStatus { get; set; } // PendingReview, Approved, Rejected
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public Customer Customer { get; set; } = null!;
    public CorporateScreeningRequest? CorporateScreeningRequest { get; set; }
}
