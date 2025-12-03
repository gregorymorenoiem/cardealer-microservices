using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using Microsoft.Extensions.Logging;

namespace RateLimitingService.Core.Services;

/// <summary>
/// Token Bucket algorithm - allows bursts up to bucket size
/// </summary>
public class TokenBucketRateLimiter : IRateLimitAlgorithm
{
    private readonly IRateLimitStorage _storage;
    private readonly ILogger<TokenBucketRateLimiter> _logger;
    private const string PREFIX = "ratelimit:tokenbucket:";

    public TokenBucketRateLimiter(
        IRateLimitStorage storage,
        ILogger<TokenBucketRateLimiter> logger)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RateLimitCheckResult> CheckAsync(string key, RateLimitRule rule, int cost = 1)
    {
        var storageKey = $"{PREFIX}{key}";
        var lastRefillKey = $"{storageKey}:refill";

        try
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var lastRefill = await _storage.GetAsync(lastRefillKey);

            if (lastRefill == 0)
            {
                // First request - initialize bucket
                await _storage.SetAsync(storageKey, rule.Limit - cost, rule.WindowSize);
                await _storage.SetAsync(lastRefillKey, now, rule.WindowSize);

                return new RateLimitCheckResult
                {
                    IsAllowed = true,
                    Remaining = rule.Limit - cost,
                    Limit = rule.Limit,
                    ResetAt = DateTimeOffset.UtcNow.Add(rule.WindowSize).ToUnixTimeSeconds()
                };
            }

            // Calculate tokens to add based on time elapsed
            var elapsed = now - lastRefill;
            var refillRate = (double)rule.Limit / rule.WindowSize.TotalSeconds;
            var tokensToAdd = (long)(elapsed * refillRate);

            var currentTokens = await _storage.GetAsync(storageKey);
            var newTokens = Math.Min(currentTokens + tokensToAdd, rule.Limit);

            if (newTokens >= cost)
            {
                // Allow request
                await _storage.SetAsync(storageKey, newTokens - cost, rule.WindowSize);
                await _storage.SetAsync(lastRefillKey, now, rule.WindowSize);

                return new RateLimitCheckResult
                {
                    IsAllowed = true,
                    Remaining = (int)(newTokens - cost),
                    Limit = rule.Limit,
                    ResetAt = DateTimeOffset.UtcNow.Add(rule.WindowSize).ToUnixTimeSeconds()
                };
            }

            // Deny request
            var timeUntilRefill = (cost - newTokens) / refillRate;

            return new RateLimitCheckResult
            {
                IsAllowed = false,
                Remaining = 0,
                Limit = rule.Limit,
                ResetAt = DateTimeOffset.UtcNow.AddSeconds(timeUntilRefill).ToUnixTimeSeconds(),
                RetryAfterSeconds = (int)Math.Ceiling(timeUntilRefill)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token bucket for key {Key}", key);
            throw;
        }
    }

    public async Task ResetAsync(string key)
    {
        var storageKey = $"{PREFIX}{key}";
        var lastRefillKey = $"{storageKey}:refill";

        await _storage.DeleteAsync(storageKey);
        await _storage.DeleteAsync(lastRefillKey);
    }

    public async Task<RateLimitCheckResult> GetStatusAsync(string key, RateLimitRule rule)
    {
        var storageKey = $"{PREFIX}{key}";
        var remaining = await _storage.GetAsync(storageKey);
        var ttl = await _storage.GetTtlAsync(storageKey);

        return new RateLimitCheckResult
        {
            IsAllowed = remaining > 0,
            Remaining = (int)Math.Max(0, remaining),
            Limit = rule.Limit,
            ResetAt = ttl.HasValue ? DateTimeOffset.UtcNow.Add(ttl.Value).ToUnixTimeSeconds() : DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }
}
