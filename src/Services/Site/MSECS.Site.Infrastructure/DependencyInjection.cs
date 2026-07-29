using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSECS.Site.Application.Common.Interfaces;

namespace MSECS.Site.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSiteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SiteDb")
            ?? throw new InvalidOperationException("ConnectionStrings:SiteDb is not configured.");

        services.AddDbContext<Persistence.SiteDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "site")));

        services.AddScoped<ISiteDbContext>(sp => sp.GetRequiredService<Persistence.SiteDbContext>());

        return services;
    }
}
