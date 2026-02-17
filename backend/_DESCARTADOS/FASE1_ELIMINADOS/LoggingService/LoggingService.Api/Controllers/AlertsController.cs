using LoggingService.Application.Interfaces;
using LoggingService.Domain;
using Microsoft.AspNetCore.Mvc;

namespace LoggingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertingService _alertingService;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(IAlertingService alertingService, ILogger<AlertsController> logger)
    {
        _alertingService = alertingService;
        _logger = logger;
    }

    // Alert Rules

    /// <summary>
    /// Create a new alert rule
    /// </summary>
    [HttpPost("rules")]
    [ProducesResponseType(typeof(AlertRule), StatusCodes.Status201Created)]
    public async Task<ActionResult<AlertRule>> CreateRule([FromBody] AlertRule rule, CancellationToken cancellationToken = default)
    {
        var created = await _alertingService.CreateRuleAsync(rule, cancellationToken);
        return CreatedAtAction(nameof(GetRule), new { id = created.Id }, created);
    }

    /// <summary>
    /// Get an alert rule by ID
    /// </summary>
    [HttpGet("rules/{id}")]
    [ProducesResponseType(typeof(AlertRule), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertRule>> GetRule(string id, CancellationToken cancellationToken = default)
    {
        var rule = await _alertingService.GetRuleAsync(id, cancellationToken);

        if (rule == null)
        {
            return NotFound();
        }

        return Ok(rule);
    }

    /// <summary>
    /// Get all alert rules
    /// </summary>
    [HttpGet("rules")]
    [ProducesResponseType(typeof(IEnumerable<AlertRule>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlertRule>>> GetAllRules(CancellationToken cancellationToken = default)
    {
        var rules = await _alertingService.GetAllRulesAsync(cancellationToken);
        return Ok(rules);
    }

    /// <summary>
    /// Update an alert rule
    /// </summary>
    [HttpPut("rules/{id}")]
    [ProducesResponseType(typeof(AlertRule), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertRule>> UpdateRule(string id, [FromBody] AlertRule rule, CancellationToken cancellationToken = default)
    {
        var existing = await _alertingService.GetRuleAsync(id, cancellationToken);

        if (existing == null)
        {
            return NotFound();
        }

        rule.Id = id;
        var updated = await _alertingService.UpdateRuleAsync(rule, cancellationToken);

        return Ok(updated);
    }

    /// <summary>
    /// Delete an alert rule
    /// </summary>
    [HttpDelete("rules/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRule(string id, CancellationToken cancellationToken = default)
    {
        var deleted = await _alertingService.DeleteRuleAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Enable an alert rule
    /// </summary>
    [HttpPost("rules/{id}/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableRule(string id, CancellationToken cancellationToken = default)
    {
        var enabled = await _alertingService.EnableRuleAsync(id, cancellationToken);

        if (!enabled)
        {
            return NotFound();
        }

        return Ok(new { message = "Rule enabled successfully" });
    }

    /// <summary>
    /// Disable an alert rule
    /// </summary>
    [HttpPost("rules/{id}/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableRule(string id, CancellationToken cancellationToken = default)
    {
        var disabled = await _alertingService.DisableRuleAsync(id, cancellationToken);

        if (!disabled)
        {
            return NotFound();
        }

        return Ok(new { message = "Rule disabled successfully" });
    }

    /// <summary>
    /// Evaluate a specific alert rule
    /// </summary>
    [HttpPost("rules/{id}/evaluate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EvaluateRule(string id, CancellationToken cancellationToken = default)
    {
        var triggered = await _alertingService.EvaluateRuleAsync(id, cancellationToken);

        return Ok(new { ruleId = id, triggered });
    }

    /// <summary>
    /// Evaluate all alert rules
    /// </summary>
    [HttpPost("evaluate-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EvaluateAllRules(CancellationToken cancellationToken = default)
    {
        await _alertingService.EvaluateRulesAsync(cancellationToken);

        return Ok(new { message = "All rules evaluated successfully" });
    }

    // Alerts

    /// <summary>
    /// Get an alert by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Alert), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Alert>> GetAlert(string id, CancellationToken cancellationToken = default)
    {
        var alert = await _alertingService.GetAlertAsync(id, cancellationToken);

        if (alert == null)
        {
            return NotFound();
        }

        return Ok(alert);
    }

    /// <summary>
    /// Get all alerts with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Alert>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Alert>>> GetAlerts(
        [FromQuery] AlertStatus? status = null,
        [FromQuery] DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        var alerts = await _alertingService.GetAlertsAsync(status, since, cancellationToken);
        return Ok(alerts);
    }

    /// <summary>
    /// Acknowledge an alert
    /// </summary>
    [HttpPost("{id}/acknowledge")]
    [ProducesResponseType(typeof(Alert), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Alert>> AcknowledgeAlert(
        string id,
        [FromBody] AcknowledgeAlertRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var alert = await _alertingService.AcknowledgeAlertAsync(id, request.UserId, cancellationToken);
            return Ok(alert);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Resolve an alert
    /// </summary>
    [HttpPost("{id}/resolve")]
    [ProducesResponseType(typeof(Alert), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Alert>> ResolveAlert(
        string id,
        [FromBody] ResolveAlertRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var alert = await _alertingService.ResolveAlertAsync(id, request.UserId, request.Notes, cancellationToken);
            return Ok(alert);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Get alert statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(AlertStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<AlertStatistics>> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var statistics = await _alertingService.GetAlertStatisticsAsync(startDate, endDate, cancellationToken);
        return Ok(statistics);
    }
}

public record AcknowledgeAlertRequest(string UserId);
public record ResolveAlertRequest(string UserId, string? Notes = null);
