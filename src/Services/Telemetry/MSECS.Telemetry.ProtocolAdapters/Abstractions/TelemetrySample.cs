namespace MSECS.Telemetry.ProtocolAdapters.Abstractions;

/// <summary>
/// Protocol-agnostic reading produced by every adapter. The Telemetry Service's
/// ingestion pipeline only ever deals with this shape, never with Modbus registers,
/// MQTT payloads, or REST bodies directly — that translation is the adapter's job.
/// </summary>
public record TelemetrySample(
    string MetricType,
    double Value,
    DateTimeOffset RecordedAtUtc,
    string? Unit = null);

public record DeviceConnectionInfo(
    Guid DeviceId,
    string? IpAddress = null,
    int? Port = null,
    int? ModbusUnitId = null,
    string? MqttTopic = null,
    IReadOnlyDictionary<string, string>? Metadata = null);
