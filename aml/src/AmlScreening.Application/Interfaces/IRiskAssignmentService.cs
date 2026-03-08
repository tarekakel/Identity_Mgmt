using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.RiskAssignment;

namespace AmlScreening.Application.Interfaces;

public interface IRiskAssignmentService
{
    Task<ApiResponse<PagedResult<RiskAssignmentDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<RiskAssignmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<RiskAssignmentDto>> CreateAsync(CreateRiskAssignmentDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<RiskAssignmentDto>> UpdateAsync(Guid id, UpdateRiskAssignmentDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
