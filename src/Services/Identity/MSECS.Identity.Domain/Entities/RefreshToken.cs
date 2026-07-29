namespace MSECS.Identity.Domain.Entities;

/// <summary>
/// Opaque, rotating refresh token. Stored hashed-at-rest would be ideal for a real
/// deployment; kept as plain token here for prototype clarity, with rotation and
/// revocation enforced so a stolen token has a bounded blast radius.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }

    private RefreshToken() { }

    public RefreshToken(Guid id, Guid userId, string token, DateTimeOffset expiresAtUtc, string? createdByIp)
    {
        Id = id;
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        CreatedByIp = createdByIp;
    }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTimeOffset.UtcNow;
    public bool IsExpired => ExpiresAtUtc <= DateTimeOffset.UtcNow;

    public void Revoke(string? revokedByIp, string? replacedByToken = null)
    {
        RevokedAtUtc = DateTimeOffset.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByToken = replacedByToken;
    }
}
