using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces.Repositories;

/// <summary>
/// Repositorio para gestión de sesiones de usuario
/// </summary>
public interface IUserSessionRepository
{
    Task<UserSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserSession?> GetByRefreshTokenIdAsync(string refreshTokenId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSession>> GetActiveSessionsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSession>> GetAllSessionsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetActiveSessionCountAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserSession session, CancellationToken cancellationToken = default);
    Task RevokeSessionAsync(Guid sessionId, string reason = "User requested", CancellationToken cancellationToken = default);
    Task RevokeAllUserSessionsAsync(string userId, Guid? exceptSessionId = null, string reason = "User requested", CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
