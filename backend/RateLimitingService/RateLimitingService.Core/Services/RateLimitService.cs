using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using Microsoft.Extensions.Logging;

namespace RateLimitingService.Core.Services;

/// <summary>
/// Main rate limiting orchestration service
/// </summary>
public class RateLimitService : IRateLimitService
{
    private readonly IRateLimitRuleService _ruleService;
    private readonly IRateLimitStorage _storage;
    private readonly ILogger<RateLimitService> _logger;
    private readonly Dictionary<RateLimitAlgorithm, IRateLimitAlgorithm> _algorithms;
    private readonly IRateLimitViolationRepository? _violationRepository;

    public RateLimitService(
        IRateLimitRuleService ruleService,
        IRateLimitStorage storage,
        ILogger<RateLimitService> logger,
        TokenBucketRateLimiter tokenBucket,
        SlidingWindowRateLimiter slidingWindow,
        FixedWindowRateLimiter fixedWindow,
        LeakyBucketRateLimiter leakyBucket,
        IRateLimitViolationRepository? violationRepository = null)
    {
        _ruleService = ruleService ?? throw new ArgumentNullException(nameof(ruleService));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _violationRepository = violationRepository;

        _algorithms = new Dictionary<RateLimitAlgorithm, IRateLimitAlgorithm>
        {
            { RateLimitAlgorithm.TokenBucket, tokenBucket },
            { RateLimitAlgorithm.SlidingWindow, slidingWindow },
            { RateLimitAlgorithm.FixedWindow, fixedWindow },
            { RateLimitAlgorithm.LeakyBucket, leakyBucket }
        };
    }

