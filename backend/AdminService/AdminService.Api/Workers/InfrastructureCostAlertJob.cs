using AdminService.Application.Services;
using AdminService.Domain.Interfaces;
using CarDealer.Contracts.Enums;
using CarDealer.Contracts.Events.Alert;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdminService.Api.Workers;

// ═══════════════════════════════════════════════════════════════════════════════
// INFRASTRUCTURE COST ALERT JOB — CONTRA #8 FIX
//
// BackgroundService that runs every 4 hours (configurable via PlanFeatureLimits).
// Evaluates projected monthly cloud cost against DigitalOcean budget ($210).
//
// Alert policy:
//   - Maximum 1 alert per day per severity level (deduplication)
//   - Three-tier severity: Warning ($168) → Critical ($189) → Emergency ($210)
//   - Publishes InfrastructureCostAlertEvent to RabbitMQ
//   - NotificationService consumes and routes to Email + SMS + Teams + Slack
//   - Runbook URL included in every alert for immediate action
//
// Schedule: Every 4 hours = 00:00, 04:00, 08:00, 12:00, 16:00, 20:00 UTC
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class InfrastructureCostAlertJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InfrastructureCostAlertJob> _logger;

    // Deduplication: track last alert date per severity
    private DateTime? _lastAlertDate;
    private string _lastAlertSeverity = string.Empty;

    public InfrastructureCostAlertJob(
        IServiceScopeFactory scopeFactory,
        ILogger<InfrastructureCostAlertJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[InfraCostAlert] Starting — Budget: ${Budget:F2}, Warning: ${Warn:F2} (80%), " +
            "Critical: ${Crit:F2} (90%), check interval: {Hours}h",
            PlanFeatureLimits.DigitalOceanMonthlyBudget,
            PlanFeatureLimits.DigitalOceanMonthlyBudget * PlanFeatureLimits.InfraCostWarningPercent,
            PlanFeatureLimits.DigitalOceanMonthlyBudget * PlanFeatureLimits.InfraCostCriticalPercent,
            PlanFeatureLimits.InfraCostAlertCheckIntervalHours);

        // Delay startup to let services initialize
        await Task.Delay(TimeSpan.FromSeconds(90), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EvaluateAndAlertAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[InfraCostAlert] Unhandled error during evaluation");
            }

            await Task.Delay(
                TimeSpan.FromHours(PlanFeatureLimits.InfraCostAlertCheckIntervalHours),
                stoppingToken);
        }

        _logger.LogInformation("[InfraCostAlert] Stopped");
    }

    private async Task EvaluateAndAlertAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var costMonitor = scope.ServiceProvider.GetRequiredService<IInfrastructureCostMonitorService>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var assessment = await costMonitor.EvaluateCurrentMonthAsync(ct);

        if (!assessment.ShouldAlert)
        {
            _logger.LogDebug(
                "[InfraCostAlert] No alert needed — Projected=${Projected:F2}, Budget=${Budget:F2}, Util={Util:P1}",
                assessment.ProjectedMonthlyCost, assessment.BudgetCeiling, assessment.BudgetUtilization);
            return;
        }

        // Deduplication: max 1 alert per calendar day for same or lower severity
        // But always escalate (e.g., Warning→Critical fires a new alert on the same day)
        var today = DateTime.UtcNow.Date;
        if (_lastAlertDate == today && !IsEscalation(_lastAlertSeverity, assessment.Severity))
        {
            _logger.LogDebug("[InfraCostAlert] Alert already sent today at severity={Sev}, skipping",
                _lastAlertSeverity);
            return;
        }

        _logger.LogWarning(
            "[InfraCostAlert] ⚠️ ALERT TRIGGERED — Period={Period}, Projected=${Projected:F2}, " +
            "Budget=${Budget:F2}, Utilization={Util:P1}, Severity={Severity}",
            assessment.Period, assessment.ProjectedMonthlyCost,
            assessment.BudgetCeiling, assessment.BudgetUtilization, assessment.Severity);

        var alertEvent = new InfrastructureCostAlertEvent
        {
            Period = assessment.Period,
            CurrentSpend = assessment.CurrentSpend,
            ProjectedMonthlyCost = assessment.ProjectedMonthlyCost,
            BudgetCeiling = assessment.BudgetCeiling,
            BudgetUtilization = assessment.BudgetUtilization,
            Overage = assessment.Overage,
            DaysElapsed = assessment.DayOfMonth,
            DaysRemaining = assessment.DaysRemaining,
            DailyCostRate = assessment.DailyCostRate,
            Severity = assessment.Severity,
            CostBreakdown = assessment.CostBreakdown,
            RecommendedActions = assessment.RecommendedActions,
            RunbookUrl = PlanFeatureLimits.InfraCostRunbookUrl,
        };

        await eventPublisher.PublishAsync(alertEvent, ct);

        _lastAlertDate = today;
        _lastAlertSeverity = assessment.Severity;

        _logger.LogInformation(
            "[InfraCostAlert] Published InfrastructureCostAlertEvent for {Period} — " +
            "Severity={Severity}, Projected=${Projected:F2}",
            assessment.Period, assessment.Severity, assessment.ProjectedMonthlyCost);
    }

    /// <summary>
    /// Returns true if newSeverity is higher than currentSeverity.
    /// Allows re-alert on same day when severity escalates.
    /// </summary>
    private static bool IsEscalation(string currentSeverity, string newSeverity)
    {
        var order = new Dictionary<string, int>
        {
            ["Info"] = 0,
            ["Warning"] = 1,
            ["Critical"] = 2,
            ["Emergency"] = 3,
        };

        var currentOrder = order.GetValueOrDefault(currentSeverity, 0);
        var newOrder = order.GetValueOrDefault(newSeverity, 0);
        return newOrder > currentOrder;
    }
}
