using LoggingService.Application.Interfaces;
using LoggingService.Domain;
using Microsoft.AspNetCore.Mvc;

namespace LoggingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly ILogAnalyzer _logAnalyzer;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(ILogAnalyzer logAnalyzer, ILogger<AnalysisController> logger)
    {
        _logAnalyzer = logAnalyzer;
        _logger = logger;
    }

    /// <summary>
    /// Analyze logs within a time range
    /// </summary>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(LogAnalysis), StatusCodes.Status200OK)]
    public async Task<ActionResult<LogAnalysis>> AnalyzeLogs(
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        CancellationToken cancellationToken = default)
    {
        var start = startTime ?? DateTime.UtcNow.AddHours(-24);
        var end = endTime ?? DateTime.UtcNow;

        _logger.LogInformation("Analyzing logs from {StartTime} to {EndTime}", start, end);

        var analysis = await _logAnalyzer.AnalyzeLogsAsync(start, end, cancellationToken);

        return Ok(analysis);
    }

    /// <summary>
    /// Detect patterns in recent logs
    /// </summary>
    [HttpGet("patterns")]
    [ProducesResponseType(typeof(List<LogPattern>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LogPattern>>> GetPatterns(
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        CancellationToken cancellationToken = default)
    {
        var start = startTime ?? DateTime.UtcNow.AddHours(-24);
        var end = endTime ?? DateTime.UtcNow;

        var analysis = await _logAnalyzer.AnalyzeLogsAsync(start, end, cancellationToken);

        return Ok(analysis.DetectedPatterns);
    }

    /// <summary>
    /// Detect anomalies in recent logs
    /// </summary>
    [HttpGet("anomalies")]
    [ProducesResponseType(typeof(List<LogAnomaly>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LogAnomaly>>> GetAnomalies(
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        CancellationToken cancellationToken = default)
    {
        var start = startTime ?? DateTime.UtcNow.AddHours(-24);
        var end = endTime ?? DateTime.UtcNow;

        var anomalies = await _logAnalyzer.DetectAnomaliesAsync(start, end, cancellationToken);

        return Ok(anomalies);
    }

    /// <summary>
    /// Get service health metrics
    /// </summary>
    [HttpGet("service-health")]
    [ProducesResponseType(typeof(Dictionary<string, ServiceHealthMetrics>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, ServiceHealthMetrics>>> GetServiceHealth(
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        CancellationToken cancellationToken = default)
    {
        var start = startTime ?? DateTime.UtcNow.AddHours(-1);
        var end = endTime ?? DateTime.UtcNow;

        var health = await _logAnalyzer.GetServiceHealthAsync(start, end, cancellationToken);

        return Ok(health);
    }

    /// <summary>
    /// Get recommendations based on analysis
    /// </summary>
    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(List<Recommendation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Recommendation>>> GetRecommendations(
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        CancellationToken cancellationToken = default)
    {
        var start = startTime ?? DateTime.UtcNow.AddHours(-24);
        var end = endTime ?? DateTime.UtcNow;

        var analysis = await _logAnalyzer.AnalyzeLogsAsync(start, end, cancellationToken);
        var recommendations = await _logAnalyzer.GenerateRecommendationsAsync(analysis, cancellationToken);

        return Ok(recommendations);
    }

    /// <summary>
    /// Get analysis summary
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(AnalysisSummary), StatusCodes.Status200OK)]
    public async Task<ActionResult<AnalysisSummary>> GetAnalysisSummary(
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        CancellationToken cancellationToken = default)
    {
        var start = startTime ?? DateTime.UtcNow.AddHours(-24);
        var end = endTime ?? DateTime.UtcNow;

        var analysis = await _logAnalyzer.AnalyzeLogsAsync(start, end, cancellationToken);

        return Ok(analysis.Summary);
    }
}
