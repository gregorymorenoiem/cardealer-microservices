using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly ApplicationDbContext _context;

    public UserSessionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<UserSession?> GetByRefreshTokenIdAsync(string refreshTokenId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .FirstOrDefaultAsync(s => s.RefreshTokenId == refreshTokenId && !s.IsRevoked, cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && (!s.ExpiresAt.HasValue || s.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(s => s.LastActiveAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetAllSessionsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastActiveAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveSessionCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .CountAsync(s => s.UserId == userId && !s.IsRevoked && (!s.ExpiresAt.HasValue || s.ExpiresAt > DateTime.UtcNow), cancellationToken);
    }

    public async Task AddAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        await _context.UserSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        _context.UserSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeSessionAsync(Guid sessionId, string reason = "User requested", CancellationToken cancellationToken = default)
    {
        var session = await GetByIdAsync(sessionId, cancellationToken);
        if (session != null)
        {
            session.Revoke(reason);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllUserSessionsAsync(string userId, Guid? exceptSessionId = null, string reason = "User requested", CancellationToken cancellationToken = default)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            if (exceptSessionId.HasValue && session.Id == exceptSessionId.Value)
                continue;

            session.Revoke(reason);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions.AnyAsync(s => s.Id == id, cancellationToken);
    }
}
