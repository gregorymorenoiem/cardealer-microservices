using Microsoft.EntityFrameworkCore;
using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Interfaces;

namespace BankReconciliationService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing reconciliations
/// </summary>
public class ReconciliationRepository : IReconciliationRepository
{
    private readonly BankReconciliationDbContext _context;

    public ReconciliationRepository(BankReconciliationDbContext context)
    {
        _context = context;
    }

    public async Task<Reconciliation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reconciliations
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Reconciliation?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reconciliations
            .Include(r => r.BankStatement)
                .ThenInclude(s => s.Lines)
            .Include(r => r.Matches)
                .ThenInclude(m => m.BankLine)
            .Include(r => r.Matches)
                .ThenInclude(m => m.InternalTransaction)
            .Include(r => r.Discrepancies)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Reconciliation>> GetByBankAccountAsync(
        Guid bankAccountConfigId, 
        CancellationToken cancellationToken = default)
    {
        // Get the bank account config to find the bank code and account number
        var config = await _context.BankAccountConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == bankAccountConfigId, cancellationToken);

        if (config == null)
            return Enumerable.Empty<Reconciliation>();

        return await _context.Reconciliations
            .Include(r => r.BankStatement)
            .Where(r => r.BankStatement.BankCode == config.BankCode 
                     && r.BankStatement.AccountNumber == config.AccountNumber)
            .OrderByDescending(r => r.ReconciliationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reconciliation>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Reconciliations
            .Include(r => r.BankStatement)
            .Where(r => r.ReconciliationDate >= startDate && r.ReconciliationDate <= endDate)
            .OrderByDescending(r => r.ReconciliationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reconciliation?> GetLatestByBankAccountAsync(
        Guid bankAccountConfigId, 
        CancellationToken cancellationToken = default)
    {
        // Get the bank account config
        var config = await _context.BankAccountConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == bankAccountConfigId, cancellationToken);

        if (config == null)
            return null;

        return await _context.Reconciliations
            .Include(r => r.BankStatement)
            .Where(r => r.BankStatement.BankCode == config.BankCode 
                     && r.BankStatement.AccountNumber == config.AccountNumber)
            .OrderByDescending(r => r.ReconciliationDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Reconciliation> AddAsync(
        Reconciliation reconciliation, 
        CancellationToken cancellationToken = default)
    {
        await _context.Reconciliations.AddAsync(reconciliation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return reconciliation;
    }

    public async Task UpdateAsync(
        Reconciliation reconciliation, 
        CancellationToken cancellationToken = default)
    {
        _context.Reconciliations.Update(reconciliation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ReconciliationMatch> AddMatchAsync(
        ReconciliationMatch match, 
        CancellationToken cancellationToken = default)
    {
        await _context.ReconciliationMatches.AddAsync(match, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return match;
    }

    public async Task<ReconciliationDiscrepancy> AddDiscrepancyAsync(
        ReconciliationDiscrepancy discrepancy, 
        CancellationToken cancellationToken = default)
    {
        await _context.ReconciliationDiscrepancies.AddAsync(discrepancy, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return discrepancy;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reconciliation = await _context.Reconciliations
            .Include(r => r.Matches)
            .Include(r => r.Discrepancies)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (reconciliation != null)
        {
            // Remove related matches and discrepancies first
            _context.ReconciliationMatches.RemoveRange(reconciliation.Matches);
            _context.ReconciliationDiscrepancies.RemoveRange(reconciliation.Discrepancies);
            _context.Reconciliations.Remove(reconciliation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
