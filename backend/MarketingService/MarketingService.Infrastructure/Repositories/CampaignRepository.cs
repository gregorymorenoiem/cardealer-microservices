using Microsoft.EntityFrameworkCore;
using MarketingService.Domain.Entities;
using MarketingService.Domain.Interfaces;
using MarketingService.Infrastructure.Persistence;

namespace MarketingService.Infrastructure.Repositories;

public class CampaignRepository : ICampaignRepository
{
    private readonly MarketingDbContext _context;

    public CampaignRepository(MarketingDbContext context)
    {
        _context = context;
    }

    public async Task<Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Campaign>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Campaign>> GetByStatusAsync(CampaignStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Campaign>> GetByTypeAsync(CampaignType type, CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns
            .Where(c => c.Type == type)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Campaign>> GetScheduledCampaignsAsync(DateTime before, CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns
            .Where(c => c.Status == CampaignStatus.Scheduled && c.ScheduledDate <= before)
            .ToListAsync(cancellationToken);
    }

    public async Task<Campaign> AddAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);
        return campaign;
    }

    public async Task UpdateAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        _context.Campaigns.Update(campaign);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var campaign = await GetByIdAsync(id, cancellationToken);
        if (campaign != null)
        {
            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns.AnyAsync(c => c.Id == id, cancellationToken);
    }
}
