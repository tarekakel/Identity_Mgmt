using AmlScreening.Application.Interfaces;
using AmlScreening.Domain.Entities;
using AmlScreening.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AmlScreening.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerStatus> CustomerStatuses => Set<CustomerStatus>();
    public DbSet<CustomerType> CustomerTypes => Set<CustomerType>();
    public DbSet<Gender> Genders => Set<Gender>();
    public DbSet<Nationality> Nationalities => Set<Nationality>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Occupation> Occupations => Set<Occupation>();
    public DbSet<SourceOfFunds> SourceOfFunds => Set<SourceOfFunds>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<RiskAssignment> RiskAssignments => Set<RiskAssignment>();
    public DbSet<SanctionsScreening> SanctionsScreenings => Set<SanctionsScreening>();
    public DbSet<SanctionActionAuditLog> SanctionActionAuditLogs => Set<SanctionActionAuditLog>();
    public DbSet<SanctionListEntry> SanctionListEntries => Set<SanctionListEntry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter: exclude soft-deleted entities
        modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Case>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RiskAssignment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SanctionsScreening>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentUser = _currentUserService.GetCurrentUserDisplayName();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditable auditable)
                continue;

            if (entry.State == EntityState.Added)
            {
                auditable.CreatedAt = now;
                auditable.CreatedBy = currentUser;
                if (entry.Entity is ISoftDelete softDelete)
                {
                    softDelete.IsActive = true;
                    softDelete.IsDeleted = false;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                auditable.UpdatedAt = now;
                auditable.UpdatedBy = currentUser;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
