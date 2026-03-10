using ContactService.Domain.Entities;

namespace ContactService.Domain.Interfaces;

/// <summary>
/// Repository for persisting and querying individual conversation overage details.
/// Enables the dealer to download a per-conversation breakdown with date/time.
///
/// CONTRA #5 / OVERAGE BILLING FIX
/// </summary>
public interface IConversationOverageRepository
{
    /// <summary>
    /// Persists an individual overage conversation detail.
    /// Called each time a conversation exceeds the plan's included limit.
    /// </summary>
    Task<ConversationOverageDetail> CreateAsync(
        ConversationOverageDetail detail,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all overage details for a dealer in a specific billing period.
    /// Used for the downloadable overage report.
    /// </summary>
    Task<List<ConversationOverageDetail>> GetByDealerAndPeriodAsync(
        Guid dealerId,
        string billingPeriod,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the count of overage conversations for a dealer in a billing period.
    /// Used for quick display without loading full details.
    /// </summary>
    Task<int> GetOverageCountAsync(
        Guid dealerId,
        string billingPeriod,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the total overage cost for a dealer in a billing period.
    /// OverageCount × UnitCost.
    /// </summary>
    Task<decimal> GetOverageTotalCostAsync(
        Guid dealerId,
        string billingPeriod,
        CancellationToken ct = default);
}
