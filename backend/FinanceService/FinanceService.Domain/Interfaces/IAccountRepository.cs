using FinanceService.Domain.Entities;

namespace FinanceService.Domain.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Account?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetByTypeAsync(AccountType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetChildAccountsAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
    Task<Account> AddAsync(Account account, CancellationToken cancellationToken = default);
    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
}
