using MSECS.Telemetry.Domain.Enums;

namespace MSECS.Telemetry.Domain.Entities;

/// <summary>
/// A single time-series data point. Deliberately NOT an AggregateRoot/AuditableEntity —
/// telemetry is high-volume, append-only, and stored in a TimescaleDB hypertable keyed
/// by (device_id, metric_type, recorded_at_utc), not the SharedKernel's Guid-PK pattern.
/// Domain events for threshold breaches are raised by the ingestion handler, not the
/// entity itself, since evaluating thresholds needs asset-level configuration context.
/// </summary>
public class TelemetryReading
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid SiteId { get; private set; }
    public Guid AssetId { get; private set; }
    public Guid DeviceId { get; private set; }
    public TelemetryMetricType MetricType { get; private set; }
    public double Value { get; private set; }
    public string? Unit { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }
    public DateTimeOffset IngestedAtUtc { get; private set; }
    public string SourceProtocol { get; private set; } = string.Empty;

    private TelemetryReading() { }

    public TelemetryReading(
        Guid organizationId, Guid siteId, Guid assetId, Guid deviceId,
        TelemetryMetricType metricType, double value, DateTimeOffset recordedAtUtc, string sourceProtocol, string? unit = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        SiteId = siteId;
        AssetId = assetId;
        DeviceId = deviceId;
        MetricType = metricType;
        Value = value;
        Unit = unit;
        RecordedAtUtc = recordedAtUtc;
        IngestedAtUtc = DateTimeOffset.UtcNow;
        SourceProtocol = sourceProtocol;
    }
}
