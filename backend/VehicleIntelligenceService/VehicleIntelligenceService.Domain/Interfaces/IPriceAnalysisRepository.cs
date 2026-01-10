using VehicleIntelligenceService.Domain.Entities;

namespace VehicleIntelligenceService.Domain.Interfaces;

public interface IPriceAnalysisRepository
{
    Task<PriceAnalysis?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PriceAnalysis?> GetLatestByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<List<PriceAnalysis>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<PriceAnalysis> CreateAsync(PriceAnalysis analysis, CancellationToken cancellationToken = default);
    Task<PriceAnalysis> UpdateAsync(PriceAnalysis analysis, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
