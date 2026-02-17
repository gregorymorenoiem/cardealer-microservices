using FeatureStoreService.Domain.Entities;

namespace FeatureStoreService.Domain.Interfaces;

public interface IFeatureStoreRepository
{
    // User Features
    Task<List<UserFeature>> GetUserFeaturesAsync(Guid userId, CancellationToken ct = default);
    Task<UserFeature?> GetUserFeatureAsync(Guid userId, string featureName, CancellationToken ct = default);
    Task<UserFeature> UpsertUserFeatureAsync(UserFeature feature, CancellationToken ct = default);
    Task DeleteUserFeatureAsync(Guid userId, string featureName, CancellationToken ct = default);
    
    // Vehicle Features
    Task<List<VehicleFeature>> GetVehicleFeaturesAsync(Guid vehicleId, CancellationToken ct = default);
    Task<VehicleFeature?> GetVehicleFeatureAsync(Guid vehicleId, string featureName, CancellationToken ct = default);
    Task<VehicleFeature> UpsertVehicleFeatureAsync(VehicleFeature feature, CancellationToken ct = default);
    Task DeleteVehicleFeatureAsync(Guid vehicleId, string featureName, CancellationToken ct = default);
    
    // Feature Definitions
    Task<List<FeatureDefinition>> GetFeatureDefinitionsAsync(string? category = null, CancellationToken ct = default);
    Task<FeatureDefinition?> GetFeatureDefinitionAsync(string featureName, CancellationToken ct = default);
    Task<FeatureDefinition> CreateOrUpdateFeatureDefinitionAsync(FeatureDefinition definition, CancellationToken ct = default);
    
    // Batch Operations
    Task<FeatureBatch> CreateBatchAsync(FeatureBatch batch, CancellationToken ct = default);
    Task<FeatureBatch?> GetBatchAsync(Guid batchId, CancellationToken ct = default);
    Task UpdateBatchAsync(FeatureBatch batch, CancellationToken ct = default);
    
    // Bulk Operations
    Task BulkUpsertUserFeaturesAsync(List<UserFeature> features, CancellationToken ct = default);
    Task BulkUpsertVehicleFeaturesAsync(List<VehicleFeature> features, CancellationToken ct = default);
    
    // Analytics
    Task<Dictionary<string, int>> GetFeatureUsageStatsAsync(CancellationToken ct = default);
    Task<List<UserFeature>> GetExpiredFeaturesAsync(CancellationToken ct = default);
}
