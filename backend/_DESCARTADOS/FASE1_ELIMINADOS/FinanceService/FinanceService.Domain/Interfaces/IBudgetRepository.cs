using FinanceService.Domain.Entities;

namespace FinanceService.Domain.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Budget>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Budget>> GetByYearAsync(int year, CancellationToken cancellationToken = default);
    Task<IEnumerable<Budget>> GetByPeriodAsync(BudgetPeriod period, CancellationToken cancellationToken = default);
    Task<IEnumerable<Budget>> GetActiveBudgetsAsync(CancellationToken cancellationToken = default);
    Task<Budget?> GetCurrentBudgetAsync(string name, CancellationToken cancellationToken = default);
    Task<Budget> AddAsync(Budget budget, CancellationToken cancellationToken = default);
    Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsForYearAsync(string name, int year, CancellationToken cancellationToken = default);
}
