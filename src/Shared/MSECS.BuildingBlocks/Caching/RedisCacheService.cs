using System.Text.Json;
using StackExchange.Redis;

namespace MSECS.BuildingBlocks.Caching;

/// <summary>
/// Used for device-health snapshots, dashboard aggregates, and JWT deny-list checks.
/// Every key is namespaced by service to avoid collisions on the shared Redis instance.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    private IDatabase Db => _connectionMultiplexer.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await Db.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        await Db.StringSetAsync(key, JsonSerializer.Serialize(value), expiry);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await Db.KeyDeleteAsync(key);
    }
}

public static class RedisExtensions
{
    public static IServiceCollection AddMsecsRedis(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(connectionString));
        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }
}
