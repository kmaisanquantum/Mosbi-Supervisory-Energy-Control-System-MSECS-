namespace MSECS.Telemetry.ProtocolAdapters.Abstractions;

/// <summary>
/// Interchangeable adapter contract for every industrial protocol MSECS speaks to a
/// device over. New protocols (OPC UA, IEC 61850, DNP3, CAN Bus) implement this same
/// interface and register themselves with the ProtocolAdapterFactory — no other part
/// of the Telemetry Service needs to change.
/// </summary>
public interface IProtocolAdapter
{
    string ProtocolName { get; }

    /// <summary>Actively polls the device and returns whatever readings it currently exposes.
    /// Used by ModbusTcp (poll-based). MQTT/REST are push-based and implement this as a no-op
    /// that returns an empty set; their data arrives via <see cref="IPushProtocolAdapter"/>.</summary>
    Task<IReadOnlyList<TelemetrySample>> PollAsync(DeviceConnectionInfo device, CancellationToken cancellationToken = default);

    Task<bool> TestConnectionAsync(DeviceConnectionInfo device, CancellationToken cancellationToken = default);
}

/// <summary>Implemented by push-based adapters (MQTT) that asynchronously deliver samples
/// as they arrive rather than being polled.</summary>
public interface IPushProtocolAdapter : IProtocolAdapter
{
    Task SubscribeAsync(DeviceConnectionInfo device, Func<TelemetrySample, Task> onSampleReceived, CancellationToken cancellationToken = default);
}
