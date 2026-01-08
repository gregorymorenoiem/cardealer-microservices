using Microsoft.EntityFrameworkCore;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;
using BillingService.Infrastructure.Persistence;

namespace BillingService.Infrastructure.Repositories;

public class EarlyBirdRepository : IEarlyBirdRepository
{
    private readonly BillingDbContext _context;

    public EarlyBirdRepository(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<EarlyBirdMember?> GetByIdAsync(Guid id)
    {
        return await _context.EarlyBirdMembers
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<EarlyBirdMember?> GetByUserIdAsync(Guid userId)
    {
        return await _context.EarlyBirdMembers
            .FirstOrDefaultAsync(m => m.UserId == userId);
    }

    public async Task<List<EarlyBirdMember>> GetAllActiveAsync()
    {
        return await _context.EarlyBirdMembers
            .Where(m => m.FreeUntil >= DateTime.UtcNow && !m.HasUsedBenefit)
            .OrderBy(m => m.EnrolledAt)
            .ToListAsync();
    }

    public async Task<bool> IsUserEnrolledAsync(Guid userId)
    {
        return await _context.EarlyBirdMembers
            .AnyAsync(m => m.UserId == userId);
    }

    public async Task CreateAsync(EarlyBirdMember member)
    {
        await _context.EarlyBirdMembers.AddAsync(member);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(EarlyBirdMember member)
    {
        _context.EarlyBirdMembers.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetTotalEnrolledCountAsync()
    {
        return await _context.EarlyBirdMembers.CountAsync();
    }
}
