using Microsoft.EntityFrameworkCore;
using MSECS.Telemetry.Domain.Entities;

namespace MSECS.Telemetry.Application.Common.Interfaces;

public interface ITelemetryDbContext
{
    DbSet<TelemetryReading> Readings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
