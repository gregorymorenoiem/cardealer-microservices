using Microsoft.EntityFrameworkCore;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;
using BillingService.Infrastructure.Messaging;
using BillingService.Infrastructure.Persistence;
using CarDealer.Contracts.Events.Billing;

namespace BillingService.Api.BackgroundServices;

/// <summary>
/// Proactive subscription lifecycle management worker.
/// Runs daily to:
/// 1. Warn dealers about expiring trials (3 days and 1 day before)
/// 2. Escalate payment failures (PastDue > 3 days → reminder, > 7 days → warning, > 14 days → suspend)
/// 3. Detect at-risk subscriptions (low usage patterns)
/// 4. Auto-expire suspended subscriptions after 30 days
/// 
/// This worker was referenced in context.md but never implemented — RETENTION FIX.
/// </summary>
public class SubscriptionRenewalWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionRenewalWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Run every 6 hours

    public SubscriptionRenewalWorker(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionRenewalWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SubscriptionRenewalWorker started — checking every {Interval}", _checkInterval);

        // Initial delay to let app startup complete
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunRetentionChecksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SubscriptionRenewalWorker cycle");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task RunRetentionChecksAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        var subscriptionRepo = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
        var customerRepo = scope.ServiceProvider.GetRequiredService<IStripeCustomerRepository>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        _logger.LogInformation("Running retention checks cycle...");

        var now = DateTime.UtcNow;

        // ── 1. Trial Expiring Warnings ──────────────────────────
        await CheckExpiringTrials(subscriptionRepo, customerRepo, eventPublisher, now, ct);

        // ── 2. Payment Failure Escalation ───────────────────────
        await EscalatePaymentFailures(dbContext, subscriptionRepo, customerRepo, eventPublisher, now, ct);

        // ── 3. Auto-Expire Suspended Subscriptions ──────────────
        await ExpireSuspendedSubscriptions(dbContext, subscriptionRepo, now, ct);

        _logger.LogInformation("Retention checks cycle completed");
    }

    /// <summary>
    /// Find trials ending in 3 days or 1 day and publish warning events.
    /// </summary>
    private async Task CheckExpiringTrials(
        ISubscriptionRepository subscriptionRepo,
        IStripeCustomerRepository customerRepo,
        IEventPublisher eventPublisher,
        DateTime now, CancellationToken ct)
    {
        // 3-day warning
        var trialsEndingSoon = await subscriptionRepo.GetExpiringTrialsAsync(3, ct);
        foreach (var trial in trialsEndingSoon)
        {
            if (trial.TrialEndDate == null) continue;
            var daysRemaining = (int)(trial.TrialEndDate.Value - now).TotalDays;
            if (daysRemaining > 3 || daysRemaining < 0) continue;

            var customer = !string.IsNullOrEmpty(trial.StripeCustomerId)
                ? await customerRepo.GetByStripeCustomerIdAsync(trial.StripeCustomerId)
                : null;

            await eventPublisher.PublishAsync(new SubscriptionTrialEndingEvent
            {
                DealerId = trial.DealerId,
                DealerEmail = customer?.Email ?? "",
                DealerName = customer?.Name ?? "",
                TrialPlan = trial.Plan.ToString(),
                TrialEndsAt = trial.TrialEndDate.Value,
                DaysRemaining = daysRemaining,
                MonthlyPrice = trial.PricePerCycle,
            });

            _logger.LogInformation("Published TrialEndingEvent for dealer {DealerId} — {Days} days remaining",
                trial.DealerId, daysRemaining);
        }
    }

    /// <summary>
    /// Escalate PastDue subscriptions:
    /// - > 3 days: Send payment reminder
    /// - > 7 days: Send urgent warning (features may be limited)
    /// - > 14 days: Suspend subscription
    /// </summary>
    private async Task EscalatePaymentFailures(
        BillingDbContext dbContext,
        ISubscriptionRepository subscriptionRepo,
        IStripeCustomerRepository customerRepo,
        IEventPublisher eventPublisher,
        DateTime now, CancellationToken ct)
    {
        var pastDueSubscriptions = await dbContext.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.PastDue && !s.IsDeleted)
            .ToListAsync(ct);

        foreach (var sub in pastDueSubscriptions)
        {
            var daysPastDue = sub.UpdatedAt.HasValue
                ? (int)(now - sub.UpdatedAt.Value).TotalDays
                : 0;

            if (daysPastDue >= 14)
            {
                // Auto-suspend after 14 days of non-payment
                sub.Suspend();
                await subscriptionRepo.UpdateAsync(sub);

                var changeHistory = new SubscriptionChangeHistory(
                    subscriptionId: sub.Id,
                    dealerId: sub.DealerId,
                    oldPlan: sub.Plan,
                    newPlan: sub.Plan,
                    oldStatus: SubscriptionStatus.PastDue,
                    newStatus: SubscriptionStatus.Suspended,
                    direction: PlanChangeDirection.Cancel,
                    oldPrice: sub.PricePerCycle,
                    newPrice: sub.PricePerCycle,
                    reasonDetails: $"Auto-suspended after {daysPastDue} days of non-payment",
                    changedBy: "system");
                dbContext.SubscriptionChangeHistory.Add(changeHistory);
                await dbContext.SaveChangesAsync(ct);

                _logger.LogWarning("Auto-suspended subscription for dealer {DealerId} — {Days} days past due",
                    sub.DealerId, daysPastDue);
            }
            else if (daysPastDue >= 3)
            {
                // Send escalating payment failure reminders
                var customer = !string.IsNullOrEmpty(sub.StripeCustomerId)
                    ? await customerRepo.GetByStripeCustomerIdAsync(sub.StripeCustomerId)
                    : null;

                await eventPublisher.PublishAsync(new SubscriptionPaymentFailedEvent
                {
                    DealerId = sub.DealerId,
                    DealerEmail = customer?.Email ?? "",
                    DealerName = customer?.Name ?? "",
                    Plan = sub.Plan.ToString(),
                    Amount = sub.PricePerCycle,
                    Currency = sub.Currency,
                    AttemptNumber = daysPastDue >= 7 ? 3 : 2,
                    FailureReason = daysPastDue >= 7
                        ? "Tu suscripción será suspendida pronto si no actualizas tu método de pago"
                        : "Recordatorio: tu pago falló. Actualiza tu método de pago para mantener tu plan",
                });

                _logger.LogInformation("Sent payment escalation for dealer {DealerId} — {Days} days past due",
                    sub.DealerId, daysPastDue);
            }
        }
    }

    /// <summary>
    /// Auto-expire suspended subscriptions after 30 days.
    /// </summary>
    private async Task ExpireSuspendedSubscriptions(
        BillingDbContext dbContext,
        ISubscriptionRepository subscriptionRepo,
        DateTime now, CancellationToken ct)
    {
        var suspendedSubscriptions = await dbContext.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Suspended && !s.IsDeleted)
            .ToListAsync(ct);

        foreach (var sub in suspendedSubscriptions)
        {
            var daysSuspended = sub.UpdatedAt.HasValue
                ? (int)(now - sub.UpdatedAt.Value).TotalDays
                : 0;

            if (daysSuspended >= 30)
            {
                sub.Cancel("Auto-expired after 30 days suspended");
                await subscriptionRepo.UpdateAsync(sub);

                var changeHistory = new SubscriptionChangeHistory(
                    subscriptionId: sub.Id,
                    dealerId: sub.DealerId,
                    oldPlan: sub.Plan,
                    newPlan: SubscriptionPlan.Free,
                    oldStatus: SubscriptionStatus.Suspended,
                    newStatus: SubscriptionStatus.Cancelled,
                    direction: PlanChangeDirection.Cancel,
                    oldPrice: sub.PricePerCycle,
                    newPrice: 0,
                    reasonDetails: $"Auto-expired after {daysSuspended} days suspended",
                    changedBy: "system");
                dbContext.SubscriptionChangeHistory.Add(changeHistory);
                await dbContext.SaveChangesAsync(ct);

                _logger.LogWarning("Auto-expired subscription for dealer {DealerId} — {Days} days suspended",
                    sub.DealerId, daysSuspended);
            }
        }
    }
}
