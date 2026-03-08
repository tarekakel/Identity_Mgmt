using IdentityManagement.Application.Common;
using IdentityManagement.Application.DTOs.Permissions;
using IdentityManagement.Application.Interfaces;
using IdentityManagement.Application.Mappings;
using IdentityManagement.Domain.Entities;
using IdentityManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityManagement.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<IReadOnlyList<PermissionDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.Permissions.OrderBy(p => p.Code).ToListAsync(cancellationToken);
        var dtos = list.Select(p => p.ToDto()).ToList();
        return ApiResponse<IReadOnlyList<PermissionDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<PermissionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await _context.Permissions.FindAsync(new object[] { id }, cancellationToken);
        if (permission == null)
            return ApiResponse<PermissionDto>.Fail("Permission not found.");
        return ApiResponse<PermissionDto>.Ok(permission.ToDto());
    }

    public async Task<ApiResponse<PermissionDto>> CreateAsync(CreatePermissionRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        var code = request.Code?.Trim() ?? string.Empty;
        var description = request.Description?.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code))
            return ApiResponse<PermissionDto>.Fail("Name and Code are required.");

        var codeExists = await _context.Permissions.AnyAsync(p => p.Code == code, cancellationToken);
        if (codeExists)
            return ApiResponse<PermissionDto>.Fail("A permission with this code already exists.");

        var entity = new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Description = string.IsNullOrEmpty(description) ? null : description
        };
        _context.Permissions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<PermissionDto>.Ok(entity.ToDto());
    }

    public async Task<ApiResponse<PermissionDto>> UpdateAsync(Guid id, UpdatePermissionRequest request, CancellationToken cancellationToken = default)
    {
        var permission = await _context.Permissions.FindAsync(new object[] { id }, cancellationToken);
        if (permission == null)
            return ApiResponse<PermissionDto>.Fail("Permission not found.");

        var name = request.Name?.Trim() ?? string.Empty;
        var code = request.Code?.Trim() ?? string.Empty;
        var description = request.Description?.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code))
            return ApiResponse<PermissionDto>.Fail("Name and Code are required.");

        if (permission.Code != code)
        {
            var codeExists = await _context.Permissions.AnyAsync(p => p.Code == code, cancellationToken);
            if (codeExists)
                return ApiResponse<PermissionDto>.Fail("A permission with this code already exists.");
        }

        permission.Name = name;
        permission.Code = code;
        permission.Description = string.IsNullOrEmpty(description) ? null : description;
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<PermissionDto>.Ok(permission.ToDto());
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await _context.Permissions.FindAsync(new object[] { id }, cancellationToken);
        if (permission == null)
            return ApiResponse.Fail("Permission not found.");

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse.Ok();
    }
}
