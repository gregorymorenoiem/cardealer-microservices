using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using Microsoft.Extensions.Logging;

namespace RateLimitingService.Core.Services;

/// <summary>
/// In-memory rule management service (should be replaced with PostgreSQL in production)
/// </summary>
public class RateLimitRuleService : IRateLimitRuleService
{
    private readonly ILogger<RateLimitRuleService> _logger;
    private readonly Dictionary<string, RateLimitRule> _rules;
    private readonly HashSet<string> _whitelist;
    private readonly HashSet<string> _blacklist;

    public RateLimitRuleService(ILogger<RateLimitRuleService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rules = new Dictionary<string, RateLimitRule>();
        _whitelist = new HashSet<string>();
        _blacklist = new HashSet<string>();

        // Initialize with default rules
        InitializeDefaultRules();
    }

    public Task<IEnumerable<RateLimitRule>> GetAllRulesAsync()
    {
        return Task.FromResult(_rules.Values.AsEnumerable());
    }

    public Task<RateLimitRule?> GetRuleByIdAsync(string id)
    {
        _rules.TryGetValue(id, out var rule);
        return Task.FromResult(rule);
    }

    public Task<RateLimitRule> CreateRuleAsync(RateLimitRule rule)
    {
        if (string.IsNullOrEmpty(rule.Id))
        {
            rule.Id = Guid.NewGuid().ToString();
        }

        _rules[rule.Id] = rule;
        _logger.LogInformation("Created rate limit rule {RuleId}", rule.Id);

        return Task.FromResult(rule);
    }

    public Task<RateLimitRule> UpdateRuleAsync(RateLimitRule rule)
    {
        if (!_rules.ContainsKey(rule.Id))
        {
            throw new KeyNotFoundException($"Rule {rule.Id} not found");
        }

        _rules[rule.Id] = rule;
        _logger.LogInformation("Updated rate limit rule {RuleId}", rule.Id);

        return Task.FromResult(rule);
    }

    public Task DeleteRuleAsync(string id)
    {
        _rules.Remove(id);
        _logger.LogInformation("Deleted rate limit rule {RuleId}", id);

        return Task.CompletedTask;
    }

    public Task<IEnumerable<RateLimitRule>> GetApplicableRulesAsync(RateLimitCheckRequest request)
    {
        var applicable = _rules.Values.Where(rule =>
        {
            // Check if identifier type matches
            if (rule.IdentifierType != request.IdentifierType &&
                rule.IdentifierType != RateLimitIdentifierType.Global)
            {
                return false;
            }

            // Check if endpoint matches (if specified in rule)
            if (!string.IsNullOrEmpty(rule.EndpointPattern) &&
                !string.IsNullOrEmpty(request.Endpoint))
            {
                if (!MatchesPattern(request.Endpoint, rule.EndpointPattern))
                {
                    return false;
                }
            }

            return true;
        }).OrderBy(r => r.Priority);

        return Task.FromResult<IEnumerable<RateLimitRule>>(applicable);
    }

    public Task<bool> IsWhitelistedAsync(string identifier, RateLimitIdentifierType type)
    {
        var key = $"{type}:{identifier}";
        return Task.FromResult(_whitelist.Contains(key));
    }

    public Task<bool> IsBlacklistedAsync(string identifier, RateLimitIdentifierType type)
    {
        var key = $"{type}:{identifier}";
        return Task.FromResult(_blacklist.Contains(key));
    }

    private void InitializeDefaultRules()
    {
        // Global rule - 1000 requests per minute
        _rules["global"] = new RateLimitRule
        {
            Id = "global",
            Name = "Global Rate Limit",
            IdentifierType = RateLimitIdentifierType.Global,
            Algorithm = RateLimitAlgorithm.SlidingWindow,
            Limit = 1000,
            WindowSeconds = 60,
            Priority = 100,
            IsEnabled = true
        };

        // Per IP - 100 requests per minute
        _rules["per-ip"] = new RateLimitRule
        {
            Id = "per-ip",
            Name = "Per IP Rate Limit",
            IdentifierType = RateLimitIdentifierType.IpAddress,
            Algorithm = RateLimitAlgorithm.TokenBucket,
            Limit = 100,
            WindowSeconds = 60,
            Priority = 50,
            IsEnabled = true
        };

        // Per User - 200 requests per minute
        _rules["per-user"] = new RateLimitRule
        {
            Id = "per-user",
            Name = "Per User Rate Limit",
            IdentifierType = RateLimitIdentifierType.UserId,
            Algorithm = RateLimitAlgorithm.SlidingWindow,
            Limit = 200,
            WindowSeconds = 60,
            Priority = 40,
            IsEnabled = true
        };

        // Per API Key - 500 requests per minute
        _rules["per-apikey"] = new RateLimitRule
        {
            Id = "per-apikey",
            Name = "Per API Key Rate Limit",
            IdentifierType = RateLimitIdentifierType.ApiKey,
            Algorithm = RateLimitAlgorithm.FixedWindow,
            Limit = 500,
            WindowSeconds = 60,
            Priority = 30,
            IsEnabled = true
        };

        _logger.LogInformation("Initialized {Count} default rate limit rules", _rules.Count);
    }

    private static bool MatchesPattern(string endpoint, string pattern)
    {
        // Simple pattern matching (support wildcards)
        if (pattern == "*")
        {
            return true;
        }

        if (pattern.Contains("*"))
        {
            var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*") + "$";
            return System.Text.RegularExpressions.Regex.IsMatch(
                endpoint,
                regexPattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return string.Equals(endpoint, pattern, StringComparison.OrdinalIgnoreCase);
    }
}
