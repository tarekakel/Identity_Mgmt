using IdentityManagement.Application.DTOs.Roles;
using IdentityManagement.Domain.Entities;

namespace IdentityManagement.Application.Mappings;

public static class RoleMappings
{
    public static RoleDto ToDto(this Role entity, IReadOnlyList<string> permissionCodes)
    {
        return new RoleDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            PermissionCodes = permissionCodes
        };
    }

    public static RoleDto ToDto(this Role entity)
    {
        return entity.ToDto(Array.Empty<string>());
    }

    public static Role ToEntity(this CreateRoleRequest request, Guid tenantId)
    {
        var name = request.Name.Trim();
        return new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Description = request.Description?.Trim(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void ApplyUpdate(this Role entity, UpdateRoleRequest request)
    {
        entity.Name = request.Name.Trim();
        entity.NormalizedName = request.Name.Trim().ToUpperInvariant();
        entity.Description = request.Description?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
