using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Domain.Entities;

namespace AmlScreening.Application.Interfaces;

public interface ICorporateScreeningRunnerService
{
    Task<ApiResponse<RunSanctionsScreeningResultDto>> RunAsync(CorporateScreeningRequest request, CancellationToken cancellationToken = default);
}
