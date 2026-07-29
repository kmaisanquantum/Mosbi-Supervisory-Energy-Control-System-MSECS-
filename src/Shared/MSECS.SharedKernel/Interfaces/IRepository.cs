using MSECS.SharedKernel.Common;

namespace MSECS.SharedKernel.Interfaces;

/// <summary>
/// Generic repository contract implemented per-aggregate in each service's
/// Infrastructure layer. Keeps Application-layer handlers persistence-agnostic.
/// </summary>
public interface IRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
