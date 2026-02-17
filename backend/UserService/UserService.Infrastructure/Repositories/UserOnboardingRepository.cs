using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class UserOnboardingRepository : IUserOnboardingRepository
{
    private readonly ApplicationDbContext _context;

    public UserOnboardingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserOnboarding?> GetByIdAsync(Guid id)
    {
        return await _context.UserOnboardings
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<UserOnboarding?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserOnboardings
            .FirstOrDefaultAsync(o => o.UserId == userId);
    }

    public async Task<List<UserOnboarding>> GetIncompleteAsync()
    {
        return await _context.UserOnboardings
            .Where(o => !o.IsCompleted && !o.WasSkipped)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(UserOnboarding onboarding)
    {
        await _context.UserOnboardings.AddAsync(onboarding);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserOnboarding onboarding)
    {
        _context.UserOnboardings.Update(onboarding);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsForUserAsync(Guid userId)
    {
        return await _context.UserOnboardings
            .AnyAsync(o => o.UserId == userId);
    }

    public async Task<int> GetCompletionRateAsync()
    {
        var total = await _context.UserOnboardings.CountAsync();
        if (total == 0) return 0;

        var completed = await _context.UserOnboardings
            .CountAsync(o => o.IsCompleted);

        return (completed * 100) / total;
    }
}
