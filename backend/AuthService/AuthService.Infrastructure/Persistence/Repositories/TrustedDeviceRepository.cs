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
            .FirstOrDefaultAsync(d => d.UserId == userId && d.FingerprintHash == fingerprintHash, cancellationToken);
    }

    public async Task<IReadOnlyList<TrustedDevice>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.TrustedDevices
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastUsedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TrustedDevice>> GetTrustedByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.TrustedDevices
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
        var devices = await _context.TrustedDevices
            .Where(d => d.UserId == userId && d.IsTrusted)
            .ToListAsync(cancellationToken);

        foreach (var device in devices)
        {
            device.Revoke(reason);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TrustedDevice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TrustedDevices.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var device = await GetByIdAsync(id, cancellationToken);
        if (device != null)
        {
            _context.TrustedDevices.Remove(device);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> CountTrustedDevicesAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.TrustedDevices
            .CountAsync(d => d.UserId == userId && d.IsTrusted, cancellationToken);
    }
}
