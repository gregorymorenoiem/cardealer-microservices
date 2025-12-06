using FinanceService.Domain.Entities;

namespace FinanceService.Domain.Interfaces;

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Expense?> GetByExpenseNumberAsync(string expenseNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Expense>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Expense>> GetByStatusAsync(ExpenseStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Expense>> GetByCategoryAsync(ExpenseCategory category, CancellationToken cancellationToken = default);
    Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Expense>> GetByVendorAsync(string vendor, CancellationToken cancellationToken = default);
    Task<IEnumerable<Expense>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Dictionary<ExpenseCategory, decimal>> GetTotalsByCategoryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default);
    Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
