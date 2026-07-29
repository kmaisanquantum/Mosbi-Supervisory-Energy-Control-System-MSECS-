using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Identity.Application.Common.Interfaces;
using MSECS.Identity.Application.DTOs;
using MSECS.Identity.Domain.Entities;
using MSECS.Identity.Domain.Enums;
using MSECS.SharedKernel.Exceptions;

namespace MSECS.Identity.Application.Auth.Commands.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResultDto>
{
    private readonly IIdentityDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public RegisterUserCommandHandler(IIdentityDbContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResultDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await _db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (emailTaken)
            throw new ConflictException($"An account with email '{normalizedEmail}' already exists.");

        var orgType = Enum.Parse<OrganizationType>(request.OrganizationType, true);
        var organization = Organization.Create(request.OrganizationName, orgType, normalizedEmail);
        await _db.Organizations.AddAsync(organization, cancellationToken);

        var orgAdminRole = await _db.Roles.FirstOrDefaultAsync(
            r => r.OrganizationId == null && r.Name == SystemRoles.OrgAdmin, cancellationToken);

        if (orgAdminRole is null)
            throw new NotFoundException("System role 'OrgAdmin' was not found. Ensure seed data has been applied.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Register(organization.Id, normalizedEmail, passwordHash, request.FirstName, request.LastName);
        user.AssignRole(orgAdminRole.Id);
        user.RegisterSuccessfulLogin();

        await _db.Users.AddAsync(user, cancellationToken);

        var refreshTokenValue = _tokenGenerator.GenerateRefreshToken();
        var refreshToken = user.IssueRefreshToken(refreshTokenValue, DateTimeOffset.UtcNow.AddDays(30), createdByIp: null);

        await _db.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenGenerator.GenerateAccessToken(
            user,
            roleNames: new[] { SystemRoles.OrgAdmin },
            permissionKeys: orgAdminRole.Permissions.Select(p => p.PermissionKey));

        return new AuthResultDto(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshToken.Token,
            refreshToken.ExpiresAtUtc,
            new UserDto(user.Id, user.OrganizationId, user.Email, user.FirstName, user.LastName,
                new[] { SystemRoles.OrgAdmin }, user.IsEmailVerified, user.LastLoginAtUtc));
    }
}
