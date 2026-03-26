using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.IndividualScreening;
using AmlScreening.Application.DTOs.SanctionsScreening;

namespace AmlScreening.Application.Interfaces;

public interface IIndividualScreeningService
{
    Task<ApiResponse<IndividualScreeningRequestDto>> GetLatestAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IndividualScreeningRequestDto>> UpsertAsync(Guid customerId, UpsertIndividualScreeningRequestDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<RunSanctionsScreeningResultDto>> RunAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>> GetResultsAsync(Guid customerId, CancellationToken cancellationToken = default);
}

