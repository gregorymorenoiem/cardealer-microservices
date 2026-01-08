using Microsoft.EntityFrameworkCore;
using AlertService.Domain.Entities;
using AlertService.Domain.Interfaces;
using AlertService.Infrastructure.Persistence;

namespace AlertService.Infrastructure.Repositories;

public class SavedSearchRepository : ISavedSearchRepository
{
    private readonly ApplicationDbContext _context;

    public SavedSearchRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SavedSearch?> GetByIdAsync(Guid id)
    {
        return await _context.SavedSearches
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<SavedSearch>> GetByUserIdAsync(Guid userId)
    {
        return await _context.SavedSearches
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<SavedSearch>> GetActiveSearchesAsync()
    {
        return await _context.SavedSearches
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    public async Task<List<SavedSearch>> GetSearchesDueForNotificationAsync()
    {
        var now = DateTime.UtcNow;

        return await _context.SavedSearches
            .Where(s => s.IsActive && s.SendEmailNotifications)
            .Where(s =>
                // Instant: always due
                s.Frequency == NotificationFrequency.Instant ||
                // Daily: 24 hours since last notification or never sent
                (s.Frequency == NotificationFrequency.Daily &&
                 (s.LastNotificationSent == null || s.LastNotificationSent.Value.AddHours(24) <= now)) ||
                // Weekly: 7 days since last notification or never sent
                (s.Frequency == NotificationFrequency.Weekly &&
                 (s.LastNotificationSent == null || s.LastNotificationSent.Value.AddDays(7) <= now)))
            .ToListAsync();
    }

    public async Task CreateAsync(SavedSearch search)
    {
        await _context.SavedSearches.AddAsync(search);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SavedSearch search)
    {
        _context.SavedSearches.Update(search);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var search = await GetByIdAsync(id);
        if (search != null)
        {
            _context.SavedSearches.Remove(search);
            await _context.SaveChangesAsync();
        }
    }
}
