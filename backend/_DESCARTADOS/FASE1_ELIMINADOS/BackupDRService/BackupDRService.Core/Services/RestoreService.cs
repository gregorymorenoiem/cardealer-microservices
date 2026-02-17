using System.Collections.Concurrent;
using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackupDRService.Core.Services;

/// <summary>
/// Restore service implementation
/// </summary>
public class RestoreService : IRestoreService
{
    private readonly ILogger<RestoreService> _logger;
    private readonly BackupOptions _options;
    private readonly IDatabaseBackupProvider _databaseProvider;
    private readonly IStorageProvider _storageProvider;
    private readonly IBackupService _backupService;

    // In-memory storage (would use database in production)
    private readonly ConcurrentDictionary<string, RestorePoint> _restorePoints = new();
    private readonly ConcurrentDictionary<string, RestoreResult> _restoreResults = new();

    public RestoreService(
        ILogger<RestoreService> logger,
        IOptions<BackupOptions> options,
        IDatabaseBackupProvider databaseProvider,
        IStorageProvider storageProvider,
        IBackupService backupService)
    {
        _logger = logger;
        _options = options.Value;
        _databaseProvider = databaseProvider;
        _storageProvider = storageProvider;
        _backupService = backupService;
    }

    #region Restore Point Management

    public Task<RestorePoint> CreateRestorePointAsync(BackupResult backupResult, string name, string? description = null)
    {
        var restorePoint = RestorePoint.FromBackupResult(backupResult, name, description);
        _restorePoints[restorePoint.Id] = restorePoint;

        _logger.LogInformation("Created restore point: {Id} - {Name} from backup {BackupId}",
            restorePoint.Id, name, backupResult.Id);

        return Task.FromResult(restorePoint);
    }

    public Task<RestorePoint?> GetRestorePointAsync(string restorePointId)
    {
        _restorePoints.TryGetValue(restorePointId, out var point);
        return Task.FromResult(point);
    }

    public Task<IEnumerable<RestorePoint>> GetAllRestorePointsAsync()
    {
        return Task.FromResult<IEnumerable<RestorePoint>>(_restorePoints.Values
            .OrderByDescending(p => p.CreatedAt)
            .ToList());
    }

    public Task<IEnumerable<RestorePoint>> GetRestorePointsByJobAsync(string jobId)
    {
        var points = _restorePoints.Values
            .Where(p => p.JobId == jobId)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
        return Task.FromResult<IEnumerable<RestorePoint>>(points);
    }

    public Task<IEnumerable<RestorePoint>> GetAvailableRestorePointsAsync()
    {
        var available = _restorePoints.Values
            .Where(p => p.Status == RestorePointStatus.Available && !p.IsExpired)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
        return Task.FromResult<IEnumerable<RestorePoint>>(available);
    }

    public async Task<bool> DeleteRestorePointAsync(string restorePointId)
    {
        if (_restorePoints.TryRemove(restorePointId, out var point))
        {
            // Optionally delete the backup file
            await _storageProvider.DeleteAsync(point.FilePath);
            _logger.LogInformation("Deleted restore point: {Id}", restorePointId);
            return true;
        }
        return false;
    }

    #endregion

    #region Restore Execution

