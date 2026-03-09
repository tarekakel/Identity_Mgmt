using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class CustomerDocumentConfiguration : IEntityTypeConfiguration<CustomerDocument>
{
    public void Configure(EntityTypeBuilder<CustomerDocument> builder)
    {
        builder.ToTable("CustomerDocuments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FileName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.FilePath).HasMaxLength(512).IsRequired();
        builder.Property(e => e.UploadedBy).HasMaxLength(256);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Documents)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.DocumentType)
            .WithMany()
            .HasForeignKey(e => e.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
