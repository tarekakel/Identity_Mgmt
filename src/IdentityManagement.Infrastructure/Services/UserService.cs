using IdentityManagement.Application.Common;
using IdentityManagement.Application.DTOs.Users;
using IdentityManagement.Application.Interfaces;
using IdentityManagement.Application.Mappings;
using IdentityManagement.Domain.Entities;
using IdentityManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityManagement.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentTenant _currentTenant;

    public UserService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ICurrentTenant currentTenant)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _currentTenant = currentTenant;
    }

    public async Task<ApiResponse<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.IsSet)
            return ApiResponse<UserDto>.Fail("Tenant context is required.");

        var user = await _context.Users
            .Include(u => u.Tenant)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null)
            return ApiResponse<UserDto>.Fail("User not found.");

        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        return ApiResponse<UserDto>.Ok(user.ToDto(roleNames));
    }

    public async Task<ApiResponse<PagedResult<UserDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.IsSet)
            return ApiResponse<PagedResult<UserDto>>.Fail("Tenant context is required.");

        var query = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(u => (u.Email != null && u.Email.ToLower().Contains(term))
                || (u.UserName != null && u.UserName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySort(query, request.SortBy, request.SortDescending);

        var pageNumber = request.GetPageNumber();
        var pageSize = request.GetPageSize();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(u => u.ToDto(u.UserRoles.Select(ur => ur.Role.Name).ToList())).ToList();
        var paged = new PagedResult<UserDto>(dtos, totalCount, pageNumber, pageSize);
        return ApiResponse<PagedResult<UserDto>>.Ok(paged);
    }

    public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.TenantId.HasValue)
            return ApiResponse<UserDto>.Fail("Tenant context is required.");

        var tenantId = _currentTenant.TenantId.Value;
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        if (await _context.Users.AnyAsync(u => u.TenantId == tenantId && u.NormalizedEmail == normalizedEmail, cancellationToken))
            return ApiResponse<UserDto>.Fail("A user with this email already exists.");

        var user = request.ToEntity(tenantId);
        user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        _context.Users.Add(user);

        if (request.RoleIds != null && request.RoleIds.Count > 0)
        {
            foreach (var roleId in request.RoleIds)
            {
                if (await _context.Roles.AnyAsync(r => r.Id == roleId && r.TenantId == tenantId, cancellationToken))
                    _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId, AssignedAt = DateTime.UtcNow });
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roleNames = await _context.UserRoles.Where(ur => ur.UserId == user.Id).Join(_context.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name).ToListAsync(cancellationToken);
        return ApiResponse<UserDto>.Ok(user.ToDto(roleNames));
    }

    public async Task<ApiResponse<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.IsSet)
            return ApiResponse<UserDto>.Fail("Tenant context is required.");

        var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null)
            return ApiResponse<UserDto>.Fail("User not found.");

        user.ApplyUpdate(request);
        _context.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        return ApiResponse<UserDto>.Ok(user.ToDto(roleNames));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.IsSet)
            return ApiResponse.Fail("Tenant context is required.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null)
            return ApiResponse.Fail("User not found.");

        _context.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> AssignRolesAsync(Guid userId, AssignRolesRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.TenantId.HasValue)
            return ApiResponse.Fail("Tenant context is required.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
            return ApiResponse.Fail("User not found.");

        var existing = await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync(cancellationToken);
        _context.UserRoles.RemoveRange(existing);

        var tenantId = _currentTenant.TenantId.Value;
        foreach (var roleId in request.RoleIds)
        {
            if (await _context.Roles.AnyAsync(r => r.Id == roleId && r.TenantId == tenantId, cancellationToken))
                _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId, AssignedAt = DateTime.UtcNow });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    private static IQueryable<User> ApplySort(IQueryable<User> query, string? sortBy, bool sortDescending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "email" => sortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "username" => sortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
            "createdat" => sortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderBy(u => u.UserName)
        };
    }
}
