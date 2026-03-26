namespace AmlScreening.Application.DTOs.IndividualScreening;

public class IndividualScreeningRequestDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }

    public string? ReferenceId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Guid? NationalityId { get; set; }
    public Guid? PlaceOfBirthCountryId { get; set; }
    public string? IdType { get; set; }
    public string? IdNumber { get; set; }
    public string? Address { get; set; }
    public Guid? GenderId { get; set; }

    public int MatchThreshold { get; set; }
    public int? BirthYearRange { get; set; }

    public bool CheckPepUkOnly { get; set; }
    public bool CheckSanctions { get; set; }
    public bool CheckProfileOfInterest { get; set; }
    public bool CheckDisqualifiedDirectorUkOnly { get; set; }
    public bool CheckReputationalRiskExposure { get; set; }
    public bool CheckRegulatoryEnforcementList { get; set; }
    public bool CheckInsolvencyUkIreland { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

public class UpsertIndividualScreeningRequestDto
{
    public Guid TenantId { get; set; }

    public string? ReferenceId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Guid? NationalityId { get; set; }
    public Guid? PlaceOfBirthCountryId { get; set; }
    public string? IdType { get; set; }
    public string? IdNumber { get; set; }
    public string? Address { get; set; }
    public Guid? GenderId { get; set; }

    public int MatchThreshold { get; set; } = 75;
    public int? BirthYearRange { get; set; }

    public bool CheckPepUkOnly { get; set; }
    public bool CheckSanctions { get; set; }
    public bool CheckProfileOfInterest { get; set; }
    public bool CheckDisqualifiedDirectorUkOnly { get; set; }
    public bool CheckReputationalRiskExposure { get; set; }
    public bool CheckRegulatoryEnforcementList { get; set; }
    public bool CheckInsolvencyUkIreland { get; set; }
}

