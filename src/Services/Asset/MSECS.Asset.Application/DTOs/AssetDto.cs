namespace MSECS.Asset.Application.DTOs;

public record AssetDto(
    Guid Id,
    Guid OrganizationId,
    Guid SiteId,
    Guid? ParentAssetId,
    string Type,
    string Manufacturer,
    string Model,
    string SerialNumber,
    decimal? RatedCapacityKw,
    string? FirmwareVersion,
    DateOnly InstallationDate,
    string Status,
    Guid? DeviceId);

public record MaintenanceRecordDto(Guid Id, Guid AssetId, string Type, string Description, string PerformedBy, DateTimeOffset PerformedAtUtc);
