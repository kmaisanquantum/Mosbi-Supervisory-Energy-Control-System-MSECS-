namespace MSECS.Site.Application.Common.Interfaces;

/// <summary>Thin re-declaration of SharedKernel.ICurrentUserService so the Application
/// project doesn't need a direct package reference beyond the interface it consumes.</summary>
public interface ICurrentUserAccessor
{
    Guid? OrganizationId { get; }
    bool HasPermission(string permission);
}
