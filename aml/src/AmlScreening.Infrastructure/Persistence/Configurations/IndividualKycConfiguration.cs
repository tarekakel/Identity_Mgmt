using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class IndividualKycConfiguration : IEntityTypeConfiguration<IndividualKyc>
{
    public void Configure(EntityTypeBuilder<IndividualKyc> builder)
    {
        builder.ToTable("IndividualKyc");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.CustomerId).IsRequired();

        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => new { e.CustomerId, e.IsActive });
        builder.HasIndex(e => e.TenantId);

        builder.Property(e => e.ApplicantName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.ApplicantAliases).HasMaxLength(256);
        builder.Property(e => e.ApplicantMobileNo).HasMaxLength(64);
        builder.Property(e => e.ApplicantResidenceStatus).HasMaxLength(128);
        builder.Property(e => e.ApplicantEmirate).HasMaxLength(128);
        builder.Property(e => e.ApplicantCountryOfBirth).HasMaxLength(128);
        builder.Property(e => e.ApplicantCity).HasMaxLength(128);
        builder.Property(e => e.ApplicantEmail).HasMaxLength(256);
        builder.Property(e => e.ApplicantResidentialAddress).HasMaxLength(1000);
        builder.Property(e => e.ApplicantOfficeNoBuildingNameStreetArea).HasMaxLength(500);
        builder.Property(e => e.ApplicantPOBox).HasMaxLength(64);
        builder.Property(e => e.ApplicantCustomerRelationship).HasMaxLength(256);
        builder.Property(e => e.ApplicantPreferredChannel).HasMaxLength(256);
        builder.Property(e => e.ApplicantProductType).HasMaxLength(256);
        builder.Property(e => e.ApplicantIndustryType).HasMaxLength(256);

        builder.Property(e => e.ApplicantSourceOfFundsComments).HasMaxLength(1000);
        builder.Property(e => e.ApplicantSourceOfFundsProofComments).HasMaxLength(1000);

        builder.Property(e => e.ApplicantSourceOfWealth).HasMaxLength(256);
        builder.Property(e => e.ApplicantSourceOfWealthComments).HasMaxLength(1000);
        builder.Property(e => e.ApplicantSourceOfWealthProofComments).HasMaxLength(1000);

        builder.Property(e => e.ClientIdTypeCode).HasMaxLength(64);
        builder.Property(e => e.ClientIdNumber).HasMaxLength(128);
        builder.Property(e => e.ClientEmiratesIdNumber).HasMaxLength(128);
        builder.Property(e => e.ClientPassportNumber).HasMaxLength(128);
        builder.Property(e => e.SponsorName).HasMaxLength(256);
        builder.Property(e => e.SponsorAliases).HasMaxLength(256);
        builder.Property(e => e.SponsorIdTypeCode).HasMaxLength(64);
        builder.Property(e => e.SponsorIdNumber).HasMaxLength(128);
        builder.Property(e => e.SponsorOtherDetails).HasMaxLength(1000);

        builder.Property(e => e.BankIbanAccountNo).HasMaxLength(128);
        builder.Property(e => e.BankName).HasMaxLength(256);
        builder.Property(e => e.AccountName).HasMaxLength(256);
        builder.Property(e => e.BankSwiftCode).HasMaxLength(64);
        builder.Property(e => e.BankAddress).HasMaxLength(1000);
        builder.Property(e => e.BankCurrency).HasMaxLength(32);

        builder.Property(e => e.EmployerCompanyName).HasMaxLength(256);
        builder.Property(e => e.EmployerCompanyWebsite).HasMaxLength(512);
        builder.Property(e => e.EmployerEmailAddress).HasMaxLength(256);
        builder.Property(e => e.EmployerTelNo).HasMaxLength(64);
        builder.Property(e => e.EmployerAddress).HasMaxLength(1000);
        builder.Property(e => e.EmployerIndustryAndBusinessDetails).HasMaxLength(1000);

        builder.Property(e => e.PepFATFIncreasedMonitoringAnswer).HasMaxLength(64);
        builder.Property(e => e.PepSanctionListOrInverseMediaAnswer).HasMaxLength(64);
        builder.Property(e => e.PepProminentPublicFunctionsAnswer).HasMaxLength(64);
        builder.Property(e => e.PepAnyPEPsAfterScreeningAnswer).HasMaxLength(64);
        builder.Property(e => e.PepSpecificPEPsAfterScreeningDetails).HasMaxLength(1000);

        builder.Property(e => e.FollowUpRemarks).HasMaxLength(2000);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

