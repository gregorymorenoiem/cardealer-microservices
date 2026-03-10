using CarDealer.Contracts.Abstractions;

namespace ContactService.Domain.Events;

/// <summary>
/// Domain event raised when a dealer reaches 80% of their monthly conversation limit.
/// Triggers WhatsApp + email notification to the dealer with month projection.
///
/// CONTRA #5 FIX: Early warning so dealers can plan their usage.
/// For ÉLITE plan: fired at 1,600 conversations (80% of 2,000).
/// </summary>
public class ConversationWarningThresholdEvent : EventBase
{
    public override string EventType => "contact.conversation.warning_threshold";

    public Guid DealerId { get; init; }
    public string DealerPlan { get; init; } = string.Empty;
    public int CurrentCount { get; init; }
    public int MaxAllowed { get; init; }
    public int ProjectedMonthlyTotal { get; init; }
    public double UsagePercentage { get; init; }

    public ConversationWarningThresholdEvent() { }

    public ConversationWarningThresholdEvent(
        Guid dealerId,
        string dealerPlan,
        int currentCount,
        int maxAllowed,
        int projectedMonthlyTotal)
    {
        DealerId = dealerId;
        DealerPlan = dealerPlan;
        CurrentCount = currentCount;
        MaxAllowed = maxAllowed;
        ProjectedMonthlyTotal = projectedMonthlyTotal;
        UsagePercentage = maxAllowed > 0 ? (double)currentCount / maxAllowed * 100 : 0;
    }
}

/// <summary>
/// Domain event raised when a dealer reaches their monthly conversation hard limit.
/// Triggers notification and activates ChatAgent basic mode.
///
/// CONTRA #5 FIX: ChatAgent enters basic mode (short responses, no extended context recovery)
/// until the start of the next billing cycle.
/// For ÉLITE plan: fired at 2,000 conversations.
/// </summary>
public class ConversationLimitReachedEvent : EventBase
{
    public override string EventType => "contact.conversation.limit_reached";

    public Guid DealerId { get; init; }
    public string DealerPlan { get; init; } = string.Empty;
    public int LimitReached { get; init; }
    public DateTime NextBillingCycleStart { get; init; }

    public ConversationLimitReachedEvent() { }

    public ConversationLimitReachedEvent(
        Guid dealerId,
        string dealerPlan,
        int limitReached)
    {
        DealerId = dealerId;
        DealerPlan = dealerPlan;
        LimitReached = limitReached;

        // Next billing cycle starts on the 1st of next month
        var now = DateTime.UtcNow;
        NextBillingCycleStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
    }
}
