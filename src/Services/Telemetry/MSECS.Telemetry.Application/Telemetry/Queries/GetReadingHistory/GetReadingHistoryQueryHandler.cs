using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Telemetry.Application.Common.Interfaces;
using MSECS.Telemetry.Application.DTOs;
using MSECS.Telemetry.Application.Telemetry.Commands.IngestReading;
using MSECS.Telemetry.Domain.Enums;

namespace MSECS.Telemetry.Application.Telemetry.Queries.GetReadingHistory;

/// <summary>
/// Powers charting: returns raw points for the requested window (TimescaleDB makes this
/// fast via the hypertable's time index). MaxPoints caps payload size; a future pass can
/// swap this for a continuous-aggregate-backed downsampled query once TimescaleDB
/// continuous aggregates are configured for common window sizes (hour/day/month).
/// </summary>
public class GetReadingHistoryQueryHandler : IRequestHandler<GetReadingHistoryQuery, IReadOnlyList<TelemetryReadingDto>>
{
    private readonly ITelemetryDbContext _db;
    public GetReadingHistoryQueryHandler(ITelemetryDbContext db) => _db = db;

    public async Task<IReadOnlyList<TelemetryReadingDto>> Handle(GetReadingHistoryQuery request, CancellationToken cancellationToken)
    {
        var metric = Enum.Parse<TelemetryMetricType>(request.MetricType, true);

        var readings = await _db.Readings.AsNoTracking()
            .Where(r => r.AssetId == request.AssetId && r.MetricType == metric
                     && r.RecordedAtUtc >= request.FromUtc && r.RecordedAtUtc <= request.ToUtc)
            .OrderBy(r => r.RecordedAtUtc)
            .Take(request.MaxPoints)
            .ToListAsync(cancellationToken);

        return readings.Select(IngestReadingCommandHandler.Map).ToList();
    }
}
