using RateLimitingService.Core.Interfaces;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace RateLimitingService.Core.Services;

/// <summary>
/// Redis-based storage for rate limiting
/// </summary>
public class RedisRateLimitStorage : IRateLimitStorage
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimitStorage> _logger;
    private readonly IDatabase _db;

    public RedisRateLimitStorage(
        IConnectionMultiplexer redis,
        ILogger<RedisRateLimitStorage> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _db = _redis.GetDatabase();
    }

    public async Task<long> IncrementAsync(string key, TimeSpan expiry)
    {
        try
        {
            var transaction = _db.CreateTransaction();
            var incrementTask = transaction.StringIncrementAsync(key);
            var expireTask = transaction.KeyExpireAsync(key, expiry);

            await transaction.ExecuteAsync();

            return await incrementTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing key {Key}", key);
            throw;
        }
    }

    public async Task<long> GetAsync(string key)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? (long)value : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting key {Key}", key);
            throw;
        }
    }

    public async Task SetAsync(string key, long value, TimeSpan expiry)
    {
        try
        {
            await _db.StringSetAsync(key, value, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting key {Key}", key);
            throw;
        }
    }

    public async Task DeleteAsync(string key)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting key {Key}", key);
            throw;
        }
    }

    public async Task<TimeSpan?> GetTtlAsync(string key)
    {
        try
        {
            return await _db.KeyTimeToLiveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TTL for key {Key}", key);
            throw;
        }
    }

    public async Task<long> AddToSortedSetAsync(string key, double score, string member, TimeSpan expiry)
    {
        try
        {
            var transaction = _db.CreateTransaction();
            var addTask = transaction.SortedSetAddAsync(key, member, score);
            var expireTask = transaction.KeyExpireAsync(key, expiry);

            await transaction.ExecuteAsync();

            return await addTask ? 1 : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to sorted set {Key}", key);
            throw;
        }
    }

    public async Task RemoveFromSortedSetByScoreAsync(string key, double minScore, double maxScore)
    {
        try
        {
            await _db.SortedSetRemoveRangeByScoreAsync(key, minScore, maxScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from sorted set {Key}", key);
            throw;
        }
    }

    public async Task<long> CountInSortedSetByScoreAsync(string key, double minScore, double maxScore)
    {
        try
        {
            return await _db.SortedSetLengthAsync(key, minScore, maxScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting in sorted set {Key}", key);
            throw;
        }
    }

    public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
    {
        try
        {
            return await _db.KeyExpireAsync(key, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting expiry for key {Key}", key);
            throw;
        }
    }
}
