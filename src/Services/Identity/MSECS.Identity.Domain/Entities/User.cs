using MSECS.SharedKernel.Common;
using MSECS.SharedKernel.Multitenancy;
using MSECS.Identity.Domain.Events;

namespace MSECS.Identity.Domain.Entities;

public class User : AggregateRoot<Guid>, ITenantAware
{
    public Guid OrganizationId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? LastLoginAtUtc { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTimeOffset? LockedOutUntilUtc { get; private set; }

    private readonly List<UserRole> _roles = new();
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { } // EF Core

    private User(Guid id, Guid organizationId, string email, string passwordHash, string firstName, string lastName)
        : base(id)
    {
        OrganizationId = organizationId;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public static User Register(Guid organizationId, string email, string passwordHash, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("A valid email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        var user = new User(Guid.NewGuid(), organizationId, email.Trim().ToLowerInvariant(), passwordHash, firstName.Trim(), lastName.Trim());
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, organizationId, user.Email));
        return user;
    }

    public void AssignRole(Guid roleId)
    {
        if (_roles.Any(r => r.RoleId == roleId)) return;
        _roles.Add(new UserRole(Id, roleId));
    }

    public void RemoveRole(Guid roleId) => _roles.RemoveAll(r => r.RoleId == roleId);

    public const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public bool IsLockedOut => LockedOutUntilUtc.HasValue && LockedOutUntilUtc > DateTimeOffset.UtcNow;

    public void RegisterFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= MaxFailedAttempts)
        {
            LockedOutUntilUtc = DateTimeOffset.UtcNow.Add(LockoutDuration);
        }
    }

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedOutUntilUtc = null;
        LastLoginAtUtc = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new UserLoggedInEvent(Id, LastLoginAtUtc.Value));
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    public void VerifyEmail() => IsEmailVerified = true;
    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;

    public RefreshToken IssueRefreshToken(string token, DateTimeOffset expiresAtUtc, string? createdByIp)
    {
        var refreshToken = new RefreshToken(Guid.NewGuid(), Id, token, expiresAtUtc, createdByIp);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }
}

/// <summary>Join entity: roles assigned to a user (a user can hold several, e.g. SiteManager + Installer).</summary>
public class UserRole
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    private UserRole() { }

    public UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}
