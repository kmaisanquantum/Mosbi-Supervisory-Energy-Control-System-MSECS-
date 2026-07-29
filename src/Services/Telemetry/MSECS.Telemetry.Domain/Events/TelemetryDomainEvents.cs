using MSECS.SharedKernel.Common;

namespace MSECS.Telemetry.Domain.Events;

public record TelemetryReadingIngestedEvent(
    Guid ReadingId, Guid DeviceId, Guid AssetId, Guid SiteId, string MetricType, double Value, DateTimeOffset RecordedAtUtc)
    : DomainEvent;

public record TelemetryThresholdBreachedEvent(
    Guid DeviceId, Guid AssetId, string MetricType, double Value, double ThresholdValue, DateTimeOffset RecordedAtUtc)
    : DomainEvent;
