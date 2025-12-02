using IdempotencyService.Core.Interfaces;
using IdempotencyService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace IdempotencyService.Api.Controllers;

/// <summary>
/// Controller for managing idempotency records
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IdempotencyController : ControllerBase
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<IdempotencyController> _logger;

    public IdempotencyController(
        IIdempotencyService idempotencyService,
        ILogger<IdempotencyController> logger)
    {
        _idempotencyService = idempotencyService;
        _logger = logger;
    }

    /// <summary>
    /// Check if an idempotency key exists
    /// </summary>
    [HttpGet("{key}")]
    [ProducesResponseType(typeof(IdempotencyRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecord(string key, CancellationToken cancellationToken)
    {
        var record = await _idempotencyService.GetAsync(key, cancellationToken);

        if (record == null)
        {
            return NotFound(new { message = $"No record found for key: {key}" });
        }

        return Ok(record);
    }

    /// <summary>
    /// Check an idempotency key without processing
    /// </summary>
    [HttpPost("check")]
    [ProducesResponseType(typeof(IdempotencyCheckResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckKey([FromBody] CheckKeyRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Key))
        {
            return BadRequest(new { message = "Key is required" });
        }

        var result = await _idempotencyService.CheckAsync(request.Key, request.RequestHash, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete an idempotency record
    /// </summary>
    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRecord(string key, CancellationToken cancellationToken)
    {
        var exists = await _idempotencyService.GetAsync(key, cancellationToken);

        if (exists == null)
        {
            return NotFound(new { message = $"No record found for key: {key}" });
        }

        await _idempotencyService.DeleteAsync(key, cancellationToken);

        _logger.LogInformation("Deleted idempotency record: {Key}", key);
        return NoContent();
    }

    /// <summary>
    /// Get idempotency statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(IdempotencyStats), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _idempotencyService.GetStatsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Manually create an idempotency record (for testing)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IdempotencyRecord), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRecord([FromBody] CreateRecordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Key))
        {
            return BadRequest(new { message = "Key is required" });
        }

        // Check if already exists
        var existing = await _idempotencyService.GetAsync(request.Key, cancellationToken);
        if (existing != null)
        {
            return Conflict(new { message = $"Record already exists for key: {request.Key}" });
        }

        var record = new IdempotencyRecord
        {
            Key = request.Key,
            HttpMethod = request.HttpMethod ?? "POST",
            Path = request.Path ?? "/",
            RequestHash = request.RequestHash ?? "",
            ResponseStatusCode = request.ResponseStatusCode ?? 200,
            ResponseBody = request.ResponseBody ?? "{}",
            ResponseContentType = request.ResponseContentType ?? "application/json",
            Status = request.Status ?? IdempotencyStatus.Completed,
            ClientId = request.ClientId
        };

        await _idempotencyService.StartProcessingAsync(record, cancellationToken);

        if (record.Status == IdempotencyStatus.Completed)
        {
            await _idempotencyService.CompleteAsync(
                record.Key,
                record.ResponseStatusCode,
                record.ResponseBody,
                record.ResponseContentType,
                cancellationToken);
        }

        _logger.LogInformation("Created idempotency record: {Key}", request.Key);
        return CreatedAtAction(nameof(GetRecord), new { key = request.Key }, record);
    }

    /// <summary>
    /// Cleanup expired records (manual trigger)
    /// </summary>
    [HttpPost("cleanup")]
    [ProducesResponseType(typeof(CleanupResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cleanup(CancellationToken cancellationToken)
    {
        var count = await _idempotencyService.CleanupExpiredAsync(cancellationToken);

        _logger.LogInformation("Cleanup completed, removed {Count} records", count);
        return Ok(new CleanupResult { RemovedCount = count });
    }
}

public class CheckKeyRequest
{
    public string Key { get; set; } = string.Empty;
    public string? RequestHash { get; set; }
}

public class CreateRecordRequest
{
    public string Key { get; set; } = string.Empty;
    public string? HttpMethod { get; set; }
    public string? Path { get; set; }
    public string? RequestHash { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ResponseContentType { get; set; }
    public IdempotencyStatus? Status { get; set; }
    public string? ClientId { get; set; }
}

public class CleanupResult
{
    public int RemovedCount { get; set; }
    public DateTime CleanedAt { get; set; } = DateTime.UtcNow;
}
