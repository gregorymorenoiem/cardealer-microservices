using BackupDRService.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackupDRService.Api.Controllers;

/// <summary>
/// Controller for scheduler monitoring and health metrics
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class SchedulerMonitoringController : ControllerBase
{
    private readonly SchedulerMonitoringService _monitoringService;
    private readonly ILogger<SchedulerMonitoringController> _logger;

    public SchedulerMonitoringController(
        SchedulerMonitoringService monitoringService,
        ILogger<SchedulerMonitoringController> logger)
    {
        _monitoringService = monitoringService;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive scheduler health metrics
    /// </summary>
    /// <returns>Health metrics including status, statistics, and active schedules</returns>
    /// <response code="200">Returns health metrics successfully</response>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthMetrics()
    {
        var metrics = await _monitoringService.GetHealthMetricsAsync();
        return Ok(metrics);
    }

    /// <summary>
    /// Get upcoming scheduled backups
    /// </summary>
    /// <param name="hours">Number of hours to look ahead (default: 24)</param>
    /// <returns>List of scheduled backups within the specified timeframe</returns>
    /// <response code="200">Returns upcoming backups successfully</response>
    [HttpGet("upcoming")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcomingBackups([FromQuery] int hours = 24)
    {
        if (hours < 1 || hours > 168) // Max 1 week
        {
            return BadRequest("Hours must be between 1 and 168");
        }

        var upcoming = await _monitoringService.GetUpcomingBackupsAsync(hours);
        return Ok(new
        {
            TimeframeHours = hours,
            Count = upcoming.Count,
            Schedules = upcoming
        });
    }

    /// <summary>
    /// Get scheduler statistics
    /// </summary>
    /// <returns>Summary statistics for the scheduler</returns>
    /// <response code="200">Returns statistics successfully</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
        var metrics = await _monitoringService.GetHealthMetricsAsync();
        return Ok(new
        {
            Statistics = metrics.Stats,
            CheckTime = metrics.CheckTime,
            IsHealthy = metrics.IsHealthy,
            Issues = metrics.Issues
        });
    }

    /// <summary>
    /// Simple health check endpoint for monitoring tools
    /// </summary>
    /// <returns>OK if scheduler is healthy, Service Unavailable otherwise</returns>
    /// <response code="200">Scheduler is healthy</response>
    /// <response code="503">Scheduler has issues</response>
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ping()
    {
        var metrics = await _monitoringService.GetHealthMetricsAsync();

        if (metrics.IsHealthy)
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            });
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, new
        {
            Status = "Degraded",
            Issues = metrics.Issues,
            Timestamp = DateTime.UtcNow
        });
    }
}
