using AmlScreening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmlScreening.Infrastructure.Persistence.Configurations;

public class CorporateScreeningCompanyDocumentConfiguration : IEntityTypeConfiguration<CorporateScreeningCompanyDocument>
{
    public void Configure(EntityTypeBuilder<CorporateScreeningCompanyDocument> builder)
    {
        builder.ToTable("CorporateScreeningCompanyDocuments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.DocumentNo).HasMaxLength(128);
        builder.Property(e => e.Details).HasMaxLength(4000);
        builder.Property(e => e.Remarks).HasMaxLength(4000);
    }
}
