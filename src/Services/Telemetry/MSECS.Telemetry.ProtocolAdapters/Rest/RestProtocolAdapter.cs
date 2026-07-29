using MSECS.Telemetry.ProtocolAdapters.Abstractions;

namespace MSECS.Telemetry.ProtocolAdapters.Rest;

/// <summary>
/// REST is push-only from the device's perspective (it POSTs directly to
/// /api/v1/telemetry/ingest using its API key) — this adapter exists so the
/// abstraction is uniform across all three Phase 1 protocols and so the ingestion
/// command can normalize a REST body into the same TelemetrySample shape Modbus
/// and MQTT produce, rather than special-casing REST elsewhere in the pipeline.
/// </summary>
public class RestProtocolAdapter : IProtocolAdapter
{
    public string ProtocolName => "Rest";

    public Task<IReadOnlyList<TelemetrySample>> PollAsync(DeviceConnectionInfo device, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TelemetrySample>>(Array.Empty<TelemetrySample>());

    public Task<bool> TestConnectionAsync(DeviceConnectionInfo device, CancellationToken cancellationToken = default) =>
        Task.FromResult(true); // Connectivity is proven by the device successfully authenticating against the ingest endpoint.

    /// <summary>Normalizes a REST ingestion request body into TelemetrySamples.</summary>
    public static IReadOnlyList<TelemetrySample> NormalizeBody(IEnumerable<(string Metric, double Value, string? Unit, DateTimeOffset? Timestamp)> readings) =>
        readings.Select(r => new TelemetrySample(r.Metric, r.Value, r.Timestamp ?? DateTimeOffset.UtcNow, r.Unit)).ToList();
}
