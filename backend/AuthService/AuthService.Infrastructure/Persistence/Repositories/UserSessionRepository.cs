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
        var now = DateTime.UtcNow;
        return await _context.UserSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && !s.IsRevoked && (!s.ExpiresAt.HasValue || s.ExpiresAt > now))
            .OrderByDescending(s => s.LastActiveAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetAllSessionsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastActiveAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveSessionCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.UserSessions
            .AsNoTracking()
            .CountAsync(s => s.UserId == userId && !s.IsRevoked && (!s.ExpiresAt.HasValue || s.ExpiresAt > now), cancellationToken);
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
        // Performance: Use ExecuteUpdateAsync to bulk-update without loading entities into memory
        var now = DateTime.UtcNow;
        var query = _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked);

        if (exceptSessionId.HasValue)
            query = query.Where(s => s.Id != exceptSessionId.Value);

        await query.ExecuteUpdateAsync(setters => setters
            .SetProperty(s => s.IsRevoked, true)
            .SetProperty(s => s.RevokedAt, now)
            .SetProperty(s => s.RevokedReason, reason),
            cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .AnyAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserSession?> GetActiveSessionByDeviceAsync(
        string userId,
        string deviceInfo,
        string browser,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.UserSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId 
                && s.DeviceInfo == deviceInfo 
                && s.Browser == browser 
                && s.IpAddress == ipAddress 
                && !s.IsRevoked 
                && (!s.ExpiresAt.HasValue || s.ExpiresAt > now))
            .OrderByDescending(s => s.LastActiveAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
