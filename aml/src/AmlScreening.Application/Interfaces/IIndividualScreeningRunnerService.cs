using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Domain.Entities;

namespace AmlScreening.Application.Interfaces;

public interface IIndividualScreeningRunnerService
{
    Task<ApiResponse<RunSanctionsScreeningResultDto>> RunAsync(IndividualScreeningRequest request, CancellationToken cancellationToken = default);
}

