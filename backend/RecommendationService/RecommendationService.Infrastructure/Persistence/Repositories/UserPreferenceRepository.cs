using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecommendationService.Domain.Entities;
using RecommendationService.Domain.Interfaces;

namespace RecommendationService.Infrastructure.Persistence.Repositories;

public class UserPreferenceRepository : IUserPreferenceRepository
{
    private readonly RecommendationDbContext _context;

    public UserPreferenceRepository(RecommendationDbContext context)
    {
        _context = context;
    }

    public async Task<UserPreference?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<UserPreference> CreateAsync(UserPreference preference)
    {
        _context.UserPreferences.Add(preference);
        await _context.SaveChangesAsync();
        return preference;
    }

    public async Task<UserPreference> UpdateAsync(UserPreference preference)
    {
        _context.UserPreferences.Update(preference);
        await _context.SaveChangesAsync();
        return preference;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var preference = await _context.UserPreferences.FindAsync(id);
        if (preference == null)
            return false;

        _context.UserPreferences.Remove(preference);
        await _context.SaveChangesAsync();
        return true;
    }
}
