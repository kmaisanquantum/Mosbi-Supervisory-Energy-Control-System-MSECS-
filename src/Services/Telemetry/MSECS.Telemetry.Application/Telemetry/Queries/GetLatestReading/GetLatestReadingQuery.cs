using MediatR;
using MSECS.Telemetry.Application.DTOs;

namespace MSECS.Telemetry.Application.Telemetry.Queries.GetLatestReading;

public record GetLatestReadingQuery(Guid AssetId, string? MetricType = null) : IRequest<IReadOnlyList<TelemetryReadingDto>>;
