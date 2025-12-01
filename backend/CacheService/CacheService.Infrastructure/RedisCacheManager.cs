using StackExchange.Redis;
using CacheService.Application.Interfaces;
using CacheService.Domain;
using System.Text.Json;

namespace CacheService.Infrastructure;

public class RedisCacheManager : ICacheManager
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IStatisticsManager _statisticsManager;
    private readonly IDatabase _db;

    public RedisCacheManager(IConnectionMultiplexer redis, IStatisticsManager statisticsManager)
    {
        _redis = redis;
        _statisticsManager = statisticsManager;
        _db = redis.GetDatabase();
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(key);
        
        if (value.HasValue)
        {
            _statisticsManager.RecordHit(key);
            return value.ToString();
        }

        _statisticsManager.RecordMiss(key);
        return null;
    }

    public async Task<string?> GetAsync(string key, string tenantId, CancellationToken cancellationToken = default)
    {
        var fullKey = GetTenantKey(key, tenantId);
        return await GetAsync(fullKey, cancellationToken);
    }

    public async Task<bool> SetAsync(string key, string value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        _statisticsManager.RecordSet();
        
        if (ttl.HasValue)
        {
            return await _db.StringSetAsync(key, value, ttl.Value);
        }

        return await _db.StringSetAsync(key, value);
    }

    public async Task<bool> SetAsync(string key, string value, string tenantId, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var fullKey = GetTenantKey(key, tenantId);
        return await SetAsync(fullKey, value, ttl, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        _statisticsManager.RecordDelete();
        return await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _db.KeyExistsAsync(key);
    }

    public async Task<Dictionary<string, string?>> GetManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        var values = await _db.StringGetAsync(redisKeys);

        var result = new Dictionary<string, string?>();
        for (int i = 0; i < redisKeys.Length; i++)
        {
            var key = keys.ElementAt(i);
            if (values[i].HasValue)
            {
                result[key] = values[i].ToString();
                _statisticsManager.RecordHit(key);
            }
            else
            {
                result[key] = null;
                _statisticsManager.RecordMiss(key);
            }
        }

        return result;
    }

    public async Task<bool> SetManyAsync(Dictionary<string, string> keyValues, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var tasks = keyValues.Select(kv => SetAsync(kv.Key, kv.Value, ttl, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    public async Task<long> DeleteManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        var deleted = await _db.KeyDeleteAsync(redisKeys);
        
        for (int i = 0; i < deleted; i++)
        {
            _statisticsManager.RecordDelete();
        }

        return deleted;
    }

    public async Task<long> DeleteByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern);
        
        long deleted = 0;
        foreach (var key in keys)
        {
            if (await _db.KeyDeleteAsync(key))
            {
                deleted++;
                _statisticsManager.RecordDelete();
            }
        }

        return deleted;
    }

    public async Task<CacheEntry?> GetEntryAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue)
            return null;

        var ttl = await _db.KeyTimeToLiveAsync(key);

        return new CacheEntry
        {
            Key = key,
            Value = value.ToString(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : null,
            Ttl = ttl,
            SizeInBytes = value.ToString().Length
        };
    }

    public async Task<TimeSpan?> GetTtlAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _db.KeyTimeToLiveAsync(key);
    }

    public async Task<bool> SetTtlAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        return await _db.KeyExpireAsync(key, ttl);
    }

    public async Task FlushAllAsync(CancellationToken cancellationToken = default)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        await server.FlushDatabaseAsync();
    }

    private string GetTenantKey(string key, string tenantId)
    {
        return $"tenant:{tenantId}:{key}";
    }
}
