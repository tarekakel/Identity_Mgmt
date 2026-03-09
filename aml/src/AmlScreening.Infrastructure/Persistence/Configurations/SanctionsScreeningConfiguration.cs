using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class SanctionsScreeningConfiguration : IEntityTypeConfiguration<SanctionsScreening>
{
    public void Configure(EntityTypeBuilder<SanctionsScreening> builder)
    {
        builder.ToTable("SanctionsScreenings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ScreeningList).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Result).HasMaxLength(32).IsRequired();
        builder.Property(e => e.MatchedName).HasMaxLength(256);
        builder.Property(e => e.MatchType).HasMaxLength(32);
        builder.Property(e => e.Score).HasPrecision(18, 2);
        builder.Property(e => e.ScreenedAt).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.HasIndex(e => e.CustomerId);
    }
}
