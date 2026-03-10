using AdminService.Application.Interfaces;
using AdminService.Application.Services;
using AdminService.Application.UseCases.Finance;
using AdminService.Infrastructure.Services;
using CarDealer.Contracts.Enums;
using CarDealer.Contracts.Events.Alert;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AdminService.Tests;

/// <summary>
/// CONTRA #8 FIX: Infrastructure Cost Alert Tests
///
/// Validates the InfrastructureCostMonitorService correctly:
///   1. Constants: $210 budget, 80% warning, 90% critical, 4h interval
///   2. Assessment: Projects cost, calculates utilization, determines severity
///   3. Alert triggers: No alert below 80%, Warning 80-90%, Critical 90-100%, Emergency 100%+
///   4. Recommended actions escalate by severity
///   5. Cost breakdown is returned from FinancialDataProvider
///   6. Edge cases: zero costs, API failures, boundary values
///   7. Event DTO: correct EventType, properties, defaults
///   8. Runbook URL is correctly set
/// </summary>
public class InfrastructureCostAlertTests
{
    private readonly Mock<IFinancialDataProvider> _financialDataMock;
    private readonly InfrastructureCostMonitorService _service;

    public InfrastructureCostAlertTests()
    {
        _financialDataMock = new Mock<IFinancialDataProvider>();
        var logger = NullLogger<InfrastructureCostMonitorService>.Instance;

        _service = new InfrastructureCostMonitorService(
            _financialDataMock.Object,
            logger);
    }

    // ════════════════════════════════════════════════════════════════════════
    // BUDGET CONSTANTS — Single source of truth in PlanFeatureLimits
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void DigitalOceanBudget_Is210()
    {
        Assert.Equal(210m, PlanFeatureLimits.DigitalOceanMonthlyBudget);
    }

    [Fact]
    public void WarningPercent_Is80()
    {
        Assert.Equal(0.80m, PlanFeatureLimits.InfraCostWarningPercent);
    }

    [Fact]
    public void CriticalPercent_Is90()
    {
        Assert.Equal(0.90m, PlanFeatureLimits.InfraCostCriticalPercent);
    }

    [Fact]
    public void AlertCheckInterval_Is4Hours()
    {
        Assert.Equal(4, PlanFeatureLimits.InfraCostAlertCheckIntervalHours);
    }

    [Fact]
    public void WarningThreshold_Is168()
    {
        // $210 × 80% = $168
        var warningThreshold = PlanFeatureLimits.DigitalOceanMonthlyBudget * PlanFeatureLimits.InfraCostWarningPercent;
        Assert.Equal(168m, warningThreshold);
    }

    [Fact]
    public void CriticalThreshold_Is189()
    {
        // $210 × 90% = $189
        var criticalThreshold = PlanFeatureLimits.DigitalOceanMonthlyBudget * PlanFeatureLimits.InfraCostCriticalPercent;
        Assert.Equal(189m, criticalThreshold);
    }

    [Fact]
    public void RunbookUrl_ContainsRunbook()
    {
        Assert.Contains("RUNBOOK_INFRA_COST.md", PlanFeatureLimits.InfraCostRunbookUrl);
    }

