using AdvertisingService.Domain.Entities;

namespace AdvertisingService.Domain.Interfaces;

public interface ICategoryConfigRepository
{
    Task<List<CategoryImageConfig>> GetAllVisibleAsync(CancellationToken ct = default);
    Task<List<CategoryImageConfig>> GetAllAsync(CancellationToken ct = default);
    Task<CategoryImageConfig?> GetByKeyAsync(string categoryKey, CancellationToken ct = default);
    Task UpdateAsync(CategoryImageConfig config, CancellationToken ct = default);
    Task AddAsync(CategoryImageConfig config, CancellationToken ct = default);
}
