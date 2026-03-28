using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Lookups;
using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Services;

public class MasterLookupCrudService : IMasterLookupCrudService
{
    private readonly ApplicationDbContext _db;

    public MasterLookupCrudService(ApplicationDbContext db)
    {
        _db = db;
    }

    private static string? ValidateUpsert(UpsertMasterLookupRequest request)
    {
        var code = request.Code?.Trim() ?? string.Empty;
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(code)) return "Code is required.";
        if (string.IsNullOrEmpty(name)) return "Name is required.";
        if (code.Length > 64) return "Code must be at most 64 characters.";
        if (name.Length > 128) return "Name must be at most 128 characters.";
        return null;
    }

    #region Country

    public async Task<ApiResponse<IReadOnlyList<CountryDto>>> ListCountriesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.Countries.AsNoTracking().OrderBy(c => c.Code)
            .Select(c => new CountryDto { Id = c.Id, Code = c.Code, Name = c.Name }).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<CountryDto>>.Ok(list);
    }

    public async Task<ApiResponse<CountryDto>> GetCountryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.Countries.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (row == null) return ApiResponse<CountryDto>.Fail("Country not found.");
        return ApiResponse<CountryDto>.Ok(new CountryDto { Id = row.Id, Code = row.Code, Name = row.Name });
    }

    public async Task<ApiResponse<CountryDto>> CreateCountryAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<CountryDto>.Fail(err);
        var code = request.Code.Trim();
        if (await _db.Countries.AnyAsync(c => c.Code == code, cancellationToken))
            return ApiResponse<CountryDto>.Fail("A country with this code already exists.");
        var e = new Country { Id = Guid.NewGuid(), Code = code, Name = request.Name.Trim() };
        _db.Countries.Add(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<CountryDto>.Ok(new CountryDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse<CountryDto>> UpdateCountryAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<CountryDto>.Fail(err);
        var e = await _db.Countries.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (e == null) return ApiResponse<CountryDto>.Fail("Country not found.");
        var code = request.Code.Trim();
        if (await _db.Countries.AnyAsync(c => c.Code == code && c.Id != id, cancellationToken))
            return ApiResponse<CountryDto>.Fail("A country with this code already exists.");
        e.Code = code;
        e.Name = request.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<CountryDto>.Ok(new CountryDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse> DeleteCountryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await _db.Customers.AnyAsync(c => c.CountryOfResidenceId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: country is referenced by one or more customers.");
        if (await _db.CorporateScreeningRequests.AnyAsync(c => c.CountryId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: country is referenced by corporate screening.");
        if (await _db.IndividualScreeningRequests.AnyAsync(c => c.PlaceOfBirthCountryId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: country is referenced by individual screening.");
        if (await _db.IndividualKyc.AnyAsync(k => k.ClientCountryIssuanceId == id || k.BankCountryId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: country is referenced by KYC records.");
        var e = await _db.Countries.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (e == null) return ApiResponse.Fail("Country not found.");
        _db.Countries.Remove(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    #endregion

    #region Nationality

    public async Task<ApiResponse<IReadOnlyList<NationalityDto>>> ListNationalitiesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.Nationalities.AsNoTracking().OrderBy(n => n.Code)
            .Select(n => new NationalityDto { Id = n.Id, Code = n.Code, Name = n.Name }).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<NationalityDto>>.Ok(list);
    }

    public async Task<ApiResponse<NationalityDto>> GetNationalityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.Nationalities.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (row == null) return ApiResponse<NationalityDto>.Fail("Nationality not found.");
        return ApiResponse<NationalityDto>.Ok(new NationalityDto { Id = row.Id, Code = row.Code, Name = row.Name });
    }

    public async Task<ApiResponse<NationalityDto>> CreateNationalityAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<NationalityDto>.Fail(err);
        var code = request.Code.Trim();
        if (await _db.Nationalities.AnyAsync(n => n.Code == code, cancellationToken))
            return ApiResponse<NationalityDto>.Fail("A nationality with this code already exists.");
        var e = new Nationality { Id = Guid.NewGuid(), Code = code, Name = request.Name.Trim() };
        _db.Nationalities.Add(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<NationalityDto>.Ok(new NationalityDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse<NationalityDto>> UpdateNationalityAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<NationalityDto>.Fail(err);
        var e = await _db.Nationalities.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (e == null) return ApiResponse<NationalityDto>.Fail("Nationality not found.");
        var code = request.Code.Trim();
        if (await _db.Nationalities.AnyAsync(n => n.Code == code && n.Id != id, cancellationToken))
            return ApiResponse<NationalityDto>.Fail("A nationality with this code already exists.");
        e.Code = code;
        e.Name = request.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<NationalityDto>.Ok(new NationalityDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse> DeleteNationalityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await _db.Customers.AnyAsync(c => c.NationalityId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: nationality is referenced by one or more customers.");
        if (await _db.IndividualScreeningRequests.AnyAsync(c => c.NationalityId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: nationality is referenced by individual screening.");
        if (await _db.Set<CorporateScreeningShareholder>().AnyAsync(s => s.NationalityId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: nationality is referenced by corporate screening shareholders.");
        if (await _db.IndividualKyc.AnyAsync(k => k.ApplicantNationalityId == id || k.SponsorNationalityId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: nationality is referenced by KYC records.");
        var e = await _db.Nationalities.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (e == null) return ApiResponse.Fail("Nationality not found.");
        _db.Nationalities.Remove(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    #endregion

    #region Gender

    public async Task<ApiResponse<IReadOnlyList<GenderDto>>> ListGendersAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.Genders.AsNoTracking().OrderBy(g => g.Code)
            .Select(g => new GenderDto { Id = g.Id, Code = g.Code, Name = g.Name }).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<GenderDto>>.Ok(list);
    }

    public async Task<ApiResponse<GenderDto>> GetGenderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.Genders.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (row == null) return ApiResponse<GenderDto>.Fail("Gender not found.");
        return ApiResponse<GenderDto>.Ok(new GenderDto { Id = row.Id, Code = row.Code, Name = row.Name });
    }

    public async Task<ApiResponse<GenderDto>> CreateGenderAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<GenderDto>.Fail(err);
        var code = request.Code.Trim();
        if (await _db.Genders.AnyAsync(g => g.Code == code, cancellationToken))
            return ApiResponse<GenderDto>.Fail("A gender with this code already exists.");
        var e = new Gender { Id = Guid.NewGuid(), Code = code, Name = request.Name.Trim() };
        _db.Genders.Add(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<GenderDto>.Ok(new GenderDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse<GenderDto>> UpdateGenderAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<GenderDto>.Fail(err);
        var e = await _db.Genders.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (e == null) return ApiResponse<GenderDto>.Fail("Gender not found.");
        var code = request.Code.Trim();
        if (await _db.Genders.AnyAsync(g => g.Code == code && g.Id != id, cancellationToken))
            return ApiResponse<GenderDto>.Fail("A gender with this code already exists.");
        e.Code = code;
        e.Name = request.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<GenderDto>.Ok(new GenderDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse> DeleteGenderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await _db.Customers.AnyAsync(c => c.GenderId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: gender is referenced by one or more customers.");
        if (await _db.IndividualScreeningRequests.AnyAsync(c => c.GenderId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: gender is referenced by individual screening.");
        if (await _db.IndividualKyc.AnyAsync(k => k.ApplicantGenderId == id || k.SponsorGenderId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: gender is referenced by KYC records.");
        var e = await _db.Genders.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (e == null) return ApiResponse.Fail("Gender not found.");
        _db.Genders.Remove(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    #endregion

    #region CustomerType

    public async Task<ApiResponse<IReadOnlyList<CustomerTypeDto>>> ListCustomerTypesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.CustomerTypes.AsNoTracking().OrderBy(t => t.Code)
            .Select(t => new CustomerTypeDto { Id = t.Id, Code = t.Code, Name = t.Name }).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<CustomerTypeDto>>.Ok(list);
    }

    public async Task<ApiResponse<CustomerTypeDto>> GetCustomerTypeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.CustomerTypes.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (row == null) return ApiResponse<CustomerTypeDto>.Fail("Customer type not found.");
        return ApiResponse<CustomerTypeDto>.Ok(new CustomerTypeDto { Id = row.Id, Code = row.Code, Name = row.Name });
    }

    public async Task<ApiResponse<CustomerTypeDto>> CreateCustomerTypeAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<CustomerTypeDto>.Fail(err);
        var code = request.Code.Trim();
        if (await _db.CustomerTypes.AnyAsync(t => t.Code == code, cancellationToken))
            return ApiResponse<CustomerTypeDto>.Fail("A customer type with this code already exists.");
        var e = new CustomerType { Id = Guid.NewGuid(), Code = code, Name = request.Name.Trim() };
        _db.CustomerTypes.Add(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<CustomerTypeDto>.Ok(new CustomerTypeDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse<CustomerTypeDto>> UpdateCustomerTypeAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<CustomerTypeDto>.Fail(err);
        var e = await _db.CustomerTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (e == null) return ApiResponse<CustomerTypeDto>.Fail("Customer type not found.");
        var code = request.Code.Trim();
        if (await _db.CustomerTypes.AnyAsync(t => t.Code == code && t.Id != id, cancellationToken))
            return ApiResponse<CustomerTypeDto>.Fail("A customer type with this code already exists.");
        e.Code = code;
        e.Name = request.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<CustomerTypeDto>.Ok(new CustomerTypeDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse> DeleteCustomerTypeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await _db.Customers.AnyAsync(c => c.CustomerTypeId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: customer type is assigned to one or more customers.");
        var e = await _db.CustomerTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (e == null) return ApiResponse.Fail("Customer type not found.");
        _db.CustomerTypes.Remove(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    #endregion

    #region CustomerStatus

    public async Task<ApiResponse<IReadOnlyList<CustomerStatusDto>>> ListCustomerStatusesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.CustomerStatuses.AsNoTracking().OrderBy(s => s.Code)
            .Select(s => new CustomerStatusDto { Id = s.Id, Code = s.Code, Name = s.Name }).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<CustomerStatusDto>>.Ok(list);
    }

    public async Task<ApiResponse<CustomerStatusDto>> GetCustomerStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.CustomerStatuses.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (row == null) return ApiResponse<CustomerStatusDto>.Fail("Customer status not found.");
        return ApiResponse<CustomerStatusDto>.Ok(new CustomerStatusDto { Id = row.Id, Code = row.Code, Name = row.Name });
    }

    public async Task<ApiResponse<CustomerStatusDto>> CreateCustomerStatusAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<CustomerStatusDto>.Fail(err);
        var code = request.Code.Trim();
        if (await _db.CustomerStatuses.AnyAsync(s => s.Code == code, cancellationToken))
            return ApiResponse<CustomerStatusDto>.Fail("A customer status with this code already exists.");
        var e = new CustomerStatus { Id = Guid.NewGuid(), Code = code, Name = request.Name.Trim() };
        _db.CustomerStatuses.Add(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<CustomerStatusDto>.Ok(new CustomerStatusDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse<CustomerStatusDto>> UpdateCustomerStatusAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<CustomerStatusDto>.Fail(err);
        var e = await _db.CustomerStatuses.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (e == null) return ApiResponse<CustomerStatusDto>.Fail("Customer status not found.");
        var code = request.Code.Trim();
        if (await _db.CustomerStatuses.AnyAsync(s => s.Code == code && s.Id != id, cancellationToken))
            return ApiResponse<CustomerStatusDto>.Fail("A customer status with this code already exists.");
        e.Code = code;
        e.Name = request.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<CustomerStatusDto>.Ok(new CustomerStatusDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse> DeleteCustomerStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await _db.Customers.AnyAsync(c => c.StatusId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: status is assigned to one or more customers.");
        var e = await _db.CustomerStatuses.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (e == null) return ApiResponse.Fail("Customer status not found.");
        _db.CustomerStatuses.Remove(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    #endregion

    #region DocumentType

    public async Task<ApiResponse<IReadOnlyList<DocumentTypeDto>>> ListDocumentTypesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.DocumentTypes.AsNoTracking().OrderBy(t => t.Code)
            .Select(t => new DocumentTypeDto { Id = t.Id, Code = t.Code, Name = t.Name }).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<DocumentTypeDto>>.Ok(list);
    }

    public async Task<ApiResponse<DocumentTypeDto>> GetDocumentTypeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.DocumentTypes.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (row == null) return ApiResponse<DocumentTypeDto>.Fail("Document type not found.");
        return ApiResponse<DocumentTypeDto>.Ok(new DocumentTypeDto { Id = row.Id, Code = row.Code, Name = row.Name });
    }

    public async Task<ApiResponse<DocumentTypeDto>> CreateDocumentTypeAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<DocumentTypeDto>.Fail(err);
        var code = request.Code.Trim();
        if (await _db.DocumentTypes.AnyAsync(t => t.Code == code, cancellationToken))
            return ApiResponse<DocumentTypeDto>.Fail("A document type with this code already exists.");
        var e = new DocumentType { Id = Guid.NewGuid(), Code = code, Name = request.Name.Trim() };
        _db.DocumentTypes.Add(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<DocumentTypeDto>.Ok(new DocumentTypeDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse<DocumentTypeDto>> UpdateDocumentTypeAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<DocumentTypeDto>.Fail(err);
        var e = await _db.DocumentTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (e == null) return ApiResponse<DocumentTypeDto>.Fail("Document type not found.");
        var code = request.Code.Trim();
        if (await _db.DocumentTypes.AnyAsync(t => t.Code == code && t.Id != id, cancellationToken))
            return ApiResponse<DocumentTypeDto>.Fail("A document type with this code already exists.");
        e.Code = code;
        e.Name = request.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<DocumentTypeDto>.Ok(new DocumentTypeDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse> DeleteDocumentTypeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await _db.CustomerDocuments.AnyAsync(d => d.DocumentTypeId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: document type is used by customer documents.");
        var e = await _db.DocumentTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (e == null) return ApiResponse.Fail("Document type not found.");
        _db.DocumentTypes.Remove(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    #endregion

    #region Occupation

    public async Task<ApiResponse<IReadOnlyList<OccupationDto>>> ListOccupationsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.Occupations.AsNoTracking().OrderBy(o => o.Code)
            .Select(o => new OccupationDto { Id = o.Id, Code = o.Code, Name = o.Name }).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<OccupationDto>>.Ok(list);
    }

    public async Task<ApiResponse<OccupationDto>> GetOccupationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.Occupations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (row == null) return ApiResponse<OccupationDto>.Fail("Occupation not found.");
        return ApiResponse<OccupationDto>.Ok(new OccupationDto { Id = row.Id, Code = row.Code, Name = row.Name });
    }

    public async Task<ApiResponse<OccupationDto>> CreateOccupationAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<OccupationDto>.Fail(err);
        var code = request.Code.Trim();
        if (await _db.Occupations.AnyAsync(o => o.Code == code, cancellationToken))
            return ApiResponse<OccupationDto>.Fail("An occupation with this code already exists.");
        var e = new Occupation { Id = Guid.NewGuid(), Code = code, Name = request.Name.Trim() };
        _db.Occupations.Add(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<OccupationDto>.Ok(new OccupationDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse<OccupationDto>> UpdateOccupationAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<OccupationDto>.Fail(err);
        var e = await _db.Occupations.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (e == null) return ApiResponse<OccupationDto>.Fail("Occupation not found.");
        var code = request.Code.Trim();
        if (await _db.Occupations.AnyAsync(o => o.Code == code && o.Id != id, cancellationToken))
            return ApiResponse<OccupationDto>.Fail("An occupation with this code already exists.");
        e.Code = code;
        e.Name = request.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<OccupationDto>.Ok(new OccupationDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse> DeleteOccupationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await _db.Customers.AnyAsync(c => c.OccupationId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: occupation is referenced by one or more customers.");
        if (await _db.IndividualKyc.AnyAsync(k => k.ApplicantOccupationId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: occupation is referenced by KYC records.");
        var e = await _db.Occupations.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (e == null) return ApiResponse.Fail("Occupation not found.");
        _db.Occupations.Remove(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    #endregion

    #region SourceOfFunds

    public async Task<ApiResponse<IReadOnlyList<SourceOfFundsDto>>> ListSourceOfFundsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.SourceOfFunds.AsNoTracking().OrderBy(s => s.Code)
            .Select(s => new SourceOfFundsDto { Id = s.Id, Code = s.Code, Name = s.Name }).ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<SourceOfFundsDto>>.Ok(list);
    }

    public async Task<ApiResponse<SourceOfFundsDto>> GetSourceOfFundsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _db.SourceOfFunds.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (row == null) return ApiResponse<SourceOfFundsDto>.Fail("Source of funds not found.");
        return ApiResponse<SourceOfFundsDto>.Ok(new SourceOfFundsDto { Id = row.Id, Code = row.Code, Name = row.Name });
    }

    public async Task<ApiResponse<SourceOfFundsDto>> CreateSourceOfFundsAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<SourceOfFundsDto>.Fail(err);
        var code = request.Code.Trim();
        if (await _db.SourceOfFunds.AnyAsync(s => s.Code == code, cancellationToken))
            return ApiResponse<SourceOfFundsDto>.Fail("A source of funds with this code already exists.");
        var e = new SourceOfFunds { Id = Guid.NewGuid(), Code = code, Name = request.Name.Trim() };
        _db.SourceOfFunds.Add(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<SourceOfFundsDto>.Ok(new SourceOfFundsDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse<SourceOfFundsDto>> UpdateSourceOfFundsAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default)
    {
        var err = ValidateUpsert(request);
        if (err != null) return ApiResponse<SourceOfFundsDto>.Fail(err);
        var e = await _db.SourceOfFunds.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (e == null) return ApiResponse<SourceOfFundsDto>.Fail("Source of funds not found.");
        var code = request.Code.Trim();
        if (await _db.SourceOfFunds.AnyAsync(s => s.Code == code && s.Id != id, cancellationToken))
            return ApiResponse<SourceOfFundsDto>.Fail("A source of funds with this code already exists.");
        e.Code = code;
        e.Name = request.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<SourceOfFundsDto>.Ok(new SourceOfFundsDto { Id = e.Id, Code = e.Code, Name = e.Name });
    }

    public async Task<ApiResponse> DeleteSourceOfFundsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (await _db.Customers.AnyAsync(c => c.SourceOfFundsId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: source of funds is referenced by one or more customers.");
        if (await _db.IndividualKyc.AnyAsync(k => k.ApplicantSourceOfFundsId == id, cancellationToken))
            return ApiResponse.Fail("Cannot delete: source of funds is referenced by KYC records.");
        var e = await _db.SourceOfFunds.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (e == null) return ApiResponse.Fail("Source of funds not found.");
        _db.SourceOfFunds.Remove(e);
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    #endregion
}