    // ════════════════════════════════════════════════════════════════════════
    // ASSESSMENT: No alert when under warning threshold
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evaluate_CostBelowWarning_NoAlert()
    {
        // $139/mo is the current baseline — well below $168 warning
        SetupInfraCosts(139m, new() { { "DigitalOcean", 96m }, { "PostgreSQL", 30m }, { "LB", 12m }, { "PVC", 1m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.False(result.ShouldAlert);
        Assert.Equal("Info", result.Severity);
        Assert.Equal(139m, result.CurrentSpend);
        Assert.Equal(210m, result.BudgetCeiling);
    }

    [Fact]
    public async Task Evaluate_CostExactlyAtWarning_TriggersAlert()
    {
        // $168 is exactly 80% of $210
        SetupInfraCosts(168m, new() { { "DigitalOcean", 168m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.True(result.ShouldAlert);
        Assert.Equal("Warning", result.Severity);
    }

    // ════════════════════════════════════════════════════════════════════════
    // SEVERITY ESCALATION: Warning → Critical → Emergency
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evaluate_CostAt80Percent_WarningLevel()
    {
        // $170 is between $168 (80%) and $189 (90%)
        SetupInfraCosts(170m, new() { { "DigitalOcean", 170m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.True(result.ShouldAlert);
        Assert.Equal("Warning", result.Severity);
        Assert.True(result.Overage > 0);
    }

    [Fact]
    public async Task Evaluate_CostAt90Percent_CriticalLevel()
    {
        // $190 is between $189 (90%) and $210 (100%)
        SetupInfraCosts(190m, new() { { "DigitalOcean", 190m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.True(result.ShouldAlert);
        Assert.Equal("Critical", result.Severity);
    }

    [Fact]
    public async Task Evaluate_CostAt100Percent_EmergencyLevel()
    {
        // $215 is above the $210 budget
        SetupInfraCosts(215m, new() { { "DigitalOcean", 215m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.True(result.ShouldAlert);
        Assert.Equal("Emergency", result.Severity);
    }

    [Fact]
    public async Task Evaluate_CostExactlyAtBudget_EmergencyLevel()
    {
        // $210 is exactly the budget ceiling
        SetupInfraCosts(210m, new() { { "DigitalOcean", 210m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.True(result.ShouldAlert);
        Assert.Equal("Emergency", result.Severity);
    }

    // ════════════════════════════════════════════════════════════════════════
    // BUDGET UTILIZATION CALCULATION
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evaluate_CalculatesCorrectUtilization()
    {
        // $105 / $210 = 50% utilization
        SetupInfraCosts(105m, new() { { "DigitalOcean", 105m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.Equal(0.50m, result.BudgetUtilization);
    }

    [Fact]
    public async Task Evaluate_OverBudget_UtilizationAbove100()
    {
        // $252 / $210 = 120% utilization
        SetupInfraCosts(252m, new() { { "DigitalOcean", 252m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.Equal(1.20m, result.BudgetUtilization);
        Assert.Equal("Emergency", result.Severity);
    }

    // ════════════════════════════════════════════════════════════════════════
    // RECOMMENDED ACTIONS ESCALATION
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evaluate_InfoSeverity_NoActions()
    {
        SetupInfraCosts(100m, new() { { "DigitalOcean", 100m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.Empty(result.RecommendedActions);
    }

    [Fact]
    public async Task Evaluate_WarningSeverity_Phase1Actions()
    {
        SetupInfraCosts(170m, new() { { "DigitalOcean", 170m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.NotEmpty(result.RecommendedActions);
        Assert.Contains(result.RecommendedActions, a => a.Contains("non-critical"));
    }

    [Fact]
    public async Task Evaluate_CriticalSeverity_Phase1And2Actions()
    {
        SetupInfraCosts(195m, new() { { "DigitalOcean", 195m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.True(result.RecommendedActions.Count > 3);
        Assert.Contains(result.RecommendedActions, a => a.Contains("single replica"));
    }

    [Fact]
    public async Task Evaluate_EmergencySeverity_AllPhaseActions()
    {
        SetupInfraCosts(220m, new() { { "DigitalOcean", 220m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.True(result.RecommendedActions.Count >= 6);
        Assert.Contains(result.RecommendedActions, a => a.Contains("DOKS nodes"));
        Assert.Contains(result.RecommendedActions, a => a.Contains("PostgreSQL"));
    }

    // ════════════════════════════════════════════════════════════════════════
    // COST BREAKDOWN
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evaluate_ReturnsCostBreakdown()
    {
        var breakdown = new Dictionary<string, decimal>
        {
            { "DigitalOcean (Servidores)", 96m },
            { "PostgreSQL (Managed DB)", 30m },
            { "Load Balancer", 12m },
            { "Block Storage", 1m },
        };
        SetupInfraCosts(139m, breakdown);

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.Equal(4, result.CostBreakdown.Count);
        Assert.Equal(96m, result.CostBreakdown["DigitalOcean (Servidores)"]);
        Assert.Equal(30m, result.CostBreakdown["PostgreSQL (Managed DB)"]);
    }

    // ════════════════════════════════════════════════════════════════════════
    // PERIOD AND DAILY RATE
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evaluate_CalculatesDailyRate()
    {
        SetupInfraCosts(150m, new() { { "DigitalOcean", 150m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        var now = DateTime.UtcNow;
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        var expectedDailyRate = 150m / daysInMonth;
        Assert.Equal(expectedDailyRate, result.DailyCostRate);
    }

    [Fact]
    public async Task Evaluate_PeriodFormatIsCorrect()
    {
        SetupInfraCosts(139m, new() { { "DigitalOcean", 139m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.Matches(@"\d{4}-\d{2}", result.Period);
        Assert.Equal(DateTime.UtcNow.ToString("yyyy-MM"), result.Period);
    }

    [Fact]
    public async Task Evaluate_DaysRemainingIsCorrect()
    {
        SetupInfraCosts(139m, new() { { "DigitalOcean", 139m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        var now = DateTime.UtcNow;
        var expectedRemaining = DateTime.DaysInMonth(now.Year, now.Month) - now.Day;
        Assert.Equal(expectedRemaining, result.DaysRemaining);
    }

    // ════════════════════════════════════════════════════════════════════════
    // EDGE CASES
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Evaluate_ZeroCosts_NoAlert()
    {
        SetupInfraCosts(0m, new());

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.False(result.ShouldAlert);
        Assert.Equal("Info", result.Severity);
        Assert.Equal(0m, result.CurrentSpend);
    }

    [Fact]
    public async Task Evaluate_ApiFailure_NoAlert()
    {
        _financialDataMock
            .Setup(x => x.GetInfrastructureCostsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("DigitalOcean API down"));

        var result = await _service.EvaluateCurrentMonthAsync();

        // Service should handle gracefully and not throw
        Assert.False(result.ShouldAlert);
        Assert.Equal(0m, result.CurrentSpend);
    }

    [Fact]
    public async Task Evaluate_JustBelowWarning_NoAlert()
    {
        // $167.99 is just below $168 warning threshold
        SetupInfraCosts(167.99m, new() { { "DigitalOcean", 167.99m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.False(result.ShouldAlert);
        Assert.Equal("Info", result.Severity);
    }

    [Fact]
    public async Task Evaluate_JustBelowCritical_WarningLevel()
    {
        // $188.99 is between warning ($168) and critical ($189)
        SetupInfraCosts(188.99m, new() { { "DigitalOcean", 188.99m } });

        var result = await _service.EvaluateCurrentMonthAsync();

        Assert.True(result.ShouldAlert);
        Assert.Equal("Warning", result.Severity);
    }

    // ════════════════════════════════════════════════════════════════════════
    // EVENT DTO TESTS
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void InfraEvent_EventType_IsCorrect()
    {
        var evt = new InfrastructureCostAlertEvent();
        Assert.Equal("alert.infra.cost_threshold_breached", evt.EventType);
    }

    [Fact]
    public void InfraEvent_DefaultSeverity_IsWarning()
    {
        var evt = new InfrastructureCostAlertEvent();
        Assert.Equal("Warning", evt.Severity);
    }

    [Fact]
    public void InfraEvent_SchemaVersion_Is1()
    {
        var evt = new InfrastructureCostAlertEvent();
        Assert.Equal(1, evt.SchemaVersion);
    }

    [Fact]
    public void InfraEvent_EventId_IsGenerated()
    {
        var evt = new InfrastructureCostAlertEvent();
        Assert.NotEqual(Guid.Empty, evt.EventId);
    }

    [Fact]
    public void InfraEvent_AllProperties_CanBeSet()
    {
        var evt = new InfrastructureCostAlertEvent
        {
            Period = "2026-03",
            CurrentSpend = 195m,
            ProjectedMonthlyCost = 195m,
            BudgetCeiling = 210m,
            BudgetUtilization = 0.929m,
            Overage = 6m,
            DaysElapsed = 15,
            DaysRemaining = 16,
            DailyCostRate = 6.29m,
            Severity = "Critical",
            CostBreakdown = new() { { "DigitalOcean", 195m } },
            RecommendedActions = new() { "Scale down non-critical services" },
            RunbookUrl = "https://example.com/runbook",
        };

        Assert.Equal("2026-03", evt.Period);
        Assert.Equal(195m, evt.CurrentSpend);
        Assert.Equal(195m, evt.ProjectedMonthlyCost);
        Assert.Equal(210m, evt.BudgetCeiling);
        Assert.Equal(0.929m, evt.BudgetUtilization);
        Assert.Equal(6m, evt.Overage);
        Assert.Equal(15, evt.DaysElapsed);
        Assert.Equal(16, evt.DaysRemaining);
        Assert.Equal(6.29m, evt.DailyCostRate);
        Assert.Equal("Critical", evt.Severity);
        Assert.Single(evt.CostBreakdown);
        Assert.Single(evt.RecommendedActions);
        Assert.Equal("https://example.com/runbook", evt.RunbookUrl);
    }

    [Fact]
    public void InfraEvent_DefaultCollections_AreEmpty()
    {
        var evt = new InfrastructureCostAlertEvent();
        Assert.Empty(evt.CostBreakdown);
        Assert.Empty(evt.RecommendedActions);
    }

    // ════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private void SetupInfraCosts(decimal total, Dictionary<string, decimal> breakdown)
    {
        var subItems = breakdown.Select(kv => new ExpenseSubItemDto
        {
            Name = kv.Key,
            Amount = kv.Value,
        }).ToList();

        _financialDataMock
            .Setup(x => x.GetInfrastructureCostsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((total, subItems));
    }
}
