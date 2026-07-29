using MediatR;
using MSECS.Identity.Application.DTOs;

namespace MSECS.Identity.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password, string? IpAddress) : IRequest<AuthResultDto>;
