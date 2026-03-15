using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Redis;

public sealed class RedisCacheService : ICacheService
{
    private readonly StackExchange.Redis.IDatabase _database;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(value!.ToString());
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, expiration);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _database.KeyDeleteAsync(key);
    }
}