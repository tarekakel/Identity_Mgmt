using IdentityManagement.Application.Common;
using IdentityManagement.Application.DTOs.Tenants;

namespace IdentityManagement.Application.Interfaces;

public interface ITenantService
{
    Task<ApiResponse<TenantDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<TenantDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TenantDto>> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TenantDto>> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
