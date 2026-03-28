using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.CorporateScreening;
using AmlScreening.Application.DTOs.SanctionsScreening;

namespace AmlScreening.Application.Interfaces;

public interface ICorporateScreeningService
{
    Task<ApiResponse<CorporateScreeningRequestDto>> GetLatestAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<CorporateScreeningRequestDto>>> ListAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<ApiResponse<CorporateScreeningRequestDto>> GetByIdAsync(Guid customerId, Guid requestId, CancellationToken cancellationToken = default);

    Task<ApiResponse<CorporateScreeningRequestDto>> UpsertAsync(Guid customerId, UpsertCorporateScreeningRequestDto dto, CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteAsync(Guid customerId, Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>Runs screening for the most recently updated corporate request for this customer (legacy).</summary>
    Task<ApiResponse<RunSanctionsScreeningResultDto>> RunAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<ApiResponse<RunSanctionsScreeningResultDto>> RunForRequestAsync(Guid customerId, Guid requestId, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>> GetResultsAsync(Guid customerId, Guid? corporateScreeningRequestId, CancellationToken cancellationToken = default);
}
