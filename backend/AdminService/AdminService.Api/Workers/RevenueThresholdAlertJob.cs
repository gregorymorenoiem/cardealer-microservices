using AdminService.Application.Services;
using AdminService.Domain.Interfaces;
using CarDealer.Contracts.Enums;
using CarDealer.Contracts.Events.Alert;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdminService.Api.Workers;

// ═══════════════════════════════════════════════════════════════════════════════
// REVENUE THRESHOLD ALERT JOB — CONTRA #7 FIX
//
// BackgroundService that runs every 6 hours (configurable via PlanFeatureLimits).
// Evaluates projected monthly revenue against OPEX threshold ($2,215).
//
// Alert policy:
//   - Only fires after day 5 of the month (volatile projections before that)
//   - Maximum 1 alert per day (deduplication via _lastAlertDate)
//   - Severity escalation: Warning (90-100% of OPEX) → Critical (below 90%)
//   - Publishes RevenueThresholdAlertEvent to RabbitMQ
//   - NotificationService consumes and routes to Email + SMS + Teams
//
// Schedule: 06:00, 12:00, 18:00, 00:00 UTC (every 6 hours)
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class RevenueThresholdAlertJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RevenueThresholdAlertJob> _logger;

    private DateTime? _lastAlertDate;

    public RevenueThresholdAlertJob(
        IServiceScopeFactory scopeFactory,
        ILogger<RevenueThresholdAlertJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[RevenueAlert] Starting — OPEX threshold: ${Opex:F2}, check interval: {Hours}h",
            PlanFeatureLimits.MonthlyOpexThreshold,
            PlanFeatureLimits.RevenueAlertCheckIntervalHours);

        // Delay startup to let services initialize
        await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

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
                _logger.LogError(ex, "[RevenueAlert] Unhandled error during evaluation");
            }

            await Task.Delay(
                TimeSpan.FromHours(PlanFeatureLimits.RevenueAlertCheckIntervalHours),
                stoppingToken);
        }

        _logger.LogInformation("[RevenueAlert] Stopped");
    }

    private async Task EvaluateAndAlertAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var projectionService = scope.ServiceProvider.GetRequiredService<IRevenueProjectionService>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var result = await projectionService.EvaluateCurrentMonthAsync(ct);

        if (!result.ShouldAlert)
        {
            _logger.LogDebug(
                "[RevenueAlert] No alert needed — Projected=${Projected:F2}, OPEX=${Opex:F2}",
                result.ProjectedMonthlyRevenue, result.OpexThreshold);
            return;
        }

        // Deduplication: max 1 alert per calendar day
        var today = DateTime.UtcNow.Date;
        if (_lastAlertDate == today)
        {
            _logger.LogDebug("[RevenueAlert] Alert already sent today, skipping");
            return;
        }

        _logger.LogWarning(
            "[RevenueAlert] ⚠️ ALERT TRIGGERED — Period={Period}, Projected=${Projected:F2}, " +
            "OPEX=${Opex:F2}, Shortfall=${Shortfall:F2}, Severity={Severity}",
            result.Period, result.ProjectedMonthlyRevenue,
            result.OpexThreshold, result.Shortfall, result.Severity);

        // Publish event to RabbitMQ for NotificationService to consume
        var alertEvent = new RevenueThresholdAlertEvent
        {
            Period = result.Period,
            AccumulatedRevenue = result.AccumulatedRevenue,
            ProjectedMonthlyRevenue = result.ProjectedMonthlyRevenue,
            OpexThreshold = result.OpexThreshold,
            Shortfall = result.Shortfall,
            ShortfallPercent = result.ShortfallPercent,
            DaysElapsed = result.DayOfMonth,
            DaysRemaining = result.DaysRemaining,
            DailyRevenueRate = result.DailyRevenueRate,
            RequiredDailyRevenue = result.RequiredDailyRevenue,
            CurrentMrr = result.CurrentMrr,
            AdditionalRevenue = result.AdditionalRevenue,
            Severity = result.Severity,
            SuggestedAction = result.SuggestedAction,
            DealersByPlan = result.DealersByPlan,
        };

        await eventPublisher.PublishAsync(alertEvent, ct);

        _lastAlertDate = today;

        _logger.LogInformation(
            "[RevenueAlert] Published RevenueThresholdAlertEvent for {Period} — " +
            "Shortfall ${Shortfall:F2} ({ShortfallPercent:P1})",
            result.Period, result.Shortfall, result.ShortfallPercent);
    }
}
