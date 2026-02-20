using AdvertisingService.Domain.Entities;

namespace AdvertisingService.Domain.Interfaces;

public interface IAdClickRepository
{
    Task AddAsync(AdClick click, CancellationToken ct = default);
    Task<int> CountByCampaignAsync(Guid campaignId, DateTime since, CancellationToken ct = default);
    Task<List<(DateTime Date, int Count)>> GetDailyCountsByCampaignAsync(Guid campaignId, DateTime since, CancellationToken ct = default);
    Task<List<(DateTime Date, int Count)>> GetDailyCountsByOwnerAsync(Guid ownerId, DateTime since, CancellationToken ct = default);
    Task<int> GetTotalClicksSinceAsync(DateTime since, CancellationToken ct = default);
}
