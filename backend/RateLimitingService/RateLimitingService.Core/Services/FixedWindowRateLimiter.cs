using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using Microsoft.Extensions.Logging;

namespace RateLimitingService.Core.Services;

/// <summary>
/// Fixed Window algorithm - simple, resets at fixed intervals
/// </summary>
public class FixedWindowRateLimiter : IRateLimitAlgorithm
{
    private readonly IRateLimitStorage _storage;
    private readonly ILogger<FixedWindowRateLimiter> _logger;
    private const string PREFIX = "ratelimit:fixedwindow:";

    public FixedWindowRateLimiter(
        IRateLimitStorage storage,
        ILogger<FixedWindowRateLimiter> logger)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RateLimitCheckResult> CheckAsync(string key, RateLimitRule rule, int cost = 1)
    {
        var window = GetCurrentWindow(rule.WindowSize);
        var storageKey = $"{PREFIX}{key}:{window}";

        try
        {
            var currentCount = await _storage.GetAsync(storageKey);

            if (currentCount + cost <= rule.Limit)
            {
                var newCount = await _storage.IncrementAsync(storageKey, rule.WindowSize);

                return new RateLimitCheckResult
                {
                    IsAllowed = true,
                    Remaining = (int)(rule.Limit - newCount),
                    Limit = rule.Limit,
                    ResetAt = GetWindowReset(window, rule.WindowSize).ToUnixTimeSeconds()
                };
            }

            // Deny request
            return new RateLimitCheckResult
            {
                IsAllowed = false,
                Remaining = 0,
                Limit = rule.Limit,
                ResetAt = GetWindowReset(window, rule.WindowSize).ToUnixTimeSeconds(),
                RetryAfterSeconds = (int)(GetWindowReset(window, rule.WindowSize) - DateTimeOffset.UtcNow).TotalSeconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking fixed window for key {Key}", key);
            throw;
        }
    }

    public async Task ResetAsync(string key)
    {
        // Delete all possible windows (last 2 to be safe)
        var storageKey = $"{PREFIX}{key}:*";
        // Note: This is simplified - in production, you'd want to track window keys
        await Task.CompletedTask;
    }

    public async Task<RateLimitCheckResult> GetStatusAsync(string key, RateLimitRule rule)
    {
        var window = GetCurrentWindow(rule.WindowSize);
        var storageKey = $"{PREFIX}{key}:{window}";
        var currentCount = await _storage.GetAsync(storageKey);

        return new RateLimitCheckResult
        {
            IsAllowed = currentCount < rule.Limit,
            Remaining = (int)Math.Max(0, rule.Limit - currentCount),
            Limit = rule.Limit,
            ResetAt = GetWindowReset(window, rule.WindowSize).ToUnixTimeSeconds()
        };
    }

    private static long GetCurrentWindow(TimeSpan windowSize)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var windowSeconds = (long)windowSize.TotalSeconds;
        return now / windowSeconds;
    }

    private static DateTimeOffset GetWindowReset(long window, TimeSpan windowSize)
    {
        var windowSeconds = (long)windowSize.TotalSeconds;
        var resetTime = (window + 1) * windowSeconds;
        return DateTimeOffset.FromUnixTimeSeconds(resetTime);
    }
}
