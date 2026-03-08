using IdentityManagement.Application.DTOs.Tenants;
using IdentityManagement.Domain.Entities;

namespace IdentityManagement.Application.Mappings;

public static class TenantMappings
{
    public static TenantDto ToDto(this Tenant entity)
    {
        return new TenantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }

    public static Tenant ToEntity(this CreateTenantRequest request)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Code.ToUpperInvariant(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void ApplyUpdate(this Tenant entity, UpdateTenantRequest request)
    {
        entity.Name = request.Name;
        entity.Code = request.Code.ToUpperInvariant();
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
