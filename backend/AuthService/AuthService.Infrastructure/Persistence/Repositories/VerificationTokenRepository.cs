using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Persistence.Repositories;

public class VerificationTokenRepository : IVerificationTokenRepository
{
    private readonly ApplicationDbContext _context;

    public VerificationTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VerificationToken?> GetByTokenAsync(string token)
    {
        return await _context.VerificationTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(vt => vt.Token == token);
    }

    public async Task<VerificationToken?> GetByTokenAndTypeAsync(string token, VerificationTokenType type)
    {
        return await _context.VerificationTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(vt => vt.Token == token && vt.Type == type);
    }

    public async Task<IEnumerable<VerificationToken>> GetByEmailAsync(string email)
    {
        return await _context.VerificationTokens
            .AsNoTracking()
            .Where(vt => vt.Email == email)
            .OrderByDescending(vt => vt.CreatedAt)
            .ToListAsync();
    }

    public async Task<VerificationToken?> GetValidByEmailAndTypeAsync(string email, VerificationTokenType type)
    {
        // Performance: Replace IsValid() client-side evaluation with server-evaluatable expression
        var now = DateTime.UtcNow;
        return await _context.VerificationTokens
            .AsNoTracking()
            .Where(vt => vt.Email == email &&
                        vt.Type == type &&
                        vt.ExpiresAt > now &&
                        !vt.IsUsed)
            .OrderByDescending(vt => vt.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(VerificationToken token)
    {
        await _context.VerificationTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(VerificationToken token)
    {
        _context.VerificationTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var token = await _context.VerificationTokens.FindAsync(id);
        if (token != null)
        {
            _context.VerificationTokens.Remove(token);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteExpiredTokensAsync()
    {
        // Performance: Use ExecuteDeleteAsync instead of loading entities into memory
        var now = DateTime.UtcNow;
        await _context.VerificationTokens
            .Where(vt => vt.ExpiresAt <= now)
            .ExecuteDeleteAsync();
    }

    public async Task<bool> ExistsValidTokenAsync(string email, VerificationTokenType type)
    {
        // Performance: Replace IsValid() client-side evaluation with server-evaluatable expression
        var now = DateTime.UtcNow;
        return await _context.VerificationTokens
            .AsNoTracking()
            .AnyAsync(vt => vt.Email == email &&
                           vt.Type == type &&
                           vt.ExpiresAt > now &&
                           !vt.IsUsed);
    }
}
