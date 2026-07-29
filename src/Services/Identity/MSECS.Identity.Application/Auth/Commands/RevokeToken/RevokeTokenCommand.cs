using MediatR;

namespace MSECS.Identity.Application.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(string Token, string? IpAddress) : IRequest;
