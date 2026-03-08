using IdentityManagement.Application.Interfaces;
using IdentityManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityManagement.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly Guid? _tenantId;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        _tenantId = null;
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenant currentTenant)
        : base(options)
    {
        _tenantId = currentTenant.TenantId;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter: when _tenantId is null (e.g. design-time/migrations), include all; otherwise filter by tenant.
        modelBuilder.Entity<User>().HasQueryFilter(e => _tenantId == null || e.TenantId == _tenantId);
        modelBuilder.Entity<Role>().HasQueryFilter(e => _tenantId == null || e.TenantId == _tenantId);
        // Matching filters on dependents so EF does not warn about required principal being filtered out.
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(rt => _tenantId == null || rt.User.TenantId == _tenantId);
        modelBuilder.Entity<RolePermission>().HasQueryFilter(rp => _tenantId == null || rp.Role.TenantId == _tenantId);
        modelBuilder.Entity<UserRole>().HasQueryFilter(ur => _tenantId == null || ur.Role.TenantId == _tenantId);
    }
}
