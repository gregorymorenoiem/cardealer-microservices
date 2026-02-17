using Microsoft.EntityFrameworkCore;
using FinanceService.Domain.Entities;
using FinanceService.Domain.Interfaces;
using FinanceService.Infrastructure.Persistence;

namespace FinanceService.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly FinanceDbContext _context;

    public BudgetRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Budget>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .OrderByDescending(b => b.Year)
            .ThenBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Budget>> GetByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .Where(b => b.Year == year)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Budget>> GetByPeriodAsync(BudgetPeriod period, CancellationToken cancellationToken = default)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .Where(b => b.Period == period)
            .OrderByDescending(b => b.Year)
            .ThenBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Budget>> GetActiveBudgetsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Budgets
            .Include(b => b.Categories)
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.Year)
            .ThenBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Budget?> GetCurrentBudgetAsync(string name, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Budgets
            .Include(b => b.Categories)
            .Where(b => b.Name == name && b.StartDate <= now && b.EndDate >= now && b.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Budget> AddAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync(cancellationToken);
        return budget;
    }

    public async Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var budget = await _context.Budgets.FindAsync(new object[] { id }, cancellationToken);
        if (budget != null)
        {
            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Budgets.AnyAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<bool> NameExistsForYearAsync(string name, int year, CancellationToken cancellationToken = default)
    {
        return await _context.Budgets.AnyAsync(b => b.Name == name && b.Year == year, cancellationToken);
    }
}
