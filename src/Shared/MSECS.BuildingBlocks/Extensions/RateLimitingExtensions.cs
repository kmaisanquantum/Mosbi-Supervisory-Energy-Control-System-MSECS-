using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MSECS.BuildingBlocks.Extensions;

public static class RateLimitingExtensions
{
    /// <summary>
    /// Per-client (by authenticated user id, falling back to remote IP) fixed-window
    /// rate limiting. "telemetry-ingest" policy is looser since devices push frequently;
    /// "default" applies to standard API traffic.
    /// </summary>
    public static IServiceCollection AddMsecsRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("default", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolvePartitionKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy("telemetry-ingest", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolvePartitionKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 1000,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 50
                    }));
        });

        return services;
    }

    private static string ResolvePartitionKey(HttpContext context) =>
        context.User.Identity?.IsAuthenticated == true
            ? context.User.Identity.Name ?? "authenticated"
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
