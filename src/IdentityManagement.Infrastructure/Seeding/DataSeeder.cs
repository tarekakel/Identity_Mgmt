using IdentityManagement.Application.Interfaces;
using IdentityManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityManagement.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        if (await context.Tenants.AnyAsync())
        {
            logger.LogInformation("Database already seeded.");
            return;
        }

        var defaultTenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Default Tenant",
            Code = "DEFAULT",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Tenants.Add(defaultTenant);

        var permissions = new[]
        {
            new Permission { Id = Guid.NewGuid(), Name = "Users Read", Code = "users.read", Description = "View users" },
            new Permission { Id = Guid.NewGuid(), Name = "Users Write", Code = "users.write", Description = "Create and update users" },
            new Permission { Id = Guid.NewGuid(), Name = "Roles Read", Code = "roles.read", Description = "View roles" },
            new Permission { Id = Guid.NewGuid(), Name = "Roles Write", Code = "roles.write", Description = "Create and update roles" },
            new Permission { Id = Guid.NewGuid(), Name = "Tenants Manage", Code = "tenants.manage", Description = "Manage tenants" },
            new Permission { Id = Guid.NewGuid(), Name = "Permissions Read", Code = "permissions.read", Description = "View permissions" }
        };
        context.Permissions.AddRange(permissions);

        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = defaultTenant.Id,
            Name = "Admin",
            NormalizedName = "ADMIN",
            Description = "Administrator with full access",
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
        context.Roles.Add(adminRole);

        foreach (var p in permissions)
            context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id, AssignedAt = DateTime.UtcNow });

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            TenantId = defaultTenant.Id,
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            PasswordHash = passwordHasher.HashPassword("Admin@123"),
            IsActive = true,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);
        context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id, AssignedAt = DateTime.UtcNow });

        await context.SaveChangesAsync();
        logger.LogInformation("Database seeded with default tenant, admin user (admin@example.com / Admin@123), and permissions.");
    }
}
