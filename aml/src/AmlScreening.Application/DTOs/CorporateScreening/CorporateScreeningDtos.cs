namespace AmlScreening.Application.DTOs.CorporateScreening;

public class CorporateScreeningCompanyDocumentDto
{
    public Guid Id { get; set; }
    public string? DocumentNo { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Details { get; set; }
    public string? Remarks { get; set; }
}

public class CorporateScreeningShareholderDocumentDto
{
    public Guid Id { get; set; }
    public string? DocumentNo { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Details { get; set; }
    public string? Remarks { get; set; }
}

public class CorporateScreeningShareholderDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Guid? NationalityId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public decimal SharePercent { get; set; }
    public List<CorporateScreeningShareholderDocumentDto> Documents { get; set; } = new();
}

public class CorporateScreeningRequestDto
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

    public int MatchThreshold { get; set; }

    public bool CheckPepUkOnly { get; set; }
    public bool CheckSanctions { get; set; }
    public bool CheckProfileOfInterest { get; set; }
    public bool CheckDisqualifiedDirectorUkOnly { get; set; }
    public bool CheckReputationalRiskExposure { get; set; }
    public bool CheckRegulatoryEnforcementList { get; set; }
    public bool CheckInsolvencyUkIreland { get; set; }

    public List<CorporateScreeningCompanyDocumentDto> CompanyDocuments { get; set; } = new();
    public List<CorporateScreeningShareholderDto> Shareholders { get; set; } = new();

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpsertCorporateScreeningCompanyDocumentDto
{
    public string? DocumentNo { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Details { get; set; }
    public string? Remarks { get; set; }
}

public class UpsertCorporateScreeningShareholderDocumentDto
{
    public string? DocumentNo { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Details { get; set; }
    public string? Remarks { get; set; }
}

public class UpsertCorporateScreeningShareholderDto
{
    public string FullName { get; set; } = string.Empty;
    public Guid? NationalityId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public decimal SharePercent { get; set; }
    public List<UpsertCorporateScreeningShareholderDocumentDto> Documents { get; set; } = new();
}

public class UpsertCorporateScreeningRequestDto
{
    /// <summary>When set, updates this existing request for the customer; when null, creates a new request.</summary>
    public Guid? Id { get; set; }

    public Guid TenantId { get; set; }

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

    public List<UpsertCorporateScreeningCompanyDocumentDto> CompanyDocuments { get; set; } = new();
    public List<UpsertCorporateScreeningShareholderDto> Shareholders { get; set; } = new();
}
