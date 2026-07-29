using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSECS.DeviceRegistry.Application.Devices.Commands.ProvisionDevice;
using MSECS.DeviceRegistry.Application.Devices.Commands.UpdateHealthStatus;
using MSECS.DeviceRegistry.Application.Devices.Queries.GetDevice;
using MSECS.DeviceRegistry.Application.Devices.Queries.ListDevicesBySite;

namespace MSECS.DeviceRegistry.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/devices")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;
    public DevicesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = "DevicesRead")]
    public async Task<IActionResult> ListBySite([FromQuery] Guid siteId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListDevicesBySiteQuery(siteId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "DevicesRead")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDeviceQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Provisions a device and returns its plaintext secret exactly once. The caller
    /// (installer app / edge gateway setup flow) must capture it immediately — it cannot be retrieved again.</summary>
    [HttpPost]
    [Authorize(Policy = "DevicesProvision")]
    public async Task<IActionResult> Provision(ProvisionDeviceCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Device.Id }, result);
    }

    [HttpPatch("{id:guid}/health")]
    [Authorize(Policy = "DevicesWrite")]
    public async Task<IActionResult> UpdateHealth(Guid id, [FromBody] UpdateHealthBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateHealthStatusCommand(id, body.HealthStatus), cancellationToken);
        return NoContent();
    }
}

public record UpdateHealthBody(string HealthStatus);
