using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSECS.Site.Application.Sites.Commands.CreateSite;
using MSECS.Site.Application.Sites.Commands.UpdateSite;
using MSECS.Site.Application.Sites.Queries.GetSite;
using MSECS.Site.Application.Sites.Queries.ListSites;

namespace MSECS.Site.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sites")]
[Authorize]
public class SitesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SitesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Policy = "SitesRead")]
    public async Task<IActionResult> List([FromQuery] Guid organizationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListSitesQuery(organizationId, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSiteQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "SitesWrite")]
    public async Task<IActionResult> Create(CreateSiteCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SitesWrite")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSiteBody body, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateSiteCommand(id, body.Name, body.InstalledCapacityKw), cancellationToken);
        return Ok(result);
    }
}

public record UpdateSiteBody(string Name, decimal InstalledCapacityKw);
