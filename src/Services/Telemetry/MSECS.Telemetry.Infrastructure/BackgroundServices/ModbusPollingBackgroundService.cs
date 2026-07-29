using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSECS.Telemetry.Application.Telemetry.Commands.IngestReading;
using MSECS.Telemetry.Application.DTOs;
using MSECS.Telemetry.ProtocolAdapters.Abstractions;

namespace MSECS.Telemetry.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically polls every registered Modbus TCP device (poll targets are cached in
/// Redis by the Device Registry's DeviceProvisionedEvent handler — omitted from this
/// pass for brevity, see docs/ROADMAP.md) and feeds results through the same
/// IngestReadingCommand used by the REST/MQTT ingestion paths, so all three protocols
/// converge on identical validation, persistence, and event-publishing logic.
/// </summary>
public class ModbusPollingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ModbusPollingBackgroundService> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    public ModbusPollingBackgroundService(IServiceProvider serviceProvider, ILogger<ModbusPollingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PollAllDevicesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Modbus polling cycle failed unexpectedly.");
            }
        }
    }

    private async Task PollAllDevicesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var adapterFactory = scope.ServiceProvider.GetRequiredService<ProtocolAdapterFactory>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var pollTargets = scope.ServiceProvider.GetRequiredService<IModbusPollTargetProvider>();

        var adapter = adapterFactory.Resolve("ModbusTcp");
        var targets = await pollTargets.GetActiveTargetsAsync(cancellationToken);

        foreach (var target in targets)
        {
            try
            {
                var samples = await adapter.PollAsync(target.Connection, cancellationToken);
                if (samples.Count == 0) continue;

                var items = samples.Select(s => new IngestReadingItem(s.MetricType, s.Value, s.Unit, s.RecordedAtUtc)).ToList();
                await mediator.Send(new IngestReadingCommand(
                    target.OrganizationId, target.SiteId, target.AssetId, target.Connection.DeviceId, "ModbusTcp", items), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to poll Modbus device {DeviceId}", target.Connection.DeviceId);
            }
        }
    }
}

/// <summary>
/// Supplies the set of currently-provisioned Modbus TCP devices to poll. The production
/// implementation subscribes to DeviceProvisioned/DeviceRevoked events from the Device
/// Registry and maintains a Redis-backed cache; a minimal in-memory stub is registered
/// by default so the service starts cleanly without that wiring in this pass.
/// </summary>
public interface IModbusPollTargetProvider
{
    Task<IReadOnlyList<ModbusPollTarget>> GetActiveTargetsAsync(CancellationToken cancellationToken = default);
}

public record ModbusPollTarget(Guid OrganizationId, Guid SiteId, Guid AssetId, DeviceConnectionInfo Connection);

public class InMemoryModbusPollTargetProvider : IModbusPollTargetProvider
{
    public Task<IReadOnlyList<ModbusPollTarget>> GetActiveTargetsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ModbusPollTarget>>(Array.Empty<ModbusPollTarget>());
}
