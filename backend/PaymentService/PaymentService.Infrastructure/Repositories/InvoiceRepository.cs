using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AzulDbContext _db;

    public InvoiceRepository(AzulDbContext db)
    {
        _db = db;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<Invoice?> GetByPaymentTransactionIdAsync(Guid paymentTransactionId, CancellationToken ct = default)
        => await _db.Invoices.AsNoTracking()
            .FirstOrDefaultAsync(i => i.PaymentTransactionId == paymentTransactionId, ct);

    public async Task<IReadOnlyList<Invoice>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        => await _db.Invoices.AsNoTracking()
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Invoice>> GetByDealerIdAsync(Guid dealerId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        => await _db.Invoices.AsNoTracking()
            .Where(i => i.DealerId == dealerId)
            .OrderByDescending(i => i.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Invoices.CountAsync(i => i.UserId == userId, ct);

    public async Task<int> GetCountByDealerIdAsync(Guid dealerId, CancellationToken ct = default)
        => await _db.Invoices.CountAsync(i => i.DealerId == dealerId, ct);

    public async Task<string> GetNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"OKLA-{year}-";
        var lastInvoice = await _db.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(ct);

        var nextNumber = 1;
        if (lastInvoice != null)
        {
            var lastNumberStr = lastInvoice.InvoiceNumber.Replace(prefix, "");
            if (int.TryParse(lastNumberStr, out var lastNum))
                nextNumber = lastNum + 1;
        }

        return $"{prefix}{nextNumber:D6}";
    }

    public async Task AddAsync(Invoice invoice, CancellationToken ct = default)
    {
        await _db.Invoices.AddAsync(invoice, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        invoice.UpdatedAt = DateTime.UtcNow;
        _db.Invoices.Update(invoice);
        await _db.SaveChangesAsync(ct);
    }
}
