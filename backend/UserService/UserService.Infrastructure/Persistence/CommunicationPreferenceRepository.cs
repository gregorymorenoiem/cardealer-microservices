using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Persistence;

/// <summary>
/// Implements ICommunicationPreferenceRepository using EF Core.
/// </summary>
public class CommunicationPreferenceRepository : ICommunicationPreferenceRepository
{
    private readonly ApplicationDbContext _context;

    public CommunicationPreferenceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CommunicationPreference?> GetByUserIdAsync(Guid userId)
    {
        return await _context.CommunicationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<CommunicationPreference> UpsertAsync(CommunicationPreference preference)
    {
        var existing = await _context.CommunicationPreferences
            .FirstOrDefaultAsync(p => p.UserId == preference.UserId);

        if (existing == null)
        {
            preference.CreatedAt = DateTime.UtcNow;
            preference.UpdatedAt = DateTime.UtcNow;
            await _context.CommunicationPreferences.AddAsync(preference);
        }
        else
        {
            // Selective update - only overwrite what the caller set
            existing.EmailActivityNotifications = preference.EmailActivityNotifications;
            existing.EmailListingUpdates = preference.EmailListingUpdates;
            existing.EmailNewsletter = preference.EmailNewsletter;
            existing.EmailPromotions = preference.EmailPromotions;
            existing.EmailPriceAlerts = preference.EmailPriceAlerts;
            existing.SmsPriceAlerts = preference.SmsPriceAlerts;
            existing.SmsPromotions = preference.SmsPromotions;
            existing.PushNewMessages = preference.PushNewMessages;
            existing.PushPriceChanges = preference.PushPriceChanges;
            existing.PushRecommendations = preference.PushRecommendations;
            existing.AllowProfiling = preference.AllowProfiling;
            existing.AllowThirdPartySharing = preference.AllowThirdPartySharing;
            existing.AllowAnalytics = preference.AllowAnalytics;
            existing.AllowRetargeting = preference.AllowRetargeting;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.CommunicationPreferences.Update(existing);
            preference = existing;
        }

        await _context.SaveChangesAsync();
        return preference;
    }
}
