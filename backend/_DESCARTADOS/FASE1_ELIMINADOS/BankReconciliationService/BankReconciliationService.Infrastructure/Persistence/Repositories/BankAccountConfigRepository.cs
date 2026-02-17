using Microsoft.EntityFrameworkCore;
using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Interfaces;

namespace BankReconciliationService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing bank account configurations
/// </summary>
public class BankAccountConfigRepository : IBankAccountConfigRepository
{
    private readonly BankReconciliationDbContext _context;

    public BankAccountConfigRepository(BankReconciliationDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccountConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BankAccountConfigs
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<BankAccountConfig?> GetByAccountNumberAsync(
        string bankCode, 
        string accountNumber, 
        CancellationToken cancellationToken = default)
    {
        return await _context.BankAccountConfigs
            .FirstOrDefaultAsync(c => c.BankCode == bankCode 
                                   && c.AccountNumber == accountNumber, 
                cancellationToken);
    }

    public async Task<IEnumerable<BankAccountConfig>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BankAccountConfigs
            .OrderBy(c => c.BankName)
            .ThenBy(c => c.AccountNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BankAccountConfig>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.BankAccountConfigs
            .Where(c => c.IsActive)
            .OrderBy(c => c.BankName)
            .ThenBy(c => c.AccountNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BankAccountConfig>> GetByBankCodeAsync(
        string bankCode, 
        CancellationToken cancellationToken = default)
    {
        return await _context.BankAccountConfigs
            .Where(c => c.BankCode == bankCode)
            .OrderBy(c => c.AccountNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<BankAccountConfig> AddAsync(
        BankAccountConfig config, 
        CancellationToken cancellationToken = default)
    {
        config.CreatedAt = DateTime.UtcNow;
        await _context.BankAccountConfigs.AddAsync(config, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return config;
    }

    public async Task UpdateAsync(
        BankAccountConfig config, 
        CancellationToken cancellationToken = default)
    {
        config.UpdatedAt = DateTime.UtcNow;
        _context.BankAccountConfigs.Update(config);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var config = await _context.BankAccountConfigs.FindAsync(new object[] { id }, cancellationToken);
        if (config != null)
        {
            _context.BankAccountConfigs.Remove(config);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(
        string bankCode, 
        string accountNumber, 
        CancellationToken cancellationToken = default)
    {
        return await _context.BankAccountConfigs
            .AnyAsync(c => c.BankCode == bankCode && c.AccountNumber == accountNumber, 
                cancellationToken);
    }
}
