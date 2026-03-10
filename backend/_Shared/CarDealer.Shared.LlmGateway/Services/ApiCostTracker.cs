// =============================================================================
// API Cost Tracker — Monthly USD Cost Accumulator
//
// Tracks LLM API spending in real-time using Redis as persistent store.
// Pricing per 1M tokens (as of 2025):
//   Claude Sonnet 4:   Input $3.00 / Output $15.00
//   Claude Haiku 3.5:  Input $0.80 / Output $4.00
//   Gemini 1.5 Flash:  Input $0.075 / Output $0.30
//   Llama 3.1 70B:     Input $0.00 / Output $0.00  (self-hosted)
//
// Thresholds:
//   $300/month = Warning → Admin alert
//   $500/month = Critical → CTO alert + monitoring escalation
//   $700/month = Aggressive cache mode auto-activation
//                (40% Gemini Flash, 20% Llama local, 40% Claude)
//
// Redis keys (rolling 30-day window):
//   okla:llm:cost:monthly:{YYYY-MM}         → total USD for month
//   okla:llm:cost:daily:{YYYY-MM-DD}        → total USD for day
//   okla:llm:cost:by-provider:{provider}:{YYYY-MM} → per-provider USD
//   okla:llm:cost:by-agent:{agent}:{YYYY-MM}       → per-agent USD
//   okla:llm:cost:aggressive-mode-active    → 1 if threshold hit
// =============================================================================

using System.Globalization;
using CarDealer.Contracts.Enums;
using CarDealer.Shared.LlmGateway.Abstractions;
using CarDealer.Shared.LlmGateway.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CarDealer.Shared.LlmGateway.Services;

/// <summary>
/// Interface for the API cost tracking system.
/// </summary>
public interface IApiCostTracker
{
    /// <summary>Record a completed LLM call's cost.</summary>
    Task RecordCostAsync(LlmResponse response, string callerAgent, Guid? dealerId = null, CancellationToken ct = default);

    /// <summary>Get the current month's accumulated cost in USD.</summary>
    Task<decimal> GetMonthlyCostAsync(CancellationToken ct = default);

    /// <summary>Get today's accumulated cost in USD.</summary>
    Task<decimal> GetDailyCostAsync(CancellationToken ct = default);

    /// <summary>Get a full cost breakdown for the current month.</summary>
    Task<CostBreakdown> GetCostBreakdownAsync(CancellationToken ct = default);

    /// <summary>Whether aggressive cache mode is currently active (auto-triggered at $700).</summary>
    Task<bool> IsAggressiveCacheModeActiveAsync(CancellationToken ct = default);

    /// <summary>Manually toggle aggressive cache mode.</summary>
    Task SetAggressiveCacheModeAsync(bool active, CancellationToken ct = default);

    /// <summary>Get daily cost history for the last N days (for chart/trending).</summary>
    Task<IReadOnlyList<DailyCostEntry>> GetDailyCostHistoryAsync(int days = 30, CancellationToken ct = default);

    /// <summary>Get the current month's cost for a specific dealer in USD.</summary>
    Task<decimal> GetDealerMonthlyCostAsync(Guid dealerId, CancellationToken ct = default);

    /// <summary>Get the current month's daily cost for a specific dealer in USD.</summary>
    Task<decimal> GetDealerDailyCostAsync(Guid dealerId, CancellationToken ct = default);

    /// <summary>
    /// Get the average cost per conversation for a specific dealer in the current month.
    /// Used to verify that degradation zone keeps cost ≤ $0.04/conversation.
    /// </summary>
    Task<decimal> GetDealerCostPerConversationAsync(Guid dealerId, int conversationCount, CancellationToken ct = default);
}

