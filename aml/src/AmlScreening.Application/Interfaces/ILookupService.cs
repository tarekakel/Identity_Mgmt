using AmlScreening.Application.DTOs.Lookups;

namespace AmlScreening.Application.Interfaces;

public interface ILookupService
{
    Task<IReadOnlyList<CustomerTypeDto>> GetCustomerTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerStatusDto>> GetCustomerStatusesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GenderDto>> GetGendersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NationalityDto>> GetNationalitiesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentTypeDto>> GetDocumentTypesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OccupationDto>> GetOccupationsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SourceOfFundsDto>> GetSourceOfFundsAsync(CancellationToken cancellationToken = default);
}
