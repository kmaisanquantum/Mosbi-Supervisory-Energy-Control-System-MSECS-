using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MSECS.Telemetry.Application.DTOs;
using MSECS.Telemetry.Application.Telemetry.Commands.IngestReading;
using MSECS.Telemetry.Application.Telemetry.Queries.GetLatestReading;
using MSECS.Telemetry.Application.Telemetry.Queries.GetReadingHistory;

namespace MSECS.Telemetry.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/telemetry")]
[Authorize]
public class TelemetryController : ControllerBase
{
    private readonly IMediator _mediator;
    public TelemetryController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Universal REST ingestion endpoint. Devices/gateways speaking REST push directly here;
    /// devices polled over Modbus TCP or subscribed via MQTT are normalized to this same
    /// shape internally before reaching the identical IngestReadingCommand handler.
    /// </summary>
    [HttpPost("ingest")]
    [Authorize(Policy = "TelemetryIngest")]
    [EnableRateLimiting("telemetry-ingest")]
    public async Task<IActionResult> Ingest(IngestReadingCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Accepted(result);
    }

    [HttpGet("latest")]
    [Authorize(Policy = "TelemetryRead")]
    public async Task<IActionResult> GetLatest([FromQuery] Guid assetId, [FromQuery] string? metricType, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLatestReadingQuery(assetId, metricType), cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    [Authorize(Policy = "TelemetryRead")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] Guid assetId, [FromQuery] string metricType,
        [FromQuery] DateTimeOffset fromUtc, [FromQuery] DateTimeOffset toUtc,
        [FromQuery] int maxPoints = 1000, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetReadingHistoryQuery(assetId, metricType, fromUtc, toUtc, maxPoints), cancellationToken);
        return Ok(result);
    }
}
