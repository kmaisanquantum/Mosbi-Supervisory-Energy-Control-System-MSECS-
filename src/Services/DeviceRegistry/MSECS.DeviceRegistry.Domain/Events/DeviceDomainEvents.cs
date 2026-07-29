using MSECS.SharedKernel.Common;

namespace MSECS.DeviceRegistry.Domain.Events;

public record DeviceProvisionedEvent(Guid DeviceId, Guid OrganizationId, Guid SiteId, Guid AssetId, string Protocol) : DomainEvent;
public record DeviceHealthChangedEvent(Guid DeviceId, string OldStatus, string NewStatus, DateTimeOffset ObservedAtUtc) : DomainEvent;
public record DeviceRevokedEvent(Guid DeviceId) : DomainEvent;
