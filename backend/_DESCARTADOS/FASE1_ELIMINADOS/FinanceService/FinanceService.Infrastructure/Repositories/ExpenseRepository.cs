using Microsoft.EntityFrameworkCore;
using FinanceService.Domain.Entities;
using FinanceService.Domain.Interfaces;
using FinanceService.Infrastructure.Persistence;

namespace FinanceService.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly FinanceDbContext _context;

    public ExpenseRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Expense?> GetByExpenseNumberAsync(string expenseNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Expenses
            .FirstOrDefaultAsync(e => e.ExpenseNumber == expenseNumber, cancellationToken);
    }

    public async Task<IEnumerable<Expense>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Expenses
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Expense>> GetByStatusAsync(ExpenseStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Expenses
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Expense>> GetByCategoryAsync(ExpenseCategory category, CancellationToken cancellationToken = default)
    {
        return await _context.Expenses
            .Where(e => e.Category == category)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Expenses
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Expense>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Expenses
            .Where(e => e.Status == ExpenseStatus.Submitted)
            .OrderBy(e => e.ExpenseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Expense>> GetByVendorAsync(string vendor, CancellationToken cancellationToken = default)
    {
        return await _context.Expenses
            .Where(e => e.Vendor == vendor)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);
        return expense;
    }

    public async Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        _context.Expenses.Update(expense);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expense = await GetByIdAsync(id, cancellationToken);
        if (expense != null)
        {
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Expenses.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<decimal> GetTotalByPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Expenses
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate && e.Status == ExpenseStatus.Paid)
            .SumAsync(e => e.Amount, cancellationToken);
    }

    public async Task<Dictionary<ExpenseCategory, decimal>> GetTotalsByCategoryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Expenses
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate && e.Status == ExpenseStatus.Paid)
            .GroupBy(e => e.Category)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Amount), cancellationToken);
    }
}
