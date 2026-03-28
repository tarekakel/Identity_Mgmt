using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class CorporateScreeningRequest : IEntity, IAuditable, ISoftDelete, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }

    public string? CompanyCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Guid? CountryId { get; set; }
    public DateTime? DateOfRegistration { get; set; }
    public string? TradeLicenceNo { get; set; }
    public string? Address { get; set; }

    public int MatchThreshold { get; set; } = 75;

    public bool CheckPepUkOnly { get; set; }
    public bool CheckSanctions { get; set; }
    public bool CheckProfileOfInterest { get; set; }
    public bool CheckDisqualifiedDirectorUkOnly { get; set; }
    public bool CheckReputationalRiskExposure { get; set; }
    public bool CheckRegulatoryEnforcementList { get; set; }
    public bool CheckInsolvencyUkIreland { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }

    public Customer Customer { get; set; } = null!;
    public Country? Country { get; set; }

    public ICollection<CorporateScreeningCompanyDocument> CompanyDocuments { get; set; } = new List<CorporateScreeningCompanyDocument>();
    public ICollection<CorporateScreeningShareholder> Shareholders { get; set; } = new List<CorporateScreeningShareholder>();
}
