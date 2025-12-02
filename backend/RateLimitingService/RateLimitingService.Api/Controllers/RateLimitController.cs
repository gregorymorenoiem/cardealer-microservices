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
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ILogger<RateLimitController> _logger;

    public RateLimitController(
        IRateLimitingService rateLimitingService,
        ILogger<RateLimitController> logger)
    {
        _rateLimitingService = rateLimitingService ?? throw new ArgumentNullException(nameof(rateLimitingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check rate limit for a client
    /// </summary>
    /// <param name="clientId">Client identifier</param>
    /// <param name="endpoint">Endpoint to check</param>
    /// <param name="tier">User tier</param>
    /// <returns>Rate limit result</returns>
    [HttpGet("check")]
    [ProducesResponseType(typeof(RateLimitResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<RateLimitResult>> CheckRateLimit(
        [FromQuery] string clientId,
        [FromQuery] string endpoint,
        [FromQuery] string? tier = null)
    {
        if (string.IsNullOrEmpty(clientId))
            return BadRequest("Client ID is required");

        if (string.IsNullOrEmpty(endpoint))
            return BadRequest("Endpoint is required");

        var result = await _rateLimitingService.CheckRateLimitAsync(clientId, endpoint, tier);
        return Ok(result);
    }

    /// <summary>
    /// Get all policies
    /// </summary>
    /// <returns>List of policies</returns>
    [HttpGet("policies")]
    [ProducesResponseType(typeof(IEnumerable<RateLimitPolicy>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RateLimitPolicy>>> GetPolicies()
    {
        var policies = await _rateLimitingService.GetAllPoliciesAsync();
        return Ok(policies);
    }

    /// <summary>
    /// Get a specific policy
    /// </summary>
    /// <param name="id">Policy ID</param>
    /// <returns>Policy if found</returns>
    [HttpGet("policies/{id}")]
    [ProducesResponseType(typeof(RateLimitPolicy), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RateLimitPolicy>> GetPolicy(string id)
    {
        var policy = await _rateLimitingService.GetPolicyAsync(id);

        if (policy == null)
            return NotFound($"Policy {id} not found");

        return Ok(policy);
    }

    /// <summary>
    /// Create a new policy
    /// </summary>
    /// <param name="request">Policy to create</param>
    /// <returns>Created policy</returns>
    [HttpPost("policies")]
    [ProducesResponseType(typeof(RateLimitPolicy), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RateLimitPolicy>> CreatePolicy([FromBody] CreatePolicyRequest request)
    {
        if (request == null)
            return BadRequest("Policy is required");

        var policy = new RateLimitPolicy
        {
            Name = request.Name,
            Description = request.Description,
            Tier = request.Tier,
            WindowSeconds = request.WindowSeconds,
            MaxRequests = request.MaxRequests,
            BurstLimit = request.BurstLimit,
            Endpoints = request.Endpoints,
            Enabled = request.Enabled
        };

        var created = await _rateLimitingService.CreatePolicyAsync(policy);

        _logger.LogInformation("Created rate limit policy {PolicyId}: {PolicyName}", created.Id, created.Name);

        return CreatedAtAction(nameof(GetPolicy), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update an existing policy
    /// </summary>
    /// <param name="id">Policy ID</param>
    /// <param name="request">Updated policy data</param>
    /// <returns>Updated policy</returns>
    [HttpPut("policies/{id}")]
    [ProducesResponseType(typeof(RateLimitPolicy), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RateLimitPolicy>> UpdatePolicy(string id, [FromBody] UpdatePolicyRequest request)
    {
        var existing = await _rateLimitingService.GetPolicyAsync(id);
        if (existing == null)
            return NotFound($"Policy {id} not found");

        existing.Name = request.Name ?? existing.Name;
        existing.Description = request.Description ?? existing.Description;
        existing.WindowSeconds = request.WindowSeconds ?? existing.WindowSeconds;
        existing.MaxRequests = request.MaxRequests ?? existing.MaxRequests;
        existing.BurstLimit = request.BurstLimit ?? existing.BurstLimit;
        existing.Endpoints = request.Endpoints ?? existing.Endpoints;
        existing.Enabled = request.Enabled ?? existing.Enabled;

        if (request.Tier.HasValue)
            existing.Tier = request.Tier.Value;

        var updated = await _rateLimitingService.UpdatePolicyAsync(existing);

        _logger.LogInformation("Updated rate limit policy {PolicyId}", id);

        return Ok(updated);
    }

    /// <summary>
    /// Delete a policy
    /// </summary>
    /// <param name="id">Policy ID</param>
    /// <returns>No content if deleted</returns>
    [HttpDelete("policies/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePolicy(string id)
    {
        var deleted = await _rateLimitingService.DeletePolicyAsync(id);

        if (!deleted)
            return NotFound($"Policy {id} not found");

        _logger.LogInformation("Deleted rate limit policy {PolicyId}", id);

        return NoContent();
    }

    /// <summary>
    /// Get rate limit statistics
    /// </summary>
    /// <param name="from">Start time</param>
    /// <param name="to">End time</param>
    /// <returns>Statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(RateLimitStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<RateLimitStatistics>> GetStatistics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var stats = await _rateLimitingService.GetStatisticsAsync(from, to);
        return Ok(stats);
    }

    /// <summary>
    /// Get client usage
    /// </summary>
    /// <param name="clientId">Client identifier</param>
    /// <returns>Client statistics</returns>
    [HttpGet("clients/{clientId}")]
    [ProducesResponseType(typeof(ClientStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientStatistics>> GetClientUsage(string clientId)
    {
        var stats = await _rateLimitingService.GetClientUsageAsync(clientId);

        if (stats == null)
            return NotFound($"No usage data for client {clientId}");

        return Ok(stats);
    }

    /// <summary>
    /// Reset rate limit for a client
    /// </summary>
    /// <param name="clientId">Client identifier</param>
    /// <returns>No content if reset</returns>
    [HttpDelete("clients/{clientId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetClientLimit(string clientId)
    {
        var reset = await _rateLimitingService.ResetClientLimitAsync(clientId);

        if (!reset)
            return NotFound($"No rate limit data for client {clientId}");

        _logger.LogInformation("Reset rate limit for client {ClientId}", clientId);

        return NoContent();
    }

    /// <summary>
    /// Get available tiers and their default limits
    /// </summary>
    /// <returns>Tier definitions</returns>
    [HttpGet("tiers")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public ActionResult<Dictionary<string, int>> GetTiers()
    {
        return Ok(UserTiers.DefaultLimits);
    }
}

/// <summary>
/// Request model for creating a policy
/// </summary>
public class CreatePolicyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RateLimitTier Tier { get; set; } = RateLimitTier.Free;
    public int WindowSeconds { get; set; } = 60;
    public int MaxRequests { get; set; } = 100;
    public int BurstLimit { get; set; } = 10;
    public List<string> Endpoints { get; set; } = new();
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Request model for updating a policy
/// </summary>
public class UpdatePolicyRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public RateLimitTier? Tier { get; set; }
    public int? WindowSeconds { get; set; }
    public int? MaxRequests { get; set; }
    public int? BurstLimit { get; set; }
    public List<string>? Endpoints { get; set; }
    public bool? Enabled { get; set; }
}
