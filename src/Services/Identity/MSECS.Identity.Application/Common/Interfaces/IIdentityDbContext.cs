using Microsoft.EntityFrameworkCore;
using MSECS.Identity.Domain.Entities;

namespace MSECS.Identity.Application.Common.Interfaces;

/// <summary>
/// Application-layer view of the persistence context. Infrastructure's IdentityDbContext
/// implements this so handlers depend on an abstraction rather than EF Core directly.
/// </summary>
public interface IIdentityDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<ApiKey> ApiKeys { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
