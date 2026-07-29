using MSECS.SharedKernel.Common;
using MSECS.Identity.Domain.Enums;
using MSECS.Identity.Domain.Events;

namespace MSECS.Identity.Domain.Entities;

/// <summary>
/// Tenant root. Every Site, Asset, Device, and User is scoped to an Organization,
/// which is the multi-tenancy boundary enforced by EF Core global query filters
/// across all MSECS services.
/// </summary>
public class Organization : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public OrganizationType Type { get; private set; }
    public Guid? ParentOrganizationId { get; private set; } // e.g. Installer's customer sub-orgs
    public string? ContactEmail { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private Organization() { } // EF Core

    private Organization(Guid id, string name, OrganizationType type, Guid? parentOrganizationId, string? contactEmail)
        : base(id)
    {
        Name = name;
        Type = type;
        ParentOrganizationId = parentOrganizationId;
        ContactEmail = contactEmail;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public static Organization Create(string name, OrganizationType type, string? contactEmail = null, Guid? parentOrganizationId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name is required.", nameof(name));

        var org = new Organization(Guid.NewGuid(), name.Trim(), type, parentOrganizationId, contactEmail);
        org.RaiseDomainEvent(new OrganizationCreatedEvent(org.Id, org.Name, org.Type.ToString()));
        return org;
    }

    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name is required.", nameof(name));
        Name = name.Trim();
        ModifiedAtUtc = DateTimeOffset.UtcNow;
    }
}
