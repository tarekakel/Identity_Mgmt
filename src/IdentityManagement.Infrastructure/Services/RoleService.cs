using IdentityManagement.Application.Common;
using IdentityManagement.Application.DTOs.Roles;
using IdentityManagement.Application.Interfaces;
using IdentityManagement.Application.Mappings;
using IdentityManagement.Domain.Entities;
using IdentityManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityManagement.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public RoleService(ApplicationDbContext context, IUnitOfWork unitOfWork, ICurrentTenant currentTenant)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<ApiResponse<RoleDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.IsSet)
            return ApiResponse<RoleDto>.Fail("Tenant context is required.");

        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (role == null)
            return ApiResponse<RoleDto>.Fail("Role not found.");

        var permissionCodes = role.RolePermissions.Select(rp => rp.Permission.Code).ToList();
        return ApiResponse<RoleDto>.Ok(role.ToDto(permissionCodes));
    }

    public async Task<ApiResponse<PagedResult<RoleDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.IsSet)
            return ApiResponse<PagedResult<RoleDto>>.Fail("Tenant context is required.");

        var query = _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(r => (r.Name != null && r.Name.ToLower().Contains(term))
                || (r.Description != null && r.Description.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        query = ApplySort(query, request.SortBy, request.SortDescending);

        var pageNumber = request.GetPageNumber();
        var pageSize = request.GetPageSize();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(r => r.ToDto(r.RolePermissions.Select(rp => rp.Permission.Code).ToList())).ToList();
        var paged = new PagedResult<RoleDto>(dtos, totalCount, pageNumber, pageSize);
        return ApiResponse<PagedResult<RoleDto>>.Ok(paged);
    }

    public async Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.TenantId.HasValue)
            return ApiResponse<RoleDto>.Fail("Tenant context is required.");

        var tenantId = _currentTenant.TenantId.Value;
        var normalizedName = request.Name.Trim().ToUpperInvariant();
        if (await _context.Roles.AnyAsync(r => r.TenantId == tenantId && r.NormalizedName == normalizedName, cancellationToken))
            return ApiResponse<RoleDto>.Fail("A role with this name already exists.");

        var role = request.ToEntity(tenantId);
        _context.Roles.Add(role);

        if (request.PermissionIds != null && request.PermissionIds.Count > 0)
        {
            foreach (var permissionId in request.PermissionIds)
                _context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId, AssignedAt = DateTime.UtcNow });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var permissionCodes = await _context.RolePermissions.Where(rp => rp.RoleId == role.Id).Join(_context.Permissions, rp => rp.PermissionId, p => p.Id, (_, p) => p.Code).ToListAsync(cancellationToken);
        return ApiResponse<RoleDto>.Ok(role.ToDto(permissionCodes));
    }

    public async Task<ApiResponse<RoleDto>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.IsSet)
            return ApiResponse<RoleDto>.Fail("Tenant context is required.");

        var role = await _context.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (role == null)
            return ApiResponse<RoleDto>.Fail("Role not found.");

        role.ApplyUpdate(request);
        _context.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var permissionCodes = role.RolePermissions.Select(rp => rp.Permission.Code).ToList();
        return ApiResponse<RoleDto>.Ok(role.ToDto(permissionCodes));
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.IsSet)
            return ApiResponse.Fail("Tenant context is required.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (role == null)
            return ApiResponse.Fail("Role not found.");

        _context.Roles.Remove(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> AssignPermissionsAsync(Guid roleId, AssignPermissionsRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentTenant.IsSet)
            return ApiResponse.Fail("Tenant context is required.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
        if (role == null)
            return ApiResponse.Fail("Role not found.");

        var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(cancellationToken);
        _context.RolePermissions.RemoveRange(existing);

        foreach (var permissionId in request.PermissionIds)
            _context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId, AssignedAt = DateTime.UtcNow });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }

    private static IQueryable<Role> ApplySort(IQueryable<Role> query, string? sortBy, bool sortDescending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "name" => sortDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
            "createdat" => sortDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            _ => query.OrderBy(r => r.Name)
        };
    }
}
