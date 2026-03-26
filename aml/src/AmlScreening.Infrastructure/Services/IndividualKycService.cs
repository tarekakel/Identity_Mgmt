using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.IndividualKyc;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class IndividualKycService : IIndividualKycService
{
    private readonly ApplicationDbContext _context;

    public IndividualKycService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<IndividualKycDto>> GetActiveAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.IndividualKyc
            .AsNoTracking()
            .Where(k => k.CustomerId == customerId && k.IsActive)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
            return new ApiResponse<IndividualKycDto> { Success = true, Data = null };

        return ApiResponse<IndividualKycDto>.Ok(MapToDto(entity));
    }

    public async Task<ApiResponse<IndividualKycDto>> CreateActiveAsync(
        Guid customerId,
        UpsertIndividualKycRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);
        if (!customerExists)
            return ApiResponse<IndividualKycDto>.Fail("Customer not found.");

        // Mark existing active rows as inactive (keep history).
        var previousActive = await _context.IndividualKyc
            .Where(k => k.CustomerId == customerId && k.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var prev in previousActive)
            prev.IsActive = false;

        var entity = new IndividualKyc
        {
            Id = Guid.NewGuid(),
            TenantId = dto.TenantId,
            CustomerId = customerId,

            ApplicantName = dto.ApplicantName?.Trim() ?? string.Empty,
            ApplicantAliases = dto.ApplicantAliases?.Trim(),
            ApplicantMobileNo = dto.ApplicantMobileNo?.Trim(),
            ApplicantNationalityId = dto.ApplicantNationalityId,
            ApplicantDualNationality = dto.ApplicantDualNationality,
            ApplicantGenderId = dto.ApplicantGenderId,
            ApplicantDateOfBirth = dto.ApplicantDateOfBirth,

            ApplicantResidenceStatus = dto.ApplicantResidenceStatus?.Trim(),
            ApplicantEmirate = dto.ApplicantEmirate?.Trim(),
            ApplicantCountryOfBirth = dto.ApplicantCountryOfBirth?.Trim(),
            ApplicantCity = dto.ApplicantCity?.Trim(),
            ApplicantEmail = dto.ApplicantEmail?.Trim(),
            ApplicantResidentialAddress = dto.ApplicantResidentialAddress?.Trim(),
            ApplicantOfficeNoBuildingNameStreetArea = dto.ApplicantOfficeNoBuildingNameStreetArea?.Trim(),
            ApplicantPOBox = dto.ApplicantPOBox?.Trim(),
            ApplicantCustomerRelationship = dto.ApplicantCustomerRelationship?.Trim(),
            ApplicantPreferredChannel = dto.ApplicantPreferredChannel?.Trim(),
            ApplicantProductType = dto.ApplicantProductType?.Trim(),
            ApplicantIndustryType = dto.ApplicantIndustryType?.Trim(),
            ApplicantOccupationId = dto.ApplicantOccupationId,

            ApplicantSourceOfFundsId = dto.ApplicantSourceOfFundsId,
            ApplicantSourceOfFundsComments = dto.ApplicantSourceOfFundsComments?.Trim(),
            ApplicantIsProofOfSourceFundsObtained = dto.ApplicantIsProofOfSourceFundsObtained,
            ApplicantSourceOfFundsProofComments = dto.ApplicantSourceOfFundsProofComments?.Trim(),

            ApplicantSourceOfWealth = dto.ApplicantSourceOfWealth?.Trim(),
            ApplicantSourceOfWealthComments = dto.ApplicantSourceOfWealthComments?.Trim(),
            ApplicantIsProofOfSourceWealthObtained = dto.ApplicantIsProofOfSourceWealthObtained,
            ApplicantSourceOfWealthProofComments = dto.ApplicantSourceOfWealthProofComments?.Trim(),

            ClientIdTypeCode = dto.ClientIdTypeCode?.Trim(),
            ClientIdNumber = dto.ClientIdNumber?.Trim(),
            ClientEmiratesIdNumber = dto.ClientEmiratesIdNumber?.Trim(),
            ClientIdExpiryDate = dto.ClientIdExpiryDate,
            ClientPassportNumber = dto.ClientPassportNumber?.Trim(),
            ClientPassportDateOfIssue = dto.ClientPassportDateOfIssue,
            ClientCountryIssuanceId = dto.ClientCountryIssuanceId,

            SponsorName = dto.SponsorName?.Trim(),
            SponsorAliases = dto.SponsorAliases?.Trim(),
            SponsorIdTypeCode = dto.SponsorIdTypeCode?.Trim(),
            SponsorIdNumber = dto.SponsorIdNumber?.Trim(),
            SponsorDateOfBirth = dto.SponsorDateOfBirth,
            SponsorNationalityId = dto.SponsorNationalityId,
            SponsorGenderId = dto.SponsorGenderId,
            SponsorDualNationality = dto.SponsorDualNationality,
            SponsorOtherDetails = dto.SponsorOtherDetails?.Trim(),

            BankCountryId = dto.BankCountryId,
            BankIbanAccountNo = dto.BankIbanAccountNo?.Trim(),
            BankName = dto.BankName?.Trim(),
            AccountName = dto.AccountName?.Trim(),
            BankSwiftCode = dto.BankSwiftCode?.Trim(),
            BankAddress = dto.BankAddress?.Trim(),
            BankCurrency = dto.BankCurrency?.Trim(),

            EmployerCompanyName = dto.EmployerCompanyName?.Trim(),
            EmployerCompanyWebsite = dto.EmployerCompanyWebsite?.Trim(),
            EmployerEmailAddress = dto.EmployerEmailAddress?.Trim(),
            EmployerTelNo = dto.EmployerTelNo?.Trim(),
            EmployerAddress = dto.EmployerAddress?.Trim(),
            EmployerIndustryAndBusinessDetails = dto.EmployerIndustryAndBusinessDetails?.Trim(),

            PepFATFIncreasedMonitoringAnswer = dto.PepFATFIncreasedMonitoringAnswer?.Trim(),
            PepSanctionListOrInverseMediaAnswer = dto.PepSanctionListOrInverseMediaAnswer?.Trim(),
            PepProminentPublicFunctionsAnswer = dto.PepProminentPublicFunctionsAnswer?.Trim(),
            PepAnyPEPsAfterScreeningAnswer = dto.PepAnyPEPsAfterScreeningAnswer?.Trim(),
            PepSpecificPEPsAfterScreeningDetails = dto.PepSpecificPEPsAfterScreeningDetails?.Trim(),

            FollowUpDate = dto.FollowUpDate,
            FollowUpRemarks = dto.FollowUpRemarks?.Trim(),

            IsActive = true
        };

        _context.IndividualKyc.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<IndividualKycDto>.Ok(MapToDto(entity));
    }

    private static IndividualKycDto MapToDto(IndividualKyc k) => new()
    {
        Id = k.Id,
        TenantId = k.TenantId,
        CustomerId = k.CustomerId,
        IsActive = k.IsActive,
        IsDeleted = k.IsDeleted,

        ApplicantName = k.ApplicantName,
        ApplicantAliases = k.ApplicantAliases,
        ApplicantMobileNo = k.ApplicantMobileNo,
        ApplicantNationalityId = k.ApplicantNationalityId,
        ApplicantDualNationality = k.ApplicantDualNationality,
        ApplicantGenderId = k.ApplicantGenderId,
        ApplicantDateOfBirth = k.ApplicantDateOfBirth,
        ApplicantResidenceStatus = k.ApplicantResidenceStatus,
        ApplicantEmirate = k.ApplicantEmirate,
        ApplicantCountryOfBirth = k.ApplicantCountryOfBirth,
        ApplicantCity = k.ApplicantCity,
        ApplicantEmail = k.ApplicantEmail,
        ApplicantResidentialAddress = k.ApplicantResidentialAddress,
        ApplicantOfficeNoBuildingNameStreetArea = k.ApplicantOfficeNoBuildingNameStreetArea,
        ApplicantPOBox = k.ApplicantPOBox,
        ApplicantCustomerRelationship = k.ApplicantCustomerRelationship,
        ApplicantPreferredChannel = k.ApplicantPreferredChannel,
        ApplicantProductType = k.ApplicantProductType,
        ApplicantIndustryType = k.ApplicantIndustryType,
        ApplicantOccupationId = k.ApplicantOccupationId,

        ApplicantSourceOfFundsId = k.ApplicantSourceOfFundsId,
        ApplicantSourceOfFundsComments = k.ApplicantSourceOfFundsComments,
        ApplicantIsProofOfSourceFundsObtained = k.ApplicantIsProofOfSourceFundsObtained,
        ApplicantSourceOfFundsProofComments = k.ApplicantSourceOfFundsProofComments,

        ApplicantSourceOfWealth = k.ApplicantSourceOfWealth,
        ApplicantSourceOfWealthComments = k.ApplicantSourceOfWealthComments,
        ApplicantIsProofOfSourceWealthObtained = k.ApplicantIsProofOfSourceWealthObtained,
        ApplicantSourceOfWealthProofComments = k.ApplicantSourceOfWealthProofComments,

        ClientIdTypeCode = k.ClientIdTypeCode,
        ClientIdNumber = k.ClientIdNumber,
        ClientEmiratesIdNumber = k.ClientEmiratesIdNumber,
        ClientIdExpiryDate = k.ClientIdExpiryDate,
        ClientPassportNumber = k.ClientPassportNumber,
        ClientPassportDateOfIssue = k.ClientPassportDateOfIssue,
        ClientCountryIssuanceId = k.ClientCountryIssuanceId,

        SponsorName = k.SponsorName,
        SponsorAliases = k.SponsorAliases,
        SponsorIdTypeCode = k.SponsorIdTypeCode,
        SponsorIdNumber = k.SponsorIdNumber,
        SponsorDateOfBirth = k.SponsorDateOfBirth,
        SponsorNationalityId = k.SponsorNationalityId,
        SponsorGenderId = k.SponsorGenderId,
        SponsorDualNationality = k.SponsorDualNationality,
        SponsorOtherDetails = k.SponsorOtherDetails,

        BankCountryId = k.BankCountryId,
        BankIbanAccountNo = k.BankIbanAccountNo,
        BankName = k.BankName,
        AccountName = k.AccountName,
        BankSwiftCode = k.BankSwiftCode,
        BankAddress = k.BankAddress,
        BankCurrency = k.BankCurrency,

        EmployerCompanyName = k.EmployerCompanyName,
        EmployerCompanyWebsite = k.EmployerCompanyWebsite,
        EmployerEmailAddress = k.EmployerEmailAddress,
        EmployerTelNo = k.EmployerTelNo,
        EmployerAddress = k.EmployerAddress,
        EmployerIndustryAndBusinessDetails = k.EmployerIndustryAndBusinessDetails,

        PepFATFIncreasedMonitoringAnswer = k.PepFATFIncreasedMonitoringAnswer,
        PepSanctionListOrInverseMediaAnswer = k.PepSanctionListOrInverseMediaAnswer,
        PepProminentPublicFunctionsAnswer = k.PepProminentPublicFunctionsAnswer,
        PepAnyPEPsAfterScreeningAnswer = k.PepAnyPEPsAfterScreeningAnswer,
        PepSpecificPEPsAfterScreeningDetails = k.PepSpecificPEPsAfterScreeningDetails,

        FollowUpDate = k.FollowUpDate,
        FollowUpRemarks = k.FollowUpRemarks
    };
}

