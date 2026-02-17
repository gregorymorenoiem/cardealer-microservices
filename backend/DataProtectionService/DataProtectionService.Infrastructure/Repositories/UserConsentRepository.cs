using Microsoft.EntityFrameworkCore;
using DataProtectionService.Domain.Entities;
using DataProtectionService.Domain.Interfaces;
using DataProtectionService.Infrastructure.Persistence;

namespace DataProtectionService.Infrastructure.Repositories;

public class UserConsentRepository : IUserConsentRepository
{
    private readonly DataProtectionDbContext _context;

    public UserConsentRepository(DataProtectionDbContext context)
    {
        _context = context;
    }

    public async Task<UserConsent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserConsents
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<UserConsent>> GetByUserIdAsync(Guid userId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.UserConsents.Where(c => c.UserId == userId);
        
        if (activeOnly)
        {
            query = query.Where(c => c.RevokedAt == null);
        }

        return await query.OrderByDescending(c => c.GrantedAt).ToListAsync(cancellationToken);
    }

    public async Task<UserConsent?> GetActiveConsentAsync(Guid userId, string type, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ConsentType>(type, true, out var consentType))
            return null;

        return await _context.UserConsents
            .Where(c => c.UserId == userId && c.Type == consentType && c.RevokedAt == null)
            .OrderByDescending(c => c.GrantedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasActiveConsentAsync(Guid userId, string type, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ConsentType>(type, true, out var consentType))
            return false;

        return await _context.UserConsents
            .AnyAsync(c => c.UserId == userId && c.Type == consentType && c.RevokedAt == null && c.Granted, cancellationToken);
    }

    public async Task AddAsync(UserConsent consent, CancellationToken cancellationToken = default)
    {
        await _context.UserConsents.AddAsync(consent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserConsent consent, CancellationToken cancellationToken = default)
    {
        _context.UserConsents.Update(consent);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetConsentCountAsync(Guid userId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.UserConsents.Where(c => c.UserId == userId);
        
        if (activeOnly)
        {
            query = query.Where(c => c.RevokedAt == null);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetConsentStatsByTypeAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserConsents
            .Where(c => c.RevokedAt == null)
            .GroupBy(c => c.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);
    }
}
