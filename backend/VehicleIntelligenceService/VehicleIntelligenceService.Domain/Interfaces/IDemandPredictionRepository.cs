using VehicleIntelligenceService.Domain.Entities;

namespace VehicleIntelligenceService.Domain.Interfaces;

public interface IDemandPredictionRepository
{
    Task<DemandPrediction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DemandPrediction?> GetLatestByMakeModelYearAsync(string make, string model, int year, CancellationToken cancellationToken = default);
    Task<List<DemandPrediction>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<DemandPrediction> CreateAsync(DemandPrediction prediction, CancellationToken cancellationToken = default);
    Task<DemandPrediction> UpdateAsync(DemandPrediction prediction, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
