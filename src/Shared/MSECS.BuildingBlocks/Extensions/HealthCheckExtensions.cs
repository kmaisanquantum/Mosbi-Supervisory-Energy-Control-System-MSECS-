namespace MSECS.BuildingBlocks.Extensions;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers liveness/readiness checks against the resources a service depends on.
    /// Any of the connection strings can be null/omitted for services that don't use them.
    /// </summary>
    public static IServiceCollection AddMsecsHealthChecks(
        this IServiceCollection services,
        string? postgresConnectionString = null,
        string? redisConnectionString = null,
        string? rabbitMqConnectionString = null)
    {
        var builder = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(postgresConnectionString))
            builder.AddNpgSql(postgresConnectionString, name: "postgresql", tags: new[] { "ready" });

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
            builder.AddRedis(redisConnectionString, name: "redis", tags: new[] { "ready" });

        if (!string.IsNullOrWhiteSpace(rabbitMqConnectionString))
            builder.AddRabbitMQ(rabbitConnectionString: rabbitMqConnectionString, name: "rabbitmq", tags: new[] { "ready" });

        return services;
    }

    public static void MapMsecsHealthChecks(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false
        });

        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });
    }
}
