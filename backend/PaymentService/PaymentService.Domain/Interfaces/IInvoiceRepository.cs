namespace PaymentService.Domain.Interfaces;

using PaymentService.Domain.Entities;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Invoice?> GetByPaymentTransactionIdAsync(Guid paymentTransactionId, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetByDealerIdAsync(Guid dealerId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetCountByDealerIdAsync(Guid dealerId, CancellationToken ct = default);
    Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default);
    Task AddAsync(Invoice invoice, CancellationToken ct = default);
    Task UpdateAsync(Invoice invoice, CancellationToken ct = default);
}
