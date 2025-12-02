using System.Collections.Concurrent;
using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackupDRService.Core.Services;

/// <summary>
/// Main backup service implementation
/// </summary>
public class BackupService : IBackupService
{
    private readonly ILogger<BackupService> _logger;
    private readonly BackupOptions _options;
    private readonly IDatabaseBackupProvider _databaseProvider;
    private readonly IStorageProvider _storageProvider;

    // In-memory storage (would use database in production)
    private readonly ConcurrentDictionary<string, BackupJob> _jobs = new();
    private readonly ConcurrentDictionary<string, BackupResult> _results = new();

    public BackupService(
        ILogger<BackupService> logger,
        IOptions<BackupOptions> options,
        IDatabaseBackupProvider databaseProvider,
        IStorageProvider storageProvider)
    {
        _logger = logger;
        _options = options.Value;
        _databaseProvider = databaseProvider;
        _storageProvider = storageProvider;
    }

    #region Job Management

    public Task<BackupJob> CreateJobAsync(BackupJob job)
    {
        job.Id = Guid.NewGuid().ToString();
        job.CreatedAt = DateTime.UtcNow;
        job.CalculateNextRun();

        _jobs[job.Id] = job;
        _logger.LogInformation("Created backup job: {JobId} - {JobName}", job.Id, job.Name);

        return Task.FromResult(job);
    }

