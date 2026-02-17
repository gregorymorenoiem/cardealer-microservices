using Microsoft.EntityFrameworkCore;
using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Interfaces;

namespace BankReconciliationService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing internal transactions
/// </summary>
public class InternalTransactionRepository : IInternalTransactionRepository
{
    private readonly BankReconciliationDbContext _context;

    public InternalTransactionRepository(BankReconciliationDbContext context)
    {
        _context = context;
    }

    public async Task<InternalTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.InternalTransactions
            .Include(t => t.Matches)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<InternalTransaction>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.InternalTransactions
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InternalTransaction>> GetUnreconciledAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.InternalTransactions
            .Where(t => !t.IsReconciled);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        return await query
            .OrderBy(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<InternalTransaction?> GetByReferenceAsync(
        string reference, 
        CancellationToken cancellationToken = default)
    {
        return await _context.InternalTransactions
            .FirstOrDefaultAsync(t => t.ReferenceNumber == reference, cancellationToken);
    }

    public async Task<IEnumerable<InternalTransaction>> FindPotentialMatchesAsync(
        decimal amount, 
        DateTime transactionDate, 
        int daysTolerance = 3, 
        CancellationToken cancellationToken = default)
    {
        var minDate = transactionDate.AddDays(-daysTolerance);
        var maxDate = transactionDate.AddDays(daysTolerance);
        
        // Use absolute value comparison for amount matching
        var minAmount = Math.Abs(amount) * 0.99m; // 1% tolerance
        var maxAmount = Math.Abs(amount) * 1.01m;

        return await _context.InternalTransactions
            .Where(t => !t.IsReconciled
                     && t.TransactionDate >= minDate 
                     && t.TransactionDate <= maxDate
                     && Math.Abs(t.Amount) >= minAmount 
                     && Math.Abs(t.Amount) <= maxAmount)
            .OrderBy(t => Math.Abs(t.Amount - Math.Abs(amount)))
            .ThenBy(t => Math.Abs((t.TransactionDate - transactionDate).Days))
            .ToListAsync(cancellationToken);
    }

    public async Task<InternalTransaction> AddAsync(
        InternalTransaction transaction, 
        CancellationToken cancellationToken = default)
    {
        await _context.InternalTransactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task UpdateAsync(
        InternalTransaction transaction, 
        CancellationToken cancellationToken = default)
    {
        _context.InternalTransactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsReconciledAsync(
        Guid id, 
        Guid reconciliationMatchId, 
        CancellationToken cancellationToken = default)
    {
        var transaction = await _context.InternalTransactions.FindAsync(
            new object[] { id }, 
            cancellationToken);
        
        if (transaction != null)
        {
            transaction.IsReconciled = true;
            transaction.ReconciledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
