using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class CorporateScreeningShareholderConfiguration : IEntityTypeConfiguration<CorporateScreeningShareholder>
{
    public void Configure(EntityTypeBuilder<CorporateScreeningShareholder> builder)
    {
        builder.ToTable("CorporateScreeningShareholders");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FullName).HasMaxLength(512).IsRequired();
        builder.Property(e => e.SharePercent).HasPrecision(18, 4);

        builder.HasOne(e => e.Nationality)
            .WithMany()
            .HasForeignKey(e => e.NationalityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Documents)
            .WithOne(d => d.Shareholder)
            .HasForeignKey(d => d.CorporateScreeningShareholderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
