using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class IndividualKycDocumentConfiguration : IEntityTypeConfiguration<IndividualKycDocument>
{
    public void Configure(EntityTypeBuilder<IndividualKycDocument> builder)
    {
        builder.ToTable("IndividualKycDocuments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.IndividualKycId).IsRequired();
        builder.Property(e => e.CustomerId).IsRequired();

        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.IndividualKycId);

        builder.Property(e => e.DocumentNo).HasMaxLength(128);
        builder.Property(e => e.ApprovedBy).HasMaxLength(256);
        builder.Property(e => e.FolderPath).HasMaxLength(512);
        builder.Property(e => e.FileName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.FilePath).HasMaxLength(512).IsRequired();
        builder.Property(e => e.UploadedBy).HasMaxLength(256);

        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);

        builder.HasOne(e => e.IndividualKyc)
            .WithMany()
            .HasForeignKey(e => e.IndividualKycId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

