using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingService.Infrastructure.Persistence.Repositories;

public class AdCampaignRepository : IAdCampaignRepository
{
    private readonly AdvertisingDbContext _context;

    public AdCampaignRepository(AdvertisingDbContext context) => _context = context;

    public async Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.AdCampaigns.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<AdCampaign>> GetByOwnerAsync(Guid ownerId, string ownerType, CampaignStatus? status = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _context.AdCampaigns
            .Where(c => c.OwnerId == ownerId && c.OwnerType == ownerType);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<int> CountByOwnerAsync(Guid ownerId, string ownerType, CampaignStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.AdCampaigns
            .Where(c => c.OwnerId == ownerId && c.OwnerType == ownerType);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        return await query.CountAsync(ct);
    }

    public async Task<List<AdCampaign>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken ct = default)
        => await _context.AdCampaigns
            .Where(c => c.VehicleId == vehicleId)
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<List<AdCampaign>> GetActiveByPlacementAsync(AdPlacementType placement, CancellationToken ct = default)
        => await _context.AdCampaigns
            .Where(c => c.PlacementType == placement && c.Status == CampaignStatus.Active)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<List<AdCampaign>> GetActiveCampaignsForReportingAsync(DateTime since, CancellationToken ct = default)
        => await _context.AdCampaigns
            .Where(c => c.Status == CampaignStatus.Active || (c.Status == CampaignStatus.Completed && c.UpdatedAt >= since))
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<List<Guid>> GetDistinctOwnerIdsWithActiveCampaignsAsync(CancellationToken ct = default)
        => await _context.AdCampaigns
            .Where(c => c.Status == CampaignStatus.Active)
            .Select(c => c.OwnerId)
            .Distinct()
            .ToListAsync(ct);

    public async Task AddAsync(AdCampaign campaign, CancellationToken ct = default)
    {
        await _context.AdCampaigns.AddAsync(campaign, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(AdCampaign campaign, CancellationToken ct = default)
    {
        _context.AdCampaigns.Update(campaign);
        await _context.SaveChangesAsync(ct);
    }
}
