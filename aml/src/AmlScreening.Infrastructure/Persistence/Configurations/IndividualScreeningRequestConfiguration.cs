using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class IndividualScreeningRequestConfiguration : IEntityTypeConfiguration<IndividualScreeningRequest>
{
    public void Configure(EntityTypeBuilder<IndividualScreeningRequest> builder)
    {
        builder.ToTable("IndividualScreeningRequests");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.HasIndex(e => e.TenantId);

        builder.Property(e => e.CustomerId).IsRequired();
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => new { e.TenantId, e.CustomerId });

        builder.Property(e => e.ReferenceId).HasMaxLength(64);
        builder.Property(e => e.FullName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.IdType).HasMaxLength(64);
        builder.Property(e => e.IdNumber).HasMaxLength(128);
        builder.Property(e => e.Address).HasMaxLength(500);

        builder.Property(e => e.MatchThreshold).IsRequired();
        builder.Property(e => e.BirthYearRange);

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

        builder.HasOne(e => e.Gender)
            .WithMany()
            .HasForeignKey(e => e.GenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Nationality)
            .WithMany()
            .HasForeignKey(e => e.NationalityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PlaceOfBirthCountry)
            .WithMany()
            .HasForeignKey(e => e.PlaceOfBirthCountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

