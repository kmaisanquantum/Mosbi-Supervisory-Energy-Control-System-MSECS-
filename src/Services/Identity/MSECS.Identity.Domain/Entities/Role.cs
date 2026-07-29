using MSECS.SharedKernel.Common;

namespace MSECS.Identity.Domain.Entities;

public class Role : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid? OrganizationId { get; private set; } // null = system-wide built-in role
    public bool IsSystemRole { get; private set; }

    private readonly List<RolePermission> _permissions = new();
    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    private Role() { }

    private Role(Guid id, string name, Guid? organizationId, bool isSystemRole) : base(id)
    {
        Name = name;
        OrganizationId = organizationId;
        IsSystemRole = isSystemRole;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public static Role CreateSystemRole(string name) => new(Guid.NewGuid(), name, null, true);

    public static Role CreateCustomRole(string name, Guid organizationId) =>
        new(Guid.NewGuid(), name, organizationId, false);

    public void GrantPermission(string permissionKey)
    {
        if (_permissions.Any(p => p.PermissionKey == permissionKey)) return;
        _permissions.Add(new RolePermission(Id, permissionKey));
    }

    public void RevokePermission(string permissionKey)
    {
        _permissions.RemoveAll(p => p.PermissionKey == permissionKey);
    }
}

/// <summary>Join entity: which permission keys (see SystemPermissions) a role grants.</summary>
public class RolePermission
{
    public Guid RoleId { get; private set; }
    public string PermissionKey { get; private set; } = string.Empty;

    private RolePermission() { }

    public RolePermission(Guid roleId, string permissionKey)
    {
        RoleId = roleId;
        PermissionKey = permissionKey;
    }
}
