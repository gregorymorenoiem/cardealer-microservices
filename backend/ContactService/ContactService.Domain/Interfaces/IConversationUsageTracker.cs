using CarDealer.Contracts.Enums;

namespace ContactService.Domain.Interfaces;

/// <summary>
/// Tracks monthly conversation usage per dealer for plan limit enforcement.
/// Redis-backed for performance; DB is the authoritative fallback.
///
/// CONTRA #5 FIX: Prevents negative margin on ÉLITE plan by enforcing
/// the 2,000 conversation/month hard limit with:
///   - 80% threshold notification (1,600 conversations)
///   - 100% hard limit → ChatAgent basic mode
///   - Overage tracking for billing at $0.08/conversation
/// </summary>
public interface IConversationUsageTracker
{
    /// <summary>
    /// Increment the conversation count for a dealer in the current billing cycle.
    /// Returns the new total count and the usage status.
    /// </summary>
    Task<ConversationUsageResult> IncrementAndCheckAsync(
        Guid dealerId,
        string dealerPlan,
        CancellationToken ct = default);

    /// <summary>
    /// Get the current conversation count for a dealer in the current billing cycle.
    /// </summary>
    Task<int> GetCurrentMonthCountAsync(
        Guid dealerId,
        CancellationToken ct = default);

    /// <summary>
    /// Get the overage count (conversations beyond the hard limit) for billing.
    /// </summary>
    Task<int> GetOverageCountAsync(
        Guid dealerId,
        string dealerPlan,
        CancellationToken ct = default);

    /// <summary>
    /// Check if the dealer's ChatAgent should operate in basic mode
    /// (limit reached, short responses only, no extended context recovery).
    /// </summary>
    Task<bool> IsInBasicModeAsync(
        Guid dealerId,
        string dealerPlan,
        CancellationToken ct = default);
}

/// <summary>
/// Result of incrementing and checking the conversation count.
/// </summary>
public record ConversationUsageResult
{
    /// <summary>The new total conversation count for the current billing cycle.</summary>
    public int CurrentCount { get; init; }

    /// <summary>The maximum allowed conversations for the dealer's plan.</summary>
    public int MaxAllowed { get; init; }

    /// <summary>Usage status (Normal, WarningThreshold, LimitReached, NoAccess).</summary>
    public ConversationUsageStatus Status { get; init; }

    /// <summary>Whether the 80% warning threshold was JUST crossed (first time this cycle).</summary>
    public bool JustCrossedWarningThreshold { get; init; }

    /// <summary>Whether the hard limit was JUST reached (first time this cycle).</summary>
    public bool JustReachedLimit { get; init; }

    /// <summary>Number of conversations over the limit (for overage billing).</summary>
    public int OverageCount { get; init; }

    /// <summary>Projected total conversations for the month based on current rate.</summary>
    public int ProjectedMonthlyTotal { get; init; }
}
