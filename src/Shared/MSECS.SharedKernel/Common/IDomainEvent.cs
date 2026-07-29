using MediatR;

namespace MSECS.SharedKernel.Common;

/// <summary>
/// A fact that happened inside the domain (e.g. DeviceProvisioned, SiteCommissioned).
/// Published through MediatR after successful persistence, and optionally forwarded
/// onto the RabbitMQ event bus by an integration-event mapper.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccurredOnUtc { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
}
