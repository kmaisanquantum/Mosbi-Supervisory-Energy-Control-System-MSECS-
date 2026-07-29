using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MSECS.Identity.Application.Auth.Commands.CreateApiKey;
using MSECS.Identity.Application.Auth.Commands.Login;
using MSECS.Identity.Application.Auth.Commands.Register;
using MSECS.Identity.Application.Auth.Commands.RevokeToken;
using RefreshTokenCmd = MSECS.Identity.Application.Auth.Commands.RefreshToken.RefreshTokenCommand;

namespace MSECS.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting("default")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>Creates a new Organization and its first OrgAdmin user.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), new { }, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, HttpContext.Connection.RemoteIpAddress?.ToString());
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCmd(request.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString());
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RevokeTokenCommand(request.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString());
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>Issues a machine-to-machine API key for the caller's organization (e.g. an edge gateway).</summary>
    [HttpPost("api-keys")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> CreateApiKey(CreateApiKeyCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(CreateApiKey), new { id = result.ApiKeyId }, result);
    }
}

public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
