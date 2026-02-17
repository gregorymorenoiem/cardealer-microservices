using InvoicingService.Domain.Entities;

namespace InvoicingService.Domain.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetOverdueAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invoice>> GetByDealAsync(Guid dealId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default);
    Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
