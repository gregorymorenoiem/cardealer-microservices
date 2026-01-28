using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Domain.Interfaces;

/// <summary>
/// Repositorio para registros de uso
/// </summary>
public interface IUsageRecordRepository
{
    Task<UsageRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageRecord>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageRecord>> GetByBillingPeriodAsync(int billingPeriod, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageRecord>> GetByProviderAsync(BackgroundRemovalProvider provider, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<UsageRecord> CreateAsync(UsageRecord record, CancellationToken cancellationToken = default);
    
    // Agregaciones para reportes
    Task<decimal> GetTotalCostByUserAsync(Guid userId, int billingPeriod, CancellationToken cancellationToken = default);
    Task<int> GetTotalRequestsByUserAsync(Guid userId, int billingPeriod, CancellationToken cancellationToken = default);
    Task<Dictionary<BackgroundRemovalProvider, int>> GetRequestCountByProviderAsync(int billingPeriod, CancellationToken cancellationToken = default);
}
