using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using Microsoft.Extensions.Logging;

namespace RateLimitingService.Core.Services;

/// <summary>
/// Sliding Window algorithm - most accurate, prevents burst at window boundaries
/// </summary>
public class SlidingWindowRateLimiter : IRateLimitAlgorithm
{
    private readonly IRateLimitStorage _storage;
    private readonly ILogger<SlidingWindowRateLimiter> _logger;
    private const string PREFIX = "ratelimit:slidingwindow:";

    public SlidingWindowRateLimiter(
        IRateLimitStorage storage,
        ILogger<SlidingWindowRateLimiter> logger)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RateLimitCheckResult> CheckAsync(string key, RateLimitRule rule, int cost = 1)
    {
        var storageKey = $"{PREFIX}{key}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowStart = now - (long)rule.WindowSize.TotalMilliseconds;

        try
        {
            // Remove old entries outside the window
            await _storage.RemoveFromSortedSetByScoreAsync(storageKey, 0, windowStart);

            // Count current requests in window
            var currentCount = await _storage.CountInSortedSetByScoreAsync(storageKey, windowStart, now);

            if (currentCount + cost <= rule.Limit)
            {
                // Add current request(s)
                for (int i = 0; i < cost; i++)
                {
                    var uniqueId = $"{now}:{Guid.NewGuid()}";
                    await _storage.AddToSortedSetAsync(storageKey, now, uniqueId, rule.WindowSize);
                }

                return new RateLimitCheckResult
                {
                    IsAllowed = true,
                    Remaining = (int)(rule.Limit - currentCount - cost),
                    Limit = rule.Limit,
                    ResetAt = DateTimeOffset.UtcNow.Add(rule.WindowSize).ToUnixTimeSeconds()
                };
            }

            // Deny request
            return new RateLimitCheckResult
            {
                IsAllowed = false,
                Remaining = 0,
                Limit = rule.Limit,
                ResetAt = DateTimeOffset.UtcNow.Add(rule.WindowSize).ToUnixTimeSeconds(),
                RetryAfterSeconds = (int)Math.Ceiling(rule.WindowSize.TotalSeconds)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking sliding window for key {Key}", key);
            throw;
        }
    }

    public async Task ResetAsync(string key)
    {
        var storageKey = $"{PREFIX}{key}";
        await _storage.DeleteAsync(storageKey);
    }

    public async Task<RateLimitCheckResult> GetStatusAsync(string key, RateLimitRule rule)
    {
        var storageKey = $"{PREFIX}{key}";
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowStart = now - (long)rule.WindowSize.TotalMilliseconds;

        await _storage.RemoveFromSortedSetByScoreAsync(storageKey, 0, windowStart);
        var currentCount = await _storage.CountInSortedSetByScoreAsync(storageKey, windowStart, now);

        return new RateLimitCheckResult
        {
            IsAllowed = currentCount < rule.Limit,
            Remaining = (int)Math.Max(0, rule.Limit - currentCount),
            Limit = rule.Limit,
            ResetAt = DateTimeOffset.UtcNow.Add(rule.WindowSize).ToUnixTimeSeconds()
        };
    }
}
