using IdentityManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.UserName).HasMaxLength(256).IsRequired();
        builder.Property(e => e.NormalizedUserName).HasMaxLength(256);
        builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
        builder.Property(e => e.NormalizedEmail).HasMaxLength(256);
        builder.Property(e => e.PasswordHash).HasMaxLength(500);
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.SecurityStamp).HasMaxLength(500);
        builder.Property(e => e.ConcurrencyStamp).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.CreatedBy).HasMaxLength(256);
        builder.Property(e => e.UpdatedBy).HasMaxLength(256);

        builder.HasIndex(e => new { e.TenantId, e.NormalizedEmail }).IsUnique();
        builder.HasIndex(e => new { e.TenantId, e.NormalizedUserName });

        builder.HasOne(e => e.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
