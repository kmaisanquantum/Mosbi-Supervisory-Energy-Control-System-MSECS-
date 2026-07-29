namespace MSECS.Identity.Domain.Enums;

public enum OrganizationType
{
    Installer = 1,
    AssetOwner = 2,
    Utility = 3,
    Platform = 4
}

/// <summary>
/// System-defined roles seeded for every organization. Custom roles can be added
/// per-organization; these are the built-ins referenced by policy checks.
/// </summary>
public static class SystemRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string OrgAdmin = "OrgAdmin";
    public const string Installer = "Installer";
    public const string SiteManager = "SiteManager";
    public const string Viewer = "Viewer";

    public static readonly IReadOnlyList<string> All = new[]
    {
        SuperAdmin, OrgAdmin, Installer, SiteManager, Viewer
    };
}

public static class SystemPermissions
{
    public const string SitesRead = "sites:read";
    public const string SitesWrite = "sites:write";
    public const string AssetsRead = "assets:read";
    public const string AssetsWrite = "assets:write";
    public const string DevicesRead = "devices:read";
    public const string DevicesWrite = "devices:write";
    public const string DevicesProvision = "devices:provision";
    public const string TelemetryRead = "telemetry:read";
    public const string TelemetryIngest = "telemetry:ingest";
    public const string AlarmsRead = "alarms:read";
    public const string AlarmsAcknowledge = "alarms:acknowledge";
    public const string CommandsIssue = "commands:issue";
    public const string ReportsRead = "reports:read";
    public const string UsersManage = "users:manage";
    public const string OrgManage = "organization:manage";
}
