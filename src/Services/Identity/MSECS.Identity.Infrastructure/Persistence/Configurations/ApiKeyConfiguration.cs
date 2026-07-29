using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSECS.Identity.Domain.Entities;

namespace MSECS.Identity.Infrastructure.Persistence.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");
        builder.HasKey(k => k.Id);

        builder.Property(k => k.Name).HasMaxLength(150).IsRequired();
        builder.Property(k => k.KeyHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(k => k.KeyHash).IsUnique();
        builder.HasIndex(k => k.OrganizationId);

        builder.Property(k => k.Scopes)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyCollection<string>>(
                (a, b) => a!.SequenceEqual(b!),
                a => a.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                a => a.ToList()));

        builder.Ignore(k => k.DomainEvents);
    }
}
