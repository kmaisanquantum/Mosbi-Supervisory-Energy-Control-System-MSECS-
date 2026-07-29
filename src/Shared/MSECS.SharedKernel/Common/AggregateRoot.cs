namespace MSECS.SharedKernel.Common;

/// <summary>
/// Marks an entity as the root of an aggregate and collects domain events raised
/// during the aggregate's lifetime so they can be dispatched after persistence.
/// </summary>
public abstract class AggregateRoot<TId> : AuditableEntity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(TId id) : base(id) { }

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
