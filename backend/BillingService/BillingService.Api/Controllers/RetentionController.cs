using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BillingService.Domain.Entities;
using BillingService.Infrastructure.Persistence;

namespace BillingService.Api.Controllers;

/// <summary>
/// Admin-only retention and churn analytics endpoints.
/// Provides real-time visibility into subscription health, churn rates,
/// MRR at risk, and dealer retention metrics.
/// 
/// RETENTION FIX: This controller was completely missing — admins had zero
/// visibility into subscription lifecycle analytics.
/// </summary>
[ApiController]
[Route("api/admin/retention")]
[Authorize(Roles = "Admin")]
public class RetentionController : ControllerBase
{
    private readonly BillingDbContext _dbContext;
    private readonly ILogger<RetentionController> _logger;

    public RetentionController(BillingDbContext dbContext, ILogger<RetentionController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive retention dashboard metrics.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetRetentionDashboard(
        [FromQuery] int monthsBack = 3, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var periodStart = now.AddMonths(-monthsBack);

        // Active subscriptions by plan
        var activeByPlan = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted && s.Status == SubscriptionStatus.Active)
            .GroupBy(s => s.Plan)
            .Select(g => new { Plan = g.Key.ToString(), Count = g.Count(), MRR = g.Sum(s => s.PricePerCycle) })
            .ToListAsync(ct);

        // Cancelled subscriptions in period
        var cancelledInPeriod = await _dbContext.Subscriptions
            .IgnoreQueryFilters()
            .Where(s => s.Status == SubscriptionStatus.Cancelled && s.CancelledAt >= periodStart)
            .CountAsync(ct);

        // Total active at start of period (approximate)
        var totalActiveNow = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial))
            .CountAsync(ct);

        // Past due (at risk)
        var pastDueCount = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted && s.Status == SubscriptionStatus.PastDue)
            .CountAsync(ct);

        var suspendedCount = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted && s.Status == SubscriptionStatus.Suspended)
            .CountAsync(ct);

        // MRR calculations
        var totalMrr = activeByPlan.Sum(x => x.MRR);
        var mrrAtRisk = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted && (s.Status == SubscriptionStatus.PastDue || s.Status == SubscriptionStatus.Suspended))
            .SumAsync(s => s.PricePerCycle, ct);

        // Monthly churn rate
        var churnRate = totalActiveNow > 0
            ? Math.Round((decimal)cancelledInPeriod / (totalActiveNow + cancelledInPeriod) * 100 / monthsBack, 2)
            : 0;

        // Net Revenue Retention (NRR)
        // Simplified: (MRR + Upgrades - Downgrades - Churn) / Starting MRR
        var upgradeMrr = await _dbContext.SubscriptionChangeHistory
            .Where(h => h.ChangedAt >= periodStart && h.Direction == PlanChangeDirection.Upgrade)
            .SumAsync(h => h.NewPrice - h.OldPrice, ct);

        var downgradeMrr = await _dbContext.SubscriptionChangeHistory
            .Where(h => h.ChangedAt >= periodStart && h.Direction == PlanChangeDirection.Downgrade)
            .SumAsync(h => h.OldPrice - h.NewPrice, ct);

        var churnMrr = await _dbContext.SubscriptionChangeHistory
            .Where(h => h.ChangedAt >= periodStart && h.Direction == PlanChangeDirection.Cancel)
            .SumAsync(h => h.OldPrice, ct);

        return Ok(new
        {
            success = true,
            data = new
            {
                period = new { from = periodStart, to = now, months = monthsBack },
                overview = new
                {
                    totalActiveSubscriptions = totalActiveNow,
                    pastDueSubscriptions = pastDueCount,
                    suspendedSubscriptions = suspendedCount,
                    cancelledInPeriod,
                    monthlyChurnRate = churnRate,
                },
                mrr = new
                {
                    totalMrr,
                    mrrAtRisk,
                    expansionMrr = upgradeMrr,
                    contractionMrr = downgradeMrr,
                    churnedMrr = churnMrr,
                    currency = "USD",
                },
                byPlan = activeByPlan,
            }
        });
    }

    /// <summary>
    /// Get subscription change history (upgrades, downgrades, cancellations).
    /// </summary>
    [HttpGet("change-history")]
    public async Task<IActionResult> GetChangeHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? direction = null,
        [FromQuery] int monthsBack = 3,
        CancellationToken ct = default)
    {
        var periodStart = DateTime.UtcNow.AddMonths(-monthsBack);

        var query = _dbContext.SubscriptionChangeHistory
            .Where(h => h.ChangedAt >= periodStart)
            .AsQueryable();

        if (!string.IsNullOrEmpty(direction) && Enum.TryParse<PlanChangeDirection>(direction, true, out var dir))
            query = query.Where(h => h.Direction == dir);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(h => h.ChangedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new
            {
                h.Id,
                h.DealerId,
                h.SubscriptionId,
                OldPlan = h.OldPlan.ToString(),
                NewPlan = h.NewPlan.ToString(),
                OldStatus = h.OldStatus.ToString(),
                NewStatus = h.NewStatus.ToString(),
                Direction = h.Direction.ToString(),
                h.ReasonType,
                h.ReasonDetails,
                h.OldPrice,
                h.NewPrice,
                h.Currency,
                h.ChangedAt,
                h.ChangedBy,
            })
            .ToListAsync(ct);

        return Ok(new { success = true, data = items, total, page, pageSize });
    }

    /// <summary>
    /// Get cancellation reasons breakdown.
    /// </summary>
    [HttpGet("cancellation-reasons")]
    public async Task<IActionResult> GetCancellationReasons(
        [FromQuery] int monthsBack = 6, CancellationToken ct = default)
    {
        var periodStart = DateTime.UtcNow.AddMonths(-monthsBack);

        var reasons = await _dbContext.SubscriptionChangeHistory
            .Where(h => h.ChangedAt >= periodStart && h.Direction == PlanChangeDirection.Cancel)
            .GroupBy(h => h.ReasonType)
            .Select(g => new
            {
                Reason = g.Key.HasValue ? g.Key.Value.ToString() : "Unknown",
                Count = g.Count(),
                TotalMrrLost = g.Sum(h => h.OldPrice),
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);

        return Ok(new { success = true, data = reasons });
    }

    /// <summary>
    /// Get at-risk dealers (PastDue or recently downgraded).
    /// </summary>
    [HttpGet("at-risk")]
    public async Task<IActionResult> GetAtRiskDealers(CancellationToken ct)
    {
        var pastDueDealers = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted && s.Status == SubscriptionStatus.PastDue)
            .Select(s => new
            {
                s.DealerId,
                Plan = s.Plan.ToString(),
                Status = "PastDue",
                s.PricePerCycle,
                s.UpdatedAt,
                DaysPastDue = s.UpdatedAt.HasValue
                    ? (int)(DateTime.UtcNow - s.UpdatedAt.Value).TotalDays
                    : 0,
                RiskLevel = "High",
            })
            .ToListAsync(ct);

        var recentDowngrades = await _dbContext.SubscriptionChangeHistory
            .Where(h => h.Direction == PlanChangeDirection.Downgrade
                && h.ChangedAt >= DateTime.UtcNow.AddDays(-30))
            .Select(h => new
            {
                h.DealerId,
                Plan = h.NewPlan.ToString(),
                Status = "Downgraded",
                PricePerCycle = h.NewPrice,
                UpdatedAt = (DateTime?)h.ChangedAt,
                DaysPastDue = 0,
                RiskLevel = "Medium",
            })
            .ToListAsync(ct);

        var allAtRisk = pastDueDealers
            .Concat(recentDowngrades)
            .OrderByDescending(x => x.RiskLevel)
            .ThenByDescending(x => x.DaysPastDue)
            .ToList();

        return Ok(new { success = true, data = allAtRisk, total = allAtRisk.Count });
    }

    /// <summary>
    /// Get monthly cohort retention data.
    /// Shows what % of dealers who subscribed in month X are still active.
    /// </summary>
    [HttpGet("cohort")]
    public async Task<IActionResult> GetCohortRetention(
        [FromQuery] int monthsBack = 6, CancellationToken ct = default)
    {
        var cohorts = new List<object>();

        for (int i = monthsBack; i >= 0; i--)
        {
            var cohortStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-i);
            var cohortEnd = cohortStart.AddMonths(1);

            var subscriptionsCreated = await _dbContext.Subscriptions
                .IgnoreQueryFilters()
                .Where(s => s.CreatedAt >= cohortStart && s.CreatedAt < cohortEnd
                    && s.Plan != SubscriptionPlan.Free)
                .CountAsync(ct);

            var stillActive = await _dbContext.Subscriptions
                .Where(s => !s.IsDeleted
                    && s.CreatedAt >= cohortStart && s.CreatedAt < cohortEnd
                    && s.Plan != SubscriptionPlan.Free
                    && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial))
                .CountAsync(ct);

            var retentionRate = subscriptionsCreated > 0
                ? Math.Round((decimal)stillActive / subscriptionsCreated * 100, 1)
                : 0;

            cohorts.Add(new
            {
                month = cohortStart.ToString("yyyy-MM"),
                subscriptionsCreated,
                stillActive,
                churned = subscriptionsCreated - stillActive,
                retentionRate,
            });
        }

        return Ok(new { success = true, data = cohorts });
    }

    // ═══════════════════════════════════════════════════════════════════
    // KPI AUDIT P0: NEW ENDPOINTS — ARPU, LTV, Trial Conversion, NRR
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get Average Revenue Per User (ARPU) — total MRR / active paying subscribers.
    /// KPI AUDIT: Was only modeled in static docs, not as a live endpoint.
    /// </summary>
    [HttpGet("arpu")]
    public async Task<IActionResult> GetArpu(CancellationToken ct)
    {
        var activePayingSubs = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted
                && s.Status == SubscriptionStatus.Active
                && s.Plan != SubscriptionPlan.Free)
            .ToListAsync(ct);

        var totalMrr = activePayingSubs.Sum(s => s.PricePerCycle);
        var payingCount = activePayingSubs.Count;
        var arpu = payingCount > 0 ? Math.Round(totalMrr / payingCount, 2) : 0;

        // ARPU by plan breakdown
        var byPlan = activePayingSubs
            .GroupBy(s => s.Plan.ToString())
            .Select(g => new { plan = g.Key, count = g.Count(), avgRevenue = Math.Round(g.Average(s => s.PricePerCycle), 2) })
            .OrderByDescending(x => x.avgRevenue)
            .ToList();

        return Ok(new
        {
            success = true,
            data = new
            {
                arpu,
                totalMrr,
                activePayingCount = payingCount,
                currency = "USD",
                byPlan,
                calculatedAt = DateTime.UtcNow,
            }
        });
    }

    /// <summary>
    /// Get Lifetime Value (LTV) — ARPU / monthly churn rate.
    /// KPI AUDIT: Was only a static estimate in docs ($1,308–$2,304). Now computed live.
    /// </summary>
    [HttpGet("ltv")]
    public async Task<IActionResult> GetLtv(
        [FromQuery] int monthsBack = 6, CancellationToken ct = default)
    {
        var periodStart = DateTime.UtcNow.AddMonths(-monthsBack);

        // ARPU
        var activePayingSubs = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted
                && s.Status == SubscriptionStatus.Active
                && s.Plan != SubscriptionPlan.Free)
            .ToListAsync(ct);

        var totalMrr = activePayingSubs.Sum(s => s.PricePerCycle);
        var payingCount = activePayingSubs.Count;
        var arpu = payingCount > 0 ? totalMrr / payingCount : 0;

        // Monthly churn rate
        var cancelledInPeriod = await _dbContext.Subscriptions
            .IgnoreQueryFilters()
            .Where(s => s.Status == SubscriptionStatus.Cancelled
                && s.CancelledAt >= periodStart
                && s.Plan != SubscriptionPlan.Free)
            .CountAsync(ct);

        var totalActiveAndCancelled = payingCount + cancelledInPeriod;
        var monthlyChurnRate = totalActiveAndCancelled > 0
            ? (decimal)cancelledInPeriod / totalActiveAndCancelled / monthsBack
            : 0;

        // LTV = ARPU / monthly churn rate (in months * ARPU)
        var avgLifetimeMonths = monthlyChurnRate > 0 ? Math.Round(1 / monthlyChurnRate, 1) : 0;
        var ltv = monthlyChurnRate > 0 ? Math.Round(arpu / monthlyChurnRate, 2) : 0;

        return Ok(new
        {
            success = true,
            data = new
            {
                ltv,
                arpu = Math.Round(arpu, 2),
                monthlyChurnRate = Math.Round(monthlyChurnRate * 100, 2),
                avgLifetimeMonths,
                currency = "USD",
                periodMonths = monthsBack,
                calculatedAt = DateTime.UtcNow,
            }
        });
    }

    /// <summary>
    /// Get trial-to-paid conversion rate.
    /// KPI AUDIT: SubscriptionRenewalWorker converts trials but this metric was never surfaced.
    /// </summary>
    [HttpGet("trial-conversion")]
    public async Task<IActionResult> GetTrialConversion(
        [FromQuery] int monthsBack = 3, CancellationToken ct = default)
    {
        var periodStart = DateTime.UtcNow.AddMonths(-monthsBack);

        // Total trials started in period (identified by having a TrialEndDate set)
        var trialsStarted = await _dbContext.Subscriptions
            .IgnoreQueryFilters()
            .Where(s => s.CreatedAt >= periodStart
                && s.TrialEndDate != null)
            .CountAsync(ct);

        // Trials that converted to paid (trial ended → status became Active with payment)
        var trialsConverted = await _dbContext.Subscriptions
            .IgnoreQueryFilters()
            .Where(s => s.CreatedAt >= periodStart
                && s.TrialEndDate != null
                && s.Status == SubscriptionStatus.Active
                && s.Plan != SubscriptionPlan.Free)
            .CountAsync(ct);

        // Currently in trial
        var currentlyInTrial = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted && s.Status == SubscriptionStatus.Trial)
            .CountAsync(ct);

        // Trials that churned (cancelled before/during trial)
        var trialsCancelled = await _dbContext.Subscriptions
            .IgnoreQueryFilters()
            .Where(s => s.CreatedAt >= periodStart
                && s.TrialEndDate != null
                && s.Status == SubscriptionStatus.Cancelled)
            .CountAsync(ct);

        var conversionRate = trialsStarted > 0
            ? Math.Round((decimal)trialsConverted / trialsStarted * 100, 2)
            : 0;

        return Ok(new
        {
            success = true,
            data = new
            {
                trialsStarted,
                trialsConverted,
                trialsCancelled,
                currentlyInTrial,
                conversionRate,
                periodMonths = monthsBack,
                calculatedAt = DateTime.UtcNow,
            }
        });
    }

    /// <summary>
    /// Get Net Revenue Retention (NRR) — measures revenue retained from existing customers.
    /// KPI AUDIT P1: Was partially computed in /dashboard but had no starting MRR baseline.
    /// NRR = (Starting MRR + Expansion - Contraction - Churn) / Starting MRR × 100
    /// </summary>
    [HttpGet("nrr")]
    public async Task<IActionResult> GetNrr(
        [FromQuery] int monthsBack = 3, CancellationToken ct = default)
    {
        var periodStart = DateTime.UtcNow.AddMonths(-monthsBack);

        // Starting MRR: MRR from subscriptions that were active at the start of the period
        // Approximation: current active MRR + cancelled MRR - new MRR since period start
        var currentMrr = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted && s.Status == SubscriptionStatus.Active)
            .SumAsync(s => s.PricePerCycle, ct);

        var newSubscriptionsMrr = await _dbContext.Subscriptions
            .Where(s => !s.IsDeleted
                && s.CreatedAt >= periodStart
                && s.Status == SubscriptionStatus.Active)
            .SumAsync(s => s.PricePerCycle, ct);

        var churnedMrr = await _dbContext.SubscriptionChangeHistory
            .Where(h => h.ChangedAt >= periodStart && h.Direction == PlanChangeDirection.Cancel)
            .SumAsync(h => h.OldPrice, ct);

        var startingMrr = currentMrr - newSubscriptionsMrr + churnedMrr;

        // Expansion MRR (upgrades)
        var expansionMrr = await _dbContext.SubscriptionChangeHistory
            .Where(h => h.ChangedAt >= periodStart && h.Direction == PlanChangeDirection.Upgrade)
            .SumAsync(h => h.NewPrice - h.OldPrice, ct);

        // Contraction MRR (downgrades)
        var contractionMrr = await _dbContext.SubscriptionChangeHistory
            .Where(h => h.ChangedAt >= periodStart && h.Direction == PlanChangeDirection.Downgrade)
            .SumAsync(h => h.OldPrice - h.NewPrice, ct);

        // NRR calculation
        var nrr = startingMrr > 0
            ? Math.Round((startingMrr + expansionMrr - contractionMrr - churnedMrr) / startingMrr * 100, 2)
            : 0;

        return Ok(new
        {
            success = true,
            data = new
            {
                nrr,
                startingMrr = Math.Round(startingMrr, 2),
                currentMrr = Math.Round(currentMrr, 2),
                expansionMrr = Math.Round(expansionMrr, 2),
                contractionMrr = Math.Round(contractionMrr, 2),
                churnedMrr = Math.Round(churnedMrr, 2),
                newMrr = Math.Round(newSubscriptionsMrr, 2),
                currency = "USD",
                periodMonths = monthsBack,
                interpretation = nrr >= 100 ? "Healthy — revenue from existing customers is growing"
                    : nrr >= 90 ? "Warning — slight revenue loss from existing customers"
                    : "Critical — significant revenue churn from existing customers",
                calculatedAt = DateTime.UtcNow,
            }
        });
    }
}
