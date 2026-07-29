using Microsoft.EntityFrameworkCore;
using MSECS.Site.Domain.Entities;

namespace MSECS.Site.Application.Common.Interfaces;

public interface ISiteDbContext
{
    DbSet<SolarSite> Sites { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
