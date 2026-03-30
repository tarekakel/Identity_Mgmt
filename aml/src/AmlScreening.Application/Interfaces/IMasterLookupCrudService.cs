using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Lookups;

namespace AmlScreening.Application.Interfaces;

public interface IMasterLookupCrudService
{
    Task<ApiResponse<IReadOnlyList<CountryDto>>> ListCountriesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CountryDto>> GetCountryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CountryDto>> CreateCountryAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CountryDto>> UpdateCountryAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteCountryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<NationalityDto>>> ListNationalitiesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<NationalityDto>> GetNationalityAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<NationalityDto>> CreateNationalityAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<NationalityDto>> UpdateNationalityAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteNationalityAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<GenderDto>>> ListGendersAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GenderDto>> GetGenderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GenderDto>> CreateGenderAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GenderDto>> UpdateGenderAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteGenderAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<CustomerTypeDto>>> ListCustomerTypesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerTypeDto>> GetCustomerTypeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerTypeDto>> CreateCustomerTypeAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerTypeDto>> UpdateCustomerTypeAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteCustomerTypeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<CustomerStatusDto>>> ListCustomerStatusesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerStatusDto>> GetCustomerStatusAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerStatusDto>> CreateCustomerStatusAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerStatusDto>> UpdateCustomerStatusAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteCustomerStatusAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<DocumentTypeDto>>> ListDocumentTypesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<DocumentTypeDto>> GetDocumentTypeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<DocumentTypeDto>> CreateDocumentTypeAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<DocumentTypeDto>> UpdateDocumentTypeAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteDocumentTypeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<OccupationDto>>> ListOccupationsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<OccupationDto>> GetOccupationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<OccupationDto>> CreateOccupationAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<OccupationDto>> UpdateOccupationAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteOccupationAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<SourceOfFundsDto>>> ListSourceOfFundsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<SourceOfFundsDto>> GetSourceOfFundsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<SourceOfFundsDto>> CreateSourceOfFundsAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SourceOfFundsDto>> UpdateSourceOfFundsAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteSourceOfFundsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<EmirateDto>>> ListEmiratesAsync(Guid? countryId, CancellationToken cancellationToken = default);
    Task<ApiResponse<EmirateDto>> GetEmirateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<EmirateDto>> CreateEmirateAsync(UpsertEmirateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<EmirateDto>> UpdateEmirateAsync(Guid id, UpsertEmirateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteEmirateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<ResidenceStatusDto>>> ListResidenceStatusesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ResidenceStatusDto>> GetResidenceStatusAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ResidenceStatusDto>> CreateResidenceStatusAsync(UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ResidenceStatusDto>> UpdateResidenceStatusAsync(Guid id, UpsertMasterLookupRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteResidenceStatusAsync(Guid id, CancellationToken cancellationToken = default);
}
