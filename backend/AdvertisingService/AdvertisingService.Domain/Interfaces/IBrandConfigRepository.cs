using AdvertisingService.Domain.Entities;

namespace AdvertisingService.Domain.Interfaces;

public interface IBrandConfigRepository
{
    Task<List<BrandConfig>> GetAllVisibleAsync(CancellationToken ct = default);
    Task<List<BrandConfig>> GetAllAsync(CancellationToken ct = default);
    Task<BrandConfig?> GetByKeyAsync(string brandKey, CancellationToken ct = default);
    Task UpdateAsync(BrandConfig config, CancellationToken ct = default);
    Task AddAsync(BrandConfig config, CancellationToken ct = default);
}
