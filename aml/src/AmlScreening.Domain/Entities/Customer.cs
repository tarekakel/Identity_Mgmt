using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class Customer : IEntity, IAuditable, ISoftDelete, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? NationalIdOrPassport { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? Address { get; set; }
    public string? Occupation { get; set; }
    public string? SourceOfFunds { get; set; }
    public bool IsPep { get; set; }
    public string? BusinessActivity { get; set; }
    public string RiskClassification { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Case> Cases { get; set; } = new List<Case>();
    public ICollection<RiskAssignment> RiskAssignments { get; set; } = new List<RiskAssignment>();
    public ICollection<SanctionsScreening> SanctionsScreenings { get; set; } = new List<SanctionsScreening>();
}
