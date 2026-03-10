using System.Globalization;
using CarDealer.Contracts.Enums;
using ContactService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ContactService.Infrastructure.Services;

/// <summary>
/// Redis-backed conversation usage tracker for plan limit enforcement.
///
/// Redis keys:
///   okla:contact:usage:{dealerId}:{YYYY-MM}              → monthly count
///   okla:contact:usage:{dealerId}:{YYYY-MM}:warning-sent  → 1 if 80% warning sent
///   okla:contact:usage:{dealerId}:{YYYY-MM}:limit-sent    → 1 if limit notification sent
///
/// CONTRA #5 FIX: Enforces 2,000 conversation/month hard limit for ÉLITE dealers.
/// </summary>
public sealed class ConversationUsageTracker : IConversationUsageTracker
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IContactRequestRepository _contactRequestRepo;
    private readonly ILogger<ConversationUsageTracker> _logger;

    private const string KeyPrefix = "okla:contact:usage";

    public ConversationUsageTracker(
        IContactRequestRepository contactRequestRepo,
        ILogger<ConversationUsageTracker> logger,
        IConnectionMultiplexer? redis = null)
    {
        _contactRequestRepo = contactRequestRepo;
        _logger = logger;
        _redis = redis;
    }

    public async Task<ConversationUsageResult> IncrementAndCheckAsync(
        Guid dealerId,
        string dealerPlan,
        CancellationToken ct = default)
    {
        var limits = PlanFeatureLimits.GetLimits(dealerPlan);
        var maxMessages = limits.ChatAgentMonthlyMessages;

        // Plan doesn't have ChatAgent access
        if (maxMessages == 0)
        {
            return new ConversationUsageResult
            {
                CurrentCount = 0,
                MaxAllowed = 0,
                Status = ConversationUsageStatus.NoAccess,
                JustCrossedWarningThreshold = false,
                JustReachedLimit = false,
                OverageCount = 0,
                ProjectedMonthlyTotal = 0
            };
        }

        var monthKey = GetMonthKey();
        var newCount = await IncrementCountAsync(dealerId, monthKey, ct);

        // Calculate status
        var status = maxMessages < 0
            ? ConversationUsageStatus.Normal // truly unlimited (shouldn't happen for Elite post-fix)
            : PlanFeatureLimits.GetConversationStatus(dealerPlan, newCount);

        var warningCount = PlanFeatureLimits.GetConversationWarningCount(dealerPlan);
        var justCrossedWarning = false;
        var justReachedLimit = false;
        var overageCount = maxMessages > 0 && newCount > maxMessages ? newCount - maxMessages : 0;

        // Check if we just crossed the 80% warning threshold
        if (warningCount > 0 && newCount == warningCount)
        {
            justCrossedWarning = await TryMarkThresholdAsync(dealerId, monthKey, "warning-sent", ct);
        }

        // Check if we just hit the hard limit
        if (maxMessages > 0 && newCount == maxMessages)
        {
            justReachedLimit = await TryMarkThresholdAsync(dealerId, monthKey, "limit-sent", ct);
        }

        // Project monthly total based on current usage rate
        var projectedTotal = CalculateProjectedMonthlyTotal(newCount);

        _logger.LogInformation(
            "[ConversationUsageTracker] Dealer {DealerId} plan={Plan} conversations={Count}/{Max} status={Status} overage={Overage}",
            dealerId, dealerPlan, newCount, maxMessages, status, overageCount);

        return new ConversationUsageResult
        {
            CurrentCount = newCount,
            MaxAllowed = maxMessages,
            Status = status,
            JustCrossedWarningThreshold = justCrossedWarning,
            JustReachedLimit = justReachedLimit,
            OverageCount = overageCount,
            ProjectedMonthlyTotal = projectedTotal
        };
    }

    public async Task<int> GetCurrentMonthCountAsync(Guid dealerId, CancellationToken ct = default)
    {
        var monthKey = GetMonthKey();

        if (_redis != null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = BuildKey(dealerId, monthKey);
                var val = await db.StringGetAsync(key);
                if (val.HasValue) return (int)val;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ConversationUsageTracker] Redis read failed, falling back to DB");
            }
        }

        // Fallback: count from database
        return await CountFromDatabaseAsync(dealerId, ct);
    }

    public async Task<int> GetOverageCountAsync(Guid dealerId, string dealerPlan, CancellationToken ct = default)
    {
        var limits = PlanFeatureLimits.GetLimits(dealerPlan);
        var maxMessages = limits.ChatAgentMonthlyMessages;

        if (maxMessages <= 0) return 0; // Unlimited or no access

        var currentCount = await GetCurrentMonthCountAsync(dealerId, ct);
        return currentCount > maxMessages ? currentCount - maxMessages : 0;
    }

    public async Task<bool> IsInBasicModeAsync(Guid dealerId, string dealerPlan, CancellationToken ct = default)
    {
        var limits = PlanFeatureLimits.GetLimits(dealerPlan);
        var maxMessages = limits.ChatAgentMonthlyMessages;

        if (maxMessages <= 0) return maxMessages == 0; // No access = always basic; unlimited = never basic

        var currentCount = await GetCurrentMonthCountAsync(dealerId, ct);
        return currentCount >= maxMessages;
    }

    // ── PRIVATE HELPERS ──────────────────────────────────────────────────

    private async Task<int> IncrementCountAsync(Guid dealerId, string monthKey, CancellationToken ct)
    {
        if (_redis != null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = BuildKey(dealerId, monthKey);
                var newVal = await db.StringIncrementAsync(key);

                // Set expiry on first increment (45 days to cover billing cycle overlap)
                if (newVal == 1)
                {
                    await db.KeyExpireAsync(key, TimeSpan.FromDays(45));
                }

                return (int)newVal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ConversationUsageTracker] Redis increment failed, falling back to DB count");
            }
        }

        // Fallback: count from database (less accurate for race conditions but works)
        return await CountFromDatabaseAsync(dealerId, ct);
    }

    private async Task<bool> TryMarkThresholdAsync(Guid dealerId, string monthKey, string thresholdType, CancellationToken ct)
    {
        if (_redis != null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = $"{BuildKey(dealerId, monthKey)}:{thresholdType}";
                // SetAsync with When.NotExists returns true only if the key didn't exist
                var wasSet = await db.StringSetAsync(key, "1", TimeSpan.FromDays(45), When.NotExists);
                return wasSet;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ConversationUsageTracker] Redis threshold mark failed");
            }
        }

        // Without Redis, always return true (may send duplicate notifications, but that's safer than none)
        return true;
    }

    private async Task<int> CountFromDatabaseAsync(Guid dealerId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

        return await _contactRequestRepo.CountBySellerIdInPeriodAsync(dealerId, startOfMonth, endOfMonth, ct);
    }

    private static int CalculateProjectedMonthlyTotal(int currentCount)
    {
        var now = DateTime.UtcNow;
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        var dayOfMonth = now.Day;

        if (dayOfMonth == 0) return currentCount;

        var dailyRate = (double)currentCount / dayOfMonth;
        return (int)Math.Ceiling(dailyRate * daysInMonth);
    }

    private static string GetMonthKey()
    {
        return DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);
    }

    private static string BuildKey(Guid dealerId, string monthKey)
    {
        return $"{KeyPrefix}:{dealerId:N}:{monthKey}";
    }
}
