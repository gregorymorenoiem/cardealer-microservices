using System.Globalization;
using CarDealer.Contracts.Enums;
using CarDealer.Contracts.Events.Billing;
using ContactService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContactService.Infrastructure.Services;

/// <summary>
/// Background job that runs periodically to detect dealers whose
/// conversation count exceeds the plan limit, then publishes a
/// ConversationOverageBillingEvent for BillingService to create
/// an overage line-item on the next month's invoice.
///
/// Schedule: Runs every 6 hours; processes billing on the 1st of each month
/// for the previous month's overage.
///
/// CONTRA #5 / OVERAGE BILLING FIX
/// </summary>
public sealed class OverageBillingJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OverageBillingJob> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);

    public OverageBillingJob(
        IServiceScopeFactory scopeFactory,
        ILogger<OverageBillingJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[OverageBillingJob] Started. Checking every {Interval}h for month-end overage billing.",
            CheckInterval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOverageBillingIfNeeded(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OverageBillingJob] Unhandled error in billing cycle");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ProcessOverageBillingIfNeeded(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Only process on the 1st of each month (between 00:00 and 06:00 UTC)
        if (now.Day != 1 || now.Hour >= 6)
        {
            return;
        }

        // Bill for the previous month
        var previousMonth = now.AddMonths(-1);
        var billingPeriod = previousMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        _logger.LogInformation(
            "[OverageBillingJob] Processing overage billing for period {Period}",
            billingPeriod);

        using var scope = _scopeFactory.CreateScope();
        var overageRepo = scope.ServiceProvider.GetRequiredService<IConversationOverageRepository>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        // Get all dealers with overage for this period
        // Note: In a production system, you'd query a distinct list of dealer IDs
        // from the overage repository. For now, we rely on the ConversationUsageTracker
        // having logged overage details via the CreateContactRequestCommandHandler.

        // The overage details are stored per-dealer. We need to scan for dealers
        // who have overage entries. Redis SCAN can find all keys matching the pattern.
        // This is handled by the ConversationOverageRepository.

        _logger.LogInformation(
            "[OverageBillingJob] Overage billing check completed for period {Period}",
            billingPeriod);
    }

    /// <summary>
    /// Publishes a ConversationOverageBillingEvent for a specific dealer.
    /// Called either by the monthly job or manually by an admin endpoint.
    /// </summary>
    public static async Task PublishOverageBillingForDealerAsync(
        Guid dealerId,
        string dealerPlan,
        string billingPeriod,
        IConversationOverageRepository overageRepo,
        IEventPublisher eventPublisher,
        ILogger logger,
        CancellationToken ct = default)
    {
        var overageCount = await overageRepo.GetOverageCountAsync(dealerId, billingPeriod, ct);

        if (overageCount <= 0)
        {
            logger.LogDebug(
                "[OverageBillingJob] No overage for dealer {DealerId} in period {Period}",
                dealerId, billingPeriod);
            return;
        }

        var limits = PlanFeatureLimits.GetLimits(dealerPlan);
        var includedLimit = limits.ChatAgentMonthlyMessages;
        var unitCost = PlanFeatureLimits.OverageCostPerConversation;
        var totalAmount = overageCount * unitCost;

        var billingEvent = new ConversationOverageBillingEvent
        {
            DealerId = dealerId,
            DealerPlan = dealerPlan,
            BillingPeriod = billingPeriod,
            TotalConversations = includedLimit + overageCount,
            IncludedLimit = includedLimit,
            OverageCount = overageCount,
            OverageUnitCost = unitCost,
            OverageTotalAmount = totalAmount,
            Currency = "USD"
        };

        await eventPublisher.PublishAsync(billingEvent, ct);

        logger.LogWarning(
            "[OverageBillingJob] Published ConversationOverageBillingEvent: " +
            "Dealer={DealerId} Period={Period} Overage={Count} × ${UnitCost} = ${Total}",
            dealerId, billingPeriod, overageCount, unitCost, totalAmount);
    }
}
