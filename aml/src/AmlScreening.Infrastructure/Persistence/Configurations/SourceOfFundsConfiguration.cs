using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class SourceOfFundsConfiguration : IEntityTypeConfiguration<SourceOfFunds>
{
    public void Configure(EntityTypeBuilder<SourceOfFunds> builder)
    {
        builder.ToTable("SourceOfFunds");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(128).IsRequired();
        builder.HasIndex(e => e.Code).IsUnique();
    }
}
