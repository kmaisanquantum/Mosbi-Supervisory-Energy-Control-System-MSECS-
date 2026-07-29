using MSECS.SharedKernel.Common;
using MSECS.SharedKernel.Multitenancy;
using MSECS.Asset.Domain.Enums;
using MSECS.Asset.Domain.Events;

namespace MSECS.Asset.Domain.Entities;

/// <summary>
/// A single piece of physical equipment at a SolarSite (panel, inverter, battery, meter,
/// weather station, or controller). One asset may have zero or one associated Device
/// (the network-addressable telemetry/control endpoint), linked via DeviceId once the
/// Device Registry provisions it.
/// </summary>
public class Asset : AggregateRoot<Guid>, ITenantAware
{
    public Guid OrganizationId { get; private set; }
    public Guid SiteId { get; private set; }
    public Guid? ParentAssetId { get; private set; } // e.g. a Panel's parent SolarArray
    public AssetType Type { get; private set; }
    public string Manufacturer { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public string SerialNumber { get; private set; } = string.Empty;
    public decimal? RatedCapacityKw { get; private set; }
    public string? FirmwareVersion { get; private set; }
    public DateOnly InstallationDate { get; private set; }
    public AssetStatus Status { get; private set; } = AssetStatus.Commissioned;
    public Guid? DeviceId { get; private set; }

    private readonly List<MaintenanceRecord> _maintenanceHistory = new();
    public IReadOnlyCollection<MaintenanceRecord> MaintenanceHistory => _maintenanceHistory.AsReadOnly();

    private Asset() { }

    private Asset(Guid id, Guid organizationId, Guid siteId, Guid? parentAssetId, AssetType type,
        string manufacturer, string model, string serialNumber, decimal? ratedCapacityKw, DateOnly installationDate)
        : base(id)
    {
        OrganizationId = organizationId;
        SiteId = siteId;
        ParentAssetId = parentAssetId;
        Type = type;
        Manufacturer = manufacturer;
        Model = model;
        SerialNumber = serialNumber;
        RatedCapacityKw = ratedCapacityKw;
        InstallationDate = installationDate;
    }

    public static Asset Register(
        Guid organizationId, Guid siteId, AssetType type, string manufacturer, string model,
        string serialNumber, DateOnly installationDate, decimal? ratedCapacityKw = null, Guid? parentAssetId = null)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("Serial number is required.", nameof(serialNumber));

        var asset = new Asset(Guid.NewGuid(), organizationId, siteId, parentAssetId, type,
            manufacturer.Trim(), model.Trim(), serialNumber.Trim(), ratedCapacityKw, installationDate);

        asset.RaiseDomainEvent(new AssetRegisteredEvent(asset.Id, siteId, type.ToString(), asset.SerialNumber));
        return asset;
    }

    public void LinkDevice(Guid deviceId) => DeviceId = deviceId;
    public void UnlinkDevice() => DeviceId = null;

    public void UpdateFirmwareVersion(string version)
    {
        FirmwareVersion = version;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ChangeStatus(AssetStatus newStatus)
    {
        if (Status == newStatus) return;
        var old = Status;
        Status = newStatus;
        ModifiedAtUtc = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new AssetStatusChangedEvent(Id, old.ToString(), newStatus.ToString()));
    }

    public MaintenanceRecord RecordMaintenance(MaintenanceType type, string description, string performedBy, DateTimeOffset performedAtUtc)
    {
        var record = new MaintenanceRecord(Guid.NewGuid(), Id, type, description, performedBy, performedAtUtc);
        _maintenanceHistory.Add(record);
        RaiseDomainEvent(new MaintenanceRecordedEvent(Id, record.Id, type.ToString()));
        return record;
    }
}
