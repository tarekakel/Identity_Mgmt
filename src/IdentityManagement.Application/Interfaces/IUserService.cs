using IdentityManagement.Application.Common;
using IdentityManagement.Application.DTOs.Users;

namespace IdentityManagement.Application.Interfaces;

public interface IUserService
{
    Task<ApiResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<UserDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse> AssignRolesAsync(Guid userId, AssignRolesRequest request, CancellationToken cancellationToken = default);
}
