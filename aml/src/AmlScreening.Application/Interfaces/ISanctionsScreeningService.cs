using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionsScreening;

namespace AmlScreening.Application.Interfaces;

public interface ISanctionsScreeningService
{
    Task<ApiResponse<PagedResult<SanctionsScreeningDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SanctionsScreeningDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<SanctionsScreeningDto>> CreateAsync(CreateSanctionsScreeningDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<SanctionsScreeningDto>> UpdateAsync(Guid id, UpdateSanctionsScreeningDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