    public Task<BackupJob?> GetJobAsync(string jobId)
    {
        _jobs.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    public Task<BackupJob?> GetJobByNameAsync(string name)
    {
        var job = _jobs.Values.FirstOrDefault(j =>
            j.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(job);
    }

    public Task<IEnumerable<BackupJob>> GetAllJobsAsync()
    {
        return Task.FromResult<IEnumerable<BackupJob>>(_jobs.Values.ToList());
    }

    public Task<IEnumerable<BackupJob>> GetEnabledJobsAsync()
    {
        var enabled = _jobs.Values.Where(j => j.IsEnabled).ToList();
        return Task.FromResult<IEnumerable<BackupJob>>(enabled);
    }

    public Task<BackupJob> UpdateJobAsync(BackupJob job)
    {
        job.UpdatedAt = DateTime.UtcNow;
        job.CalculateNextRun();

        _jobs[job.Id] = job;
        _logger.LogInformation("Updated backup job: {JobId} - {JobName}", job.Id, job.Name);

        return Task.FromResult(job);
    }

    public Task<bool> DeleteJobAsync(string jobId)
    {
        var removed = _jobs.TryRemove(jobId, out _);
        if (removed)
        {
            _logger.LogInformation("Deleted backup job: {JobId}", jobId);
        }
        return Task.FromResult(removed);
    }

    public async Task<bool> EnableJobAsync(string jobId)
    {
        var job = await GetJobAsync(jobId);
        if (job == null) return false;

        job.IsEnabled = true;
        job.Status = BackupJobStatus.Idle;
        job.UpdatedAt = DateTime.UtcNow;
        job.CalculateNextRun();

        _logger.LogInformation("Enabled backup job: {JobId}", jobId);
        return true;
    }

    public async Task<bool> DisableJobAsync(string jobId)
    {
        var job = await GetJobAsync(jobId);
        if (job == null) return false;

        job.IsEnabled = false;
        job.Status = BackupJobStatus.Disabled;
        job.UpdatedAt = DateTime.UtcNow;
        job.NextRunAt = null;

        _logger.LogInformation("Disabled backup job: {JobId}", jobId);
        return true;
    }

    #endregion

    #region Backup Execution

    public async Task<BackupResult> ExecuteBackupAsync(string jobId)
    {
        var job = await GetJobAsync(jobId);
        if (job == null)
        {
            return BackupResult.Failure(jobId, "Unknown", "Job not found");
        }

        return await ExecuteBackupAsync(job);
    }

    public async Task<BackupResult> ExecuteBackupAsync(BackupJob job)
    {
        var result = BackupResult.Running(job.Id, job.Name);
        result.BackupType = job.Type;
        result.Target = job.Target;
        result.StorageType = job.StorageType;
        _results[result.Id] = result;

        job.MarkRunning();

        try
        {
            _logger.LogInformation("Starting backup execution: {JobId} - {JobName}", job.Id, job.Name);

            // Generate output file path
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{job.DatabaseName}_{timestamp}.backup";
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);

            // Execute database backup
            var dbResult = await _databaseProvider.BackupAsync(new DatabaseBackupRequest
            {
                ConnectionString = job.ConnectionString,
                DatabaseName = job.DatabaseName,
                OutputPath = tempPath,
                BackupType = job.Type,
                Compress = job.CompressBackup,
                TimeoutSeconds = _options.BackupTimeoutMinutes * 60
            });

            if (!dbResult.Success)
            {
                result = BackupResult.Failure(job.Id, job.Name,
                    dbResult.ErrorMessage ?? "Database backup failed", dbResult.ErrorDetails);
                _results[result.Id] = result;
                job.MarkFailure();
                return result;
            }

            // Upload to storage
            var storagePath = Path.Combine(job.StoragePath, fileName);
            var uploadResult = await _storageProvider.UploadAsync(tempPath, storagePath);

            // Clean up temp file
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            if (!uploadResult.Success)
            {
                result = BackupResult.Failure(job.Id, job.Name,
                    uploadResult.ErrorMessage ?? "Storage upload failed");
                _results[result.Id] = result;
                job.MarkFailure();
                return result;
            }

            // Update result
            result = BackupResult.Success(job.Id, job.Name, storagePath,
                uploadResult.FileSizeBytes, uploadResult.Checksum);
            result.BackupType = job.Type;
            result.Target = job.Target;
            result.StorageType = job.StorageType;
            result.ExpiresAt = DateTime.UtcNow.AddDays(job.RetentionDays);
            _results[result.Id] = result;

            job.MarkSuccess();

            // Verify if enabled
            if (_options.VerifyBackupAfterCreation)
            {
                result.IsVerified = await VerifyBackupIntegrityAsync(storagePath, uploadResult.Checksum);
                result.VerifiedAt = DateTime.UtcNow;
            }

            _logger.LogInformation(
                "Backup completed: {JobId}, File: {FilePath}, Size: {Size}",
                job.Id, storagePath, result.GetFormattedSize());

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup execution failed: {JobId}", job.Id);

            result = BackupResult.Failure(job.Id, job.Name, ex.Message, ex.StackTrace);
            _results[result.Id] = result;
            job.MarkFailure();

            return result;
        }
    }

    public async Task<bool> CancelBackupAsync(string backupResultId)
    {
        if (_results.TryGetValue(backupResultId, out var result))
        {
            if (result.Status == BackupExecutionStatus.Running)
            {
                result.Status = BackupExecutionStatus.Cancelled;
                result.CompletedAt = DateTime.UtcNow;
                _logger.LogInformation("Cancelled backup: {ResultId}", backupResultId);
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Backup Results

    public Task<BackupResult?> GetBackupResultAsync(string resultId)
    {
        _results.TryGetValue(resultId, out var result);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<BackupResult>> GetBackupResultsAsync(string jobId)
    {
        var results = _results.Values
            .Where(r => r.JobId == jobId)
            .OrderByDescending(r => r.StartedAt)
            .ToList();
        return Task.FromResult<IEnumerable<BackupResult>>(results);
    }

    public Task<IEnumerable<BackupResult>> GetRecentBackupResultsAsync(int count = 10)
    {
        var results = _results.Values
            .OrderByDescending(r => r.StartedAt)
            .Take(count)
            .ToList();
        return Task.FromResult<IEnumerable<BackupResult>>(results);
    }

    public Task<IEnumerable<BackupResult>> GetBackupResultsByDateRangeAsync(DateTime from, DateTime to)
    {
        var results = _results.Values
            .Where(r => r.StartedAt >= from && r.StartedAt <= to)
            .OrderByDescending(r => r.StartedAt)
            .ToList();
        return Task.FromResult<IEnumerable<BackupResult>>(results);
    }

    #endregion

    #region Verification & Cleanup

    public async Task<bool> VerifyBackupAsync(string backupResultId)
    {
        var result = await GetBackupResultAsync(backupResultId);
        if (result == null || string.IsNullOrEmpty(result.Checksum))
        {
            return false;
        }

        return await VerifyBackupIntegrityAsync(result.FilePath, result.Checksum);
    }

    public async Task<bool> VerifyBackupIntegrityAsync(string filePath, string? expectedChecksum)
    {
        if (string.IsNullOrEmpty(expectedChecksum))
        {
            return false;
        }

        return await _storageProvider.VerifyIntegrityAsync(filePath, expectedChecksum);
    }

    public async Task<int> CleanupExpiredBackupsAsync()
    {
        return await CleanupBackupsOlderThanAsync(DateTime.UtcNow);
    }

    public async Task<int> CleanupBackupsOlderThanAsync(DateTime cutoffDate)
    {
        var expiredResults = _results.Values
            .Where(r => r.ExpiresAt.HasValue && r.ExpiresAt.Value <= cutoffDate)
            .ToList();

        var deletedCount = 0;
        foreach (var result in expiredResults)
        {
            if (await _storageProvider.DeleteAsync(result.FilePath))
            {
                result.Status = BackupExecutionStatus.Expired;
                deletedCount++;
                _logger.LogInformation("Cleaned up expired backup: {FilePath}", result.FilePath);
            }
        }

        return deletedCount;
    }

    #endregion

    #region Statistics

    public async Task<BackupStatistics> GetStatisticsAsync()
    {
        var jobs = _jobs.Values.ToList();
        var results = _results.Values.ToList();
        var storageUsed = await _storageProvider.GetTotalStorageUsedAsync();

        var stats = BackupStatistics.Empty()
            .WithJobCounts(
                jobs.Count,
                jobs.Count(j => j.IsEnabled),
                jobs.Count(j => j.Status == BackupJobStatus.Running))
            .WithBackupCounts(
                results.Count,
                results.Count(r => r.Status == BackupExecutionStatus.Completed),
                results.Count(r => r.Status == BackupExecutionStatus.Failed),
                results.Count(r => r.Status == BackupExecutionStatus.Pending || r.Status == BackupExecutionStatus.Running));

        stats.TotalStorageUsedBytes = storageUsed;
        stats.LastBackupAt = results
            .Where(r => r.Status == BackupExecutionStatus.Completed)
            .OrderByDescending(r => r.CompletedAt)
            .FirstOrDefault()?.CompletedAt;

        var completedBackups = results
            .Where(r => r.Status == BackupExecutionStatus.Completed && r.Duration.HasValue)
            .ToList();

        if (completedBackups.Any())
        {
            stats.AverageBackupDurationSeconds = completedBackups
                .Average(r => r.Duration!.Value.TotalSeconds);
        }

        stats.BackupsByTarget = results
            .GroupBy(r => r.Target)
            .ToDictionary(g => g.Key, g => g.Count());

        stats.BackupsByStorageType = results
            .GroupBy(r => r.StorageType)
            .ToDictionary(g => g.Key, g => g.Count());

        stats.BackupsByJob = results
            .GroupBy(r => r.JobName)
            .ToDictionary(g => g.Key, g => g.Count());

        return stats;
    }

    #endregion
}
