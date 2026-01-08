using DealerManagementService.Domain.Entities;

namespace DealerManagementService.Domain.Interfaces;

public interface IDealerLocationRepository
{
    Task<DealerLocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DealerLocation>> GetByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task<DealerLocation?> GetPrimaryLocationAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DealerLocation>> GetByProvinceAsync(string province, CancellationToken cancellationToken = default);
    Task<DealerLocation> AddAsync(DealerLocation location, CancellationToken cancellationToken = default);
    Task UpdateAsync(DealerLocation location, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
