using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class Customer : IEntity, IAuditable, ISoftDelete, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Guid? GenderId { get; set; }
    public Guid? NationalityId { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiryDate { get; set; }
    public string? NationalId { get; set; }
    public Guid? CountryOfResidenceId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public Guid? OccupationId { get; set; }
    public string? EmployerName { get; set; }
    public Guid? SourceOfFundsId { get; set; }
    public decimal? AnnualIncome { get; set; }
    public decimal? ExpectedMonthlyTransactionVolume { get; set; }
    public decimal? ExpectedMonthlyTransactionValue { get; set; }
    public Guid CustomerTypeId { get; set; }
    public string? AccountPurpose { get; set; }
    public decimal? RiskScore { get; set; }
    public string? RiskLevel { get; set; }
    public Guid StatusId { get; set; }
    public bool IsPep { get; set; }
    public string? BusinessActivity { get; set; }
    public string? NationalIdOrPassport { get; set; }
    public string? RiskClassification { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public CustomerStatus Status { get; set; } = null!;
    public CustomerType CustomerType { get; set; } = null!;
    public Gender? Gender { get; set; }
    public Nationality? Nationality { get; set; }
    public Country? CountryOfResidence { get; set; }
    public Occupation? Occupation { get; set; }
    public SourceOfFunds? SourceOfFunds { get; set; }
    public ICollection<Case> Cases { get; set; } = new List<Case>();
    public ICollection<RiskAssignment> RiskAssignments { get; set; } = new List<RiskAssignment>();
    public ICollection<SanctionsScreening> SanctionsScreenings { get; set; } = new List<SanctionsScreening>();
    public ICollection<CustomerDocument> Documents { get; set; } = new List<CustomerDocument>();
}
