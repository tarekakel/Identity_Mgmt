using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;

namespace AmlScreening.Application.Interfaces;

public interface IRunSanctionsScreeningService
{
    Task<ApiResponse<RunSanctionsScreeningResultDto>> RunScreeningForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>> GetResultsForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
