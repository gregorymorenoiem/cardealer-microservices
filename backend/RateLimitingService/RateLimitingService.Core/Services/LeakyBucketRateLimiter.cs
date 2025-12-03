using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using Microsoft.Extensions.Logging;

namespace RateLimitingService.Core.Services;

/// <summary>
/// Leaky Bucket algorithm - smooths traffic, no bursts allowed
/// </summary>
public class LeakyBucketRateLimiter : IRateLimitAlgorithm
{
    private readonly IRateLimitStorage _storage;
    private readonly ILogger<LeakyBucketRateLimiter> _logger;
    private const string PREFIX = "ratelimit:leakybucket:";

    public LeakyBucketRateLimiter(
        IRateLimitStorage storage,
        ILogger<LeakyBucketRateLimiter> logger)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RateLimitCheckResult> CheckAsync(string key, RateLimitRule rule, int cost = 1)
    {
        var storageKey = $"{PREFIX}{key}";
        var lastLeakKey = $"{storageKey}:leak";

        try
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var lastLeak = await _storage.GetAsync(lastLeakKey);

            if (lastLeak == 0)
            {
                // First request - initialize bucket
                await _storage.SetAsync(storageKey, cost, rule.WindowSize);
                await _storage.SetAsync(lastLeakKey, now, rule.WindowSize);

                return new RateLimitCheckResult
                {
                    IsAllowed = true,
                    Remaining = rule.Limit - cost,
                    Limit = rule.Limit,
                    ResetAt = DateTimeOffset.UtcNow.Add(rule.WindowSize).ToUnixTimeSeconds()
                };
            }

            // Calculate leaked requests based on time elapsed
            var elapsed = now - lastLeak;
            var leakRate = (double)rule.Limit / rule.WindowSize.TotalSeconds;
            var leaked = (long)(elapsed * leakRate);

            var currentLevel = await _storage.GetAsync(storageKey);
            var newLevel = Math.Max(0, currentLevel - leaked);

            if (newLevel + cost <= rule.Limit)
            {
                // Allow request
                await _storage.SetAsync(storageKey, newLevel + cost, rule.WindowSize);
                await _storage.SetAsync(lastLeakKey, now, rule.WindowSize);

                return new RateLimitCheckResult
                {
                    IsAllowed = true,
                    Remaining = (int)(rule.Limit - newLevel - cost),
                    Limit = rule.Limit,
                    ResetAt = DateTimeOffset.UtcNow.Add(rule.WindowSize).ToUnixTimeSeconds()
                };
            }

            // Deny request - bucket full
            var timeUntilSpace = (newLevel + cost - rule.Limit) / leakRate;

            return new RateLimitCheckResult
            {
                IsAllowed = false,
                Remaining = 0,
                Limit = rule.Limit,
                ResetAt = DateTimeOffset.UtcNow.AddSeconds(timeUntilSpace).ToUnixTimeSeconds(),
                RetryAfterSeconds = (int)Math.Ceiling(timeUntilSpace)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking leaky bucket for key {Key}", key);
            throw;
        }
    }

    public async Task ResetAsync(string key)
    {
        var storageKey = $"{PREFIX}{key}";
        var lastLeakKey = $"{storageKey}:leak";

        await _storage.DeleteAsync(storageKey);
        await _storage.DeleteAsync(lastLeakKey);
    }

    public async Task<RateLimitCheckResult> GetStatusAsync(string key, RateLimitRule rule)
    {
        var storageKey = $"{PREFIX}{key}";
        var currentLevel = await _storage.GetAsync(storageKey);
        var ttl = await _storage.GetTtlAsync(storageKey);

        return new RateLimitCheckResult
        {
            IsAllowed = currentLevel < rule.Limit,
            Remaining = (int)Math.Max(0, rule.Limit - currentLevel),
            Limit = rule.Limit,
            ResetAt = ttl.HasValue ? DateTimeOffset.UtcNow.Add(ttl.Value).ToUnixTimeSeconds() : DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }
}
