using Microsoft.EntityFrameworkCore;
using MSECS.Telemetry.Application.Common.Interfaces;
using MSECS.Telemetry.Domain.Entities;

namespace MSECS.Telemetry.Infrastructure.Persistence;

/// <summary>
/// No SharedKernel audit-stamping or MediatR domain-event dispatch here on purpose:
/// telemetry ingestion is a hot path (potentially thousands of rows/sec across a fleet)
/// and TelemetryReading is intentionally not an AggregateRoot. Integration events are
/// published explicitly by IngestReadingCommandHandler instead.
/// </summary>
public class TelemetryDbContext : DbContext, ITelemetryDbContext
{
    public TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : base(options) { }

    public DbSet<TelemetryReading> Readings => Set<TelemetryReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("telemetry");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TelemetryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
