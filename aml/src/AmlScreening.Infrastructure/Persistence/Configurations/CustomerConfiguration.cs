using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.HasIndex(e => e.TenantId);
        builder.Property(e => e.CustomerNumber).HasMaxLength(32).IsRequired();
        builder.HasIndex(e => e.CustomerNumber).IsUnique();
        builder.Property(e => e.FirstName).HasMaxLength(128);
        builder.Property(e => e.LastName).HasMaxLength(128);
        builder.Property(e => e.FullName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.GenderId);
        builder.Property(e => e.NationalityId);
        builder.Property(e => e.PassportNumber).HasMaxLength(64);
        builder.Property(e => e.NationalId).HasMaxLength(64);
        builder.Property(e => e.CountryOfResidenceId);
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.Phone).HasMaxLength(64);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.Country).HasMaxLength(100);
        builder.Property(e => e.OccupationId);
        builder.Property(e => e.EmployerName).HasMaxLength(256);
        builder.Property(e => e.SourceOfFundsId);
        builder.Property(e => e.AnnualIncome).HasPrecision(18, 2);
        builder.Property(e => e.ExpectedMonthlyTransactionVolume).HasPrecision(18, 2);
        builder.Property(e => e.ExpectedMonthlyTransactionValue).HasPrecision(18, 2);
        builder.Property(e => e.AccountPurpose).HasMaxLength(256);
        builder.Property(e => e.RiskScore).HasPrecision(18, 2);
        builder.Property(e => e.RiskLevel).HasMaxLength(32);
        builder.Property(e => e.BusinessActivity).HasMaxLength(256);
        builder.Property(e => e.NationalIdOrPassport).HasMaxLength(100);
        builder.Property(e => e.RiskClassification).HasMaxLength(20);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasOne(e => e.Status)
            .WithMany()
            .HasForeignKey(e => e.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CustomerType)
            .WithMany()
            .HasForeignKey(e => e.CustomerTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Gender)
            .WithMany()
            .HasForeignKey(e => e.GenderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Nationality)
            .WithMany()
            .HasForeignKey(e => e.NationalityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CountryOfResidence)
            .WithMany()
            .HasForeignKey(e => e.CountryOfResidenceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Occupation)
            .WithMany()
            .HasForeignKey(e => e.OccupationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.SourceOfFunds)
            .WithMany()
            .HasForeignKey(e => e.SourceOfFundsId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Cases)
            .WithOne(c => c.Customer)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(e => e.RiskAssignments)
            .WithOne(r => r.Customer)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.SanctionsScreenings)
            .WithOne(s => s.Customer)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
