using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.InstantSanctionScreening;

namespace AmlScreening.Application.Interfaces;

public interface IInstantSanctionScreeningService
{
    Task<ApiResponse<IReadOnlyList<InstantSanctionScreeningResultItemDto>>> SearchAsync(
        InstantSanctionScreeningSearchRequestDto request,
        CancellationToken cancellationToken = default);
}
