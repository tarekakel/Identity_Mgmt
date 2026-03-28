using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class CorporateScreeningRequestConfiguration : IEntityTypeConfiguration<CorporateScreeningRequest>
{
    public void Configure(EntityTypeBuilder<CorporateScreeningRequest> builder)
    {
        builder.ToTable("CorporateScreeningRequests");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.HasIndex(e => e.TenantId);
        builder.Property(e => e.CustomerId).IsRequired();
        builder.HasIndex(e => e.CustomerId);

        builder.Property(e => e.CompanyCode).HasMaxLength(64);
        builder.Property(e => e.FullName).HasMaxLength(512).IsRequired();
        builder.Property(e => e.TradeLicenceNo).HasMaxLength(128);
        builder.Property(e => e.Address).HasMaxLength(2000);
        builder.Property(e => e.MatchThreshold).IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Country)
            .WithMany()
            .HasForeignKey(e => e.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.CompanyDocuments)
            .WithOne(d => d.CorporateScreeningRequest)
            .HasForeignKey(d => d.CorporateScreeningRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Shareholders)
            .WithOne(s => s.CorporateScreeningRequest)
            .HasForeignKey(s => s.CorporateScreeningRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
