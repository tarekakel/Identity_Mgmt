using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.IndividualKyc;

namespace AmlScreening.Application.Interfaces;

public interface IIndividualKycService
{
    Task<ApiResponse<IndividualKycDto>> GetActiveAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IndividualKycDto>> CreateActiveAsync(Guid customerId, UpsertIndividualKycRequestDto dto, CancellationToken cancellationToken = default);
}

