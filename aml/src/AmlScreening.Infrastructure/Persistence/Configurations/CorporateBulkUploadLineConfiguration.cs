using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class CorporateBulkUploadLineConfiguration : IEntityTypeConfiguration<CorporateBulkUploadLine>
{
    public void Configure(EntityTypeBuilder<CorporateBulkUploadLine> builder)
    {
        builder.ToTable("CorporateBulkUploadLines");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.BatchId);
        builder.Property(e => e.LineIndex).IsRequired();
        builder.Property(e => e.CustomerId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.FullName).HasMaxLength(512).IsRequired();
        builder.Property(e => e.IncorporatedCountry).HasMaxLength(128);
        builder.Property(e => e.DateOfIncorporationRaw).HasMaxLength(64);
        builder.Property(e => e.CompanyReferenceCode).HasMaxLength(128);
        builder.Property(e => e.TradeLicense).HasMaxLength(256);
        builder.Property(e => e.IncorporatedCountryResolvedCode).HasMaxLength(8);
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
    }
}
