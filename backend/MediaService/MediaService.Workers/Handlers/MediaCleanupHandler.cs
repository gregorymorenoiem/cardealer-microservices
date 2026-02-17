using MediaService.Domain.Enums;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace MediaService.Workers.Handlers;

/// <summary>
/// Periodically cleans up orphaned and stale media assets from S3 and the database.
/// - Removes uploads that were never finalized (status=Uploaded, older than 48h)
/// - Removes orphan media assets not associated with any vehicle (older than 7 days)
/// </summary>
public class MediaCleanupHandler
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IMediaStorageService _storageService;
    private readonly ILogger<MediaCleanupHandler> _logger;

    private static readonly TimeSpan StaleUploadThreshold = TimeSpan.FromHours(48);
    private static readonly TimeSpan OrphanThreshold = TimeSpan.FromDays(7);
    private const int OrphanAlertThreshold = 100;

    public MediaCleanupHandler(
        IMediaRepository mediaRepository,
        IMediaStorageService storageService,
        ILogger<MediaCleanupHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Run the full cleanup cycle: stale uploads + orphaned media.
    /// Intended to be called by a scheduled background worker (cron every 24h).
    /// </summary>
    public async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting media cleanup cycle at {Time}", DateTime.UtcNow);

        var staleCount = await CleanupStaleUploadsAsync(cancellationToken);
        var orphanCount = await CleanupOrphanedMediaAsync(cancellationToken);

        _logger.LogInformation(
            "Media cleanup completed: {StaleCount} stale uploads cleaned, {OrphanCount} orphans cleaned",
            staleCount, orphanCount);
    }

    /// <summary>
    /// Find and remove media assets stuck in 'Uploaded' status for over 48 hours.
    /// These are uploads that were initiated but never finalized.
    /// </summary>
    private async Task<int> CleanupStaleUploadsAsync(CancellationToken cancellationToken)
    {
        var cutoffDate = DateTime.UtcNow - StaleUploadThreshold;
        var cleanedCount = 0;

        try
        {
            var staleAssets = await _mediaRepository.GetByStatusAndDateAsync(
                MediaStatus.Uploaded, cutoffDate, cancellationToken);

            foreach (var asset in staleAssets)
            {
                try
                {
                    // Delete from S3
                    await _storageService.DeleteFileAsync(asset.StorageKey);

                    // Also delete any variants that may have been partially created
                    foreach (var variant in asset.Variants)
                    {
                        try
                        {
                            await _storageService.DeleteFileAsync(variant.StorageKey);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete variant {VariantKey} during cleanup", variant.StorageKey);
                        }
                    }

                    // Mark as failed in DB
                    asset.MarkAsFailed("Cleanup: upload never finalized (stale > 48h)");
                    await _mediaRepository.UpdateAsync(asset, cancellationToken);
                    cleanedCount++;

                    _logger.LogDebug("Cleaned stale upload: {MediaId} (created {CreatedAt})", asset.Id, asset.CreatedAt);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cleanup stale MediaAsset {MediaId}", asset.Id);
                }
            }

            if (cleanedCount > 0)
            {
                _logger.LogInformation("Cleaned {Count} stale uploads (older than {Hours}h)", cleanedCount, StaleUploadThreshold.TotalHours);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during stale upload cleanup");
        }

        return cleanedCount;
    }

    /// <summary>
    /// Find media assets not associated with any vehicle image and older than 7 days.
    /// These are likely orphans from abandoned publish flows.
    /// </summary>
    private async Task<int> CleanupOrphanedMediaAsync(CancellationToken cancellationToken)
    {
        var cutoffDate = DateTime.UtcNow - OrphanThreshold;
        var cleanedCount = 0;

        try
        {
            var orphanAssets = await _mediaRepository.GetOrphansOlderThanAsync(cutoffDate, cancellationToken);

            if (orphanAssets.Count > OrphanAlertThreshold)
            {
                _logger.LogWarning(
                    "⚠️ High orphan count detected: {Count} orphaned media assets (threshold: {Threshold}). Possible storage leak.",
                    orphanAssets.Count, OrphanAlertThreshold);
            }

            foreach (var asset in orphanAssets)
            {
                try
                {
                    // Delete all storage keys (original + variants)
                    var allKeys = asset.GetAllStorageKeys();
                    foreach (var key in allKeys)
                    {
                        try
                        {
                            await _storageService.DeleteFileAsync(key);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete storage key {Key} during orphan cleanup", key);
                        }
                    }

                    asset.MarkAsFailed("Cleanup: orphaned media (no vehicle association > 7 days)");
                    await _mediaRepository.UpdateAsync(asset, cancellationToken);
                    cleanedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cleanup orphaned MediaAsset {MediaId}", asset.Id);
                }
            }

            if (cleanedCount > 0)
            {
                _logger.LogInformation("Cleaned {Count} orphaned media assets (older than {Days} days)", cleanedCount, OrphanThreshold.TotalDays);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during orphaned media cleanup");
        }

        return cleanedCount;
    }
}
