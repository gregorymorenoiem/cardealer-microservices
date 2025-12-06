using Microsoft.EntityFrameworkCore;
using MarketingService.Domain.Entities;
using MarketingService.Domain.Interfaces;
using MarketingService.Infrastructure.Persistence;

namespace MarketingService.Infrastructure.Repositories;

public class AudienceRepository : IAudienceRepository
{
    private readonly MarketingDbContext _context;

    public AudienceRepository(MarketingDbContext context)
    {
        _context = context;
    }

    public async Task<Audience?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Audiences.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Audience?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Audiences
            .Include(a => a.Members)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Audience>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Audiences.OrderBy(a => a.Name).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Audience>> GetByTypeAsync(AudienceType type, CancellationToken cancellationToken = default)
    {
        return await _context.Audiences
            .Where(a => a.Type == type)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Audience>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Audiences
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Audience> AddAsync(Audience audience, CancellationToken cancellationToken = default)
    {
        _context.Audiences.Add(audience);
        await _context.SaveChangesAsync(cancellationToken);
        return audience;
    }

    public async Task UpdateAsync(Audience audience, CancellationToken cancellationToken = default)
    {
        _context.Audiences.Update(audience);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var audience = await GetByIdAsync(id, cancellationToken);
        if (audience != null)
        {
            _context.Audiences.Remove(audience);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Audiences.AnyAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<int> GetMemberCountAsync(Guid audienceId, CancellationToken cancellationToken = default)
    {
        return await _context.AudienceMembers.CountAsync(m => m.AudienceId == audienceId, cancellationToken);
    }

    public async Task AddMemberAsync(AudienceMember member, CancellationToken cancellationToken = default)
    {
        _context.AudienceMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        var member = await _context.AudienceMembers.FindAsync(new object[] { memberId }, cancellationToken);
        if (member != null)
        {
            _context.AudienceMembers.Remove(member);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
