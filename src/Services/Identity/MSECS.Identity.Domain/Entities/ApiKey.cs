using MSECS.SharedKernel.Common;
using MSECS.SharedKernel.Multitenancy;
using MSECS.Identity.Domain.Events;

namespace MSECS.Identity.Domain.Entities;

/// <summary>
/// Long-lived credential for machine-to-machine access (edge gateways, integrations).
/// The key itself is never stored; only its SHA-256 hash, matching the pattern used
/// for device credentials in the Device Registry service.
/// </summary>
public class ApiKey : AggregateRoot<Guid>, ITenantAware
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string KeyHash { get; private set; } = string.Empty;
    public string KeyPrefix { get; private set; } = string.Empty; // shown to user for identification
    public DateTimeOffset? ExpiresAtUtc { get; private set; }
    public DateTimeOffset? LastUsedAtUtc { get; private set; }
    public bool IsRevoked { get; private set; }

    private readonly List<string> _scopes = new();
    public IReadOnlyCollection<string> Scopes => _scopes.AsReadOnly();

    private ApiKey() { }

    private ApiKey(Guid id, Guid organizationId, string name, string keyHash, string keyPrefix, DateTimeOffset? expiresAtUtc, IEnumerable<string> scopes)
        : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        KeyHash = keyHash;
        KeyPrefix = keyPrefix;
        ExpiresAtUtc = expiresAtUtc;
        _scopes.AddRange(scopes);
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public static ApiKey Issue(Guid organizationId, string name, string keyHash, string keyPrefix, IEnumerable<string> scopes, DateTimeOffset? expiresAtUtc = null)
    {
        var apiKey = new ApiKey(Guid.NewGuid(), organizationId, name, keyHash, keyPrefix, expiresAtUtc, scopes);
        apiKey.RaiseDomainEvent(new ApiKeyIssuedEvent(apiKey.Id, organizationId, name));
        return apiKey;
    }

    public bool IsValid => !IsRevoked && (!ExpiresAtUtc.HasValue || ExpiresAtUtc > DateTimeOffset.UtcNow);

    public void Revoke() => IsRevoked = true;
    public void RecordUsage() => LastUsedAtUtc = DateTimeOffset.UtcNow;
}
