using VehicleIntelligenceService.Domain.Entities;

namespace VehicleIntelligenceService.Domain.Interfaces;

public interface ICategoryDemandRepository
{
    Task<List<CategoryDemandSnapshot>> GetAllAsync(CancellationToken ct = default);
    Task UpsertManyAsync(IEnumerable<CategoryDemandSnapshot> snapshots, CancellationToken ct = default);
}
