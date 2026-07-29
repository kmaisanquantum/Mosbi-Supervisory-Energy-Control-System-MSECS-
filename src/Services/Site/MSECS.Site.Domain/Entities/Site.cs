using MSECS.SharedKernel.Common;
using MSECS.SharedKernel.Multitenancy;
using MSECS.Site.Domain.Events;

namespace MSECS.Site.Domain.Entities;

/// <summary>
/// A physical solar installation site: an owner's property, an installer's project site,
/// or a standalone array location. Aggregate root for site-level configuration; the
/// physical equipment installed at the site lives in the Asset Service, linked by SiteId.
/// </summary>
public class SolarSite : AggregateRoot<Guid>, ITenantAware
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public GpsCoordinates Coordinates { get; private set; } = null!;
    public string WeatherZone { get; private set; } = string.Empty;
    public string Timezone { get; private set; } = "UTC";
    public decimal InstalledCapacityKw { get; private set; }
    public DateOnly InstallationDate { get; private set; }
    public string? Address { get; private set; }
    public SiteStatus Status { get; private set; } = SiteStatus.Planned;

    private SolarSite() { }

    private SolarSite(Guid id, Guid organizationId, string name, GpsCoordinates coordinates, string weatherZone,
        string timezone, decimal installedCapacityKw, DateOnly installationDate, string? address) : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        Coordinates = coordinates;
        WeatherZone = weatherZone;
        Timezone = timezone;
        InstalledCapacityKw = installedCapacityKw;
        InstallationDate = installationDate;
        Address = address;
    }

    public static SolarSite Commission(
        Guid organizationId, string name, GpsCoordinates coordinates, string weatherZone,
        string timezone, decimal installedCapacityKw, DateOnly installationDate, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Site name is required.", nameof(name));
        if (installedCapacityKw < 0) throw new ArgumentOutOfRangeException(nameof(installedCapacityKw));

        var site = new SolarSite(Guid.NewGuid(), organizationId, name.Trim(), coordinates, weatherZone,
            timezone, installedCapacityKw, installationDate, address)
        {
            Status = SiteStatus.Active
        };

        site.RaiseDomainEvent(new SiteCommissionedEvent(site.Id, organizationId, site.Name));
        return site;
    }

    public void UpdateCapacity(decimal newCapacityKw)
    {
        if (newCapacityKw < 0) throw new ArgumentOutOfRangeException(nameof(newCapacityKw));
        var old = InstalledCapacityKw;
        InstalledCapacityKw = newCapacityKw;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new SiteCapacityChangedEvent(Id, old, newCapacityKw));
    }

    public void Relocate(GpsCoordinates coordinates, string weatherZone)
    {
        Coordinates = coordinates;
        WeatherZone = weatherZone;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Site name is required.", nameof(name));
        Name = name.Trim();
        ModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Decommission() => Status = SiteStatus.Decommissioned;
    public void Suspend() => Status = SiteStatus.Suspended;
    public void Reactivate() => Status = SiteStatus.Active;
}

public enum SiteStatus
{
    Planned = 1,
    Active = 2,
    Suspended = 3,
    Decommissioned = 4
}
