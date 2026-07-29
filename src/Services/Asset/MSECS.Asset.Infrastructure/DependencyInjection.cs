using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSECS.Asset.Application.Common.Interfaces;

namespace MSECS.Asset.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAssetInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AssetDb")
            ?? throw new InvalidOperationException("ConnectionStrings:AssetDb is not configured.");

        services.AddDbContext<Persistence.AssetDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "asset")));

        services.AddScoped<IAssetDbContext>(sp => sp.GetRequiredService<Persistence.AssetDbContext>());

        return services;
    }
}
