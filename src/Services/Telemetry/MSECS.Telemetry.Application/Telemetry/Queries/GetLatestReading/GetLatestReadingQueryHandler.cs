using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Telemetry.Application.Common.Interfaces;
using MSECS.Telemetry.Application.DTOs;
using MSECS.Telemetry.Application.Telemetry.Commands.IngestReading;
using MSECS.Telemetry.Domain.Enums;

namespace MSECS.Telemetry.Application.Telemetry.Queries.GetLatestReading;

/// <summary>Returns the most recent reading per metric type for an asset — the query
/// pattern that backs a live "site status" dashboard tile.</summary>
public class GetLatestReadingQueryHandler : IRequestHandler<GetLatestReadingQuery, IReadOnlyList<TelemetryReadingDto>>
{
    private readonly ITelemetryDbContext _db;
    public GetLatestReadingQueryHandler(ITelemetryDbContext db) => _db = db;

    public async Task<IReadOnlyList<TelemetryReadingDto>> Handle(GetLatestReadingQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Readings.AsNoTracking().Where(r => r.AssetId == request.AssetId);

        if (!string.IsNullOrWhiteSpace(request.MetricType) && Enum.TryParse<TelemetryMetricType>(request.MetricType, true, out var metric))
            query = query.Where(r => r.MetricType == metric);

        var latestPerMetric = await query
            .GroupBy(r => r.MetricType)
            .Select(g => g.OrderByDescending(r => r.RecordedAtUtc).First())
            .ToListAsync(cancellationToken);

        return latestPerMetric.Select(IngestReadingCommandHandler.Map).ToList();
    }
}
