using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Domain.Interfaces;

public interface IAdCampaignRepository
{
    Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<AdCampaign>> GetByOwnerAsync(Guid ownerId, string ownerType, CampaignStatus? status = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<int> CountByOwnerAsync(Guid ownerId, string ownerType, CampaignStatus? status = null, CancellationToken ct = default);
    Task<List<AdCampaign>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken ct = default);
    Task<List<AdCampaign>> GetActiveByPlacementAsync(AdPlacementType placement, CancellationToken ct = default);
    Task<List<AdCampaign>> GetActiveCampaignsForReportingAsync(DateTime since, CancellationToken ct = default);
    Task<List<Guid>> GetDistinctOwnerIdsWithActiveCampaignsAsync(CancellationToken ct = default);
    Task AddAsync(AdCampaign campaign, CancellationToken ct = default);
    Task UpdateAsync(AdCampaign campaign, CancellationToken ct = default);
}
