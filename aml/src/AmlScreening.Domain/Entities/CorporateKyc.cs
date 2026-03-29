using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class CorporateKyc : IEntity, IAuditable, ISoftDelete, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }

    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public string FormPayload { get; set; } = "{}";

    public Customer Customer { get; set; } = null!;
}
