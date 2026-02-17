using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageService.Api.Controllers;

/// <summary>
/// Health check controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IStorageProvider _storageProvider;
    private readonly IVirusScanService _virusScanService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IStorageProvider storageProvider,
        IVirusScanService virusScanService,
        ILogger<HealthController> logger)
    {
        _storageProvider = storageProvider;
        _virusScanService = virusScanService;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthStatus>> GetHealth()
    {
        var status = new HealthStatus
        {
            Service = "FileStorageService",
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Check storage
            var storageHealthy = await _storageProvider.IsHealthyAsync();
            status.Components["Storage"] = new ComponentHealth
            {
                Status = storageHealthy ? "Healthy" : "Unhealthy",
                Provider = _storageProvider.ProviderType.ToString()
            };

            // Check virus scanner
            var scannerHealthy = await _virusScanService.IsHealthyAsync();
            var scannerInfo = await _virusScanService.GetScannerInfoAsync();
            status.Components["VirusScanner"] = new ComponentHealth
            {
                Status = scannerHealthy ? "Healthy" : "Unhealthy",
                Details = scannerInfo
            };

            status.IsHealthy = storageHealthy && scannerHealthy;
            status.Status = status.IsHealthy ? "Healthy" : "Degraded";

            return status.IsHealthy ? Ok(status) : StatusCode(503, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            status.IsHealthy = false;
            status.Status = "Unhealthy";
            status.Error = ex.Message;
            return StatusCode(503, status);
        }
    }

    /// <summary>
    /// Liveness probe
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Live()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Readiness probe
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready()
    {
        var storageHealthy = await _storageProvider.IsHealthyAsync();

        if (storageHealthy)
        {
            return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
        }

        return StatusCode(503, new { status = "not_ready", reason = "Storage not available" });
    }
}

/// <summary>
/// Health status response
/// </summary>
public class HealthStatus
{
    public string Service { get; set; } = string.Empty;
    public string Status { get; set; } = "Unknown";
    public bool IsHealthy { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, ComponentHealth> Components { get; set; } = new();
}

/// <summary>
/// Component health status
/// </summary>
public class ComponentHealth
{
    public string Status { get; set; } = "Unknown";
    public string? Provider { get; set; }
    public Dictionary<string, string>? Details { get; set; }
}
