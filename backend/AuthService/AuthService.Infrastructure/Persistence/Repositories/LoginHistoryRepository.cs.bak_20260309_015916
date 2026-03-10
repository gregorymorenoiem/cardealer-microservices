using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence.Repositories;

public class LoginHistoryRepository : ILoginHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public LoginHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LoginHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.LoginHistories
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<LoginHistory>> GetByUserIdAsync(string userId, int limit = 20, CancellationToken cancellationToken = default)
    {
        return await _context.LoginHistories
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.LoginTime)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoginHistory>> GetRecentFailedAttemptsAsync(string userId, TimeSpan window, CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow - window;
        return await _context.LoginHistories
            .Where(l => l.UserId == userId && !l.Success && l.LoginTime >= since)
            .OrderByDescending(l => l.LoginTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetFailedAttemptsCountAsync(string userId, TimeSpan window, CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow - window;
        return await _context.LoginHistories
            .CountAsync(l => l.UserId == userId && !l.Success && l.LoginTime >= since, cancellationToken);
    }

    public async Task<LoginHistory?> GetLastSuccessfulLoginAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.LoginHistories
            .Where(l => l.UserId == userId && l.Success)
            .OrderByDescending(l => l.LoginTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<LoginHistory?> GetLastLoginAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.LoginHistories
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.LoginTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(LoginHistory loginHistory, CancellationToken cancellationToken = default)
    {
        await _context.LoginHistories.AddAsync(loginHistory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<LoginHistory>> GetByIpAddressAsync(string ipAddress, TimeSpan window, CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow - window;
        return await _context.LoginHistories
            .Where(l => l.IpAddress == ipAddress && l.LoginTime >= since)
            .OrderByDescending(l => l.LoginTime)
            .ToListAsync(cancellationToken);
    }
}
