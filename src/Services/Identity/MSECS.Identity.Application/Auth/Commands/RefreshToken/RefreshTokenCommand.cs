using MediatR;
using MSECS.Identity.Application.DTOs;

namespace MSECS.Identity.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string? IpAddress) : IRequest<AuthResultDto>;
