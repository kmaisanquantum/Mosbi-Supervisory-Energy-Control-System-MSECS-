using MediatR;
using MSECS.Telemetry.Application.DTOs;

namespace MSECS.Telemetry.Application.Telemetry.Commands.IngestReading;

/// <summary>
/// Accepts one or more readings for a single device in one call (a Modbus poll cycle or
/// an MQTT payload typically yields several metrics at once). OrganizationId/SiteId/AssetId
/// are supplied by the caller (the device's provisioning record, cached at the edge or by
/// the gateway) rather than looked up synchronously from the Device Registry, keeping the
/// ingestion hot path free of cross-service calls.
/// </summary>
public record IngestReadingCommand(
    Guid OrganizationId,
    Guid SiteId,
    Guid AssetId,
    Guid DeviceId,
    string SourceProtocol,
    IReadOnlyList<IngestReadingItem> Readings) : IRequest<IReadOnlyList<TelemetryReadingDto>>;
