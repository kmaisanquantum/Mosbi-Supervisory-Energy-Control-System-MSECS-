using MSECS.SharedKernel.Common;

namespace MSECS.Site.Domain.Events;

public record SiteCommissionedEvent(Guid SiteId, Guid OrganizationId, string Name) : DomainEvent;
public record SiteCapacityChangedEvent(Guid SiteId, decimal OldCapacityKw, decimal NewCapacityKw) : DomainEvent;
