using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Redis;

public sealed class RedisCacheService : ICacheService
{
    private readonly StackExchange.Redis.IDatabase _database;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
        _connectionMultiplexer = connectionMultiplexer;

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
    public async Task RemoveByPrefixAsync(string prefix)
    {
        var endpoints = _connectionMultiplexer.GetEndPoints();

        foreach (var endpoint in endpoints)
        {
            var server = _connectionMultiplexer.GetServer(endpoint);

            foreach (var key in server.Keys(pattern: $"{prefix}*"))
            {
                await _database.KeyDeleteAsync(key);
            }
        }
    }
}