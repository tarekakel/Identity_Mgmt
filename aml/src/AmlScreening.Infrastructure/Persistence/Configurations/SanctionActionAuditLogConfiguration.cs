using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class SanctionActionAuditLogConfiguration : IEntityTypeConfiguration<SanctionActionAuditLog>
{
    public void Configure(EntityTypeBuilder<SanctionActionAuditLog> builder)
    {
        builder.ToTable("SanctionActionAuditLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Action).HasMaxLength(32).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.HasIndex(e => e.SanctionsScreeningId);
        builder.HasOne(e => e.SanctionsScreening)
            .WithMany()
            .HasForeignKey(e => e.SanctionsScreeningId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
