using IdentityManagement.Application.Common;
using IdentityManagement.Application.DTOs.Permissions;

namespace IdentityManagement.Application.Interfaces;

public interface IPermissionService
{
    Task<ApiResponse<IReadOnlyList<PermissionDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PermissionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PermissionDto>> CreateAsync(CreatePermissionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PermissionDto>> UpdateAsync(Guid id, UpdatePermissionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
