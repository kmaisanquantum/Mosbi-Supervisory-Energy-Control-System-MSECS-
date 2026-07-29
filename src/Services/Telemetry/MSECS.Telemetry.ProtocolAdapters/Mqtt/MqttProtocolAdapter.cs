using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MSECS.Telemetry.ProtocolAdapters.Abstractions;

namespace MSECS.Telemetry.ProtocolAdapters.Mqtt;

/// <summary>
/// Push-based adapter: subscribes to a device's telemetry topic and translates each
/// incoming JSON payload into TelemetrySamples. Expected payload shape (published by
/// edge gateways / smart meters):
/// { "readings": [ { "metric": "PowerKw", "value": 4.21, "unit": "kW", "ts": "2026-07-28T10:00:00Z" } ] }
/// </summary>
public class MqttProtocolAdapter : IPushProtocolAdapter, IAsyncDisposable
{
    public string ProtocolName => "Mqtt";

    private readonly ILogger<MqttProtocolAdapter> _logger;
    private readonly string _brokerHost;
    private readonly int _brokerPort;
    private IMqttClient? _client;

    public MqttProtocolAdapter(ILogger<MqttProtocolAdapter> logger, string brokerHost, int brokerPort = 1883)
    {
        _logger = logger;
        _brokerHost = brokerHost;
        _brokerPort = brokerPort;
    }

    /// <summary>MQTT is push-only; polling always returns empty. Use SubscribeAsync instead.</summary>
    public Task<IReadOnlyList<TelemetrySample>> PollAsync(DeviceConnectionInfo device, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TelemetrySample>>(Array.Empty<TelemetrySample>());

    public async Task SubscribeAsync(DeviceConnectionInfo device, Func<TelemetrySample, Task> onSampleReceived, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(device.MqttTopic))
            throw new InvalidOperationException($"Device {device.DeviceId} has no MQTT topic configured.");

        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_brokerHost, _brokerPort)
            .WithClientId($"msecs-telemetry-{device.DeviceId}")
            .WithCleanSession()
            .Build();

        _client.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                var samples = ParsePayload(payload);
                foreach (var sample in samples) await onSampleReceived(sample);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse MQTT payload from topic {Topic} for device {DeviceId}",
                    device.MqttTopic, device.DeviceId);
            }
        };

        await _client.ConnectAsync(options, cancellationToken);
        await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(device.MqttTopic).Build(), cancellationToken);

        _logger.LogInformation("Subscribed to MQTT topic {Topic} for device {DeviceId}", device.MqttTopic, device.DeviceId);
    }

    private static IReadOnlyList<TelemetrySample> ParsePayload(string json)
    {
        var doc = JsonDocument.Parse(json);
        var results = new List<TelemetrySample>();

        if (!doc.RootElement.TryGetProperty("readings", out var readings)) return results;

        foreach (var reading in readings.EnumerateArray())
        {
            var metric = reading.GetProperty("metric").GetString() ?? "Unknown";
            var value = reading.GetProperty("value").GetDouble();
            var unit = reading.TryGetProperty("unit", out var u) ? u.GetString() : null;
            var timestamp = reading.TryGetProperty("ts", out var ts) && ts.TryGetDateTimeOffset(out var dto)
                ? dto
                : DateTimeOffset.UtcNow;

            results.Add(new TelemetrySample(metric, value, timestamp, unit));
        }

        return results;
    }

    public async Task<bool> TestConnectionAsync(DeviceConnectionInfo device, CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new MqttFactory();
            using var probe = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder().WithTcpServer(_brokerHost, _brokerPort).Build();
            var result = await probe.ConnectAsync(options, cancellationToken);
            await probe.DisconnectAsync(cancellationToken: cancellationToken);
            return result.ResultCode == MqttClientConnectResultCode.Success;
        }
        catch
        {
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
        {
            await _client.DisconnectAsync();
            _client.Dispose();
        }
    }
}
