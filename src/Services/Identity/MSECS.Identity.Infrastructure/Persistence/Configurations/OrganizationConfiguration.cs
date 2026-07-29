using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSECS.Identity.Domain.Entities;

namespace MSECS.Identity.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(o => o.ContactEmail).HasMaxLength(256);

        builder.HasIndex(o => o.Name);
        builder.HasIndex(o => o.ParentOrganizationId);

        builder.HasMany(o => o.Users)
            .WithOne()
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(o => o.DomainEvents);
    }
}
