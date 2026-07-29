namespace MSECS.BuildingBlocks.Auth;

/// <summary>
/// Maps ASP.NET Core authorization policy names (used on [Authorize(Policy = "...")])
/// to the "permission" claim values issued by the Identity Service's JWT (see
/// SystemPermissions in MSECS.Identity.Domain.Enums). Kept in BuildingBlocks so every
/// service can reference the same policy names without depending on Identity's domain.
/// </summary>
public static class PermissionPolicies
{
    public record Permission(string PolicyName, string ClaimValue);

    public static readonly Permission SitesRead = new("SitesRead", "sites:read");
    public static readonly Permission SitesWrite = new("SitesWrite", "sites:write");
    public static readonly Permission AssetsRead = new("AssetsRead", "assets:read");
    public static readonly Permission AssetsWrite = new("AssetsWrite", "assets:write");
    public static readonly Permission DevicesRead = new("DevicesRead", "devices:read");
    public static readonly Permission DevicesWrite = new("DevicesWrite", "devices:write");
    public static readonly Permission DevicesProvision = new("DevicesProvision", "devices:provision");
    public static readonly Permission TelemetryRead = new("TelemetryRead", "telemetry:read");
    public static readonly Permission TelemetryIngest = new("TelemetryIngest", "telemetry:ingest");
    public static readonly Permission AlarmsRead = new("AlarmsRead", "alarms:read");
    public static readonly Permission AlarmsAcknowledge = new("AlarmsAcknowledge", "alarms:acknowledge");
    public static readonly Permission CommandsIssue = new("CommandsIssue", "commands:issue");
    public static readonly Permission ReportsRead = new("ReportsRead", "reports:read");
    public static readonly Permission UsersManage = new("UsersManage", "users:manage");
    public static readonly Permission OrgManage = new("OrgManage", "organization:manage");

    public static readonly IReadOnlyList<Permission> All = new[]
    {
        SitesRead, SitesWrite, AssetsRead, AssetsWrite, DevicesRead, DevicesWrite, DevicesProvision,
        TelemetryRead, TelemetryIngest, AlarmsRead, AlarmsAcknowledge, CommandsIssue, ReportsRead,
        UsersManage, OrgManage
    };
}
