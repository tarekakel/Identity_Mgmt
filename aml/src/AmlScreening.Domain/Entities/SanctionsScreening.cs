using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class SanctionsScreening : IEntity, IAuditable, ISoftDelete
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string ScreeningList { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? MatchedName { get; set; }
    public decimal? Score { get; set; }
    public DateTime ScreenedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public Customer Customer { get; set; } = null!;
}
