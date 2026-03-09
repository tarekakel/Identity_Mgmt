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

        var permissions = new List<Permission>
        {
            new() { Id = Guid.NewGuid(), Name = "Users Read", Code = "users.read", Description = "View users" },
            new() { Id = Guid.NewGuid(), Name = "Users Write", Code = "users.write", Description = "Create and update users" },
            new() { Id = Guid.NewGuid(), Name = "Roles Read", Code = "roles.read", Description = "View roles" },
            new() { Id = Guid.NewGuid(), Name = "Roles Write", Code = "roles.write", Description = "Create and update roles" },
            new() { Id = Guid.NewGuid(), Name = "Tenants Manage", Code = "tenants.manage", Description = "Manage tenants" },
            new() { Id = Guid.NewGuid(), Name = "Permissions Read", Code = "permissions.read", Description = "View permissions" },
            new() { Id = Guid.NewGuid(), Name = "Users Manage", Code = "users.manage", Description = "Manage users" },
            new() { Id = Guid.NewGuid(), Name = "Sanction Lists Manage", Code = "sanctionlists.manage", Description = "Manage sanction lists" },
            new() { Id = Guid.NewGuid(), Name = "Risk Matrices Manage", Code = "riskmatrices.manage", Description = "Manage risk matrices" },
            new() { Id = Guid.NewGuid(), Name = "Customers Create", Code = "customers.create", Description = "Create customer" },
            new() { Id = Guid.NewGuid(), Name = "KYC Upload", Code = "kyc.upload", Description = "Upload KYC" },
            new() { Id = Guid.NewGuid(), Name = "Onboarding Submit", Code = "onboarding.submit", Description = "Submit onboarding request" },
            new() { Id = Guid.NewGuid(), Name = "Onboarding Review", Code = "onboarding.review", Description = "Review onboarding" },
            new() { Id = Guid.NewGuid(), Name = "Onboarding Approve Reject", Code = "onboarding.approve_reject", Description = "Approve or reject onboarding" },
            new() { Id = Guid.NewGuid(), Name = "Onboarding Request Documents", Code = "onboarding.request_documents", Description = "Request more documents" },
            new() { Id = Guid.NewGuid(), Name = "Cases Investigate", Code = "cases.investigate", Description = "Investigate flagged cases" },
            new() { Id = Guid.NewGuid(), Name = "Cases Override", Code = "cases.override", Description = "Override decisions" },
            new() { Id = Guid.NewGuid(), Name = "Customers Approve High Risk", Code = "customers.approve_high_risk", Description = "Approve high-risk customers" },
            new() { Id = Guid.NewGuid(), Name = "Dashboard View", Code = "dashboard.view", Description = "View dashboards and risk reports" },
            new() { Id = Guid.NewGuid(), Name = "Reports Risk", Code = "reports.risk", Description = "View risk reports" }
        };
        context.Permissions.AddRange(permissions);

        var perm = (string code) => permissions.First(p => p.Code == code);

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

        foreach (var p in permissions.Take(6))
            context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id, AssignedAt = DateTime.UtcNow });
        foreach (var code in new[] { "users.manage", "sanctionlists.manage", "riskmatrices.manage" })
            context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = perm(code).Id, AssignedAt = DateTime.UtcNow });

        var makerRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = defaultTenant.Id,
            Name = "Maker",
            NormalizedName = "MAKER",
            Description = "Create customers and submit onboarding",
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
        context.Roles.Add(makerRole);
        foreach (var code in new[] { "customers.create", "kyc.upload", "onboarding.submit" })
            context.RolePermissions.Add(new RolePermission { RoleId = makerRole.Id, PermissionId = perm(code).Id, AssignedAt = DateTime.UtcNow });

        var checkerRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = defaultTenant.Id,
            Name = "Checker",
            NormalizedName = "CHECKER",
            Description = "Review and approve/reject onboarding",
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
        context.Roles.Add(checkerRole);
        foreach (var code in new[] { "onboarding.review", "onboarding.approve_reject", "onboarding.request_documents" })
            context.RolePermissions.Add(new RolePermission { RoleId = checkerRole.Id, PermissionId = perm(code).Id, AssignedAt = DateTime.UtcNow });

        var supervisorRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = defaultTenant.Id,
            Name = "Supervisor",
            NormalizedName = "SUPERVISOR",
            Description = "Investigate flagged cases and approve high-risk",
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
        context.Roles.Add(supervisorRole);
        foreach (var code in new[] { "cases.investigate", "cases.override", "customers.approve_high_risk" })
            context.RolePermissions.Add(new RolePermission { RoleId = supervisorRole.Id, PermissionId = perm(code).Id, AssignedAt = DateTime.UtcNow });

        var ceoRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = defaultTenant.Id,
            Name = "CEO",
            NormalizedName = "CEO",
            Description = "View dashboards and risk reports",
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
        context.Roles.Add(ceoRole);
        foreach (var code in new[] { "dashboard.view", "reports.risk" })
            context.RolePermissions.Add(new RolePermission { RoleId = ceoRole.Id, PermissionId = perm(code).Id, AssignedAt = DateTime.UtcNow });

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
