using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Use AsNoTracking for read-only query
        var now = DateTime.UtcNow;
        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > now)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllForUserAsync(string userId, string reason, CancellationToken cancellationToken = default)
    {
        // Performance: Use ExecuteUpdateAsync to bulk-update without loading entities into memory
        var now = DateTime.UtcNow;
        await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(rt => rt.RevokedAt, now)
                .SetProperty(rt => rt.RevokedByIp, "system")
                .SetProperty(rt => rt.RevokedReason, reason),
                cancellationToken);
    }

    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        // Performance: Use ExecuteDeleteAsync to delete without loading entities into memory
        var now = DateTime.UtcNow;
        await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt <= now || rt.RevokedAt != null)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