    public async Task<RateLimitCheckResult> CheckAsync(RateLimitCheckRequest request)
    {
        try
        {
            // Check whitelist (allow immediately)
            if (await _ruleService.IsWhitelistedAsync(request.Identifier, request.IdentifierType))
            {
                _logger.LogInformation("Identifier {Identifier} is whitelisted", request.Identifier);
                return new RateLimitCheckResult
                {
                    IsAllowed = true,
                    Remaining = int.MaxValue,
                    Limit = int.MaxValue,
                    ResetAt = DateTimeOffset.MaxValue.ToUnixTimeSeconds()
                };
            }

            // Check blacklist
            if (await _ruleService.IsBlacklistedAsync(request.Identifier, request.IdentifierType))
            {
                _logger.LogWarning("Identifier {Identifier} is blacklisted", request.Identifier);
                var violation = new RateLimitViolation
                {
                    Identifier = request.Identifier,
                    IdentifierType = request.IdentifierType,
                    Endpoint = request.Endpoint,
                    ViolatedAt = DateTime.UtcNow,
                    RuleName = "Blacklist"
                };
                await LogViolationAsync(violation);

                return new RateLimitCheckResult
                {
                    IsAllowed = false,
                    Remaining = 0,
                    Limit = 0,
                    ResetAt = DateTimeOffset.MaxValue.ToUnixTimeSeconds(),
                    RetryAfterSeconds = int.MaxValue
                };
            }

            // Get applicable rules (ordered by priority)
            var rules = await _ruleService.GetApplicableRulesAsync(request);
            var applicableRules = rules.Where(r => r.IsEnabled).OrderBy(r => r.Priority).ToList();

            if (!applicableRules.Any())
            {
                _logger.LogDebug("No applicable rules for {Identifier}", request.Identifier);
                return new RateLimitCheckResult
                {
                    IsAllowed = true,
                    Remaining = int.MaxValue,
                    Limit = int.MaxValue,
                    ResetAt = DateTimeOffset.MaxValue.ToUnixTimeSeconds()
                };
            }

            // Check all applicable rules (most restrictive wins)
            RateLimitCheckResult? mostRestrictiveResult = null;

            foreach (var rule in applicableRules)
            {
                var algorithm = _algorithms[rule.Algorithm];
                var key = BuildKey(request, rule);
                var result = await algorithm.CheckAsync(key, rule, request.Cost);

                if (!result.IsAllowed)
                {
                    _logger.LogWarning(
                        "Rate limit exceeded for {Identifier} on rule {RuleId}",
                        request.Identifier,
                        rule.Id);

                    var violation = new RateLimitViolation
                    {
                        Identifier = request.Identifier,
                        IdentifierType = request.IdentifierType,
                        Endpoint = request.Endpoint,
                        RuleId = rule.Id,
                        RuleName = rule.Name,
                        AllowedLimit = rule.Limit,
                        AttemptedRequests = rule.Limit + 1,
                        ViolatedAt = DateTime.UtcNow
                    };
                    await LogViolationAsync(violation);

                    return result;
                }

                // Track most restrictive allowed result
                if (mostRestrictiveResult == null || result.Remaining < mostRestrictiveResult.Remaining)
                {
                    mostRestrictiveResult = result;
                }
            }

            return mostRestrictiveResult ?? new RateLimitCheckResult
            {
                IsAllowed = true,
                Remaining = int.MaxValue,
                Limit = int.MaxValue,
                ResetAt = DateTimeOffset.MaxValue.ToUnixTimeSeconds()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for {Identifier}", request.Identifier);
            // Fail open - allow request on error
            return new RateLimitCheckResult
            {
                IsAllowed = true,
                Remaining = 0,
                Limit = 0,
                ResetAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }
    }

    public async Task ResetAsync(string identifier, RateLimitIdentifierType type)
    {
        _logger.LogInformation("Resetting rate limits for {Identifier}", identifier);

        // Reset across all algorithms (simplified - in production, track keys)
        var request = new RateLimitCheckRequest
        {
            Identifier = identifier,
            IdentifierType = type
        };

        var rules = await _ruleService.GetApplicableRulesAsync(request);

        foreach (var rule in rules)
        {
            var algorithm = _algorithms[rule.Algorithm];
            var key = BuildKey(request, rule);
            await algorithm.ResetAsync(key);
        }
    }

    public async Task<RateLimitCheckResult> GetStatusAsync(
        string identifier,
        RateLimitIdentifierType type,
        string? endpoint = null)
    {
        var request = new RateLimitCheckRequest
        {
            Identifier = identifier,
            IdentifierType = type,
            Endpoint = endpoint
        };

        var rules = await _ruleService.GetApplicableRulesAsync(request);
        var applicableRules = rules.Where(r => r.IsEnabled).OrderBy(r => r.Priority).ToList();

        if (!applicableRules.Any())
        {
            return new RateLimitCheckResult
            {
                IsAllowed = true,
                Remaining = int.MaxValue,
                Limit = int.MaxValue,
                ResetAt = DateTimeOffset.MaxValue.ToUnixTimeSeconds()
            };
        }

        // Get status from most restrictive rule
        RateLimitCheckResult? mostRestrictive = null;

        foreach (var rule in applicableRules)
        {
            var algorithm = _algorithms[rule.Algorithm];
            var key = BuildKey(request, rule);
            var status = await algorithm.GetStatusAsync(key, rule);

            if (mostRestrictive == null || status.Remaining < mostRestrictive.Remaining)
            {
                mostRestrictive = status;
            }
        }

        return mostRestrictive ?? new RateLimitCheckResult
        {
            IsAllowed = true,
            Remaining = int.MaxValue,
            Limit = int.MaxValue,
            ResetAt = DateTimeOffset.MaxValue.ToUnixTimeSeconds()
        };
    }

    public async Task LogViolationAsync(RateLimitViolation violation)
    {
        try
        {
            // Store in PostgreSQL for permanent audit trail
            if (_violationRepository != null)
            {
                await _violationRepository.AddViolationAsync(violation);
            }

            // Store in Redis for recent violations (short TTL)
            var key = $"ratelimit:violations:{violation.Identifier}:{DateTime.UtcNow.Ticks}";
            var json = System.Text.Json.JsonSerializer.Serialize(violation);
            await _storage.SetAsync(key, 1, TimeSpan.FromHours(1));

            _logger.LogWarning(
                "Rate limit violation: {Identifier} ({Type}) - {Reason}",
                violation.Identifier,
                violation.IdentifierType,
                violation.Reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging violation for {Identifier}", violation.Identifier);
        }
    }

    public async Task<IEnumerable<RateLimitViolation>> GetViolationsAsync(int count = 100)
    {
        // Simplified - in production, use Redis scan or PostgreSQL
        await Task.CompletedTask;
        return Enumerable.Empty<RateLimitViolation>();
    }

    public async Task<RateLimitStatistics> GetStatisticsAsync(DateTime? from = null, DateTime? to = null)
    {
        // Simplified - in production, aggregate from Redis/PostgreSQL
        await Task.CompletedTask;
        return new RateLimitStatistics
        {
            TotalRequests = 0,
            AllowedRequests = 0,
            BlockedRequests = 0,
            From = from ?? DateTime.UtcNow.AddHours(-1),
            To = to ?? DateTime.UtcNow,
            ClientStats = new List<ClientStatistics>(),
            EndpointStats = new List<EndpointStatistics>()
        };
    }

    private static string BuildKey(RateLimitCheckRequest request, RateLimitRule rule)
    {
        var parts = new List<string> { rule.Id, request.Identifier };

        if (!string.IsNullOrEmpty(request.Endpoint))
        {
            parts.Add(request.Endpoint);
        }

        return string.Join(":", parts);
    }
}
