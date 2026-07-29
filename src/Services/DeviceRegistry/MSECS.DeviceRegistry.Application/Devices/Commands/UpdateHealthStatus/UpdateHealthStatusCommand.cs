using MediatR;

namespace MSECS.DeviceRegistry.Application.Devices.Commands.UpdateHealthStatus;

/// <summary>Called by the Telemetry Service's ingestion pipeline on every accepted reading
/// (marks Online) and by a scheduled offline-detector job (marks Offline after a timeout).</summary>
public record UpdateHealthStatusCommand(Guid DeviceId, string HealthStatus) : IRequest;
