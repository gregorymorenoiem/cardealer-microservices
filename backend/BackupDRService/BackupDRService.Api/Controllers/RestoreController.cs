using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackupDRService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestoreController : ControllerBase
{
    private readonly IRestoreService _restoreService;
    private readonly IBackupService _backupService;
    private readonly ILogger<RestoreController> _logger;

    public RestoreController(
        IRestoreService restoreService,
        IBackupService backupService,
        ILogger<RestoreController> logger)
    {
        _restoreService = restoreService;
        _backupService = backupService;
        _logger = logger;
    }

    #region Restore Points

    /// <summary>
    /// Get all restore points
    /// </summary>
    [HttpGet("points")]
    public async Task<ActionResult<IEnumerable<RestorePoint>>> GetAllRestorePoints()
    {
        var points = await _restoreService.GetAllRestorePointsAsync();
        return Ok(points);
    }

    /// <summary>
    /// Get available restore points
    /// </summary>
    [HttpGet("points/available")]
    public async Task<ActionResult<IEnumerable<RestorePoint>>> GetAvailableRestorePoints()
    {
        var points = await _restoreService.GetAvailableRestorePointsAsync();
        return Ok(points);
    }

    /// <summary>
    /// Get restore points for a job
    /// </summary>
    [HttpGet("points/by-job/{jobId}")]
    public async Task<ActionResult<IEnumerable<RestorePoint>>> GetRestorePointsByJob(string jobId)
    {
        var points = await _restoreService.GetRestorePointsByJobAsync(jobId);
        return Ok(points);
    }

    /// <summary>
    /// Get a specific restore point
    /// </summary>
    [HttpGet("points/{id}")]
    public async Task<ActionResult<RestorePoint>> GetRestorePoint(string id)
    {
        var point = await _restoreService.GetRestorePointAsync(id);
        if (point == null)
        {
            return NotFound(new { message = $"Restore point {id} not found" });
        }
        return Ok(point);
    }

    /// <summary>
    /// Create a restore point from a backup result
    /// </summary>
    [HttpPost("points")]
    public async Task<ActionResult<RestorePoint>> CreateRestorePoint([FromBody] CreateRestorePointRequest request)
    {
        var backupResult = await _backupService.GetBackupResultAsync(request.BackupResultId);
        if (backupResult == null)
        {
            return NotFound(new { message = $"Backup result {request.BackupResultId} not found" });
        }

        var restorePoint = await _restoreService.CreateRestorePointAsync(
            backupResult,
            request.Name,
            request.Description);

        return CreatedAtAction(nameof(GetRestorePoint), new { id = restorePoint.Id }, restorePoint);
    }

    /// <summary>
    /// Delete a restore point
    /// </summary>
    [HttpDelete("points/{id}")]
    public async Task<IActionResult> DeleteRestorePoint(string id)
    {
        var deleted = await _restoreService.DeleteRestorePointAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Restore point {id} not found" });
        }
        return NoContent();
    }

    /// <summary>
    /// Verify a restore point
    /// </summary>
    [HttpPost("points/{id}/verify")]
    public async Task<IActionResult> VerifyRestorePoint(string id)
    {
        var verified = await _restoreService.VerifyRestorePointAsync(id);
        return Ok(new { verified, restorePointId = id });
    }

    /// <summary>
    /// Test a restore point (verify file exists and checksum)
    /// </summary>
    [HttpPost("points/{id}/test")]
    public async Task<IActionResult> TestRestorePoint(string id)
    {
        var success = await _restoreService.TestRestoreAsync(id);
        return Ok(new { success, restorePointId = id, message = success ? "Restore point is valid" : "Restore point validation failed" });
    }

    #endregion

    #region Restore Execution

    /// <summary>
    /// Restore from a restore point
    /// </summary>
    [HttpPost("points/{id}/restore")]
    public async Task<ActionResult<RestoreResult>> RestoreFromPoint(string id, [FromBody] RestoreRequest? request = null)
    {
        _logger.LogInformation("Triggering restore from point: {RestorePointId}", id);

        var options = request != null ? new RestoreOptions
        {
            TargetConnectionString = request.TargetConnectionString,
            TargetDatabaseName = request.TargetDatabaseName,
            Mode = request.Mode,
            DropExistingDatabase = request.DropExistingDatabase,
            CreateIfNotExists = request.CreateIfNotExists,
            InitiatedBy = request.InitiatedBy
        } : null;

        var result = await _restoreService.RestoreFromPointAsync(id, options);

        if (result.Status == RestoreExecutionStatus.Failed)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Restore directly from a backup result
    /// </summary>
    [HttpPost("from-backup/{backupResultId}")]
    public async Task<ActionResult<RestoreResult>> RestoreFromBackup(string backupResultId, [FromBody] RestoreRequest? request = null)
    {
        _logger.LogInformation("Triggering restore from backup: {BackupResultId}", backupResultId);

        var options = request != null ? new RestoreOptions
        {
            TargetConnectionString = request.TargetConnectionString,
            TargetDatabaseName = request.TargetDatabaseName,
            Mode = request.Mode,
            DropExistingDatabase = request.DropExistingDatabase,
            CreateIfNotExists = request.CreateIfNotExists,
            InitiatedBy = request.InitiatedBy
        } : null;

        var result = await _restoreService.RestoreFromBackupAsync(backupResultId, options);

        if (result.Status == RestoreExecutionStatus.Failed)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel a running restore
    /// </summary>
    [HttpPost("results/{id}/cancel")]
    public async Task<IActionResult> CancelRestore(string id)
    {
        var cancelled = await _restoreService.CancelRestoreAsync(id);
        if (!cancelled)
        {
            return BadRequest(new { message = "Cannot cancel restore - not found or not running" });
        }
        return Ok(new { message = $"Restore {id} cancelled" });
    }

    #endregion

    #region Restore Results

    /// <summary>
    /// Get all restore results
    /// </summary>
    [HttpGet("results")]
    public async Task<ActionResult<IEnumerable<RestoreResult>>> GetRestoreResults()
    {
        var results = await _restoreService.GetRestoreResultsAsync();
        return Ok(results);
    }

    /// <summary>
    /// Get recent restore results
    /// </summary>
    [HttpGet("results/recent")]
    public async Task<ActionResult<IEnumerable<RestoreResult>>> GetRecentRestoreResults([FromQuery] int count = 10)
    {
        var results = await _restoreService.GetRecentRestoreResultsAsync(count);
        return Ok(results);
    }

    /// <summary>
    /// Get a specific restore result
    /// </summary>
    [HttpGet("results/{id}")]
    public async Task<ActionResult<RestoreResult>> GetRestoreResult(string id)
    {
        var result = await _restoreService.GetRestoreResultAsync(id);
        if (result == null)
        {
            return NotFound(new { message = $"Restore result {id} not found" });
        }
        return Ok(result);
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Cleanup expired restore points
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<IActionResult> CleanupExpiredRestorePoints()
    {
        var count = await _restoreService.CleanupExpiredRestorePointsAsync();
        return Ok(new { deletedCount = count, message = $"Cleaned up {count} expired restore points" });
    }

    #endregion
}

#region Request DTOs

public class CreateRestorePointRequest
{
    public string BackupResultId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class RestoreRequest
{
    public string? TargetConnectionString { get; set; }
    public string? TargetDatabaseName { get; set; }
    public RestoreMode Mode { get; set; } = RestoreMode.InPlace;
    public bool DropExistingDatabase { get; set; } = false;
    public bool CreateIfNotExists { get; set; } = true;
    public string? InitiatedBy { get; set; }
}

#endregion
