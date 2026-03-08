using IdentityManagement.Application.DTOs.Permissions;
using IdentityManagement.Domain.Entities;

namespace IdentityManagement.Application.Mappings;

public static class PermissionMappings
{
    public static PermissionDto ToDto(this Permission entity)
    {
        return new PermissionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            Description = entity.Description
        };
    }
}
