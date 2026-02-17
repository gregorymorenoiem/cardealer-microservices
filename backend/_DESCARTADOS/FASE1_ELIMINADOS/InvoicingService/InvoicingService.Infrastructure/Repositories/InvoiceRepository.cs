using InvoicingService.Domain.Entities;
using InvoicingService.Domain.Interfaces;
using InvoicingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoicingService.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoicingDbContext _context;

    public InvoiceRepository(InvoicingDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Items.OrderBy(item => item.SortOrder))
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Items.OrderBy(item => item.SortOrder))
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Invoices
            .Where(i => i.DueDate < today &&
                       (i.Status == InvoiceStatus.Sent ||
                        i.Status == InvoiceStatus.PartiallyPaid ||
                        i.Status == InvoiceStatus.Pending))
            .OrderBy(i => i.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => i.IssueDate >= from && i.IssueDate <= to)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Invoice>> GetByDealAsync(Guid dealId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => i.DealId == dealId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid);

        if (from.HasValue)
            query = query.Where(i => i.IssueDate >= from.Value);

        if (to.HasValue)
            query = query.Where(i => i.IssueDate <= to.Value);

        return await query.SumAsync(i => i.Total, cancellationToken);
    }

    public async Task<int> GetCountByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .CountAsync(i => i.Status == status, cancellationToken);
    }

    public async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Invoices
            .CountAsync(i => i.CreatedAt.Year == year, cancellationToken);

        return $"INV-{year}-{(count + 1):D6}";
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(id, cancellationToken);
        if (invoice != null)
        {
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
