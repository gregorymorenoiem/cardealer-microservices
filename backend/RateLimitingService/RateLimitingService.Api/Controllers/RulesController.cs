using Microsoft.AspNetCore.Mvc;
using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;

namespace RateLimitingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RulesController : ControllerBase
{
    private readonly IRateLimitRuleService _ruleService;
    private readonly ILogger<RulesController> _logger;

    public RulesController(
        IRateLimitRuleService ruleService,
        ILogger<RulesController> logger)
    {
        _ruleService = ruleService ?? throw new ArgumentNullException(nameof(ruleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all rate limit rules
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RateLimitRule>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RateLimitRule>>> GetAll()
    {
        var rules = await _ruleService.GetAllRulesAsync();
        return Ok(rules);
    }

    /// <summary>
    /// Get rate limit rule by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RateLimitRule), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RateLimitRule>> GetById(string id)
    {
        var rule = await _ruleService.GetRuleByIdAsync(id);

        if (rule == null)
        {
            return NotFound($"Rule with ID '{id}' not found");
        }

        return Ok(rule);
    }

    /// <summary>
    /// Create new rate limit rule
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RateLimitRule), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RateLimitRule>> Create([FromBody] RateLimitRule rule)
    {
        if (rule.Limit <= 0)
        {
            return BadRequest("Limit must be greater than 0");
        }

        if (rule.WindowSize <= TimeSpan.Zero)
        {
            return BadRequest("WindowSize must be greater than 0");
        }

        var created = await _ruleService.CreateRuleAsync(rule);
        _logger.LogInformation("Created rate limit rule {RuleId}", created.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update existing rate limit rule
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RateLimitRule), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RateLimitRule>> Update(string id, [FromBody] RateLimitRule rule)
    {
        if (id != rule.Id)
        {
            return BadRequest("ID mismatch");
        }

        if (rule.Limit <= 0)
        {
            return BadRequest("Limit must be greater than 0");
        }

        if (rule.WindowSize <= TimeSpan.Zero)
        {
            return BadRequest("WindowSize must be greater than 0");
        }

        try
        {
            var updated = await _ruleService.UpdateRuleAsync(rule);
            _logger.LogInformation("Updated rate limit rule {RuleId}", id);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Rule with ID '{id}' not found");
        }
    }

    /// <summary>
    /// Delete rate limit rule
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var rule = await _ruleService.GetRuleByIdAsync(id);

        if (rule == null)
        {
            return NotFound($"Rule with ID '{id}' not found");
        }

        await _ruleService.DeleteRuleAsync(id);
        _logger.LogInformation("Deleted rate limit rule {RuleId}", id);

        return NoContent();
    }

    /// <summary>
    /// Get applicable rules for a request
    /// </summary>
    [HttpPost("applicable")]
    [ProducesResponseType(typeof(IEnumerable<RateLimitRule>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RateLimitRule>>> GetApplicable([FromBody] RateLimitCheckRequest request)
    {
        var rules = await _ruleService.GetApplicableRulesAsync(request);
        return Ok(rules);
    }
}
