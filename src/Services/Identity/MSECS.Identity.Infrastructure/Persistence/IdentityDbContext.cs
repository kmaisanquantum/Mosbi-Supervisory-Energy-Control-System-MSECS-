using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.Identity.Application.Common.Interfaces;
using MSECS.Identity.Domain.Entities;
using MSECS.SharedKernel.Common;

namespace MSECS.Identity.Infrastructure.Persistence;

public class IdentityDbContext : DbContext, IIdentityDbContext
{
    private readonly IMediator? _mediator;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, IMediator? mediator = null)
        : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Stamp audit fields for every AuditableEntity in the change set.
        foreach (var entry in ChangeTracker.Entries<AuditableEntity<Guid>>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAtUtc = DateTimeOffset.UtcNow;
            else if (entry.State == EntityState.Modified)
                entry.Entity.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        // Collect and publish domain events raised on aggregates in this change set
        // AFTER a successful save, so subscribers see committed state.
        var aggregatesWithEvents = ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<AggregateRoot<Guid>>()
            .Where(a => a.DomainEvents.Count != 0)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_mediator is not null)
        {
            var events = aggregatesWithEvents.SelectMany(a => a.DomainEvents).ToList();
            foreach (var aggregate in aggregatesWithEvents) aggregate.ClearDomainEvents();
            foreach (var domainEvent in events) await _mediator.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
