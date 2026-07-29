using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSECS.Telemetry.ProtocolAdapters.Abstractions;
using MSECS.Telemetry.ProtocolAdapters.Modbus;
using MSECS.Telemetry.ProtocolAdapters.Mqtt;
using MSECS.Telemetry.ProtocolAdapters.Rest;
using NModbus;

namespace MSECS.Telemetry.ProtocolAdapters;

public static class DependencyInjection
{
    public static IServiceCollection AddMsecsProtocolAdapters(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IModbusFactory, ModbusFactory>();
        services.AddSingleton<IProtocolAdapter, ModbusTcpAdapter>();
        services.AddSingleton<IProtocolAdapter, RestProtocolAdapter>();

        services.AddSingleton<IProtocolAdapter>(sp =>
            new MqttProtocolAdapter(
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MqttProtocolAdapter>>(),
                configuration["Mqtt:BrokerHost"] ?? "mosquitto",
                int.TryParse(configuration["Mqtt:BrokerPort"], out var p) ? p : 1883));

        services.AddSingleton<ProtocolAdapterFactory>();

        return services;
    }
}
