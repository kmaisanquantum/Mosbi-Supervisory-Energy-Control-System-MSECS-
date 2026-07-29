using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSECS.DeviceRegistry.Application.Common.Interfaces;

namespace MSECS.DeviceRegistry.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDeviceRegistryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DeviceRegistryDb")
            ?? throw new InvalidOperationException("ConnectionStrings:DeviceRegistryDb is not configured.");

        services.AddDbContext<Persistence.DeviceDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "device_registry")));

        services.AddScoped<IDeviceDbContext>(sp => sp.GetRequiredService<Persistence.DeviceDbContext>());

        return services;
    }
}
