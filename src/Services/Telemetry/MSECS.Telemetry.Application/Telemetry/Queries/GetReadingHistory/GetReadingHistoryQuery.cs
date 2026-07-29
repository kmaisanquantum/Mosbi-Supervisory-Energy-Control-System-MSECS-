using MediatR;
using MSECS.Telemetry.Application.DTOs;

namespace MSECS.Telemetry.Application.Telemetry.Queries.GetReadingHistory;

public record GetReadingHistoryQuery(
    Guid AssetId, string MetricType, DateTimeOffset FromUtc, DateTimeOffset ToUtc, int MaxPoints = 1000)
    : IRequest<IReadOnlyList<TelemetryReadingDto>>;
