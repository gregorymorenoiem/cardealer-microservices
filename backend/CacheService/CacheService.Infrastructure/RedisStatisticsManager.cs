using StackExchange.Redis;
using CacheService.Application.Interfaces;
using CacheService.Domain;

namespace CacheService.Infrastructure;

public class RedisStatisticsManager : IStatisticsManager
{
    private readonly IConnectionMultiplexer _redis;
    private readonly CacheStatistics _statistics;

    public RedisStatisticsManager(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _statistics = new CacheStatistics
        {
            LastResetAt = DateTime.UtcNow
        };
    }

    public Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_statistics);
    }

    public Task ResetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        _statistics.Reset();
        return Task.CompletedTask;
    }

    public void RecordHit(string key)
    {
        _statistics.RecordHit(key);
    }

    public void RecordMiss(string key)
    {
        _statistics.RecordMiss(key);
    }

    public void RecordSet()
    {
        _statistics.RecordSet();
    }

    public void RecordDelete()
    {
        _statistics.RecordDelete();
    }

    public async Task<long> GetCacheSizeAsync(CancellationToken cancellationToken = default)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var info = await server.InfoAsync("memory");
        
        foreach (var group in info)
        {
            foreach (var item in group)
            {
                if (item.Key == "used_memory")
                {
                    return long.Parse(item.Value);
                }
            }
        }

        return 0;
    }

    public async Task<long> GetKeyCountAsync(CancellationToken cancellationToken = default)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var info = await server.InfoAsync("keyspace");
        
        foreach (var group in info)
        {
            foreach (var item in group)
            {
                if (item.Key.StartsWith("db"))
                {
                    // Parse "keys=123,expires=45" format
                    var parts = item.Value.Split(',');
                    if (parts.Length > 0 && parts[0].StartsWith("keys="))
                    {
                        return long.Parse(parts[0].Replace("keys=", ""));
                    }
                }
            }
        }

        return 0;
    }
}
