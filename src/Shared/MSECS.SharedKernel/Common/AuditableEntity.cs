namespace MSECS.SharedKernel.Common;

/// <summary>
/// Adds creation/modification audit fields, used by nearly every MSECS entity.
/// </summary>
public abstract class AuditableEntity<TId> : Entity<TId> where TId : notnull
{
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }

    protected AuditableEntity() { }
    protected AuditableEntity(TId id) : base(id) { }
}
