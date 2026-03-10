namespace AdminService.Application.Services;

// ═══════════════════════════════════════════════════════════════════════════════
// INFRASTRUCTURE COST MONITOR SERVICE — CONTRA #8 FIX
//
// Evaluates current month's cloud infrastructure costs against the DigitalOcean
// $210 budget. Returns an assessment with severity and recommended actions.
//
// Data source: IFinancialDataProvider.GetInfrastructureCostsAsync() which reads
// from appsettings.json (FinancialDashboard:MonthlyCosts:Infrastructure) or
// falls back to hardcoded defaults ($285/mo total, $150/mo DigitalOcean).
// ═══════════════════════════════════════════════════════════════════════════════

public interface IInfrastructureCostMonitorService
{
    /// <summary>
    /// Evaluate current infrastructure costs against the DigitalOcean budget.
    /// </summary>
    Task<InfraCostAssessment> EvaluateCurrentMonthAsync(CancellationToken ct = default);
}

/// <summary>
/// Result of infrastructure cost evaluation against budget.
/// </summary>
public class InfraCostAssessment
{
    /// <summary>Whether an alert should be fired based on current costs.</summary>
    public bool ShouldAlert { get; set; }

    /// <summary>Period being evaluated (e.g., "2026-03").</summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>Current accumulated infrastructure cost this month.</summary>
    public decimal CurrentSpend { get; set; }

    /// <summary>Projected monthly cost based on daily rate.</summary>
    public decimal ProjectedMonthlyCost { get; set; }

    /// <summary>Monthly DigitalOcean budget ceiling ($210).</summary>
    public decimal BudgetCeiling { get; set; }

    /// <summary>Budget utilization ratio (0.0–1.0+).</summary>
    public decimal BudgetUtilization { get; set; }

    /// <summary>Overage above the warning threshold in USD.</summary>
    public decimal Overage { get; set; }

    /// <summary>Day of month.</summary>
    public int DayOfMonth { get; set; }

    /// <summary>Days remaining in the billing month.</summary>
    public int DaysRemaining { get; set; }

    /// <summary>Average daily infrastructure cost.</summary>
    public decimal DailyCostRate { get; set; }

    /// <summary>Severity: Warning (80–90%), Critical (90–100%), Emergency (100%+).</summary>
    public string Severity { get; set; } = "Info";

    /// <summary>Cost breakdown by category.</summary>
    public Dictionary<string, decimal> CostBreakdown { get; set; } = new();

    /// <summary>Recommended actions from runbook.</summary>
    public List<string> RecommendedActions { get; set; } = new();
}
