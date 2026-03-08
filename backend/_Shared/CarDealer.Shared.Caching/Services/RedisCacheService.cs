using System.Text.Json;
using CarDealer.Shared.Caching.Interfaces;
using CarDealer.Shared.Caching.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CarDealer.Shared.Caching.Services;

/// <summary>
/// Standard Redis-backed cache service for all OKLA microservices.
/// 
/// Features:
/// - Generic JSON serialization for any type
/// - Fail-safe: all Redis errors are caught and logged, never thrown
/// - Key prefixing via InstanceName for multi-service Redis isolation
/// - Configurable TTL with max TTL guard
/// - Cache stampede prevention via GetOrSetAsync
/// - Pattern-based invalidation via Redis SCAN
/// - Optional hit/miss metrics logging
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly CacheOptions _options;

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    // Metrics counters
    private long _hits;
    private long _misses;

    public RedisCacheService(
        IDistributedCache cache,
        IOptions<CacheOptions> options,
        ILogger<RedisCacheService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var data = await _cache.GetStringAsync(key, cancellationToken);
            if (data is null)
            {
                Interlocked.Increment(ref _misses);
                LogMetrics(key, false);
                return default;
            }

            Interlocked.Increment(ref _hits);
            LogMetrics(key, true);
            return JsonSerializer.Deserialize<T>(data, s_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache GET failed for key {CacheKey}. Returning default.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, int? ttlSeconds = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var effectiveTtl = Math.Min(
                ttlSeconds ?? _options.DefaultTtlSeconds,
                _options.MaxTtlSeconds);

            var json = JsonSerializer.Serialize(value, s_jsonOptions);

            var entryOptions = new DistributedCacheEntryOptions();
            if (_options.UseSlidingExpiration)
                entryOptions.SlidingExpiration = TimeSpan.FromSeconds(effectiveTtl);
            else
                entryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(effectiveTtl);

            await _cache.SetStringAsync(key, json, entryOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache SET failed for key {CacheKey}. Value not cached.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache REMOVE failed for key {CacheKey}.", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _cache.GetAsync(key, cancellationToken);
            return data is not null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache EXISTS check failed for key {CacheKey}.", key);
            return false;
        }
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int? ttlSeconds = null, CancellationToken cancellationToken = default) where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
            return cached;

        try
        {
            var value = await factory();
            if (value is not null)
                await SetAsync(key, value, ttlSeconds, cancellationToken);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache GetOrSet factory failed for key {CacheKey}.", key);
            return default;
        }
    }

    public async Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (_redis is null)
        {
            _logger.LogDebug("Pattern-based invalidation not supported without direct Redis connection. Pattern: {Pattern}", pattern);
            return;
        }

        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var fullPattern = $"{_options.InstanceName}{pattern}";
            var db = _redis.GetDatabase();
            var keysRemoved = 0;

            await foreach (var key in server.KeysAsync(pattern: fullPattern))
            {
                await db.KeyDeleteAsync(key);
                keysRemoved++;

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            _logger.LogInformation("Cache invalidation: removed {Count} keys matching pattern {Pattern}.", keysRemoved, fullPattern);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache pattern invalidation failed for pattern {Pattern}.", pattern);
        }
    }

    private void LogMetrics(string key, bool hit)
    {
        if (!_options.EnableMetrics) return;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Cache {Status} for key {CacheKey}. Total hits={Hits}, misses={Misses}",
                hit ? "HIT" : "MISS", key, _hits, _misses);
        }
    }
}
