namespace MSECS.Asset.Domain.Enums;

/// <summary>Phase 1 asset types. Do NOT add utility-side asset types here (see project scope).</summary>
public enum AssetType
{
    SolarArray = 1,
    Panel = 2,
    Inverter = 3,
    Battery = 4,
    Meter = 5,
    WeatherStation = 6,
    Controller = 7
}

public enum AssetStatus
{
    Commissioned = 1,
    Active = 2,
    UnderMaintenance = 3,
    Faulted = 4,
    Decommissioned = 5
}

public enum MaintenanceType
{
    Scheduled = 1,
    Corrective = 2,
    Inspection = 3,
    FirmwareUpdate = 4
}
