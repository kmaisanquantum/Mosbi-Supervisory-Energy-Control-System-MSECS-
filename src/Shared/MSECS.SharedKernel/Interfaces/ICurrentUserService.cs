namespace MSECS.SharedKernel.Interfaces;

/// <summary>
/// Resolves the authenticated principal from the JWT for the lifetime of a request.
/// Backed by an HttpContext accessor in MSECS.BuildingBlocks so Application-layer
/// handlers never depend on ASP.NET Core directly.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? OrganizationId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool HasPermission(string permission);
}
