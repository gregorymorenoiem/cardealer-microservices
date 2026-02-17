using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackupDRService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly ILogger<BackupController> _logger;

    public BackupController(IBackupService backupService, ILogger<BackupController> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    #region Backup Jobs

    /// <summary>
    /// Get all backup jobs
    /// </summary>
    [HttpGet("jobs")]
    public async Task<ActionResult<IEnumerable<BackupJob>>> GetAllJobs()
    {
        var jobs = await _backupService.GetAllJobsAsync();
        return Ok(jobs);
    }

    /// <summary>
    /// Get enabled backup jobs
    /// </summary>
    [HttpGet("jobs/enabled")]
    public async Task<ActionResult<IEnumerable<BackupJob>>> GetEnabledJobs()
    {
        var jobs = await _backupService.GetEnabledJobsAsync();
        return Ok(jobs);
    }

    /// <summary>
    /// Get a backup job by ID
    /// </summary>
    [HttpGet("jobs/{id}")]
    public async Task<ActionResult<BackupJob>> GetJob(string id)
    {
        var job = await _backupService.GetJobAsync(id);
        if (job == null)
        {
            return NotFound(new { message = $"Backup job {id} not found" });
        }
        return Ok(job);
    }

    /// <summary>
    /// Get a backup job by name
    /// </summary>
    [HttpGet("jobs/by-name/{name}")]
    public async Task<ActionResult<BackupJob>> GetJobByName(string name)
    {
        var job = await _backupService.GetJobByNameAsync(name);
        if (job == null)
        {
            return NotFound(new { message = $"Backup job '{name}' not found" });
        }
        return Ok(job);
    }

    /// <summary>
    /// Create a new backup job
    /// </summary>
    [HttpPost("jobs")]
    public async Task<ActionResult<BackupJob>> CreateJob([FromBody] CreateBackupJobRequest request)
    {
        var job = new BackupJob
        {
            Name = request.Name,
            Description = request.Description ?? "",
            Type = request.Type,
            Target = request.Target,
            ConnectionString = request.ConnectionString,
            DatabaseName = request.DatabaseName,
            Schedule = request.Schedule ?? "",
            StorageType = request.StorageType,
            StoragePath = request.StoragePath,
            RetentionDays = request.RetentionDays,
            IsEnabled = request.IsEnabled,
            CompressBackup = request.CompressBackup,
            EncryptBackup = request.EncryptBackup
        };

        var created = await _backupService.CreateJobAsync(job);
        return CreatedAtAction(nameof(GetJob), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update an existing backup job
    /// </summary>
    [HttpPut("jobs/{id}")]
    public async Task<ActionResult<BackupJob>> UpdateJob(string id, [FromBody] UpdateBackupJobRequest request)
    {
        var job = await _backupService.GetJobAsync(id);
        if (job == null)
        {
            return NotFound(new { message = $"Backup job {id} not found" });
        }

        if (!string.IsNullOrEmpty(request.Name)) job.Name = request.Name;
        if (request.Description != null) job.Description = request.Description;
        if (request.Type.HasValue) job.Type = request.Type.Value;
        if (request.Schedule != null) job.Schedule = request.Schedule;
        if (request.StoragePath != null) job.StoragePath = request.StoragePath;
        if (request.RetentionDays.HasValue) job.RetentionDays = request.RetentionDays.Value;
        if (request.IsEnabled.HasValue) job.IsEnabled = request.IsEnabled.Value;
        if (request.CompressBackup.HasValue) job.CompressBackup = request.CompressBackup.Value;
        if (request.EncryptBackup.HasValue) job.EncryptBackup = request.EncryptBackup.Value;

        var updated = await _backupService.UpdateJobAsync(job);
        return Ok(updated);
    }

    /// <summary>
    /// Delete a backup job
    /// </summary>
    [HttpDelete("jobs/{id}")]
    public async Task<IActionResult> DeleteJob(string id)
    {
        var deleted = await _backupService.DeleteJobAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Backup job {id} not found" });
        }
        return NoContent();
    }

    /// <summary>
    /// Enable a backup job
    /// </summary>
    [HttpPost("jobs/{id}/enable")]
    public async Task<IActionResult> EnableJob(string id)
    {
        var enabled = await _backupService.EnableJobAsync(id);
        if (!enabled)
        {
            return NotFound(new { message = $"Backup job {id} not found" });
        }
        return Ok(new { message = $"Backup job {id} enabled" });
    }

    /// <summary>
    /// Disable a backup job
    /// </summary>
    [HttpPost("jobs/{id}/disable")]
    public async Task<IActionResult> DisableJob(string id)
    {
        var disabled = await _backupService.DisableJobAsync(id);
        if (!disabled)
        {
            return NotFound(new { message = $"Backup job {id} not found" });
        }
        return Ok(new { message = $"Backup job {id} disabled" });
    }

    #endregion

    #region Backup Execution

    /// <summary>
    /// Trigger a backup job execution
    /// </summary>
    [HttpPost("jobs/{id}/execute")]
    public async Task<ActionResult<BackupResult>> ExecuteBackup(string id)
    {
        _logger.LogInformation("Triggering backup execution for job: {JobId}", id);

        var result = await _backupService.ExecuteBackupAsync(id);

        if (result.Status == BackupExecutionStatus.Failed)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel a running backup
    /// </summary>
    [HttpPost("results/{id}/cancel")]
    public async Task<IActionResult> CancelBackup(string id)
    {
        var cancelled = await _backupService.CancelBackupAsync(id);
        if (!cancelled)
        {
            return BadRequest(new { message = "Cannot cancel backup - not found or not running" });
        }
        return Ok(new { message = $"Backup {id} cancelled" });
    }

    #endregion

    #region Backup Results

    /// <summary>
    /// Get recent backup results
    /// </summary>
    [HttpGet("results")]
    public async Task<ActionResult<IEnumerable<BackupResult>>> GetRecentResults([FromQuery] int count = 10)
    {
        var results = await _backupService.GetRecentBackupResultsAsync(count);
        return Ok(results);
    }

    /// <summary>
    /// Get backup results for a job
    /// </summary>
    [HttpGet("jobs/{jobId}/results")]
    public async Task<ActionResult<IEnumerable<BackupResult>>> GetJobResults(string jobId)
    {
        var results = await _backupService.GetBackupResultsAsync(jobId);
        return Ok(results);
    }

    /// <summary>
    /// Get a specific backup result
    /// </summary>
    [HttpGet("results/{id}")]
    public async Task<ActionResult<BackupResult>> GetResult(string id)
    {
        var result = await _backupService.GetBackupResultAsync(id);
        if (result == null)
        {
            return NotFound(new { message = $"Backup result {id} not found" });
        }
        return Ok(result);
    }

    /// <summary>
    /// Get backup results by date range
    /// </summary>
    [HttpGet("results/by-date")]
    public async Task<ActionResult<IEnumerable<BackupResult>>> GetResultsByDateRange(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var results = await _backupService.GetBackupResultsByDateRangeAsync(from, to);
        return Ok(results);
    }

    /// <summary>
    /// Verify a backup result
    /// </summary>
    [HttpPost("results/{id}/verify")]
    public async Task<IActionResult> VerifyBackup(string id)
    {
        var verified = await _backupService.VerifyBackupAsync(id);
        return Ok(new { verified, resultId = id });
    }

    #endregion

    #region Cleanup & Statistics

    /// <summary>
    /// Cleanup expired backups
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<IActionResult> CleanupExpiredBackups()
    {
        var count = await _backupService.CleanupExpiredBackupsAsync();
        return Ok(new { deletedCount = count, message = $"Cleaned up {count} expired backups" });
    }

    /// <summary>
    /// Get backup statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<BackupStatistics>> GetStatistics()
    {
        var stats = await _backupService.GetStatisticsAsync();
        return Ok(stats);
    }

    #endregion
}

#region Request DTOs

public class CreateBackupJobRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BackupType Type { get; set; } = BackupType.Full;
    public BackupTarget Target { get; set; } = BackupTarget.PostgreSQL;
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string? Schedule { get; set; }
    public StorageType StorageType { get; set; } = StorageType.Local;
    public string StoragePath { get; set; } = string.Empty;
    public int RetentionDays { get; set; } = 30;
    public bool IsEnabled { get; set; } = true;
    public bool CompressBackup { get; set; } = true;
    public bool EncryptBackup { get; set; } = false;
}

public class UpdateBackupJobRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public BackupType? Type { get; set; }
    public string? Schedule { get; set; }
    public string? StoragePath { get; set; }
    public int? RetentionDays { get; set; }
    public bool? IsEnabled { get; set; }
    public bool? CompressBackup { get; set; }
    public bool? EncryptBackup { get; set; }
}

#endregion
