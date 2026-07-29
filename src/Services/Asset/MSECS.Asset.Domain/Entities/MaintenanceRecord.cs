using MSECS.Asset.Domain.Enums;

namespace MSECS.Asset.Domain.Entities;

public class MaintenanceRecord
{
    public Guid Id { get; private set; }
    public Guid AssetId { get; private set; }
    public MaintenanceType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string PerformedBy { get; private set; } = string.Empty;
    public DateTimeOffset PerformedAtUtc { get; private set; }

    private MaintenanceRecord() { }

    public MaintenanceRecord(Guid id, Guid assetId, MaintenanceType type, string description, string performedBy, DateTimeOffset performedAtUtc)
    {
        Id = id;
        AssetId = assetId;
        Type = type;
        Description = description;
        PerformedBy = performedBy;
        PerformedAtUtc = performedAtUtc;
    }
}
