using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MSECS.Asset.Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Domain.Entities.Asset>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Asset> builder)
    {
        builder.ToTable("assets");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Manufacturer).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Model).HasMaxLength(150).IsRequired();
        builder.Property(a => a.SerialNumber).HasMaxLength(150).IsRequired();
        builder.Property(a => a.RatedCapacityKw).HasColumnType("numeric(12,3)");
        builder.Property(a => a.FirmwareVersion).HasMaxLength(50);

        builder.HasIndex(a => a.SerialNumber).IsUnique();
        builder.HasIndex(a => a.SiteId);
        builder.HasIndex(a => a.OrganizationId);
        builder.HasIndex(a => a.ParentAssetId);
        builder.HasIndex(a => a.DeviceId);

        builder.OwnsMany(a => a.MaintenanceHistory, mb =>
        {
            mb.ToTable("maintenance_records");
            mb.WithOwner().HasForeignKey("AssetId");
            mb.HasKey(m => m.Id);
            mb.Property(m => m.Type).HasConversion<string>().HasMaxLength(30);
            mb.Property(m => m.Description).HasMaxLength(2000);
            mb.Property(m => m.PerformedBy).HasMaxLength(200);
        });

        builder.Navigation(a => a.MaintenanceHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(a => a.DomainEvents);
    }
}
