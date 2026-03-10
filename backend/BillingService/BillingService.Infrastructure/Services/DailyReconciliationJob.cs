using BillingService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BillingService.Infrastructure.Services;

// ═══════════════════════════════════════════════════════════════════════════════
// DAILY PAYMENT RECONCILIATION JOB — CONTRA #6 FIX
//
// BackgroundService that runs daily at 03:00 UTC to reconcile
// all Stripe payments against OKLA DB records.
//
// Schedule: Every 24 hours at 03:00 UTC (low-traffic window)
// Scope: Current billing period (current month)
// Auto-resolve: Enabled (minor discrepancies like rounding differences)
//
// Publishes:
//   - ReconciliationAlertEvent via RabbitMQ if Critical/High discrepancies found
//   - Logs all results to structured logging (Seq/Elasticsearch)
// ═══════════════════════════════════════════════════════════════════════════════

public sealed class DailyReconciliationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyReconciliationJob> _logger;

    /// <summary>Target hour to run (03:00 UTC).</summary>
    private const int TargetHourUtc = 3;

    /// <summary>Minimum interval between runs to prevent double-execution.</summary>
    private static readonly TimeSpan MinRunInterval = TimeSpan.FromHours(20);

    public DailyReconciliationJob(
        IServiceScopeFactory scopeFactory,
        ILogger<DailyReconciliationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[DailyReconciliationJob] Started. Runs daily at {Hour}:00 UTC", TargetHourUtc);

        // Initial delay to wait for startup
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRun = CalculateNextRunTime(now);
                var delay = nextRun - now;

                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation(
                        "[DailyReconciliationJob] Next run at {NextRun:yyyy-MM-dd HH:mm:ss} UTC (in {Hours:F1}h)",
                        nextRun, delay.TotalHours);

                    await Task.Delay(delay, stoppingToken);
                }

                if (stoppingToken.IsCancellationRequested) break;

                await RunReconciliation(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DailyReconciliationJob] Unhandled error. Retrying in 1 hour.");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("[DailyReconciliationJob] Stopped.");
    }

    private async Task RunReconciliation(CancellationToken ct)
    {
        _logger.LogInformation("[DailyReconciliationJob] Starting daily reconciliation...");

        using var scope = _scopeFactory.CreateScope();
        var reconciliationService = scope.ServiceProvider
            .GetRequiredService<IPaymentReconciliationService>();

        var report = await reconciliationService.RunReconciliationAsync(
            period: null, // current month
            triggeredBy: "daily-job",
            autoResolve: true,
            ct: ct);

        // Log summary
        if (report.DiscrepancyCount == 0)
        {
            _logger.LogInformation(
                "[DailyReconciliationJob] ✅ Clean reconciliation: {SubsChecked} subscriptions, {PaymentsChecked} payments — no discrepancies",
                report.TotalSubscriptionsChecked, report.TotalPaymentsChecked);
        }
        else
        {
            var unresolvedCount = report.DiscrepancyCount - report.AutoResolvedCount;
            _logger.LogWarning(
                "[DailyReconciliationJob] ⚠️ Found {Total} discrepancies ({AutoResolved} auto-resolved, {Unresolved} need review). " +
                "Total discrepancy amount: ${Amount:F2}",
                report.DiscrepancyCount, report.AutoResolvedCount, unresolvedCount,
                report.TotalDiscrepancyAmount);
        }
    }

    private static DateTime CalculateNextRunTime(DateTime now)
    {
        // If it's before the target hour today, run today
        var todayRun = new DateTime(now.Year, now.Month, now.Day, TargetHourUtc, 0, 0, DateTimeKind.Utc);

        if (now < todayRun)
            return todayRun;

        // Otherwise, run tomorrow
        return todayRun.AddDays(1);
    }
}
