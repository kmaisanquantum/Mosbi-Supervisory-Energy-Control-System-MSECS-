using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSECS.Telemetry.Application.Common.Interfaces;
using MSECS.Telemetry.Infrastructure.BackgroundServices;
using MSECS.Telemetry.Infrastructure.Messaging;

namespace MSECS.Telemetry.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTelemetryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TelemetryDb")
            ?? throw new InvalidOperationException("ConnectionStrings:TelemetryDb is not configured.");

        services.AddDbContext<Persistence.TelemetryDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "telemetry")));

        services.AddScoped<ITelemetryDbContext>(sp => sp.GetRequiredService<Persistence.TelemetryDbContext>());
        services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

        services.AddSingleton<IModbusPollTargetProvider, InMemoryModbusPollTargetProvider>();
        services.AddHostedService<ModbusPollingBackgroundService>();

        return services;
    }

    /// <summary>
    /// Converts the "readings" table into a TimescaleDB hypertable partitioned on
    /// recorded_at_utc, and applies a compression + retention policy. Idempotent —
    /// safe to call on every startup. Call after EF migrations have run.
    /// </summary>
    public static async Task EnsureTimescaleHypertableAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Persistence.TelemetryDbContext>();

        await db.Database.ExecuteSqlRawAsync(@"
            SELECT create_hypertable('telemetry.readings', 'recorded_at_utc',
                if_not_exists => TRUE, migrate_data => TRUE);
        ");

        await db.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE telemetry.readings SET (
                timescaledb.compress,
                timescaledb.compress_segmentby = 'device_id, metric_type'
            );
        ");

        await db.Database.ExecuteSqlRawAsync(@"
            SELECT add_compression_policy('telemetry.readings', INTERVAL '7 days', if_not_exists => TRUE);
        ");

        await db.Database.ExecuteSqlRawAsync(@"
            SELECT add_retention_policy('telemetry.readings', INTERVAL '2 years', if_not_exists => TRUE);
        ");
    }
}
