using MSECS.SharedKernel.Common;

namespace MSECS.Identity.Domain.Events;

public record UserRegisteredEvent(Guid UserId, Guid OrganizationId, string Email) : DomainEvent;
public record UserLoggedInEvent(Guid UserId, DateTimeOffset LoginAtUtc) : DomainEvent;
public record OrganizationCreatedEvent(Guid OrganizationId, string Name, string Type) : DomainEvent;
public record ApiKeyIssuedEvent(Guid ApiKeyId, Guid OrganizationId, string Name) : DomainEvent;
