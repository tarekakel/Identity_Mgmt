using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class SanctionActionAuditLog : IEntity, IAuditable
{
    public Guid Id { get; set; }
    public Guid SanctionsScreeningId { get; set; }
    public string Action { get; set; } = string.Empty; // Approve, Reject
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public SanctionsScreening SanctionsScreening { get; set; } = null!;
}
