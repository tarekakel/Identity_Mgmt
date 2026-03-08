using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Cases;

namespace AmlScreening.Application.Interfaces;

public interface ICaseService
{
    Task<ApiResponse<PagedResult<CaseDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CaseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CaseDto>> CreateAsync(CreateCaseDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CaseDto>> UpdateAsync(Guid id, UpdateCaseDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
