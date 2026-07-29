using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Identity.Application.Common.Interfaces;
using MSECS.Identity.Application.DTOs;

namespace MSECS.Identity.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Rotates a refresh token: the presented token is revoked and replaced atomically,
/// so a replayed (already-rotated) token is detectable and rejected.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IIdentityDbContext _db;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public RefreshTokenCommandHandler(IIdentityDbContext db, IJwtTokenGenerator tokenGenerator)
    {
        _db = db;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(u => u.Roles)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == request.Token), cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var existingToken = user.RefreshTokens.First(rt => rt.Token == request.Token);

        if (!existingToken.IsActive)
            throw new UnauthorizedAccessException("Refresh token is expired or has been revoked.");

        var roleIds = user.Roles.Select(r => r.RoleId).ToList();
        var roles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync(cancellationToken);

        var newRefreshTokenValue = _tokenGenerator.GenerateRefreshToken();
        var newRefreshToken = user.IssueRefreshToken(newRefreshTokenValue, DateTimeOffset.UtcNow.AddDays(30), request.IpAddress);
        existingToken.Revoke(request.IpAddress, newRefreshToken.Token);

        await _db.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenGenerator.GenerateAccessToken(
            user,
            roleNames: roles.Select(r => r.Name),
            permissionKeys: roles.SelectMany(r => r.Permissions.Select(p => p.PermissionKey)).Distinct());

        return new AuthResultDto(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            newRefreshToken.Token,
            newRefreshToken.ExpiresAtUtc,
            new DTOs.UserDto(user.Id, user.OrganizationId, user.Email, user.FirstName, user.LastName,
                roles.Select(r => r.Name).ToList(), user.IsEmailVerified, user.LastLoginAtUtc));
    }
}
