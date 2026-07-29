using Serilog;
using Serilog.Events;

namespace MSECS.BuildingBlocks.Extensions;

public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog from configuration (Serilog section) with sane MSECS defaults:
    /// console sink always on, Seq sink when SEQ_URL is configured, service name enrichment.
    /// Call as builder.Host.UseMsecsSerilog(builder.Configuration, "MSECS.Identity").
    /// </summary>
    public static IHostBuilder UseMsecsSerilog(this IHostBuilder hostBuilder, IConfiguration configuration, string serviceName)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Service", serviceName)
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] ({Service}) {CorrelationId} {Message:lj}{NewLine}{Exception}");

            var seqUrl = configuration["Serilog:SeqUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                loggerConfig.WriteTo.Seq(seqUrl);
            }
        });
    }
}
