using Asp.Versioning;

namespace MSECS.BuildingBlocks.Extensions;

public static class ApiVersioningExtensions
{
    /// <summary>
    /// URL-segment API versioning (/api/v1/...) matching the versioning scheme
    /// specified for every MSECS REST API.
    /// </summary>
    public static IServiceCollection AddMsecsApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
