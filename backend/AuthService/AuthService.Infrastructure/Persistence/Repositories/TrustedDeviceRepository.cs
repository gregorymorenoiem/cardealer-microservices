using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence.Repositories;

/// <summary>
/// US-18.4: Repository implementation for trusted devices.
/// </summary>
public class TrustedDeviceRepository : ITrustedDeviceRepository
{
    private readonly ApplicationDbContext _context;

    public TrustedDeviceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TrustedDevice?> GetByFingerprintAsync(string userId, string fingerprintHash, CancellationToken cancellationToken = default)
    {
        return await _context.TrustedDevices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == userId && d.FingerprintHash == fingerprintHash, cancellationToken);
    }

    public async Task<IReadOnlyList<TrustedDevice>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.TrustedDevices
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastUsedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TrustedDevice>> GetTrustedByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.TrustedDevices
            .AsNoTracking()
            .Where(d => d.UserId == userId && d.IsTrusted)
            .OrderByDescending(d => d.LastUsedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TrustedDevice> AddAsync(TrustedDevice device, CancellationToken cancellationToken = default)
    {
        await _context.TrustedDevices.AddAsync(device, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return device;
    }

    public async Task UpdateAsync(TrustedDevice device, CancellationToken cancellationToken = default)
    {
        _context.TrustedDevices.Update(device);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllForUserAsync(string userId, string reason, CancellationToken cancellationToken = default)
    {
        // Performance: Use ExecuteUpdateAsync to bulk-update without loading entities into memory
        var now = DateTime.UtcNow;
        await _context.TrustedDevices
            .Where(d => d.UserId == userId && d.IsTrusted)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.IsTrusted, false)
                .SetProperty(d => d.RevokedAt, now)
                .SetProperty(d => d.RevokeReason, reason),
                cancellationToken);
    }

    public async Task<TrustedDevice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TrustedDevices.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Performance: Use ExecuteDeleteAsync to avoid loading entity first
        await _context.TrustedDevices
            .Where(d => d.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<int> CountTrustedDevicesAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.TrustedDevices
            .AsNoTracking()
            .CountAsync(d => d.UserId == userId && d.IsTrusted, cancellationToken);
    }
}
