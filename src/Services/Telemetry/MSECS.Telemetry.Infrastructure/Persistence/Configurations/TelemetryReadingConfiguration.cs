using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSECS.Telemetry.Domain.Entities;

namespace MSECS.Telemetry.Infrastructure.Persistence.Configurations;

public class TelemetryReadingConfiguration : IEntityTypeConfiguration<TelemetryReading>
{
    public void Configure(EntityTypeBuilder<TelemetryReading> builder)
    {
        builder.ToTable("readings");

        // Composite key including the partitioning column (RecordedAtUtc) — required by
        // TimescaleDB for any unique/primary key on a hypertable.
        builder.HasKey(r => new { r.Id, r.RecordedAtUtc });

        builder.Property(r => r.MetricType).HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Unit).HasMaxLength(20);
        builder.Property(r => r.SourceProtocol).HasMaxLength(20);

        builder.HasIndex(r => new { r.AssetId, r.MetricType, r.RecordedAtUtc });
        builder.HasIndex(r => new { r.DeviceId, r.RecordedAtUtc });
        builder.HasIndex(r => new { r.OrganizationId, r.SiteId, r.RecordedAtUtc });
    }
}
