using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using StackExchange.Redis;

namespace RateLimitingService.Core.Services;

/// <summary>
/// Redis-based distributed rate limiting service using sliding window algorithm
/// </summary>
public class RedisRateLimitingService : IRateLimitingService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimitingService> _logger;
    private readonly RateLimitOptions _options;
    private readonly IDatabase _db;

    // Redis key prefixes
    private const string PolicyKeyPrefix = "policy:";
    private const string CounterKeyPrefix = "counter:";
    private const string StatsKeyPrefix = "stats:";

    // Lua script for sliding window rate limiting (atomic operation)
    private static readonly string SlidingWindowScript = @"
        local key = KEYS[1]
        local now = tonumber(ARGV[1])
        local window = tonumber(ARGV[2])
        local limit = tonumber(ARGV[3])
        local windowStart = now - window

        -- Remove old entries outside the window
        redis.call('ZREMRANGEBYSCORE', key, '-inf', windowStart)

        -- Count current requests in window
        local count = redis.call('ZCARD', key)

        if count < limit then
            -- Add current request
            redis.call('ZADD', key, now, now .. ':' .. math.random())
            -- Set expiry on the key
            redis.call('EXPIRE', key, window)
            return {1, limit - count - 1, window - (now - windowStart)}
        else
            -- Get oldest entry to calculate retry after
            local oldest = redis.call('ZRANGE', key, 0, 0, 'WITHSCORES')
            local retryAfter = 0
            if oldest and #oldest > 0 then
                retryAfter = oldest[2] + window - now
            end
            return {0, 0, retryAfter}
        end
    ";

    public RedisRateLimitingService(
        IConnectionMultiplexer redis,
        ILogger<RedisRateLimitingService> logger,
        IOptions<RateLimitOptions> options)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _db = _redis.GetDatabase();
    }

    public async Task<RateLimitResult> CheckRateLimitAsync(
        string clientId,
        string endpoint,
        string? tier = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentNullException(nameof(clientId));

        if (!_options.Enabled)
        {
            return RateLimitResult.Allowed(clientId, "unlimited", int.MaxValue, DateTime.UtcNow.AddDays(1));
        }

        try
        {
            // Get limit based on tier
            var effectiveTier = tier ?? UserTiers.Free;
            var limit = GetLimitForTier(effectiveTier);
            var windowSeconds = _options.DefaultWindowSeconds;

            // Check for endpoint-specific policy
            var policy = await GetPolicyForEndpointAsync(endpoint, cancellationToken);
            if (policy != null && policy.Enabled)
            {
                limit = policy.MaxRequests;
                windowSeconds = policy.WindowSeconds;
            }

            // Generate Redis key
            var key = $"{_options.KeyPrefix}{CounterKeyPrefix}{clientId}";

            // Execute sliding window algorithm
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var windowMs = windowSeconds * 1000L;

            var result = await _db.ScriptEvaluateAsync(
                SlidingWindowScript,
                new RedisKey[] { key },
                new RedisValue[] { now, windowMs, limit });

            var resultArray = (RedisResult[])result!;
            var allowed = (int)resultArray[0] == 1;
            var remaining = (int)resultArray[1];
            var retryAfterMs = (long)resultArray[2];

            var resetAt = DateTime.UtcNow.AddSeconds(windowSeconds);

            // Record statistics
            await RecordStatisticsAsync(clientId, endpoint, allowed, cancellationToken);

            if (allowed)
            {
                _logger.LogDebug(
                    "Rate limit check passed for client {ClientId} on {Endpoint}. Remaining: {Remaining}",
                    clientId, endpoint, remaining);

                return RateLimitResult.Allowed(clientId, effectiveTier, remaining, resetAt, limit);
            }
            else
            {
                _logger.LogWarning(
                    "Rate limit exceeded for client {ClientId} on {Endpoint}. Retry after: {RetryAfter}ms",
                    clientId, endpoint, retryAfterMs);

                return RateLimitResult.RateLimited(
                    clientId,
                    effectiveTier,
                    limit,
                    resetAt,
                    TimeSpan.FromMilliseconds(retryAfterMs));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for client {ClientId}", clientId);

            // Fail open - allow request on error
            return RateLimitResult.Allowed(clientId, tier ?? "unknown", _options.DefaultLimit, DateTime.UtcNow.AddMinutes(1));
        }
    }

    public async Task<RateLimitPolicy?> GetPolicyAsync(string policyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{_options.KeyPrefix}{PolicyKeyPrefix}{policyId}";
            var data = await _db.StringGetAsync(key);

            if (data.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<RateLimitPolicy>(data!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policy {PolicyId}", policyId);
            throw;
        }
    }

    public async Task<RateLimitPolicy?> GetPolicyByNameAsync(string policyName, CancellationToken cancellationToken = default)
    {
        try
        {
            var policies = await GetAllPoliciesAsync(cancellationToken);
            return policies.FirstOrDefault(p => p.Name.Equals(policyName, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policy by name {PolicyName}", policyName);
            throw;
        }
    }

    public async Task<IEnumerable<RateLimitPolicy>> GetAllPoliciesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var policies = new List<RateLimitPolicy>();
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var pattern = $"{_options.KeyPrefix}{PolicyKeyPrefix}*";

            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                var data = await _db.StringGetAsync(key);
                if (!data.IsNullOrEmpty)
                {
                    var policy = JsonSerializer.Deserialize<RateLimitPolicy>(data!);
                    if (policy != null)
                        policies.Add(policy);
                }
            }

            // Add configured policies
            policies.AddRange(_options.Policies.Where(p => !policies.Any(ep => ep.Id == p.Id)));

            return policies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all policies");
            throw;
        }
    }

    public async Task<RateLimitPolicy> CreatePolicyAsync(RateLimitPolicy policy, CancellationToken cancellationToken = default)
    {
        if (policy == null)
            throw new ArgumentNullException(nameof(policy));

        try
        {
            if (string.IsNullOrEmpty(policy.Id))
                policy.Id = Guid.NewGuid().ToString();

            policy.CreatedAt = DateTime.UtcNow;
            policy.UpdatedAt = DateTime.UtcNow;

            var key = $"{_options.KeyPrefix}{PolicyKeyPrefix}{policy.Id}";
            var json = JsonSerializer.Serialize(policy);

            await _db.StringSetAsync(key, json);

            _logger.LogInformation("Created rate limit policy {PolicyId}: {PolicyName}", policy.Id, policy.Name);

            return policy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy {PolicyName}", policy.Name);
            throw;
        }
    }

    public async Task<RateLimitPolicy> UpdatePolicyAsync(RateLimitPolicy policy, CancellationToken cancellationToken = default)
    {
        if (policy == null)
            throw new ArgumentNullException(nameof(policy));

        if (string.IsNullOrEmpty(policy.Id))
            throw new ArgumentException("Policy ID is required for update", nameof(policy));

        try
        {
            var existing = await GetPolicyAsync(policy.Id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"Policy {policy.Id} not found");

            policy.CreatedAt = existing.CreatedAt;
            policy.UpdatedAt = DateTime.UtcNow;

            var key = $"{_options.KeyPrefix}{PolicyKeyPrefix}{policy.Id}";
            var json = JsonSerializer.Serialize(policy);

            await _db.StringSetAsync(key, json);

            _logger.LogInformation("Updated rate limit policy {PolicyId}", policy.Id);

            return policy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating policy {PolicyId}", policy.Id);
            throw;
        }
    }

    public async Task<bool> DeletePolicyAsync(string policyId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(policyId))
            throw new ArgumentNullException(nameof(policyId));

        try
        {
            var key = $"{_options.KeyPrefix}{PolicyKeyPrefix}{policyId}";
            var deleted = await _db.KeyDeleteAsync(key);

            if (deleted)
                _logger.LogInformation("Deleted rate limit policy {PolicyId}", policyId);

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting policy {PolicyId}", policyId);
            throw;
        }
    }

    public async Task<RateLimitStatistics> GetStatisticsAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fromTime = from ?? DateTime.UtcNow.AddHours(-1);
            var toTime = to ?? DateTime.UtcNow;

            var stats = new RateLimitStatistics
            {
                From = fromTime,
                To = toTime
            };

            // Get global stats
            var totalKey = $"{_options.KeyPrefix}{StatsKeyPrefix}total";
            var blockedKey = $"{_options.KeyPrefix}{StatsKeyPrefix}blocked";

            var totalStr = await _db.StringGetAsync(totalKey);
            var blockedStr = await _db.StringGetAsync(blockedKey);

            stats.TotalRequests = totalStr.IsNullOrEmpty ? 0 : (long)totalStr;
            stats.BlockedRequests = blockedStr.IsNullOrEmpty ? 0 : (long)blockedStr;
            stats.AllowedRequests = stats.TotalRequests - stats.BlockedRequests;

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics");
            throw;
        }
    }

    public async Task<ClientStatistics?> GetClientUsageAsync(string clientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentNullException(nameof(clientId));

        try
        {
            var key = $"{_options.KeyPrefix}{CounterKeyPrefix}{clientId}";
            var count = await _db.SortedSetLengthAsync(key);

            if (count == 0)
                return null;

            var limit = _options.DefaultLimit;
            var windowSeconds = _options.DefaultWindowSeconds;

            return new ClientStatistics
            {
                ClientId = clientId,
                CurrentUsage = (int)count,
                MaxAllowed = limit,
                ResetAt = DateTime.UtcNow.AddSeconds(windowSeconds)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client usage for {ClientId}", clientId);
            throw;
        }
    }

    public async Task<bool> ResetClientLimitAsync(string clientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentNullException(nameof(clientId));

        try
        {
            var key = $"{_options.KeyPrefix}{CounterKeyPrefix}{clientId}";
            var deleted = await _db.KeyDeleteAsync(key);

            if (deleted)
                _logger.LogInformation("Reset rate limit for client {ClientId}", clientId);

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting limit for client {ClientId}", clientId);
            throw;
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var pong = await _db.PingAsync();
            return pong.TotalMilliseconds < 1000;
        }
        catch
        {
            return false;
        }
    }

    private int GetLimitForTier(string tier)
    {
        if (UserTiers.DefaultLimits.TryGetValue(tier.ToLowerInvariant(), out var limit))
            return limit;

        return _options.DefaultLimit;
    }

    private async Task<RateLimitPolicy?> GetPolicyForEndpointAsync(string endpoint, CancellationToken cancellationToken)
    {
        var policies = await GetAllPoliciesAsync(cancellationToken);

        foreach (var policy in policies.Where(p => p.Enabled))
        {
            foreach (var policyEndpoint in policy.Endpoints)
            {
                if (endpoint.StartsWith(policyEndpoint, StringComparison.OrdinalIgnoreCase) ||
                    MatchesWildcard(endpoint, policyEndpoint))
                {
                    return policy;
                }
            }
        }

        return null;
    }

    private static bool MatchesWildcard(string text, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return false;

        if (pattern == "*")
            return true;

        if (pattern.EndsWith("*"))
        {
            var prefix = pattern[..^1];
            return text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.StartsWith("*"))
        {
            var suffix = pattern[1..];
            return text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }

        return text.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private async Task RecordStatisticsAsync(string clientId, string endpoint, bool allowed, CancellationToken cancellationToken)
    {
        try
        {
            var totalKey = $"{_options.KeyPrefix}{StatsKeyPrefix}total";
            var blockedKey = $"{_options.KeyPrefix}{StatsKeyPrefix}blocked";

            await _db.StringIncrementAsync(totalKey);

            if (!allowed)
            {
                await _db.StringIncrementAsync(blockedKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error recording statistics");
        }
    }
}