/// <summary>
/// Redis-backed API cost tracker with threshold monitoring.
/// </summary>
public sealed class ApiCostTracker : IApiCostTracker
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly LlmGatewayOptions _options;
    private readonly ILogger<ApiCostTracker> _logger;

    // In-memory fast accumulator (synced to Redis periodically)
    private static decimal _monthlyCostAccumulator;
    private static readonly object _lock = new();
    private static string _currentMonth = DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);
    private static bool _warningFired;
    private static bool _criticalFired;
    private static bool _aggressiveModeFired;

    public ApiCostTracker(
        IOptions<LlmGatewayOptions> options,
        ILogger<ApiCostTracker> logger,
        IConnectionMultiplexer? redis = null)
    {
        _options = options.Value;
        _logger = logger;
        _redis = redis;
    }

    public async Task RecordCostAsync(LlmResponse response, string callerAgent, Guid? dealerId = null, CancellationToken ct = default)
    {
        // Skip cache/fallback responses (no real API cost)
        if (response.FromCache || response.Provider == LlmProviderType.Cache)
            return;

        var cost = CalculateCost(response.Provider, response.ModelId, response.InputTokens, response.OutputTokens);
        if (cost <= 0m)
            return;

        var now = DateTime.UtcNow;
        var monthKey = now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        var dayKey = now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        // Reset in-memory accumulator on month rollover
        lock (_lock)
        {
            if (monthKey != _currentMonth)
            {
                _currentMonth = monthKey;
                _monthlyCostAccumulator = 0m;
                _warningFired = false;
                _criticalFired = false;
                _aggressiveModeFired = false;
            }
            _monthlyCostAccumulator += cost;
        }

        // Persist to Redis (non-blocking, fire-and-forget for performance)
        if (_redis != null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var costStr = ((double)cost).ToString("F8", CultureInfo.InvariantCulture);
                var expiry = TimeSpan.FromDays(90); // Keep 3 months of history

                var batch = db.CreateBatch();

                // Monthly total
                _ = batch.StringIncrementAsync($"okla:llm:cost:monthly:{monthKey}", (double)cost);
                _ = batch.KeyExpireAsync($"okla:llm:cost:monthly:{monthKey}", expiry);

                // Daily total
                _ = batch.StringIncrementAsync($"okla:llm:cost:daily:{dayKey}", (double)cost);
                _ = batch.KeyExpireAsync($"okla:llm:cost:daily:{dayKey}", TimeSpan.FromDays(60));

                // Per-provider
                var providerName = response.Provider.ToString().ToLowerInvariant();
                _ = batch.StringIncrementAsync($"okla:llm:cost:by-provider:{providerName}:{monthKey}", (double)cost);
                _ = batch.KeyExpireAsync($"okla:llm:cost:by-provider:{providerName}:{monthKey}", expiry);

                // Per-agent
                var agentName = callerAgent.ToLowerInvariant();
                _ = batch.StringIncrementAsync($"okla:llm:cost:by-agent:{agentName}:{monthKey}", (double)cost);
                _ = batch.KeyExpireAsync($"okla:llm:cost:by-agent:{agentName}:{monthKey}", expiry);

                // ── CONTRA #5 FIX: Per-dealer cost tracking ──────────────────
                if (dealerId.HasValue && dealerId.Value != Guid.Empty)
                {
                    var dealerIdStr = dealerId.Value.ToString("N");
                    _ = batch.StringIncrementAsync($"okla:llm:cost:by-dealer:{dealerIdStr}:{monthKey}", (double)cost);
                    _ = batch.KeyExpireAsync($"okla:llm:cost:by-dealer:{dealerIdStr}:{monthKey}", expiry);
                    _ = batch.StringIncrementAsync($"okla:llm:cost:by-dealer:{dealerIdStr}:daily:{dayKey}", (double)cost);
                    _ = batch.KeyExpireAsync($"okla:llm:cost:by-dealer:{dealerIdStr}:daily:{dayKey}", TimeSpan.FromDays(60));
                }

                batch.Execute();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CostTracker] Redis write failed — in-memory tracking continues");
            }
        }

        // ── THRESHOLD CHECKS ─────────────────────────────────────────────────
        await CheckThresholdsAsync(cost, callerAgent, ct);

        // ── CONTRA #5 FIX: Per-dealer ÉLITE cost threshold check ($179.10) ──
        if (dealerId.HasValue && dealerId.Value != Guid.Empty)
        {
            await CheckDealerCostThresholdAsync(dealerId.Value, cost, callerAgent, ct);
        }
    }

    public async Task<decimal> GetMonthlyCostAsync(CancellationToken ct = default)
    {
        if (_redis == null)
        {
            lock (_lock) { return _monthlyCostAccumulator; }
        }

        try
        {
            var db = _redis.GetDatabase();
            var monthKey = DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            var val = await db.StringGetAsync($"okla:llm:cost:monthly:{monthKey}");
            return val.HasValue ? (decimal)(double)val : 0m;
        }
        catch
        {
            lock (_lock) { return _monthlyCostAccumulator; }
        }
    }

    public async Task<decimal> GetDailyCostAsync(CancellationToken ct = default)
    {
        if (_redis == null) return 0m;

        try
        {
            var db = _redis.GetDatabase();
            var dayKey = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var val = await db.StringGetAsync($"okla:llm:cost:daily:{dayKey}");
            return val.HasValue ? (decimal)(double)val : 0m;
        }
        catch
        {
            return 0m;
        }
    }

    public async Task<CostBreakdown> GetCostBreakdownAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var monthKey = now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        var dayKey = now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var breakdown = new CostBreakdown
        {
            Month = monthKey,
            MonthlyTotalUsd = await GetMonthlyCostAsync(ct),
            DailyTotalUsd = await GetDailyCostAsync(ct),
            Thresholds = new CostThresholds
            {
                WarningUsd = _options.CostAlerts.WarningThresholdUsd,
                CriticalUsd = _options.CostAlerts.CriticalThresholdUsd,
                AggressiveCacheUsd = _options.CostAlerts.AggressiveCacheThresholdUsd
            },
            IsAggressiveCacheModeActive = await IsAggressiveCacheModeActiveAsync(ct),
            GeneratedAt = DateTimeOffset.UtcNow
        };

        // Per-provider breakdown from Redis
        if (_redis != null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var providers = new[] { "claude", "gemini", "llama" };

                foreach (var provider in providers)
                {
                    var val = await db.StringGetAsync($"okla:llm:cost:by-provider:{provider}:{monthKey}");
                    if (val.HasValue)
                        breakdown.ByProvider[provider] = (decimal)(double)val;
                }

                // Top agents by cost
                var server = _redis.GetServer(_redis.GetEndPoints()[0]);
                var agentKeys = server.Keys(pattern: $"okla:llm:cost:by-agent:*:{monthKey}");

                foreach (var key in agentKeys)
                {
                    var agentName = key.ToString()
                        .Replace($"okla:llm:cost:by-agent:", "")
                        .Replace($":{monthKey}", "");
                    var val = await db.StringGetAsync(key);
                    if (val.HasValue)
                        breakdown.ByAgent[agentName] = (decimal)(double)val;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CostTracker] Failed to read breakdown from Redis");
            }
        }

        // Calculate projected monthly cost (extrapolate from current daily rate)
        var dayOfMonth = now.Day;
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        if (dayOfMonth > 0 && breakdown.MonthlyTotalUsd > 0)
        {
            breakdown.ProjectedMonthlyUsd = breakdown.MonthlyTotalUsd / dayOfMonth * daysInMonth;
        }

        return breakdown;
    }

    public async Task<bool> IsAggressiveCacheModeActiveAsync(CancellationToken ct = default)
    {
        if (_redis == null)
        {
            lock (_lock) { return _aggressiveModeFired; }
        }

        try
        {
            var db = _redis.GetDatabase();
            var val = await db.StringGetAsync("okla:llm:cost:aggressive-mode-active");
            return val.HasValue && val == "1";
        }
        catch
        {
            lock (_lock) { return _aggressiveModeFired; }
        }
    }

    public async Task SetAggressiveCacheModeAsync(bool active, CancellationToken ct = default)
    {
        lock (_lock) { _aggressiveModeFired = active; }

        if (_redis == null) return;

        try
        {
            var db = _redis.GetDatabase();
            if (active)
                await db.StringSetAsync("okla:llm:cost:aggressive-mode-active", "1");
            else
                await db.KeyDeleteAsync("okla:llm:cost:aggressive-mode-active");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CostTracker] Failed to update aggressive mode flag in Redis");
        }
    }

    public async Task<IReadOnlyList<DailyCostEntry>> GetDailyCostHistoryAsync(int days = 30, CancellationToken ct = default)
    {
        var entries = new List<DailyCostEntry>();
        var today = DateTime.UtcNow.Date;

        if (_redis == null)
            return entries.AsReadOnly();

        try
        {
            var db = _redis.GetDatabase();
            for (var i = days - 1; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dayKey = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var val = await db.StringGetAsync($"okla:llm:cost:daily:{dayKey}");
                entries.Add(new DailyCostEntry
                {
                    Date = dayKey,
                    CostUsd = val.HasValue ? (decimal)(double)val : 0m
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CostTracker] Failed to read daily history from Redis");
        }

        return entries.AsReadOnly();
    }

    // =========================================================================
    // PER-DEALER COST QUERIES — CONTRA #5 FIX
    // =========================================================================

    public async Task<decimal> GetDealerMonthlyCostAsync(Guid dealerId, CancellationToken ct = default)
    {
        if (_redis == null) return 0m;

        try
        {
            var db = _redis.GetDatabase();
            var monthKey = DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            var dealerIdStr = dealerId.ToString("N");
            var val = await db.StringGetAsync($"okla:llm:cost:by-dealer:{dealerIdStr}:{monthKey}");
            return val.HasValue ? (decimal)(double)val : 0m;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CostTracker] Failed to read dealer monthly cost for {DealerId}", dealerId);
            return 0m;
        }
    }

    public async Task<decimal> GetDealerDailyCostAsync(Guid dealerId, CancellationToken ct = default)
    {
        if (_redis == null) return 0m;

        try
        {
            var db = _redis.GetDatabase();
            var dayKey = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var dealerIdStr = dealerId.ToString("N");
            var val = await db.StringGetAsync($"okla:llm:cost:by-dealer:{dealerIdStr}:daily:{dayKey}");
            return val.HasValue ? (decimal)(double)val : 0m;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CostTracker] Failed to read dealer daily cost for {DealerId}", dealerId);
            return 0m;
        }
    }

    /// <summary>
    /// Get the average cost per conversation for a dealer in the current month.
    /// Used to measure whether degradation zone keeps cost ≤ $0.04/conversation.
    ///
    /// CONTRA #5 FIX: Degradation zone cost tracking.
    /// </summary>
    public async Task<decimal> GetDealerCostPerConversationAsync(
        Guid dealerId, int conversationCount, CancellationToken ct = default)
    {
        if (conversationCount <= 0) return 0m;

        var monthlyCost = await GetDealerMonthlyCostAsync(dealerId, ct);
        return monthlyCost / conversationCount;
    }

    /// <summary>
    /// Per-dealer ÉLITE cost threshold check.
    /// When a dealer's projected monthly API cost exceeds $179.10 (90% of $199 plan revenue),
    /// an internal alert is fired so the team can review the case before month-end.
    ///
    /// CONTRA #5 FIX: EliteCostAlertThreshold enforcement.
    /// </summary>
    private async Task CheckDealerCostThresholdAsync(
        Guid dealerId, decimal costJustAdded, string callerAgent, CancellationToken ct)
    {
        if (_redis == null) return;

        try
        {
            var db = _redis.GetDatabase();
            var monthKey = DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            var dealerIdStr = dealerId.ToString("N");
            var key = $"okla:llm:cost:by-dealer:{dealerIdStr}:{monthKey}";

            var val = await db.StringGetAsync(key);
            if (!val.HasValue) return;

            var dealerMonthlyCost = (decimal)(double)val;

            // Project monthly cost
            var dayOfMonth = DateTime.UtcNow.Day;
            var daysInMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
            var projectedMonthlyCost = dayOfMonth > 0
                ? dealerMonthlyCost / dayOfMonth * daysInMonth
                : dealerMonthlyCost;

            // Check against EliteCostAlertThreshold ($179.10 = 90% of $199 revenue)
            if (projectedMonthlyCost >= PlanFeatureLimits.EliteCostAlertThreshold)
            {
                // Only fire once per dealer per month
                var alertKey = $"okla:llm:cost:by-dealer:{dealerIdStr}:{monthKey}:elite-alert-sent";
                var wasSet = await db.StringSetAsync(alertKey, "1", TimeSpan.FromDays(45), When.NotExists);

                if (wasSet)
                {
                    _logger.LogCritical(
                        "[CostTracker] 🔴 ELITE DEALER COST ALERT — Dealer {DealerId} projected monthly API cost " +
                        "${Projected:F2} exceeds ${Threshold} (90% of $199 plan revenue). " +
                        "Actual so far: ${Actual:F2}. Agent={Agent}. " +
                        "ACTION REQUIRED: Review dealer usage before month-end billing.",
                        dealerId, projectedMonthlyCost,
                        PlanFeatureLimits.EliteCostAlertThreshold,
                        dealerMonthlyCost, callerAgent);
                }
            }

            // ── DEGRADATION ZONE COST MONITORING ─────────────────────────────
            // Track per-dealer request count to compute cost per conversation.
            // Increment the per-dealer request count for cost-per-conversation calculation.
            var reqCountKey = $"okla:llm:cost:by-dealer:{dealerIdStr}:{monthKey}:request-count";
            await db.StringIncrementAsync(reqCountKey);
            await db.KeyExpireAsync(reqCountKey, TimeSpan.FromDays(90));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "[CostTracker] Failed to check dealer cost threshold for {DealerId}", dealerId);
        }
    }

    // =========================================================================
    // COST CALCULATION — Per-Model Pricing
    // =========================================================================

    /// <summary>
    /// Calculate the USD cost of an LLM call based on provider, model, and token counts.
    /// Pricing as of January 2025.
    /// </summary>
    internal static decimal CalculateCost(LlmProviderType provider, string modelId, int inputTokens, int outputTokens)
    {
        var (inputPricePer1M, outputPricePer1M) = GetPricing(provider, modelId);
        var inputCost = inputTokens / 1_000_000m * inputPricePer1M;
        var outputCost = outputTokens / 1_000_000m * outputPricePer1M;
        return inputCost + outputCost;
    }

    /// <summary>
    /// Get pricing per 1M tokens for a given provider + model combination.
    /// </summary>
    internal static (decimal InputPer1M, decimal OutputPer1M) GetPricing(LlmProviderType provider, string modelId)
    {
        // Normalize model name for matching
        var model = (modelId ?? "").ToLowerInvariant();

        return provider switch
        {
            LlmProviderType.Claude => model switch
            {
                _ when model.Contains("haiku") => (0.80m, 4.00m),     // Claude Haiku 3.5
                _ when model.Contains("sonnet") => (3.00m, 15.00m),   // Claude Sonnet 4
                _ when model.Contains("opus") => (15.00m, 75.00m),    // Claude Opus 4
                _ => (3.00m, 15.00m)                                   // Default to Sonnet pricing
            },
            LlmProviderType.Gemini => model switch
            {
                _ when model.Contains("flash") => (0.075m, 0.30m),    // Gemini 1.5 Flash
                _ when model.Contains("pro") => (1.25m, 5.00m),       // Gemini 1.5 Pro
                _ => (0.075m, 0.30m)                                   // Default to Flash pricing
            },
            LlmProviderType.Llama => (0.00m, 0.00m),                  // Self-hosted = free
            LlmProviderType.Cache => (0.00m, 0.00m),                  // No cost
            _ => (0.00m, 0.00m)
        };
    }

    // =========================================================================
    // THRESHOLD CHECKS — Warning / Critical / Aggressive Cache Mode
    // =========================================================================

    private async Task CheckThresholdsAsync(decimal costJustAdded, string callerAgent, CancellationToken ct)
    {
        decimal currentMonthly;
        lock (_lock) { currentMonthly = _monthlyCostAccumulator; }

        var thresholds = _options.CostAlerts;

        // ── $700 — AGGRESSIVE CACHE MODE ─────────────────────────────────────
        if (currentMonthly >= thresholds.AggressiveCacheThresholdUsd && !_aggressiveModeFired)
        {
            _aggressiveModeFired = true;
            await SetAggressiveCacheModeAsync(true, ct);

            _logger.LogCritical(
                "[CostTracker] 🚨 AGGRESSIVE CACHE MODE ACTIVATED — Monthly cost ${Cost:F2} exceeded ${Threshold}/month. " +
                "Traffic split: {ClaudePct}% Claude, {GeminiPct}% Gemini Flash, {LlamaPct}% Llama local. " +
                "Triggered by agent={Agent}",
                currentMonthly,
                thresholds.AggressiveCacheThresholdUsd,
                thresholds.AggressiveModeClaudePercent,
                thresholds.AggressiveModeGeminiPercent,
                thresholds.AggressiveModeLlamaPercent,
                callerAgent);
        }

        // ── $500 — CRITICAL ALERT (CTO) ─────────────────────────────────────
        if (currentMonthly >= thresholds.CriticalThresholdUsd && !_criticalFired)
        {
            _criticalFired = true;

            _logger.LogCritical(
                "[CostTracker] 🔴 CRITICAL: Monthly LLM cost ${Cost:F2} exceeded ${Threshold}/month. " +
                "CTO alert dispatched. Aggressive cache mode at ${AggressiveThreshold}. " +
                "Agent={Agent}, DailyCost=${DailyCost:F2}",
                currentMonthly,
                thresholds.CriticalThresholdUsd,
                thresholds.AggressiveCacheThresholdUsd,
                callerAgent,
                costJustAdded);
        }

        // ── $300 — WARNING ALERT (Admin) ─────────────────────────────────────
        if (currentMonthly >= thresholds.WarningThresholdUsd && !_warningFired)
        {
            _warningFired = true;

            _logger.LogWarning(
                "[CostTracker] ⚠️ WARNING: Monthly LLM cost ${Cost:F2} exceeded ${Threshold}/month warning threshold. " +
                "Projected monthly: ${Projected:F2}. Agent={Agent}",
                currentMonthly,
                thresholds.WarningThresholdUsd,
                currentMonthly / DateTime.UtcNow.Day * DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month),
                callerAgent);
        }
    }
}

