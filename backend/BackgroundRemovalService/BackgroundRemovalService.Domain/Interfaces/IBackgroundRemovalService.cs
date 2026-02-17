using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Domain.Interfaces;

/// <summary>
/// Main service interface for background removal operations
/// </summary>
public interface IBackgroundRemovalService
{
    /// <summary>
    /// Gets the current active provider
    /// </summary>
    BackgroundRemovalProvider CurrentProvider { get; }
    
    /// <summary>
    /// Remove background from image bytes synchronously
    /// </summary>
    Task<BackgroundRemovalResult> RemoveBackgroundAsync(
        byte[] imageData,
        OutputFormat outputFormat = OutputFormat.Png,
        BackgroundRemovalProvider? provider = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove background from an image URL synchronously
    /// </summary>
    Task<BackgroundRemovalResult> RemoveBackgroundFromUrlAsync(
        string imageUrl,
        OutputFormat outputFormat = OutputFormat.Png,
        BackgroundRemovalProvider? provider = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create an async job for background removal
    /// </summary>
    Task<BackgroundRemovalJob> CreateJobAsync(
        string sourceImageUrl,
        Guid? userId = null,
        OutputFormat outputFormat = OutputFormat.Png,
        BackgroundRemovalProvider? provider = null,
        string? callbackUrl = null,
        string? metadata = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get job by ID
    /// </summary>
    Task<BackgroundRemovalJob?> GetJobAsync(Guid jobId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get jobs by user ID
    /// </summary>
    Task<IEnumerable<BackgroundRemovalJob>> GetUserJobsAsync(
        Guid userId, 
        int skip = 0, 
        int take = 20, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancel a pending job
    /// </summary>
    Task<bool> CancelJobAsync(Guid jobId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get available providers
    /// </summary>
    IEnumerable<(BackgroundRemovalProvider Provider, string Name, bool IsAvailable)> GetAvailableProviders();
}
