using Microsoft.EntityFrameworkCore;
using FeatureStoreService.Domain.Entities;
using FeatureStoreService.Domain.Interfaces;

namespace FeatureStoreService.Infrastructure.Persistence.Repositories;

public class FeatureStoreRepository : IFeatureStoreRepository
{
    private readonly FeatureStoreDbContext _context;

    public FeatureStoreRepository(FeatureStoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserFeature>> GetUserFeaturesAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.UserFeatures
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.FeatureName)
            .ToListAsync(ct);
    }

    public async Task<UserFeature?> GetUserFeatureAsync(Guid userId, string featureName, CancellationToken ct = default)
    {
        return await _context.UserFeatures
            .FirstOrDefaultAsync(f => f.UserId == userId && f.FeatureName == featureName, ct);
    }

    public async Task<UserFeature> UpsertUserFeatureAsync(UserFeature feature, CancellationToken ct = default)
    {
        var existing = await GetUserFeatureAsync(feature.UserId, feature.FeatureName, ct);

        if (existing != null)
        {
            existing.FeatureValue = feature.FeatureValue;
            existing.FeatureType = feature.FeatureType;
            existing.Version = feature.Version + 1;
            existing.ComputedAt = DateTime.UtcNow;
            existing.ExpiresAt = feature.ExpiresAt;
            existing.Source = feature.Source;
            _context.UserFeatures.Update(existing);
        }
        else
        {
            _context.UserFeatures.Add(feature);
        }

        await _context.SaveChangesAsync(ct);
        return existing ?? feature;
    }

    public async Task DeleteUserFeatureAsync(Guid userId, string featureName, CancellationToken ct = default)
    {
        var feature = await GetUserFeatureAsync(userId, featureName, ct);
        if (feature != null)
        {
            _context.UserFeatures.Remove(feature);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<List<VehicleFeature>> GetVehicleFeaturesAsync(Guid vehicleId, CancellationToken ct = default)
    {
        return await _context.VehicleFeatures
            .Where(f => f.VehicleId == vehicleId)
            .OrderBy(f => f.FeatureName)
            .ToListAsync(ct);
    }

    public async Task<VehicleFeature?> GetVehicleFeatureAsync(Guid vehicleId, string featureName, CancellationToken ct = default)
    {
        return await _context.VehicleFeatures
            .FirstOrDefaultAsync(f => f.VehicleId == vehicleId && f.FeatureName == featureName, ct);
    }

    public async Task<VehicleFeature> UpsertVehicleFeatureAsync(VehicleFeature feature, CancellationToken ct = default)
    {
        var existing = await GetVehicleFeatureAsync(feature.VehicleId, feature.FeatureName, ct);

        if (existing != null)
        {
            existing.FeatureValue = feature.FeatureValue;
            existing.FeatureType = feature.FeatureType;
            existing.Version = feature.Version + 1;
            existing.ComputedAt = DateTime.UtcNow;
            existing.ExpiresAt = feature.ExpiresAt;
            existing.Source = feature.Source;
            _context.VehicleFeatures.Update(existing);
        }
        else
        {
            _context.VehicleFeatures.Add(feature);
        }

        await _context.SaveChangesAsync(ct);
        return existing ?? feature;
    }

    public async Task DeleteVehicleFeatureAsync(Guid vehicleId, string featureName, CancellationToken ct = default)
    {
        var feature = await GetVehicleFeatureAsync(vehicleId, featureName, ct);
        if (feature != null)
        {
            _context.VehicleFeatures.Remove(feature);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<List<FeatureDefinition>> GetFeatureDefinitionsAsync(string? category = null, CancellationToken ct = default)
    {
        var query = _context.FeatureDefinitions.AsQueryable();
        if (!string.IsNullOrEmpty(category))
            query = query.Where(d => d.Category == category);
        
        return await query.OrderBy(d => d.Category).ThenBy(d => d.FeatureName).ToListAsync(ct);
    }

    public async Task<FeatureDefinition?> GetFeatureDefinitionAsync(string featureName, CancellationToken ct = default)
    {
        return await _context.FeatureDefinitions
            .FirstOrDefaultAsync(d => d.FeatureName == featureName, ct);
    }

    public async Task<FeatureDefinition> CreateOrUpdateFeatureDefinitionAsync(FeatureDefinition definition, CancellationToken ct = default)
    {
        var existing = await GetFeatureDefinitionAsync(definition.FeatureName, ct);

        if (existing != null)
        {
            existing.Category = definition.Category;
            existing.Description = definition.Description;
            existing.FeatureType = definition.FeatureType;
            existing.IsActive = definition.IsActive;
            existing.ComputationLogic = definition.ComputationLogic;
            existing.RefreshIntervalHours = definition.RefreshIntervalHours;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.FeatureDefinitions.Update(existing);
        }
        else
        {
            definition.CreatedAt = DateTime.UtcNow;
            definition.UpdatedAt = DateTime.UtcNow;
            _context.FeatureDefinitions.Add(definition);
        }

        await _context.SaveChangesAsync(ct);
        return existing ?? definition;
    }

    public async Task<FeatureBatch> CreateBatchAsync(FeatureBatch batch, CancellationToken ct = default)
    {
        _context.FeatureBatches.Add(batch);
        await _context.SaveChangesAsync(ct);
        return batch;
    }

    public async Task<FeatureBatch?> GetBatchAsync(Guid batchId, CancellationToken ct = default)
    {
        return await _context.FeatureBatches.FindAsync(new object[] { batchId }, ct);
    }

    public async Task UpdateBatchAsync(FeatureBatch batch, CancellationToken ct = default)
    {
        _context.FeatureBatches.Update(batch);
        await _context.SaveChangesAsync(ct);
    }

    public async Task BulkUpsertUserFeaturesAsync(List<UserFeature> features, CancellationToken ct = default)
    {
        foreach (var feature in features)
        {
            await UpsertUserFeatureAsync(feature, ct);
        }
    }

    public async Task BulkUpsertVehicleFeaturesAsync(List<VehicleFeature> features, CancellationToken ct = default)
    {
        foreach (var feature in features)
        {
            await UpsertVehicleFeatureAsync(feature, ct);
        }
    }

    public async Task<Dictionary<string, int>> GetFeatureUsageStatsAsync(CancellationToken ct = default)
    {
        var userFeatures = await _context.UserFeatures
            .GroupBy(f => f.FeatureName)
            .Select(g => new { FeatureName = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var vehicleFeatures = await _context.VehicleFeatures
            .GroupBy(f => f.FeatureName)
            .Select(g => new { FeatureName = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var combined = userFeatures.Concat(vehicleFeatures)
            .GroupBy(x => x.FeatureName)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

        return combined;
    }

    public async Task<List<UserFeature>> GetExpiredFeaturesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.UserFeatures
            .Where(f => f.ExpiresAt.HasValue && f.ExpiresAt < now)
            .ToListAsync(ct);
    }
}
