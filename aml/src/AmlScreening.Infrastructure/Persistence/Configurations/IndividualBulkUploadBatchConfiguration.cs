using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class IndividualBulkUploadBatchConfiguration : IEntityTypeConfiguration<IndividualBulkUploadBatch>
{
    public void Configure(EntityTypeBuilder<IndividualBulkUploadBatch> builder)
    {
        builder.ToTable("IndividualBulkUploadBatches");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.HasIndex(e => e.TenantId);
        builder.Property(e => e.OriginalFileName).HasMaxLength(512).IsRequired();
        builder.Property(e => e.MatchThreshold).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.HasMany(e => e.Lines)
            .WithOne(l => l.Batch)
            .HasForeignKey(l => l.BatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
