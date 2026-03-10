using AdminService.Application.Interfaces;
using AdminService.Application.Services;
using AdminService.Application.UseCases.Finance;
using CarDealer.Contracts.Enums;
using Microsoft.Extensions.Logging;

namespace AdminService.Infrastructure.Services;

// ═══════════════════════════════════════════════════════════════════════════════
// INFRASTRUCTURE COST MONITOR SERVICE — CONTRA #8 FIX
//
// Evaluates current infrastructure costs against the DigitalOcean $210 budget.
// Uses IFinancialDataProvider to get real or configured infrastructure costs,
// then projects monthly cost and determines alert severity.
//
// Three-tier alert logic:
//   Warning  (80% = $168): Review resource usage
//   Critical (90% = $189): CTO acknowledgment required
//   Emergency (100% = $210): Auto-reduce non-critical resources
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class InfrastructureCostMonitorService : IInfrastructureCostMonitorService
{
    private readonly IFinancialDataProvider _financialDataProvider;
    private readonly ILogger<InfrastructureCostMonitorService> _logger;

    public InfrastructureCostMonitorService(
        IFinancialDataProvider financialDataProvider,
        ILogger<InfrastructureCostMonitorService> logger)
    {
        _financialDataProvider = financialDataProvider;
        _logger = logger;
    }

    public async Task<InfraCostAssessment> EvaluateCurrentMonthAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var period = now.ToString("yyyy-MM");
        var dayOfMonth = now.Day;
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        var daysRemaining = daysInMonth - dayOfMonth;

        var budget = PlanFeatureLimits.DigitalOceanMonthlyBudget;
        var warningThreshold = budget * PlanFeatureLimits.InfraCostWarningPercent;
        var criticalThreshold = budget * PlanFeatureLimits.InfraCostCriticalPercent;

        // Get infrastructure costs from FinancialDataProvider
        decimal totalCost;
        var costBreakdown = new Dictionary<string, decimal>();

        try
        {
            var (total, subItems) = await _financialDataProvider.GetInfrastructureCostsAsync(period, ct);
            totalCost = total;

            foreach (var item in subItems)
            {
                costBreakdown[item.Name] = item.Amount;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[InfraCostMonitor] Failed to fetch infrastructure costs, using $0");
            totalCost = 0m;
        }

        // Project monthly cost based on daily rate
        // For the FinancialDataProvider, the cost returned is the monthly configured cost,
        // so we use it directly as the projected monthly cost.
        // In production, this would be replaced by actual DigitalOcean API billing data
        // where we'd compute: (spendToDate / dayOfMonth) * daysInMonth
        var projectedMonthlyCost = totalCost;
        var dailyCostRate = dayOfMonth > 0 ? totalCost / daysInMonth : 0m;
        var budgetUtilization = budget > 0 ? projectedMonthlyCost / budget : 0m;

        // Determine severity
        var severity = "Info";
        var shouldAlert = false;
        var overage = 0m;

        if (projectedMonthlyCost >= budget)
        {
            severity = "Emergency";
            shouldAlert = true;
            overage = projectedMonthlyCost - budget;
        }
        else if (projectedMonthlyCost >= criticalThreshold)
        {
            severity = "Critical";
            shouldAlert = true;
            overage = projectedMonthlyCost - criticalThreshold;
        }
        else if (projectedMonthlyCost >= warningThreshold)
        {
            severity = "Warning";
            shouldAlert = true;
            overage = projectedMonthlyCost - warningThreshold;
        }

        // Generate recommended actions based on severity
        var recommendedActions = GetRecommendedActions(severity, projectedMonthlyCost, budget);

        _logger.LogDebug(
            "[InfraCostMonitor] Period={Period}, Projected=${Projected:F2}, Budget=${Budget:F2}, " +
            "Utilization={Util:P1}, Severity={Severity}",
            period, projectedMonthlyCost, budget, budgetUtilization, severity);

        return new InfraCostAssessment
        {
            ShouldAlert = shouldAlert,
            Period = period,
            CurrentSpend = totalCost,
            ProjectedMonthlyCost = projectedMonthlyCost,
            BudgetCeiling = budget,
            BudgetUtilization = budgetUtilization,
            Overage = overage,
            DayOfMonth = dayOfMonth,
            DaysRemaining = daysRemaining,
            DailyCostRate = dailyCostRate,
            Severity = severity,
            CostBreakdown = costBreakdown,
            RecommendedActions = recommendedActions,
        };
    }

    private static List<string> GetRecommendedActions(string severity, decimal projectedCost, decimal budget)
    {
        var actions = new List<string>();

        if (severity == "Info")
        {
            return actions;
        }

        // Phase 1 actions (all alert levels)
        actions.Add("Scale non-critical services to 0: llm-server, video360, backgroundremovalservice, apidocsservice");
        actions.Add("Delete unused PVCs (llm-model-pvc if llm-server at 0 replicas, saves $1/mo)");
        actions.Add("Check for abandoned DigitalOcean volumes and snapshots");

        if (severity is "Critical" or "Emergency")
        {
            // Phase 2 actions
            actions.Add("Reduce duplicate services to 0: cacheservice, messagebusservice, servicediscovery, configurationservice");
            actions.Add("Scale active services to single replica (except gateway)");
            actions.Add("Disable non-essential CronJobs");
        }

        if (severity == "Emergency")
        {
            // Phase 3 actions
            actions.Add($"URGENT: Projected ${projectedCost:F0} exceeds ${budget:F0} budget — consider reducing DOKS nodes from 2→1 (saves ~$48/mo)");
            actions.Add("Review PostgreSQL plan for possible downgrade (saves ~$15/mo)");
            actions.Add("Consider switching managed Redis to sidecar container (saves ~$15/mo)");
        }

        return actions;
    }
}
