using Microsoft.EntityFrameworkCore;
using MSECS.Identity.Domain.Entities;
using MSECS.Identity.Domain.Enums;

namespace MSECS.Identity.Infrastructure.Persistence;

/// <summary>
/// Seeds the built-in system roles and their default permission grants. Run at startup
/// (see Program.cs) so a fresh environment is immediately usable without manual SQL.
/// </summary>
public static class IdentitySeedData
{
    public static async Task SeedAsync(IdentityDbContext db)
    {
        if (await db.Roles.AnyAsync(r => r.OrganizationId == null)) return;

        var superAdmin = Role.CreateSystemRole(SystemRoles.SuperAdmin);
        foreach (var p in AllPermissions) superAdmin.GrantPermission(p);

        var orgAdmin = Role.CreateSystemRole(SystemRoles.OrgAdmin);
        foreach (var p in AllPermissions) orgAdmin.GrantPermission(p);

        var installer = Role.CreateSystemRole(SystemRoles.Installer);
        foreach (var p in new[]
        {
            SystemPermissions.SitesRead, SystemPermissions.SitesWrite,
            SystemPermissions.AssetsRead, SystemPermissions.AssetsWrite,
            SystemPermissions.DevicesRead, SystemPermissions.DevicesWrite, SystemPermissions.DevicesProvision,
            SystemPermissions.TelemetryRead, SystemPermissions.AlarmsRead, SystemPermissions.CommandsIssue
        }) installer.GrantPermission(p);

        var siteManager = Role.CreateSystemRole(SystemRoles.SiteManager);
        foreach (var p in new[]
        {
            SystemPermissions.SitesRead, SystemPermissions.AssetsRead, SystemPermissions.DevicesRead,
            SystemPermissions.TelemetryRead, SystemPermissions.AlarmsRead, SystemPermissions.AlarmsAcknowledge,
            SystemPermissions.ReportsRead
        }) siteManager.GrantPermission(p);

        var viewer = Role.CreateSystemRole(SystemRoles.Viewer);
        foreach (var p in new[]
        {
            SystemPermissions.SitesRead, SystemPermissions.AssetsRead, SystemPermissions.TelemetryRead,
            SystemPermissions.AlarmsRead, SystemPermissions.ReportsRead
        }) viewer.GrantPermission(p);

        await db.Roles.AddRangeAsync(superAdmin, orgAdmin, installer, siteManager, viewer);
        await db.SaveChangesAsync();
    }

    private static readonly string[] AllPermissions =
    {
        SystemPermissions.SitesRead, SystemPermissions.SitesWrite,
        SystemPermissions.AssetsRead, SystemPermissions.AssetsWrite,
        SystemPermissions.DevicesRead, SystemPermissions.DevicesWrite, SystemPermissions.DevicesProvision,
        SystemPermissions.TelemetryRead, SystemPermissions.TelemetryIngest,
        SystemPermissions.AlarmsRead, SystemPermissions.AlarmsAcknowledge,
        SystemPermissions.CommandsIssue, SystemPermissions.ReportsRead,
        SystemPermissions.UsersManage, SystemPermissions.OrgManage
    };
}
