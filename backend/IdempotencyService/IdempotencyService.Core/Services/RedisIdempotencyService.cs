using System.Text.Json;
using IdempotencyService.Core.Interfaces;
using IdempotencyService.Core.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdempotencyService.Core.Services;

/// <summary>
/// Redis-based implementation of IIdempotencyService
/// </summary>
public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IDistributedCache _cache;
    private readonly IdempotencyOptions _options;
    private readonly ILogger<RedisIdempotencyService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    // Stats tracking keys
    private const string StatsKey = "idempotency:stats";
    private const string DuplicatesCounterKey = "idempotency:duplicates:count";

    public RedisIdempotencyService(
        IDistributedCache cache,
        IOptions<IdempotencyOptions> options,
        ILogger<RedisIdempotencyService> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<IdempotencyCheckResult> CheckAsync(string key, string? requestHash = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GetCacheKey(key);
            var data = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(data))
            {
                return IdempotencyCheckResult.NotFound();
            }

            var record = JsonSerializer.Deserialize<IdempotencyRecord>(data, _jsonOptions);

            if (record == null)
            {
                return IdempotencyCheckResult.NotFound();
            }

            // Check if request hash matches (for conflict detection)
            if (_options.ValidateRequestHash && !string.IsNullOrEmpty(requestHash) &&
                !string.IsNullOrEmpty(record.RequestHash) && record.RequestHash != requestHash)
            {
                _logger.LogWarning("Idempotency conflict: key {Key} has different request body", key);
                return IdempotencyCheckResult.Conflict(record,
                    "Request body differs from the original request with this idempotency key");
            }

            // Increment duplicate counter
            await IncrementDuplicateCounterAsync(cancellationToken);

            if (record.Status == IdempotencyStatus.Processing)
            {
                _logger.LogInformation("Request with key {Key} is still processing", key);
                return IdempotencyCheckResult.Processing(record);
            }

            _logger.LogInformation("Returning cached response for key {Key}", key);
            return IdempotencyCheckResult.Completed(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking idempotency key {Key}", key);
            // On error, allow the request to proceed
            return IdempotencyCheckResult.NotFound();
        }
    }

    public async Task<bool> StartProcessingAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GetCacheKey(record.Key);
            record.Status = IdempotencyStatus.Processing;
            record.CreatedAt = DateTime.UtcNow;
            record.ExpiresAt = DateTime.UtcNow.AddSeconds(_options.DefaultTtlSeconds);

            var data = JsonSerializer.Serialize(record, _jsonOptions);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.DefaultTtlSeconds)
            };

            await _cache.SetStringAsync(cacheKey, data, cacheOptions, cancellationToken);

            _logger.LogDebug("Started processing for idempotency key {Key}", record.Key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting processing for key {Key}", record.Key);
            return false;
        }
    }

    public async Task<bool> CompleteAsync(string key, int statusCode, string responseBody, string contentType = "application/json", CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GetCacheKey(key);
            var data = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(data))
            {
                _logger.LogWarning("Cannot complete: record not found for key {Key}", key);
                return false;
            }

            var record = JsonSerializer.Deserialize<IdempotencyRecord>(data, _jsonOptions);

            if (record == null)
            {
                return false;
            }

            record.Status = IdempotencyStatus.Completed;
            record.ResponseStatusCode = statusCode;
            record.ResponseBody = responseBody;
            record.ResponseContentType = contentType;

            var updatedData = JsonSerializer.Serialize(record, _jsonOptions);

            var remainingTtl = record.ExpiresAt - DateTime.UtcNow;
            if (remainingTtl <= TimeSpan.Zero)
            {
                remainingTtl = TimeSpan.FromSeconds(_options.DefaultTtlSeconds);
            }

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = remainingTtl
            };

            await _cache.SetStringAsync(cacheKey, updatedData, cacheOptions, cancellationToken);

            _logger.LogDebug("Completed idempotency key {Key} with status {StatusCode}", key, statusCode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing key {Key}", key);
            return false;
        }
    }

    public async Task<bool> FailAsync(string key, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GetCacheKey(key);
            var data = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(data))
            {
                return false;
            }

            var record = JsonSerializer.Deserialize<IdempotencyRecord>(data, _jsonOptions);

            if (record == null)
            {
                return false;
            }

            record.Status = IdempotencyStatus.Failed;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                record.Metadata["error"] = errorMessage;
            }

            var updatedData = JsonSerializer.Serialize(record, _jsonOptions);

            // Failed records have shorter TTL
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.MinTtlSeconds)
            };

            await _cache.SetStringAsync(cacheKey, updatedData, cacheOptions, cancellationToken);

            _logger.LogDebug("Failed idempotency key {Key}: {Error}", key, errorMessage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error failing key {Key}", key);
            return false;
        }
    }

    public async Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GetCacheKey(key);
            var data = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            return JsonSerializer.Deserialize<IdempotencyRecord>(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting key {Key}", key);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = GetCacheKey(key);
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Deleted idempotency key {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting key {Key}", key);
            return false;
        }
    }

    public async Task<IdempotencyStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var duplicatesData = await _cache.GetStringAsync(DuplicatesCounterKey, cancellationToken);
            var duplicates = long.TryParse(duplicatesData, out var d) ? d : 0;

            return new IdempotencyStats
            {
                DuplicateRequestsBlocked = duplicates,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats");
            return new IdempotencyStats();
        }
    }

    public Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        // Redis automatically handles expiration, so this is a no-op
        _logger.LogDebug("Cleanup requested - Redis handles expiration automatically");
        return Task.FromResult(0);
    }

    private string GetCacheKey(string key) => $"{_options.KeyPrefix}{key}";

    private async Task IncrementDuplicateCounterAsync(CancellationToken cancellationToken)
    {
        try
        {
            var currentData = await _cache.GetStringAsync(DuplicatesCounterKey, cancellationToken);
            var current = long.TryParse(currentData, out var c) ? c : 0;

            await _cache.SetStringAsync(DuplicatesCounterKey, (current + 1).ToString(),
                new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(30) },
                cancellationToken);
        }
        catch
        {
            // Ignore counter errors
        }
    }
}
