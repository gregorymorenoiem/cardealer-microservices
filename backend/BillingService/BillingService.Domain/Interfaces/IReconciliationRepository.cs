using BillingService.Domain.Entities;

namespace BillingService.Domain.Interfaces;

/// <summary>
/// Repository for reconciliation reports and discrepancies.
/// CONTRA #6 FIX: Enables persistence and querying of reconciliation results.
/// </summary>
public interface IReconciliationRepository
{
    Task<ReconciliationReport?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ReconciliationReport?> GetLatestByPeriodAsync(string period, CancellationToken ct = default);
    Task<IEnumerable<ReconciliationReport>> GetAllAsync(int limit = 30, CancellationToken ct = default);
    Task<IEnumerable<ReconciliationReport>> GetByStatusAsync(ReconciliationStatus status, CancellationToken ct = default);
    Task<ReconciliationReport> AddAsync(ReconciliationReport report, CancellationToken ct = default);
    Task UpdateAsync(ReconciliationReport report, CancellationToken ct = default);

    Task<ReconciliationDiscrepancy?> GetDiscrepancyByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ReconciliationDiscrepancy>> GetUnresolvedDiscrepanciesAsync(CancellationToken ct = default);
    Task<IEnumerable<ReconciliationDiscrepancy>> GetDiscrepanciesByDealerAsync(Guid dealerId, CancellationToken ct = default);
    Task UpdateDiscrepancyAsync(ReconciliationDiscrepancy discrepancy, CancellationToken ct = default);
}
