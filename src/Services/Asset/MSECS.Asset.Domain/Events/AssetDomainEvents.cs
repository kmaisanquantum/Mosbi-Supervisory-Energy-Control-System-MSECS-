using MSECS.SharedKernel.Common;

namespace MSECS.Asset.Domain.Events;

public record AssetRegisteredEvent(Guid AssetId, Guid SiteId, string AssetType, string SerialNumber) : DomainEvent;
public record AssetStatusChangedEvent(Guid AssetId, string OldStatus, string NewStatus) : DomainEvent;
public record MaintenanceRecordedEvent(Guid AssetId, Guid MaintenanceRecordId, string MaintenanceType) : DomainEvent;
