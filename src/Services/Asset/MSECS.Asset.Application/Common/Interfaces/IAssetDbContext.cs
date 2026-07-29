using Microsoft.EntityFrameworkCore;
using MSECS.Asset.Domain.Entities;

namespace MSECS.Asset.Application.Common.Interfaces;

public interface IAssetDbContext
{
    DbSet<Domain.Entities.Asset> Assets { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
