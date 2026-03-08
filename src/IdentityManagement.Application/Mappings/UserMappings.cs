using IdentityManagement.Application.DTOs.Users;
using IdentityManagement.Domain.Entities;

namespace IdentityManagement.Application.Mappings;

public static class UserMappings
{
    public static UserDto ToDto(this User entity, IReadOnlyList<string> roleNames)
    {
        return new UserDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            UserName = entity.UserName,
            Email = entity.Email,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            RoleNames = roleNames
        };
    }

    public static UserDto ToDto(this User entity)
    {
        return entity.ToDto(Array.Empty<string>());
    }

    public static User ToEntity(this CreateUserRequest request, Guid tenantId)
    {
        var email = request.Email.Trim();
        var userName = request.UserName.Trim();
        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            IsActive = request.IsActive,
            EmailConfirmed = false,
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void ApplyUpdate(this User entity, UpdateUserRequest request)
    {
        entity.UserName = request.UserName.Trim();
        entity.NormalizedUserName = request.UserName.Trim().ToUpperInvariant();
        entity.Email = request.Email.Trim();
        entity.NormalizedEmail = request.Email.Trim().ToUpperInvariant();
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
