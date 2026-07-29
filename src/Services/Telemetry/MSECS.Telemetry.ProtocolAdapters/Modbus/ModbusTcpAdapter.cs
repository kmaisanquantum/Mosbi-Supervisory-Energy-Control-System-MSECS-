using Microsoft.Extensions.Logging;
using MSECS.SharedKernel.Exceptions;
using MSECS.Telemetry.ProtocolAdapters.Abstractions;
using NModbus;

namespace MSECS.Telemetry.ProtocolAdapters.Modbus;

/// <summary>
/// Polls a solar inverter/meter over Modbus TCP using a fixed, documented register map.
/// The register map below follows the common SunSpec-style layout used by many inverter
/// vendors (scaled 16-bit holding registers); a real deployment would load per-model
/// maps from configuration, but a single well-documented default map is enough for the
/// Phase 1 prototype and keeps the adapter's contract obvious.
/// </summary>
public class ModbusTcpAdapter : IProtocolAdapter
{
    public string ProtocolName => "ModbusTcp";

    private readonly ILogger<ModbusTcpAdapter> _logger;
    private readonly IModbusFactory _modbusFactory;

    // (register address, metric type, scale factor, unit) — holding registers, function code 3.
    private static readonly (ushort Address, string MetricType, double Scale, string Unit)[] RegisterMap =
    {
        (30001, "VoltageV",        0.1,  "V"),
        (30002, "CurrentA",        0.01, "A"),
        (30003, "PowerKw",         0.001,"kW"),
        (30004, "FrequencyHz",     0.01, "Hz"),
        (30005, "EnergyKwh",       1.0,  "kWh"),
        (30006, "TemperatureC",    0.1,  "C"),
        (30007, "InverterStatusCode", 1.0, null!)
    };

    public ModbusTcpAdapter(ILogger<ModbusTcpAdapter> logger, IModbusFactory modbusFactory)
    {
        _logger = logger;
        _modbusFactory = modbusFactory;
    }

    public async Task<IReadOnlyList<TelemetrySample>> PollAsync(DeviceConnectionInfo device, CancellationToken cancellationToken = default)
    {
        if (device.IpAddress is null || device.Port is null || device.ModbusUnitId is null)
            throw new DeviceCommunicationException($"Device {device.DeviceId} is missing Modbus TCP connection details (IP/Port/UnitId).");

        using var client = new System.Net.Sockets.TcpClient();

        try
        {
            await client.ConnectAsync(device.IpAddress, device.Port.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DeviceCommunicationException($"Failed to connect to device {device.DeviceId} at {device.IpAddress}:{device.Port}.", ex);
        }

        var master = _modbusFactory.CreateMaster(client);
        var readings = new List<TelemetrySample>();
        var recordedAt = DateTimeOffset.UtcNow;

        foreach (var (address, metricType, scale, unit) in RegisterMap)
        {
            try
            {
                var registers = await master.ReadHoldingRegistersAsync((byte)device.ModbusUnitId.Value, address, 1);
                var rawValue = registers[0];
                readings.Add(new TelemetrySample(metricType, rawValue * scale, recordedAt, unit));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read Modbus register {Address} ({MetricType}) from device {DeviceId}",
                    address, metricType, device.DeviceId);
            }
        }

        return readings;
    }

    public async Task<bool> TestConnectionAsync(DeviceConnectionInfo device, CancellationToken cancellationToken = default)
    {
        if (device.IpAddress is null || device.Port is null) return false;

        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(device.IpAddress, device.Port.Value, cancellationToken);
            return client.Connected;
        }
        catch
        {
            return false;
        }
    }
}
