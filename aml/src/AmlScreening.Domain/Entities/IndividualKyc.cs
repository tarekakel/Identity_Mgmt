using AmlScreening.Domain.Interfaces;

namespace AmlScreening.Domain.Entities;

public class IndividualKyc : IEntity, IAuditable, ISoftDelete, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }

    // Versioning: keep historical rows, only one active per customer at a time.
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // 1) Applicant personal details
    public string ApplicantName { get; set; } = string.Empty;
    public string? ApplicantAliases { get; set; }
    public string? ApplicantMobileNo { get; set; }
    public Guid? ApplicantNationalityId { get; set; }
    public bool? ApplicantDualNationality { get; set; }
    public Guid? ApplicantGenderId { get; set; }
    public DateTime? ApplicantDateOfBirth { get; set; }
    public Guid? ApplicantResidenceStatusId { get; set; }
    public Guid? ApplicantEmirateId { get; set; }
    public Guid? ApplicantPlaceOfBirthCountryId { get; set; }
    public string? ApplicantCity { get; set; }
    public string? ApplicantEmail { get; set; }
    public string? ApplicantResidentialAddress { get; set; }
    public string? ApplicantOfficeNoBuildingNameStreetArea { get; set; }
    public string? ApplicantPOBox { get; set; }
    public string? ApplicantCustomerRelationship { get; set; }
    public string? ApplicantPreferredChannel { get; set; }
    public string? ApplicantProductType { get; set; }
    public string? ApplicantIndustryType { get; set; }
    public Guid? ApplicantOccupationId { get; set; }

    public Guid? ApplicantSourceOfFundsId { get; set; }
    public string? ApplicantSourceOfFundsComments { get; set; }
    public bool? ApplicantIsProofOfSourceFundsObtained { get; set; }
    public string? ApplicantSourceOfFundsProofComments { get; set; }

    public string? ApplicantSourceOfWealth { get; set; }
    public string? ApplicantSourceOfWealthComments { get; set; }
    public bool? ApplicantIsProofOfSourceWealthObtained { get; set; }
    public string? ApplicantSourceOfWealthProofComments { get; set; }

    // 2) Client identification document details
    public string? ClientIdTypeCode { get; set; }
    public string? ClientIdNumber { get; set; }
    public string? ClientEmiratesIdNumber { get; set; }
    public DateTime? ClientIdExpiryDate { get; set; }
    public string? ClientPassportNumber { get; set; }
    public DateTime? ClientPassportDateOfIssue { get; set; }
    public Guid? ClientCountryIssuanceId { get; set; }

    // 3) Sponsor details
    public string? SponsorName { get; set; }
    public string? SponsorAliases { get; set; }
    public string? SponsorIdTypeCode { get; set; }
    public string? SponsorIdNumber { get; set; }
    public DateTime? SponsorDateOfBirth { get; set; }
    public Guid? SponsorNationalityId { get; set; }
    public Guid? SponsorGenderId { get; set; }
    public bool? SponsorDualNationality { get; set; }
    public string? SponsorOtherDetails { get; set; }

    // 4) Bank details
    public Guid? BankCountryId { get; set; }
    public string? BankIbanAccountNo { get; set; }
    public string? BankName { get; set; }
    public string? AccountName { get; set; }
    public string? BankSwiftCode { get; set; }
    public string? BankAddress { get; set; }
    public string? BankCurrency { get; set; }

    // 5) Employer details
    public string? EmployerCompanyName { get; set; }
    public string? EmployerCompanyWebsite { get; set; }
    public string? EmployerEmailAddress { get; set; }
    public string? EmployerTelNo { get; set; }
    public string? EmployerAddress { get; set; }
    public string? EmployerIndustryAndBusinessDetails { get; set; }

    // 6) Politically exposed person status
    public string? PepFATFIncreasedMonitoringAnswer { get; set; }
    public string? PepSanctionListOrInverseMediaAnswer { get; set; }
    public string? PepProminentPublicFunctionsAnswer { get; set; }
    public string? PepAnyPEPsAfterScreeningAnswer { get; set; }
    public string? PepSpecificPEPsAfterScreeningDetails { get; set; }

    // 7) Follow-up document details
    public DateTime? FollowUpDate { get; set; }
    public string? FollowUpRemarks { get; set; }

    public ResidenceStatus? ApplicantResidenceStatus { get; set; }
    public Emirate? ApplicantEmirate { get; set; }
    public Country? ApplicantPlaceOfBirthCountry { get; set; }

    public Customer Customer { get; set; } = null!;
}

