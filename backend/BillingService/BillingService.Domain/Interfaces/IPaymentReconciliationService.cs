using BillingService.Domain.Entities;

namespace BillingService.Domain.Interfaces;

/// <summary>
/// Service that performs Stripe↔OKLA payment reconciliation.
///
/// CONTRA #6 FIX: Daily reconciliation ensures:
///   1. Every Stripe payment has a corresponding OKLA DB record with confirmed status
///   2. Every active OKLA subscription has at least one successful payment in the billing period
///   3. Invoice amounts match between Stripe and OKLA
///   4. No orphaned Stripe customers exist without OKLA dealer records
///   5. Subscription statuses are synchronized between Stripe and OKLA
///
/// Alerts are published via RabbitMQ for discrepancies found.
/// </summary>
public interface IPaymentReconciliationService
{
    /// <summary>
    /// Run a full reconciliation for the specified period.
    /// Compares all Stripe transactions against OKLA DB records.
    /// </summary>
    /// <param name="period">Period in YYYY-MM format. Null = current month.</param>
    /// <param name="triggeredBy">Who triggered this (e.g., "system", admin email).</param>
    /// <param name="autoResolve">Whether to auto-resolve minor discrepancies.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The reconciliation report with all discrepancies found.</returns>
    Task<ReconciliationReport> RunReconciliationAsync(
        string? period = null,
        string triggeredBy = "system",
        bool autoResolve = true,
        CancellationToken ct = default);

    /// <summary>
    /// Quick check: verify a single dealer's payments are reconciled.
    /// </summary>
    Task<IEnumerable<ReconciliationDiscrepancy>> ReconcileDealerAsync(
        Guid dealerId, string? period = null, CancellationToken ct = default);
}
