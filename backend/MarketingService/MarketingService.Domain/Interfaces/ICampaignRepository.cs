using MarketingService.Domain.Entities;

namespace MarketingService.Domain.Interfaces;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Campaign>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Campaign>> GetByStatusAsync(CampaignStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Campaign>> GetByTypeAsync(CampaignType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Campaign>> GetScheduledCampaignsAsync(DateTime before, CancellationToken cancellationToken = default);
    Task<Campaign> AddAsync(Campaign campaign, CancellationToken cancellationToken = default);
    Task UpdateAsync(Campaign campaign, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
