using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSECS.Asset.Application.Assets.Commands.RecordMaintenance;
using MSECS.Asset.Application.Assets.Commands.RegisterAsset;
using MSECS.Asset.Application.Assets.Queries.GetAsset;
using MSECS.Asset.Application.Assets.Queries.ListAssetsBySite;

namespace MSECS.Asset.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/assets")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AssetsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = "AssetsRead")]
    public async Task<IActionResult> ListBySite([FromQuery] Guid siteId, [FromQuery] string? type, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListAssetsBySiteQuery(siteId, type), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AssetsRead")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAssetQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AssetsWrite")]
    public async Task<IActionResult> Register(RegisterAssetCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/maintenance")]
    [Authorize(Policy = "AssetsWrite")]
    public async Task<IActionResult> RecordMaintenance(Guid id, [FromBody] RecordMaintenanceBody body, CancellationToken cancellationToken)
    {
        var command = new RecordMaintenanceCommand(id, body.Type, body.Description, body.PerformedBy, body.PerformedAtUtc);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }
}

public record RecordMaintenanceBody(string Type, string Description, string PerformedBy, DateTimeOffset PerformedAtUtc);
