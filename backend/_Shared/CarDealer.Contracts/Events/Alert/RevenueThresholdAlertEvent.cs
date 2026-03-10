using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Alert;

// ═══════════════════════════════════════════════════════════════════════════════
// REVENUE THRESHOLD ALERT EVENT — CONTRA #7 FIX
//
// Published by AdminService when projected monthly revenue is below OPEX.
// Consumed by NotificationService to alert the founder via Email + SMS + Teams.
//
// This event carries all the data needed for an actionable alert:
//   - Current vs projected revenue
//   - OPEX threshold and shortfall
//   - Remaining days in month
//   - Recommended conversion targets
// ═══════════════════════════════════════════════════════════════════════════════

public class RevenueThresholdAlertEvent : EventBase
{
    public override string EventType => "alert.revenue.threshold_breached";

    /// <summary>Period being monitored (e.g., "2026-03").</summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>Revenue accumulated so far this month.</summary>
    public decimal AccumulatedRevenue { get; set; }

    /// <summary>Projected revenue if current daily rate holds to end of month.</summary>
    public decimal ProjectedMonthlyRevenue { get; set; }

    /// <summary>Monthly OPEX threshold ($2,215).</summary>
    public decimal OpexThreshold { get; set; }

    /// <summary>Shortfall = OPEX - ProjectedRevenue (positive means below OPEX).</summary>
    public decimal Shortfall { get; set; }

    /// <summary>How far below OPEX as a percentage (e.g., 0.15 = 15% below).</summary>
    public decimal ShortfallPercent { get; set; }

    /// <summary>Days elapsed so far in the billing month.</summary>
    public int DaysElapsed { get; set; }

    /// <summary>Days remaining in the billing month.</summary>
    public int DaysRemaining { get; set; }

    /// <summary>Average daily revenue based on accumulated.</summary>
    public decimal DailyRevenueRate { get; set; }

    /// <summary>Daily revenue needed for the remaining days to hit OPEX.</summary>
    public decimal RequiredDailyRevenue { get; set; }

    /// <summary>Current MRR breakdown.</summary>
    public decimal CurrentMrr { get; set; }

    /// <summary>Overage + advertising revenue this month.</summary>
    public decimal AdditionalRevenue { get; set; }

    /// <summary>Severity: Warning (projected 90-100% of OPEX), Critical (below 90%).</summary>
    public string Severity { get; set; } = "Warning";

    /// <summary>
    /// Suggested conversions needed to close the gap.
    /// e.g., "Convert 3 Free→Visible ($29/mo) or 1 Visible→Pro ($89/mo) to cover $87 shortfall."
    /// </summary>
    public string SuggestedAction { get; set; } = string.Empty;

    /// <summary>Total active dealers by plan for context.</summary>
    public Dictionary<string, int> DealersByPlan { get; set; } = new();
}
