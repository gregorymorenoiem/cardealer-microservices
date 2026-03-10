namespace AdminService.Application.Services;

// ═══════════════════════════════════════════════════════════════════════════════
// REVENUE PROJECTION SERVICE INTERFACE — CONTRA #7 FIX
//
// Computes projected monthly revenue and compares against OPEX threshold.
// Used by the RevenueThresholdAlertJob to determine when to fire alerts.
// ═══════════════════════════════════════════════════════════════════════════════

public interface IRevenueProjectionService
{
    /// <summary>
    /// Computes the revenue projection for the current month and evaluates
    /// whether an alert should be triggered.
    /// </summary>
    /// <returns>Projection result with alert decision and all supporting data.</returns>
    Task<RevenueProjectionResult> EvaluateCurrentMonthAsync(CancellationToken ct = default);
}

/// <summary>
/// Result of a revenue projection evaluation.
/// </summary>
public sealed class RevenueProjectionResult
{
    /// <summary>Whether an alert should be fired (projected revenue &lt; OPEX threshold).</summary>
    public bool ShouldAlert { get; set; }

    /// <summary>Current billing period (e.g., "2026-03").</summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>Day of the month when evaluation was performed.</summary>
    public int DayOfMonth { get; set; }

    /// <summary>Total days in the current month.</summary>
    public int DaysInMonth { get; set; }

    /// <summary>Days remaining until end of month.</summary>
    public int DaysRemaining { get; set; }

    /// <summary>Revenue accumulated so far this month (MRR + overage + ads).</summary>
    public decimal AccumulatedRevenue { get; set; }

    /// <summary>Projected revenue extrapolated to end of month.</summary>
    public decimal ProjectedMonthlyRevenue { get; set; }

    /// <summary>OPEX threshold from PlanFeatureLimits ($2,215).</summary>
    public decimal OpexThreshold { get; set; }

    /// <summary>Shortfall = OPEX - ProjectedRevenue. Positive = below OPEX.</summary>
    public decimal Shortfall { get; set; }

    /// <summary>Shortfall as a percentage of OPEX (0.0–1.0).</summary>
    public decimal ShortfallPercent { get; set; }

    /// <summary>Average daily revenue based on accumulation.</summary>
    public decimal DailyRevenueRate { get; set; }

    /// <summary>Required daily revenue for remaining days to reach OPEX.</summary>
    public decimal RequiredDailyRevenue { get; set; }

    /// <summary>Current MRR from subscriptions.</summary>
    public decimal CurrentMrr { get; set; }

    /// <summary>Overage + advertising revenue.</summary>
    public decimal AdditionalRevenue { get; set; }

    /// <summary>Severity level: "Warning" (90-100% of OPEX) or "Critical" (below 90%).</summary>
    public string Severity { get; set; } = "Info";

    /// <summary>Human-readable suggestion for closing the gap.</summary>
    public string SuggestedAction { get; set; } = string.Empty;

    /// <summary>Active dealer counts by plan name.</summary>
    public Dictionary<string, int> DealersByPlan { get; set; } = new();
}
