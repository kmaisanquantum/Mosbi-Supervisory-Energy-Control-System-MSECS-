using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSECS.Identity.Domain.Entities;

namespace MSECS.Identity.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.OrganizationId);

        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();

        builder.OwnsMany(u => u.Roles, rb =>
        {
            rb.ToTable("user_roles");
            rb.WithOwner().HasForeignKey("UserId");
            rb.HasKey("UserId", "RoleId");
        });

        builder.OwnsMany(u => u.RefreshTokens, rt =>
        {
            rt.ToTable("refresh_tokens");
            rt.WithOwner().HasForeignKey("UserId");
            rt.HasKey(t => t.Id);
            rt.Property(t => t.Token).HasMaxLength(512).IsRequired();
            rt.HasIndex(t => t.Token).IsUnique();
        });

        builder.Navigation(u => u.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.RefreshTokens).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(u => u.DomainEvents);
    }
}