// =============================================================================
// COST BREAKDOWN DTOs
// =============================================================================

/// <summary>
/// Full cost breakdown for the admin dashboard.
/// </summary>
public sealed class CostBreakdown
{
    /// <summary>Month key (YYYY-MM).</summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>Total cost this month in USD.</summary>
    public decimal MonthlyTotalUsd { get; set; }

    /// <summary>Total cost today in USD.</summary>
    public decimal DailyTotalUsd { get; set; }

    /// <summary>Projected cost for the full month based on daily rate.</summary>
    public decimal ProjectedMonthlyUsd { get; set; }

    /// <summary>Configured thresholds.</summary>
    public CostThresholds Thresholds { get; set; } = new();

    /// <summary>Whether aggressive cache mode is currently active.</summary>
    public bool IsAggressiveCacheModeActive { get; set; }

    /// <summary>Cost per provider (claude, gemini, llama).</summary>
    public Dictionary<string, decimal> ByProvider { get; set; } = new();

    /// <summary>Cost per agent.</summary>
    public Dictionary<string, decimal> ByAgent { get; set; } = new();

    /// <summary>When this breakdown was generated.</summary>
    public DateTimeOffset GeneratedAt { get; set; }
}

/// <summary>
/// Alert thresholds for the cost monitoring system.
/// </summary>
public sealed class CostThresholds
{
    public decimal WarningUsd { get; set; }
    public decimal CriticalUsd { get; set; }
    public decimal AggressiveCacheUsd { get; set; }
}

/// <summary>
/// A single day's cost entry for trending/charts.
/// </summary>
public sealed class DailyCostEntry
{
    /// <summary>Date in YYYY-MM-DD format.</summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>Total cost for that day in USD.</summary>
    public decimal CostUsd { get; set; }
}
