namespace MSECS.SharedKernel.Multitenancy;

/// <summary>
/// Implemented by any entity that must be scoped to an Organization for
/// multi-tenant isolation. Enforced via EF Core global query filters.
/// </summary>
public interface ITenantAware
{
    Guid OrganizationId { get; }
}
