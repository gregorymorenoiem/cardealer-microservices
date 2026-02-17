using InvoicingService.Domain.Entities;
using InvoicingService.Domain.Interfaces;
using InvoicingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoicingService.Infrastructure.Repositories;

public class QuoteRepository : IQuoteRepository
{
    private readonly InvoicingDbContext _context;

    public QuoteRepository(InvoicingDbContext context)
    {
        _context = context;
    }

    public async Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<Quote?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .Include(q => q.Items.OrderBy(item => item.SortOrder))
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<Quote?> GetByQuoteNumberAsync(string quoteNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .Include(q => q.Items.OrderBy(item => item.SortOrder))
            .FirstOrDefaultAsync(q => q.QuoteNumber == quoteNumber, cancellationToken);
    }

    public async Task<IEnumerable<Quote>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Quote>> GetByStatusAsync(QuoteStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .Where(q => q.Status == status)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Quote>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .Where(q => q.CustomerId == customerId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Quote>> GetExpiringAsync(int days, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var expirationDate = today.AddDays(days);

        return await _context.Quotes
            .Where(q => q.ValidUntil <= expirationDate &&
                       q.ValidUntil >= today &&
                       (q.Status == QuoteStatus.Draft ||
                        q.Status == QuoteStatus.Sent ||
                        q.Status == QuoteStatus.Viewed))
            .OrderBy(q => q.ValidUntil)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Quote>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .Where(q => q.IssueDate >= from && q.IssueDate <= to)
            .OrderByDescending(q => q.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Quote>> GetByDealAsync(Guid dealId, CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .Where(q => q.DealId == dealId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Quote>> GetByLeadAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .Where(q => q.LeadId == leadId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByStatusAsync(QuoteStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Quotes
            .CountAsync(q => q.Status == status, cancellationToken);
    }

    public async Task<string> GenerateQuoteNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Quotes
            .CountAsync(q => q.CreatedAt.Year == year, cancellationToken);

        return $"QT-{year}-{(count + 1):D6}";
    }

    public async Task AddAsync(Quote quote, CancellationToken cancellationToken = default)
    {
        await _context.Quotes.AddAsync(quote, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Quote quote, CancellationToken cancellationToken = default)
    {
        _context.Quotes.Update(quote);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quote = await GetByIdAsync(id, cancellationToken);
        if (quote != null)
        {
            _context.Quotes.Remove(quote);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
