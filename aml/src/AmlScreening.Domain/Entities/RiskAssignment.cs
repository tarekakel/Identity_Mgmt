using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class RiskAssignment : IEntity, IAuditable, ISoftDelete
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public int CountryRisk { get; set; }
    public int CustomerTypeRisk { get; set; }
    public int PepRisk { get; set; }
    public int TransactionRisk { get; set; }
    public int IndustryRisk { get; set; }
    public int TotalScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public Customer Customer { get; set; } = null!;
}
