using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class RiskAssignmentConfiguration : IEntityTypeConfiguration<RiskAssignment>
{
    public void Configure(EntityTypeBuilder<RiskAssignment> builder)
    {
        builder.ToTable("RiskAssignments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RiskLevel).HasMaxLength(20).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.HasIndex(e => e.CustomerId).IsUnique();
    }
}
