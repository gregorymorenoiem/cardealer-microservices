using Microsoft.EntityFrameworkCore;
using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Interfaces;

namespace BankReconciliationService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing bank statements
/// </summary>
public class BankStatementRepository : IBankStatementRepository
{
    private readonly BankReconciliationDbContext _context;

    public BankStatementRepository(BankReconciliationDbContext context)
    {
        _context = context;
    }

    public async Task<BankStatement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BankStatements
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<BankStatement>> GetByBankAccountAsync(
        Guid bankAccountConfigId, 
        CancellationToken cancellationToken = default)
    {
        // Get the bank account config to find the bank code and account number
        var config = await _context.BankAccountConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == bankAccountConfigId, cancellationToken);

        if (config == null)
            return Enumerable.Empty<BankStatement>();

        return await _context.BankStatements
            .Include(s => s.Lines)
            .Where(s => s.BankCode == config.BankCode && s.AccountNumber == config.AccountNumber)
            .OrderByDescending(s => s.StatementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BankStatement>> GetByDateRangeAsync(
        Guid bankAccountConfigId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        // Get the bank account config to find the bank code and account number
        var config = await _context.BankAccountConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == bankAccountConfigId, cancellationToken);

        if (config == null)
            return Enumerable.Empty<BankStatement>();

        return await _context.BankStatements
            .Include(s => s.Lines)
            .Where(s => s.BankCode == config.BankCode 
                     && s.AccountNumber == config.AccountNumber
                     && s.StatementDate >= startDate 
                     && s.StatementDate <= endDate)
            .OrderByDescending(s => s.StatementDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<BankStatement?> GetByFileHashAsync(string fileHash, CancellationToken cancellationToken = default)
    {
        // FileHash is not a property on BankStatement, use ApiTransactionId as unique identifier instead
        return await _context.BankStatements
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.ApiTransactionId == fileHash, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid bankAccountConfigId, 
        DateTime statementDate, 
        CancellationToken cancellationToken = default)
    {
        // Get the bank account config
        var config = await _context.BankAccountConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == bankAccountConfigId, cancellationToken);

        if (config == null)
            return false;

        return await _context.BankStatements
            .AnyAsync(s => s.BankCode == config.BankCode 
                        && s.AccountNumber == config.AccountNumber 
                        && s.StatementDate.Date == statementDate.Date, 
                cancellationToken);
    }

    public async Task<BankStatement> AddAsync(BankStatement statement, CancellationToken cancellationToken = default)
    {
        await _context.BankStatements.AddAsync(statement, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return statement;
    }

    public async Task UpdateAsync(BankStatement statement, CancellationToken cancellationToken = default)
    {
        _context.BankStatements.Update(statement);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var statement = await _context.BankStatements.FindAsync(new object[] { id }, cancellationToken);
        if (statement != null)
        {
            _context.BankStatements.Remove(statement);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetUnreconciledCountAsync(
        Guid bankAccountConfigId, 
        CancellationToken cancellationToken = default)
    {
        // Get the bank account config
        var config = await _context.BankAccountConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == bankAccountConfigId, cancellationToken);

        if (config == null)
            return 0;

        return await _context.BankStatementLines
            .CountAsync(l => l.BankStatement.BankCode == config.BankCode 
                          && l.BankStatement.AccountNumber == config.AccountNumber 
                          && !l.IsReconciled, 
                cancellationToken);
    }

    public async Task<IEnumerable<BankStatementLine>> GetUnreconciledLinesAsync(
        Guid bankAccountConfigId, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default)
    {
        // Get the bank account config
        var config = await _context.BankAccountConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == bankAccountConfigId, cancellationToken);

        if (config == null)
            return Enumerable.Empty<BankStatementLine>();

        var query = _context.BankStatementLines
            .Include(l => l.BankStatement)
            .Where(l => l.BankStatement.BankCode == config.BankCode 
                     && l.BankStatement.AccountNumber == config.AccountNumber 
                     && !l.IsReconciled);

        if (startDate.HasValue)
            query = query.Where(l => l.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.TransactionDate <= endDate.Value);

        return await query
            .OrderBy(l => l.TransactionDate)
            .ToListAsync(cancellationToken);
    }
}
