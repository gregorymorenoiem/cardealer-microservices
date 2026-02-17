using Microsoft.AspNetCore.Mvc;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Api.Controllers;

/// <summary>
/// Test controller for Spyne API integration testing
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TestController : ControllerBase
{
    private readonly ISpyneApiClient _spyneClient;
    private readonly ILogger<TestController> _logger;

    public TestController(ISpyneApiClient spyneClient, ILogger<TestController> logger)
    {
        _spyneClient = spyneClient;
        _logger = logger;
    }

    /// <summary>
    /// Get available background presets (no database required)
    /// </summary>
    [HttpGet("presets")]
    [ProducesResponseType(typeof(List<SpyneBackgroundPreset>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPresets()
    {
        var presets = await _spyneClient.GetBackgroundPresetsAsync();
        return Ok(presets);
    }

    /// <summary>
    /// Test image transformation with Spyne API
    /// </summary>
    /// <param name="imageUrl">URL of the image to transform</param>
    /// <param name="backgroundId">Background ID (default: 923 - Studio White)</param>
    /// <param name="maskPlate">Whether to mask license plate (default: true)</param>
    [HttpPost("transform")]
    [ProducesResponseType(typeof(SpyneTransformResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestTransform(
        [FromQuery] string imageUrl, 
        [FromQuery] string backgroundId = "923",
        [FromQuery] bool maskPlate = true)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            return BadRequest(new { error = "imageUrl is required" });
        }

        try
        {
            _logger.LogInformation("Testing Spyne transform: {ImageUrl}, bg: {BackgroundId}, mask: {MaskPlate}", 
                imageUrl, backgroundId, maskPlate);

            var request = new SpyneTransformRequest
            {
                ImageUrl = imageUrl,
                BackgroundId = backgroundId,
                MaskLicensePlate = maskPlate,
                EnhanceQuality = true,
                StockNumber = $"test-{DateTime.UtcNow:yyyyMMddHHmmss}"
            };

            var response = await _spyneClient.TransformImageAsync(request);
            
            return Ok(new
            {
                success = true,
                jobId = response.JobId,
                status = response.Status,
                estimatedSeconds = response.EstimatedSeconds,
                message = "Image submitted for processing. Use /api/test/status/{jobId} to check progress."
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Spyne API error");
            return BadRequest(new { error = "Spyne API error", details = ex.Message });
        }
    }

    /// <summary>
    /// Check status of a transform job
    /// </summary>
    [HttpGet("status/{jobId}")]
    [ProducesResponseType(typeof(SpyneJobStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobStatus(string jobId)
    {
        try
        {
            var status = await _spyneClient.GetImageStatusAsync(jobId);
            return Ok(status);
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new { error = "Failed to get status", details = ex.Message });
        }
    }

    /// <summary>
    /// Health check that doesn't require database
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new 
        { 
            status = "ok", 
            service = "SpyneIntegrationService",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
