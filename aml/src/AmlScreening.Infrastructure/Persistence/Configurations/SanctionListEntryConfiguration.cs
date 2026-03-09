using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class SanctionListEntryConfiguration : IEntityTypeConfiguration<SanctionListEntry>
{
    public void Configure(EntityTypeBuilder<SanctionListEntry> builder)
    {
        builder.ToTable("SanctionListEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ListSource).HasMaxLength(64).IsRequired();
        builder.Property(e => e.FullName).HasMaxLength(512).IsRequired();
        builder.Property(e => e.Nationality).HasMaxLength(128);
        builder.Property(e => e.ReferenceNumber).HasMaxLength(128);
        builder.Property(e => e.EntryType).HasMaxLength(32);

        builder.Property(e => e.DataId).HasMaxLength(64);
        builder.Property(e => e.VersionNum).HasMaxLength(16);
        builder.Property(e => e.FirstName).HasMaxLength(256);
        builder.Property(e => e.SecondName).HasMaxLength(256);
        builder.Property(e => e.UnListType).HasMaxLength(64);
        builder.Property(e => e.ListType).HasMaxLength(64);
        builder.Property(e => e.Gender).HasMaxLength(32);
        builder.Property(e => e.Designation).HasMaxLength(256);
        builder.Property(e => e.Comments).HasMaxLength(2000);
        builder.Property(e => e.Aliases).HasMaxLength(1000);
        builder.Property(e => e.AddressCity).HasMaxLength(128);
        builder.Property(e => e.AddressCountry).HasMaxLength(128);
        builder.Property(e => e.AddressNote).HasMaxLength(512);
        builder.Property(e => e.PlaceOfBirthCountry).HasMaxLength(128);
        builder.Property(e => e.SortKey).HasMaxLength(64);

        builder.Property(e => e.FullNameArabic).HasMaxLength(512);
        builder.Property(e => e.FamilyNameArabic).HasMaxLength(256);
        builder.Property(e => e.FamilyNameLatin).HasMaxLength(256);
        builder.Property(e => e.DocumentNumber).HasMaxLength(128);
        builder.Property(e => e.IssuingAuthority).HasMaxLength(256);
        builder.Property(e => e.OtherInformation).HasMaxLength(2000);
        builder.Property(e => e.TypeDetail).HasMaxLength(64);

        builder.HasIndex(e => e.ListSource);
    }
}
