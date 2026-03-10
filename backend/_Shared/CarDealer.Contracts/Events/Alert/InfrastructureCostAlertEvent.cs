using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Alert;

// ═══════════════════════════════════════════════════════════════════════════════
// INFRASTRUCTURE COST ALERT EVENT — CONTRA #8 FIX
//
// Published by AdminService when projected monthly cloud cost breaches the
// DigitalOcean budget ceiling ($210). Consumed by NotificationService to
// alert the CTO/platform team via Email + SMS + Teams + Slack.
//
// Three-tier alert: Warning ($168, 80%) → Critical ($189, 90%) → Emergency ($210, 100%)
// ═══════════════════════════════════════════════════════════════════════════════

public class InfrastructureCostAlertEvent : EventBase
{
    public override string EventType => "alert.infra.cost_threshold_breached";

    /// <summary>Period being monitored (e.g., "2026-03").</summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>Current accumulated cloud spend this month in USD.</summary>
    public decimal CurrentSpend { get; set; }

    /// <summary>Projected monthly cost based on daily rate extrapolation.</summary>
    public decimal ProjectedMonthlyCost { get; set; }

    /// <summary>Monthly DigitalOcean budget ceiling ($210).</summary>
    public decimal BudgetCeiling { get; set; }

    /// <summary>Budget utilization as a ratio (0.0–1.0+).</summary>
    public decimal BudgetUtilization { get; set; }

    /// <summary>How much over the budget threshold in USD (positive = over).</summary>
    public decimal Overage { get; set; }

    /// <summary>Days elapsed so far in the billing month.</summary>
    public int DaysElapsed { get; set; }

    /// <summary>Days remaining in the billing month.</summary>
    public int DaysRemaining { get; set; }

    /// <summary>Average daily cloud cost based on accumulated spend.</summary>
    public decimal DailyCostRate { get; set; }

    /// <summary>Severity: Warning (80–90% of budget), Critical (90–100%), Emergency (100%+).</summary>
    public string Severity { get; set; } = "Warning";

    /// <summary>Breakdown of infrastructure costs by category.</summary>
    public Dictionary<string, decimal> CostBreakdown { get; set; } = new();

    /// <summary>
    /// Recommended actions from the runbook to reduce costs.
    /// e.g., "Scale video360service to 0 (saves ~$4/mo)" or "Reduce node count from 2→1".
    /// </summary>
    public List<string> RecommendedActions { get; set; } = new();

    /// <summary>URL to the infrastructure cost runbook.</summary>
    public string RunbookUrl { get; set; } = string.Empty;
}
