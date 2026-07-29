namespace MSECS.Telemetry.Application.DTOs;

public record TelemetryReadingDto(
    Guid Id, Guid OrganizationId, Guid SiteId, Guid AssetId, Guid DeviceId,
    string MetricType, double Value, string? Unit, DateTimeOffset RecordedAtUtc, string SourceProtocol);

public record IngestReadingItem(string MetricType, double Value, string? Unit, DateTimeOffset? RecordedAtUtc);
