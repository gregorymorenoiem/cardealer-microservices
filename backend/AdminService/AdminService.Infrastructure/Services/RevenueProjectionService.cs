using System.Globalization;
using AdminService.Application.Interfaces;
using AdminService.Application.Services;
using AdminService.Application.UseCases.Dealers;
using CarDealer.Contracts.Enums;
using Microsoft.Extensions.Logging;

namespace AdminService.Infrastructure.Services;

// ═══════════════════════════════════════════════════════════════════════════════
// REVENUE PROJECTION SERVICE — CONTRA #7 FIX
//
// Computes projected monthly revenue and evaluates against OPEX threshold.
//
// Revenue model:
//   MRR = (Visible × $29) + (Pro × $89) + (Elite × $199)
//   AdditionalRevenue = Overage billing + Advertising
//   TotalRevenue = MRR + AdditionalRevenue
//
// Projection formula:
//   DailyRate = AccumulatedRevenue / DaysElapsed
//   ProjectedMonthly = DailyRate × DaysInMonth
//
// Alert triggers:
//   - After day 5 (min sample size for reliable projection)
//   - Before day 15: ProjectedRevenue < (OPEX × 1.15 safety margin)
//   - After day 15: ProjectedRevenue < OPEX
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class RevenueProjectionService : IRevenueProjectionService
{
    private readonly IFinancialDataProvider _financialDataProvider;
    private readonly IDealerService _dealerService;
    private readonly ILogger<RevenueProjectionService> _logger;

    // Plan pricing — same as FinancialDashboardHandlers
    private static readonly Dictionary<string, decimal> PlanPrices = new()
    {
        ["libre"] = 0m,
        ["visible"] = 29m,
        ["pro"] = 89m,
        ["elite"] = 199m,
    };

    public RevenueProjectionService(
        IFinancialDataProvider financialDataProvider,
        IDealerService dealerService,
        ILogger<RevenueProjectionService> logger)
    {
        _financialDataProvider = financialDataProvider;
        _dealerService = dealerService;
        _logger = logger;
    }

    public async Task<RevenueProjectionResult> EvaluateCurrentMonthAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var period = now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        var dayOfMonth = now.Day;
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        var daysRemaining = daysInMonth - dayOfMonth;

        _logger.LogInformation(
            "[RevenueProjection] Evaluating {Period} — Day {Day}/{Total}, {Remaining} days remaining",
            period, dayOfMonth, daysInMonth, daysRemaining);

        var result = new RevenueProjectionResult
        {
            Period = period,
            DayOfMonth = dayOfMonth,
            DaysInMonth = daysInMonth,
            DaysRemaining = daysRemaining,
            OpexThreshold = PlanFeatureLimits.MonthlyOpexThreshold,
        };

        // ── STEP 1: Get dealer counts and calculate MRR ─────────────────
        var dealerStats = await SafeGetDealerStats(ct);
        var dealersByPlan = new Dictionary<string, int>
        {
            ["libre"] = dealerStats.ByPlan.Libre,
            ["visible"] = dealerStats.ByPlan.Visible,
            ["pro"] = dealerStats.ByPlan.Pro,
            ["elite"] = dealerStats.ByPlan.Elite,
        };
        result.DealersByPlan = dealersByPlan;

        var mrr = dealersByPlan.Sum(kvp =>
            PlanPrices.TryGetValue(kvp.Key, out var price) ? kvp.Value * price : 0m);
        result.CurrentMrr = mrr;

        // ── STEP 2: Get additional revenue (overage + ads) ──────────────
        var overageRevenue = await SafeGetOverageRevenue(period, ct);
        var adRevenue = await SafeGetAdRevenue(period, ct);
        result.AdditionalRevenue = overageRevenue + adRevenue;

        // ── STEP 3: Calculate accumulated and projected revenue ─────────
        var accumulatedRevenue = mrr + result.AdditionalRevenue;
        result.AccumulatedRevenue = accumulatedRevenue;

        // Project to end of month using daily average
        if (dayOfMonth > 0)
        {
            result.DailyRevenueRate = Math.Round(accumulatedRevenue / dayOfMonth, 2);
            result.ProjectedMonthlyRevenue = Math.Round(result.DailyRevenueRate * daysInMonth, 2);
        }

        // Calculate what's needed daily to reach OPEX threshold
        if (daysRemaining > 0)
        {
            var revenueNeeded = PlanFeatureLimits.MonthlyOpexThreshold - accumulatedRevenue;
            result.RequiredDailyRevenue = revenueNeeded > 0
                ? Math.Round(revenueNeeded / daysRemaining, 2)
                : 0m;
        }

        // ── STEP 4: Evaluate alert condition ────────────────────────────
        result.Shortfall = Math.Max(0, PlanFeatureLimits.MonthlyOpexThreshold - result.ProjectedMonthlyRevenue);
        result.ShortfallPercent = PlanFeatureLimits.MonthlyOpexThreshold > 0
            ? Math.Round(result.Shortfall / PlanFeatureLimits.MonthlyOpexThreshold, 4)
            : 0m;

        // Don't alert before minimum day (too volatile)
        if (dayOfMonth < PlanFeatureLimits.RevenueAlertMinDayOfMonth)
        {
            result.ShouldAlert = false;
            result.Severity = "Info";
            result.SuggestedAction = $"Projection available after day {PlanFeatureLimits.RevenueAlertMinDayOfMonth} of the month.";

            _logger.LogDebug("[RevenueProjection] Day {Day} < MinDay {Min}, skipping alert evaluation",
                dayOfMonth, PlanFeatureLimits.RevenueAlertMinDayOfMonth);
            return result;
        }

        // Apply safety margin for early-month projections
        var effectiveThreshold = dayOfMonth <= 15
            ? PlanFeatureLimits.MonthlyOpexThreshold * PlanFeatureLimits.RevenueAlertEarlyMonthSafetyMargin
            : PlanFeatureLimits.MonthlyOpexThreshold;

        result.ShouldAlert = result.ProjectedMonthlyRevenue < effectiveThreshold;

        // Determine severity
        if (result.ProjectedMonthlyRevenue < PlanFeatureLimits.MonthlyOpexThreshold * 0.9m)
        {
            result.Severity = "Critical";
        }
        else if (result.ProjectedMonthlyRevenue < PlanFeatureLimits.MonthlyOpexThreshold)
        {
            result.Severity = "Warning";
        }
        else
        {
            result.Severity = "Info";
        }

        // Generate suggested action
        result.SuggestedAction = GenerateSuggestedAction(result);

        _logger.LogInformation(
            "[RevenueProjection] Period={Period} | Accumulated=${Accumulated:F2} | " +
            "Projected=${Projected:F2} | OPEX=${Opex:F2} | Shortfall=${Shortfall:F2} | " +
            "Alert={ShouldAlert} | Severity={Severity}",
            period, accumulatedRevenue, result.ProjectedMonthlyRevenue,
            PlanFeatureLimits.MonthlyOpexThreshold, result.Shortfall,
            result.ShouldAlert, result.Severity);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static string GenerateSuggestedAction(RevenueProjectionResult result)
    {
        if (!result.ShouldAlert)
            return "Revenue on track. No action needed.";

        var shortfall = result.Shortfall;
        var suggestions = new List<string>();

        // Calculate how many conversions of each type would close the gap
        var freeToVisible = (int)Math.Ceiling(shortfall / 29m);
        var visibleToPro = (int)Math.Ceiling(shortfall / (89m - 29m));
        var proToElite = (int)Math.Ceiling(shortfall / (199m - 89m));
        var freeToElite = (int)Math.Ceiling(shortfall / 199m);

        var freeAvailable = result.DealersByPlan.GetValueOrDefault("libre", 0);
        var visibleAvailable = result.DealersByPlan.GetValueOrDefault("visible", 0);
        var proAvailable = result.DealersByPlan.GetValueOrDefault("pro", 0);

        if (freeAvailable > 0 && freeToVisible <= freeAvailable)
            suggestions.Add($"Convert {freeToVisible} Free→Visible ($29/mo each)");
        if (visibleAvailable > 0 && visibleToPro <= visibleAvailable)
            suggestions.Add($"Upgrade {visibleToPro} Visible→Pro (+$60/mo each)");
        if (proAvailable > 0 && proToElite <= proAvailable)
            suggestions.Add($"Upgrade {proToElite} Pro→Elite (+$110/mo each)");
        if (freeAvailable > 0)
            suggestions.Add($"Convert {freeToElite} Free→Elite ($199/mo each)");

        var daysLeft = result.DaysRemaining;
        var header = $"⚠️ Projected ${result.ProjectedMonthlyRevenue:F0} is ${shortfall:F0} below OPEX ${result.OpexThreshold:F0}. " +
                     $"{daysLeft} days remain to close the gap.";

        if (suggestions.Count == 0)
            return $"{header}\nRecommend: Activate new marketing campaigns or outreach to trial dealers.";

        return $"{header}\nOptions: {string.Join(" | ", suggestions.Take(3))}";
    }

    private async Task<DealerStatsDto> SafeGetDealerStats(CancellationToken ct)
    {
        try
        {
            return await _dealerService.GetDealerStatsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[RevenueProjection] Failed to fetch dealer stats, using empty");
            return new DealerStatsDto();
        }
    }

    private async Task<decimal> SafeGetOverageRevenue(string period, CancellationToken ct)
    {
        try
        {
            return await _financialDataProvider.GetOverageRevenueAsync(period, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[RevenueProjection] Failed to fetch overage revenue");
            return 0m;
        }
    }

    private async Task<decimal> SafeGetAdRevenue(string period, CancellationToken ct)
    {
        try
        {
            return await _financialDataProvider.GetAdvertisingRevenueAsync(period, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[RevenueProjection] Failed to fetch ad revenue");
            return 0m;
        }
    }
}
