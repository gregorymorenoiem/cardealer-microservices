using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces.Repositories;

/// <summary>
/// Repositorio para historial de logins
/// </summary>
public interface ILoginHistoryRepository
{
    Task<LoginHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoginHistory>> GetByUserIdAsync(string userId, int limit = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoginHistory>> GetRecentFailedAttemptsAsync(string userId, TimeSpan window, CancellationToken cancellationToken = default);
    Task<int> GetFailedAttemptsCountAsync(string userId, TimeSpan window, CancellationToken cancellationToken = default);
    Task<LoginHistory?> GetLastSuccessfulLoginAsync(string userId, CancellationToken cancellationToken = default);
    Task<LoginHistory?> GetLastLoginAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(LoginHistory loginHistory, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoginHistory>> GetByIpAddressAsync(string ipAddress, TimeSpan window, CancellationToken cancellationToken = default);
}
