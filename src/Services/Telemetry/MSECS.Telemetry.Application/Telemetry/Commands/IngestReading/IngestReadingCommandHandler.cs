using MediatR;
using MSECS.Telemetry.Application.Common.Interfaces;
using MSECS.Telemetry.Application.DTOs;
using MSECS.Telemetry.Domain.Entities;
using MSECS.Telemetry.Domain.Enums;
using MSECS.Telemetry.Domain.Events;

namespace MSECS.Telemetry.Application.Telemetry.Commands.IngestReading;

/// <summary>
/// Persists incoming readings to TimescaleDB and publishes one integration event per
/// reading onto RabbitMQ (routing key "telemetry.reading.ingested") so the Alarm Service
/// can evaluate thresholds and the Analytics Service can update rollups, without either
/// of them needing to poll the Telemetry Service directly.
/// </summary>
public class IngestReadingCommandHandler : IRequestHandler<IngestReadingCommand, IReadOnlyList<TelemetryReadingDto>>
{
    private readonly ITelemetryDbContext _db;
    private readonly IEventPublisher _eventPublisher;

    public IngestReadingCommandHandler(ITelemetryDbContext db, IEventPublisher eventPublisher)
    {
        _db = db;
        _eventPublisher = eventPublisher;
    }

    public async Task<IReadOnlyList<TelemetryReadingDto>> Handle(IngestReadingCommand request, CancellationToken cancellationToken)
    {
        var persisted = new List<TelemetryReading>();

        foreach (var item in request.Readings)
        {
            var metricType = Enum.Parse<TelemetryMetricType>(item.MetricType, true);
            var reading = new TelemetryReading(
                request.OrganizationId, request.SiteId, request.AssetId, request.DeviceId,
                metricType, item.Value, item.RecordedAtUtc ?? DateTimeOffset.UtcNow, request.SourceProtocol, item.Unit);

            await _db.Readings.AddAsync(reading, cancellationToken);
            persisted.Add(reading);
        }

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var reading in persisted)
        {
            var evt = new TelemetryReadingIngestedEvent(
                reading.Id, reading.DeviceId, reading.AssetId, reading.SiteId,
                reading.MetricType.ToString(), reading.Value, reading.RecordedAtUtc);

            _eventPublisher.Publish("telemetry.reading.ingested", evt, nameof(TelemetryReadingIngestedEvent));
        }

        return persisted.Select(Map).ToList();
    }

    public static TelemetryReadingDto Map(TelemetryReading r) => new(
        r.Id, r.OrganizationId, r.SiteId, r.AssetId, r.DeviceId,
        r.MetricType.ToString(), r.Value, r.Unit, r.RecordedAtUtc, r.SourceProtocol);
}
