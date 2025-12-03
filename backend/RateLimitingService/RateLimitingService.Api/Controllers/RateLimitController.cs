using Microsoft.AspNetCore.Mvc;
using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;

namespace RateLimitingService.Api.Controllers;

/// <summary>
/// Controller for rate limiting management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RateLimitController : ControllerBase
{
    private readonly IRateLimitService _rateLimitService;
    private readonly ILogger<RateLimitController> _logger;

    public RateLimitController(
        IRateLimitService rateLimitService,
        ILogger<RateLimitController> logger)
    {
        _rateLimitService = rateLimitService ?? throw new ArgumentNullException(nameof(rateLimitService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check if request is allowed under rate limit
    /// </summary>
    [HttpPost("check")]
    [ProducesResponseType(typeof(RateLimitCheckResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RateLimitCheckResult>> Check([FromBody] RateLimitCheckRequest request)
    {
        if (string.IsNullOrEmpty(request.Identifier))
        {
            return BadRequest("Identifier is required");
        }

        var result = await _rateLimitService.CheckAsync(request);

        // Add headers
        Response.Headers.Append("X-RateLimit-Limit", result.Limit.ToString());
        Response.Headers.Append("X-RateLimit-Remaining", result.Remaining.ToString());
        Response.Headers.Append("X-RateLimit-Reset", result.ResetAt.ToString());

        if (!result.IsAllowed && result.RetryAfterSeconds > 0)
        {
            Response.Headers.Append("Retry-After", result.RetryAfterSeconds.ToString());
        }

        return Ok(result);
    }

    /// <summary>
    /// Get current rate limit status for identifier
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(RateLimitCheckResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RateLimitCheckResult>> GetStatus(
        [FromQuery] string identifier,
        [FromQuery] RateLimitIdentifierType type,
        [FromQuery] string? endpoint = null)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return BadRequest("Identifier is required");
        }

        var result = await _rateLimitService.GetStatusAsync(identifier, type, endpoint);

        Response.Headers.Append("X-RateLimit-Limit", result.Limit.ToString());
        Response.Headers.Append("X-RateLimit-Remaining", result.Remaining.ToString());
        Response.Headers.Append("X-RateLimit-Reset", result.ResetAt.ToString());

        return Ok(result);
    }

    /// <summary>
    /// Reset rate limit for identifier
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reset(
        [FromQuery] string identifier,
        [FromQuery] RateLimitIdentifierType type)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return BadRequest("Identifier is required");
        }

        await _rateLimitService.ResetAsync(identifier, type);
        _logger.LogInformation("Reset rate limit for {Identifier} ({Type})", identifier, type);

        return NoContent();
    }

    /// <summary>
    /// Get recent violations
    /// </summary>
    [HttpGet("violations")]
    [ProducesResponseType(typeof(IEnumerable<RateLimitViolation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RateLimitViolation>>> GetViolations([FromQuery] int count = 100)
    {
        var violations = await _rateLimitService.GetViolationsAsync(count);
        return Ok(violations);
    }

    /// <summary>
    /// Get rate limit statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(RateLimitStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<RateLimitStatistics>> GetStatistics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var stats = await _rateLimitService.GetStatisticsAsync(from, to);
        return Ok(stats);
    }
}
