using AuditService.Application.DTOs;
using AuditService.Application.Features.Audit.Commands.CreateAudit;
using AuditService.Application.Features.Audit.Queries.GetAuditLogById;
using AuditService.Application.Features.Audit.Queries.GetAuditLogs;
using AuditService.Application.Features.Audit.Queries.GetAuditStats;
using AuditService.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuditService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IMediator mediator, ILogger<AuditController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated audit logs with filtering options
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<AuditLogDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<AuditLogDto>>), 400)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? resource = null,
        [FromQuery] string? serviceName = null,
        [FromQuery] string? severity = null,
        [FromQuery] bool? success = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        [FromQuery] string? searchText = null)
    {
        try
        {
            var query = new GetAuditLogsQuery
            {
                UserId = userId,
                Action = action,
                Resource = resource,
                ServiceName = serviceName,
                Severity = severity,
                Success = success,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending,
                SearchText = searchText
            };

            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, ApiResponse<PaginatedResult<AuditLogDto>>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Get a specific audit log by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), 404)]
    public async Task<IActionResult> GetAuditLogById(string id)
    {
        try
        {
            var query = new GetAuditLogByIdQuery(id);
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log with ID: {AuditLogId}", id);
            return StatusCode(500, ApiResponse<AuditLogDto>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Create a new audit log entry
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<string>), 201)]
    [ProducesResponseType(typeof(ApiResponse<string>), 400)]
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(
                nameof(GetAuditLogById),
                new { id = result.Data },
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log");
            return StatusCode(500, ApiResponse<string>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Get audit statistics and analytics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<AuditStatsDto>), 200)]
    public async Task<IActionResult> GetAuditStats(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? serviceName = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? action = null)
    {
        try
        {
            var query = new GetAuditStatsQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                ServiceName = serviceName,
                UserId = userId,
                Action = action
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit statistics");
            return StatusCode(500, ApiResponse<AuditStatsDto>.Fail("Internal server error"));
        }
    }

    /// <summary>
    /// Get audit logs for a specific user
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<AuditLogDto>>), 200)]
    public async Task<IActionResult> GetUserAuditLogs(
        string userId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = new GetAuditLogsQuery
            {
                UserId = userId,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user: {UserId}", userId);
            return StatusCode(500, ApiResponse<PaginatedResult<AuditLogDto>>.Fail("Internal server error"));
        }
    }
}