    public async Task<RestoreResult> RestoreFromPointAsync(string restorePointId, RestoreOptions? options = null)
    {
        var restorePoint = await GetRestorePointAsync(restorePointId);
        if (restorePoint == null)
        {
            return RestoreResult.Failure(restorePointId, "Unknown", "Restore point not found");
        }

        options ??= new RestoreOptions();

        var result = RestoreResult.Running(restorePointId, restorePoint.Name, options.InitiatedBy ?? "system");
        _restoreResults[result.Id] = result;
        restorePoint.Status = RestorePointStatus.Restoring;

        try
        {
            _logger.LogInformation("Starting restore from point: {Id} - {Name}",
                restorePointId, restorePoint.Name);

            // Download backup file if not local
            var localFilePath = restorePoint.FilePath;
            if (restorePoint.StorageType != StorageType.Local)
            {
                var tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(restorePoint.FilePath));
                var downloadResult = await _storageProvider.DownloadAsync(restorePoint.FilePath, tempPath);
                if (!downloadResult.Success)
                {
                    result = RestoreResult.Failure(restorePointId, restorePoint.Name,
                        downloadResult.ErrorMessage ?? "Failed to download backup file");
                    _restoreResults[result.Id] = result;
                    restorePoint.Status = RestorePointStatus.Available;
                    return result;
                }
                localFilePath = tempPath;
            }

            // Get target database from backup job
            var job = await _backupService.GetJobAsync(restorePoint.JobId);
            var targetConnString = options.TargetConnectionString ?? job?.ConnectionString ?? "";
            var targetDatabase = options.TargetDatabaseName ?? job?.DatabaseName ?? "";

            if (string.IsNullOrEmpty(targetConnString) || string.IsNullOrEmpty(targetDatabase))
            {
                return RestoreResult.Failure(restorePointId, restorePoint.Name,
                    "Target connection string and database name are required");
            }

            // Execute restore
            var dbResult = await _databaseProvider.RestoreAsync(new DatabaseRestoreRequest
            {
                ConnectionString = targetConnString,
                DatabaseName = targetDatabase,
                BackupFilePath = localFilePath,
                DropExistingDatabase = options.DropExistingDatabase,
                CreateIfNotExists = options.CreateIfNotExists,
                TimeoutSeconds = options.TimeoutMinutes * 60
            });

            // Cleanup temp file
            if (restorePoint.StorageType != StorageType.Local && File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }

            if (!dbResult.Success)
            {
                result = RestoreResult.Failure(restorePointId, restorePoint.Name,
                    dbResult.ErrorMessage ?? "Database restore failed", dbResult.ErrorDetails);
                _restoreResults[result.Id] = result;
                restorePoint.Status = RestorePointStatus.Available;
                return result;
            }

            // Update result
            result = RestoreResult.Success(restorePointId, restorePoint.Name, dbResult.BytesRestored);
            result.TablesRestored = dbResult.TablesRestored;
            result.RecordsRestored = (int)dbResult.RowsRestored;
            result.TargetConnectionString = targetConnString;
            result.TargetDatabaseName = targetDatabase;
            result.Mode = options.Mode;
            result.InitiatedBy = options.InitiatedBy ?? "system";
            _restoreResults[result.Id] = result;

            restorePoint.Status = RestorePointStatus.Restored;

            _logger.LogInformation(
                "Restore completed: {RestorePointId}, Database: {Database}, Duration: {Duration}",
                restorePointId, targetDatabase, result.Duration);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Restore failed: {RestorePointId}", restorePointId);

            result = RestoreResult.Failure(restorePointId, restorePoint.Name, ex.Message, ex.StackTrace);
            _restoreResults[result.Id] = result;
            restorePoint.Status = RestorePointStatus.Available;

            return result;
        }
    }

    public async Task<RestoreResult> RestoreFromBackupAsync(string backupResultId, RestoreOptions? options = null)
    {
        var backupResult = await _backupService.GetBackupResultAsync(backupResultId);
        if (backupResult == null)
        {
            return RestoreResult.Failure(backupResultId, "Unknown", "Backup result not found");
        }

        // Create a temporary restore point
        var restorePoint = await CreateRestorePointAsync(
            backupResult,
            $"Auto-{DateTime.UtcNow:yyyyMMddHHmmss}",
            "Automatically created restore point");

        return await RestoreFromPointAsync(restorePoint.Id, options);
    }

    public Task<bool> CancelRestoreAsync(string restoreResultId)
    {
        if (_restoreResults.TryGetValue(restoreResultId, out var result))
        {
            if (result.Status == RestoreExecutionStatus.Running)
            {
                result.Status = RestoreExecutionStatus.Cancelled;
                result.CompletedAt = DateTime.UtcNow;
                _logger.LogInformation("Cancelled restore: {ResultId}", restoreResultId);
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }

    #endregion

    #region Restore Results

    public Task<RestoreResult?> GetRestoreResultAsync(string resultId)
    {
        _restoreResults.TryGetValue(resultId, out var result);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<RestoreResult>> GetRestoreResultsAsync()
    {
        return Task.FromResult<IEnumerable<RestoreResult>>(_restoreResults.Values
            .OrderByDescending(r => r.StartedAt)
            .ToList());
    }

    public Task<IEnumerable<RestoreResult>> GetRecentRestoreResultsAsync(int count = 10)
    {
        var results = _restoreResults.Values
            .OrderByDescending(r => r.StartedAt)
            .Take(count)
            .ToList();
        return Task.FromResult<IEnumerable<RestoreResult>>(results);
    }

    #endregion

    #region Verification & Cleanup

    public async Task<bool> VerifyRestorePointAsync(string restorePointId)
    {
        var restorePoint = await GetRestorePointAsync(restorePointId);
        if (restorePoint == null || string.IsNullOrEmpty(restorePoint.Checksum))
        {
            return false;
        }

        var isValid = await _storageProvider.VerifyIntegrityAsync(restorePoint.FilePath, restorePoint.Checksum);

        if (isValid)
        {
            restorePoint.IsVerified = true;
        }
        else
        {
            restorePoint.Status = RestorePointStatus.Corrupted;
        }

        return isValid;
    }

    public async Task<bool> TestRestoreAsync(string restorePointId)
    {
        // Verify the restore point exists and file is accessible
        var restorePoint = await GetRestorePointAsync(restorePointId);
        if (restorePoint == null)
        {
            return false;
        }

        // Check if file exists in storage
        var exists = await _storageProvider.ExistsAsync(restorePoint.FilePath);
        if (!exists)
        {
            restorePoint.Status = RestorePointStatus.Deleted;
            return false;
        }

        // Verify checksum if available
        if (!string.IsNullOrEmpty(restorePoint.Checksum))
        {
            var isValid = await _storageProvider.VerifyIntegrityAsync(restorePoint.FilePath, restorePoint.Checksum);
            if (!isValid)
            {
                restorePoint.Status = RestorePointStatus.Corrupted;
                return false;
            }
        }

        restorePoint.IsVerified = true;
        return true;
    }

    public Task<int> CleanupExpiredRestorePointsAsync()
    {
        var expiredPoints = _restorePoints.Values
            .Where(p => p.IsExpired)
            .ToList();

        var deletedCount = 0;
        foreach (var point in expiredPoints)
        {
            if (_restorePoints.TryRemove(point.Id, out _))
            {
                point.Status = RestorePointStatus.Expired;
                deletedCount++;
                _logger.LogInformation("Cleaned up expired restore point: {Id}", point.Id);
            }
        }

        return Task.FromResult(deletedCount);
    }

    #endregion
}
