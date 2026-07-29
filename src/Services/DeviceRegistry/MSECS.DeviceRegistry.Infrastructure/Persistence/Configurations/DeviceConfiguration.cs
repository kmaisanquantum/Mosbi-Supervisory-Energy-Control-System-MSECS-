using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSECS.DeviceRegistry.Domain.Entities;

namespace MSECS.DeviceRegistry.Infrastructure.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.SerialNumber).HasMaxLength(150).IsRequired();
        builder.Property(d => d.Protocol).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.ProvisioningStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.HealthStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.CredentialHash).HasMaxLength(128).IsRequired();
        builder.Property(d => d.IpAddress).HasMaxLength(45);
        builder.Property(d => d.MqttTopic).HasMaxLength(300);

        builder.Property(d => d.Metadata)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new())
            .HasColumnType("jsonb");

        builder.HasIndex(d => d.SerialNumber).IsUnique();
        builder.HasIndex(d => d.CredentialHash).IsUnique();
        builder.HasIndex(d => d.SiteId);
        builder.HasIndex(d => d.AssetId).IsUnique();
        builder.HasIndex(d => d.OrganizationId);

        builder.Ignore(d => d.DomainEvents);
    }
}
