using Microsoft.EntityFrameworkCore;
using MSECS.Identity.Infrastructure.Persistence;

namespace MSECS.Identity.UnitTests;

/// <summary>Builds an isolated EF Core InMemory IdentityDbContext per test so handlers
/// can be exercised against real LINQ/EF behavior without a live Postgres instance.</summary>
public static class TestDbContextFactory
{
    public static IdentityDbContext Create()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new IdentityDbContext(options, mediator: null);
    }
}
