using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Identity.Application.Common.Interfaces;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.Identity.Application.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand>
{
    private readonly IIdentityDbContext _db;

    public RevokeTokenCommandHandler(IIdentityDbContext db) => _db = db;

    public async Task Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == request.Token), cancellationToken);

        if (user is null)
            throw new NotFoundException("Refresh token was not found.");

        var token = user.RefreshTokens.First(rt => rt.Token == request.Token);
        if (!token.IsActive)
            throw new ConflictException("Refresh token is already revoked or expired.");

        token.Revoke(request.IpAddress);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
