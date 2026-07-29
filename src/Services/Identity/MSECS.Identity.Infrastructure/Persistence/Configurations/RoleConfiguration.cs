using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSECS.Identity.Domain.Entities;

namespace MSECS.Identity.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(r => new { r.OrganizationId, r.Name }).IsUnique();

        builder.OwnsMany(r => r.Permissions, pb =>
        {
            pb.ToTable("role_permissions");
            pb.WithOwner().HasForeignKey("RoleId");
            pb.HasKey("RoleId", "PermissionKey");
            pb.Property(p => p.PermissionKey).HasMaxLength(100);
        });

        builder.Navigation(r => r.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(r => r.DomainEvents);
    }
}
