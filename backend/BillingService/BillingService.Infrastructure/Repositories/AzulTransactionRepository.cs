using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;
using BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Repositories;

public class AzulTransactionRepository : IAzulTransactionRepository
{
    private readonly BillingDbContext _context;

    public AzulTransactionRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<AzulTransaction?> GetByIdAsync(Guid id)
    {
        return await _context.AzulTransactions
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<AzulTransaction?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _context.AzulTransactions
            .FirstOrDefaultAsync(t => t.OrderNumber == orderNumber);
    }

    public async Task<AzulTransaction?> GetByAzulOrderIdAsync(string azulOrderId)
    {
        return await _context.AzulTransactions
            .FirstOrDefaultAsync(t => t.AzulOrderId == azulOrderId);
    }

    public async Task<List<AzulTransaction>> GetByUserIdAsync(Guid userId)
    {
        return await _context.AzulTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<AzulTransaction>> GetApprovedTransactionsAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.AzulTransactions
            .Where(t => t.Status == "Approved");

        if (from.HasValue)
            query = query.Where(t => t.TransactionDateTime >= from.Value);

        if (to.HasValue)
            query = query.Where(t => t.TransactionDateTime <= to.Value);

        return await query
            .OrderByDescending(t => t.TransactionDateTime)
            .ToListAsync();
    }

    public async Task<AzulTransaction> CreateAsync(AzulTransaction transaction)
    {
        transaction.Id = Guid.NewGuid();
        transaction.CreatedAt = DateTime.UtcNow;

        _context.AzulTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<AzulTransaction> UpdateAsync(AzulTransaction transaction)
    {
        _context.AzulTransactions.Update(transaction);
        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<bool> ExistsAsync(string orderNumber)
    {
        return await _context.AzulTransactions
            .AnyAsync(t => t.OrderNumber == orderNumber);
    }
}
