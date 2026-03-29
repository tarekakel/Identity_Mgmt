using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class CorporateKycDocumentConfiguration : IEntityTypeConfiguration<CorporateKycDocument>
{
    public void Configure(EntityTypeBuilder<CorporateKycDocument> builder)
    {
        builder.ToTable("CorporateKycDocuments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CorporateKycId).IsRequired();
        builder.Property(e => e.CustomerId).IsRequired();

        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.Property(e => e.DocumentNo).HasMaxLength(128);
        builder.Property(e => e.ApprovedBy).HasMaxLength(256);
        builder.Property(e => e.FolderPath).HasMaxLength(512);
        builder.Property(e => e.FileName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.FilePath).HasMaxLength(512).IsRequired();
        builder.Property(e => e.UploadedBy).HasMaxLength(256);

        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.CorporateKycId);

        builder.HasOne(e => e.CorporateKyc)
            .WithMany()
            .HasForeignKey(e => e.CorporateKycId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
