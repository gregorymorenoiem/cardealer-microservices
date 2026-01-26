using SpyneIntegrationService.Domain.Entities;

namespace SpyneIntegrationService.Domain.Interfaces;

/// <summary>
/// Repository interface for Video360Spin entities
/// </summary>
public interface IVideo360SpinRepository
{
    /// <summary>Get a video 360 spin by ID</summary>
    Task<Video360Spin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>Get a video 360 spin by Spyne job ID</summary>
    Task<Video360Spin?> GetBySpyneJobIdAsync(string spyneJobId, CancellationToken cancellationToken = default);
    
    /// <summary>Get all video 360 spins for a vehicle</summary>
    Task<List<Video360Spin>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    
    /// <summary>Get all video 360 spins for a dealer</summary>
    Task<List<Video360Spin>> GetByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default);
    
    /// <summary>Get the latest video 360 spin for a vehicle</summary>
    Task<Video360Spin?> GetLatestByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    
    /// <summary>Get pending video 360 spins (for background processing)</summary>
    Task<List<Video360Spin>> GetPendingAsync(int limit = 50, CancellationToken cancellationToken = default);
    
    /// <summary>Get processing video 360 spins (for status polling)</summary>
    Task<List<Video360Spin>> GetProcessingAsync(int limit = 50, CancellationToken cancellationToken = default);
    
    /// <summary>Add a new video 360 spin</summary>
    Task<Video360Spin> AddAsync(Video360Spin entity, CancellationToken cancellationToken = default);
    
    /// <summary>Update an existing video 360 spin</summary>
    Task UpdateAsync(Video360Spin entity, CancellationToken cancellationToken = default);
    
    /// <summary>Delete a video 360 spin</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>Check if a video 360 spin exists for a vehicle</summary>
    Task<bool> ExistsForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
