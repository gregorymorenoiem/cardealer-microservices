using InvoicingService.Domain.Entities;

namespace InvoicingService.Domain.Interfaces;

public interface IQuoteRepository
{
    Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Quote?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Quote?> GetByQuoteNumberAsync(string quoteNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Quote>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Quote>> GetByStatusAsync(QuoteStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Quote>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Quote>> GetExpiringAsync(int days, CancellationToken cancellationToken = default);
    Task<IEnumerable<Quote>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<Quote>> GetByDealAsync(Guid dealId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Quote>> GetByLeadAsync(Guid leadId, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(QuoteStatus status, CancellationToken cancellationToken = default);
    Task<string> GenerateQuoteNumberAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Quote quote, CancellationToken cancellationToken = default);
    Task UpdateAsync(Quote quote, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
