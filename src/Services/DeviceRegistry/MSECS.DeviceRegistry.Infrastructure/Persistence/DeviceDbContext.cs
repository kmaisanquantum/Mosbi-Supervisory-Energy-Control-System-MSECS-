using MediatR;
using Microsoft.EntityFrameworkCore;
using MSECS.DeviceRegistry.Application.Common.Interfaces;
using MSECS.DeviceRegistry.Domain.Entities;
using MSECS.SharedKernel.Common;

namespace MSECS.DeviceRegistry.Infrastructure.Persistence;

public class DeviceDbContext : DbContext, IDeviceDbContext
{
    private readonly IMediator? _mediator;

    public DeviceDbContext(DbContextOptions<DeviceDbContext> options, IMediator? mediator = null) : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Device> Devices => Set<Device>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("device_registry");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeviceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity<Guid>>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAtUtc = DateTimeOffset.UtcNow;
            else if (entry.State == EntityState.Modified) entry.Entity.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }

        var aggregatesWithEvents = ChangeTracker.Entries()
            .Select(e => e.Entity).OfType<AggregateRoot<Guid>>()
            .Where(a => a.DomainEvents.Count != 0).ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_mediator is not null)
        {
            var events = aggregatesWithEvents.SelectMany(a => a.DomainEvents).ToList();
            foreach (var a in aggregatesWithEvents) a.ClearDomainEvents();
            foreach (var e in events) await _mediator.Publish(e, cancellationToken);
        }

        return result;
    }
}
