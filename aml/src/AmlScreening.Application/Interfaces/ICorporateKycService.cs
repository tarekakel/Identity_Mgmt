using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.CorporateKyc;

namespace AmlScreening.Application.Interfaces;

public interface ICorporateKycService
{
    Task<ApiResponse<CorporateKycDto>> GetActiveAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<ApiResponse<CorporateKycDto>> CreateActiveAsync(
        Guid customerId,
        UpsertCorporateKycRequestDto dto,
        CancellationToken cancellationToken = default);
}
