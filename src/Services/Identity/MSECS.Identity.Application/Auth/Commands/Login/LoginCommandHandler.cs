using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Identity.Application.Common.Interfaces;
using MSECS.Identity.Application.DTOs;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.Identity.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IIdentityDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginCommandHandler(IIdentityDbContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        // Deliberately identical error for "not found" and "wrong password" to avoid
        // leaking account existence.
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.IsLockedOut)
            throw new UnauthorizedAccessException($"Account is locked until {user.LockedOutUntilUtc:u} due to repeated failed login attempts.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RegisterFailedLogin();
            await _db.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var roleIds = user.Roles.Select(r => r.RoleId).ToList();
        var roles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync(cancellationToken);

        user.RegisterSuccessfulLogin();

        var refreshTokenValue = _tokenGenerator.GenerateRefreshToken();
        var refreshToken = user.IssueRefreshToken(refreshTokenValue, DateTimeOffset.UtcNow.AddDays(30), request.IpAddress);

        await _db.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenGenerator.GenerateAccessToken(
            user,
            roleNames: roles.Select(r => r.Name),
            permissionKeys: roles.SelectMany(r => r.Permissions.Select(p => p.PermissionKey)).Distinct());

        return new AuthResultDto(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshToken.Token,
            refreshToken.ExpiresAtUtc,
            new UserDto(user.Id, user.OrganizationId, user.Email, user.FirstName, user.LastName,
                roles.Select(r => r.Name).ToList(), user.IsEmailVerified, user.LastLoginAtUtc));
    }
}
