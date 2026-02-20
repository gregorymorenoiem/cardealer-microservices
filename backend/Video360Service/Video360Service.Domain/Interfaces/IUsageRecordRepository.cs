using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;

namespace Video360Service.Domain.Interfaces;

/// <summary>
/// Repositorio para registros de uso
/// </summary>
public interface IUsageRecordRepository
{
    Task<UsageRecord> CreateAsync(UsageRecord record, CancellationToken cancellationToken = default);
    Task<UsageRecord> AddAsync(UsageRecord record, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageRecord>> GetByUserIdAsync(Guid userId, string? billingPeriod = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageRecord>> GetByTenantIdAsync(string tenantId, string? billingPeriod = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageRecord>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalCostByUserAsync(Guid userId, string billingPeriod, CancellationToken cancellationToken = default);
    Task<int> GetUsageCountByProviderAsync(Video360Provider provider, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsageRecord>> GetUnbilledAsync(string billingPeriod, CancellationToken cancellationToken = default);
    Task MarkAsBilledAsync(IEnumerable<Guid> recordIds, string invoiceId, CancellationToken cancellationToken = default);
}
