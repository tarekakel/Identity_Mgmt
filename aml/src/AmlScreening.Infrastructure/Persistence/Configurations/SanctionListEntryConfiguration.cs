using System.Text.Json;
using AmlScreening.Domain.Entities;
using AmlScreening.Domain.Entities.SanctionList;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class SanctionListEntryConfiguration : IEntityTypeConfiguration<SanctionListEntry>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

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

        // ----- Multi-valued data persisted as JSON columns -----

        builder.OwnsMany(e => e.AliasItems, ob =>
        {
            ob.ToJson("AliasesJson");
            ob.Property(a => a.Name).HasMaxLength(512);
            ob.Property(a => a.Quality).HasMaxLength(32);
        });

        builder.OwnsMany(e => e.DatesOfBirth, ob =>
        {
            ob.ToJson("DatesOfBirthJson");
            ob.Property(d => d.TypeOfDate).HasMaxLength(32);
            ob.Property(d => d.Note).HasMaxLength(256);
        });

        builder.OwnsMany(e => e.Addresses, ob =>
        {
            ob.ToJson("AddressesJson");
            ob.Property(a => a.Street).HasMaxLength(256);
            ob.Property(a => a.City).HasMaxLength(128);
            ob.Property(a => a.StateProvince).HasMaxLength(128);
            ob.Property(a => a.Country).HasMaxLength(128);
            ob.Property(a => a.Note).HasMaxLength(512);
        });

        builder.OwnsMany(e => e.PlacesOfBirth, ob =>
        {
            ob.ToJson("PlacesOfBirthJson");
            ob.Property(p => p.City).HasMaxLength(128);
            ob.Property(p => p.StateProvince).HasMaxLength(128);
            ob.Property(p => p.Country).HasMaxLength(128);
        });

        builder.OwnsMany(e => e.Documents, ob =>
        {
            ob.ToJson("DocumentsJson");
            ob.Property(d => d.Type).HasMaxLength(128);
            ob.Property(d => d.Type2).HasMaxLength(128);
            ob.Property(d => d.Number).HasMaxLength(128);
            ob.Property(d => d.IssuingCountry).HasMaxLength(128);
            ob.Property(d => d.Note).HasMaxLength(512);
        });

        var stringListComparer = new ValueComparer<List<string>>(
            (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
            v => v == null ? 0 : v.Aggregate(0, (h, s) => HashCode.Combine(h, s == null ? 0 : s.GetHashCode())),
            v => v == null ? new List<string>() : v.ToList());

        var dateListComparer = new ValueComparer<List<DateTime>>(
            (a, b) => (a ?? new List<DateTime>()).SequenceEqual(b ?? new List<DateTime>()),
            v => v == null ? 0 : v.Aggregate(0, (h, d) => HashCode.Combine(h, d.GetHashCode())),
            v => v == null ? new List<DateTime>() : v.ToList());

        builder.Property(e => e.Nationalities)
            .HasColumnName("NationalitiesJson")
            .HasConversion(
                v => JsonSerializer.Serialize(v ?? new List<string>(), JsonOptions),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(stringListComparer);

        builder.Property(e => e.Designations)
            .HasColumnName("DesignationsJson")
            .HasConversion(
                v => JsonSerializer.Serialize(v ?? new List<string>(), JsonOptions),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(stringListComparer);

        builder.Property(e => e.LastDayUpdates)
            .HasColumnName("LastDayUpdatesJson")
            .HasConversion(
                v => JsonSerializer.Serialize(v ?? new List<DateTime>(), JsonOptions),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<DateTime>()
                    : JsonSerializer.Deserialize<List<DateTime>>(v, JsonOptions) ?? new List<DateTime>())
            .Metadata.SetValueComparer(dateListComparer);
    }
}
