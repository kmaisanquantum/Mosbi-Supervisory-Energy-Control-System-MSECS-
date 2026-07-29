using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSECS.Site.Domain.Entities;

namespace MSECS.Site.Infrastructure.Persistence.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<SolarSite>
{
    public void Configure(EntityTypeBuilder<SolarSite> builder)
    {
        builder.ToTable("sites");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.WeatherZone).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Timezone).HasMaxLength(60).IsRequired();
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.InstalledCapacityKw).HasColumnType("numeric(12,3)");

        builder.OwnsOne(s => s.Coordinates, cb =>
        {
            cb.Property(c => c.Latitude).HasColumnName("latitude").HasColumnType("double precision");
            cb.Property(c => c.Longitude).HasColumnName("longitude").HasColumnType("double precision");
        });

        builder.HasIndex(s => s.OrganizationId);
        builder.HasIndex(s => new { s.OrganizationId, s.Name });

        builder.Ignore(s => s.DomainEvents);
    }
}
