using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminService.Infrastructure.External;
using AdminService.Application.UseCases.Reports.ResolveReport;

namespace AdminService.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IReportsServiceClient _reportsServiceClient;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IMediator mediator,
        IReportsServiceClient reportsServiceClient,
        ILogger<ReportsController> logger)
    {
        _mediator = mediator;
        _reportsServiceClient = reportsServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of content moderation reports with optional filters.
    /// Proxies to ReportsService /api/contentreports.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetReports(
        [FromQuery] string? type = null,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        try
        {
            var filters = new ContentReportSearchFilters
            {
                Type = type,
                Status = status,
                Priority = priority,
                Search = search,
                Page = page,
                PageSize = pageSize
            };

            var result = await _reportsServiceClient.GetReportsAsync(filters, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reports from ReportsService");
            return StatusCode(500, new { Error = "Failed to get reports" });
        }
    }

    /// <summary>
    /// Get content report statistics.
    /// Proxies to ReportsService /api/contentreports/stats.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetReportStats(CancellationToken ct = default)
    {
        try
        {
            var result = await _reportsServiceClient.GetReportStatsAsync(ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report stats from ReportsService");
            return StatusCode(500, new { Error = "Failed to get report stats" });
        }
    }

    /// <summary>
    /// Get a specific report by ID.
    /// Proxies to ReportsService /api/contentreports/{id}.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReportById(Guid id, CancellationToken ct = default)
    {
        try
        {
            var result = await _reportsServiceClient.GetReportByIdAsync(id, ct);
            if (result is null)
                return NotFound(new { Error = $"Report {id} not found" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report {ReportId} from ReportsService", id);
            return StatusCode(500, new { Error = "Failed to get report" });
        }
    }

    /// <summary>
    /// Update report status (investigating, resolved, dismissed).
    /// Proxies to ReportsService PATCH /api/contentreports/{id}/status.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateReportStatus(
        Guid id,
        [FromBody] UpdateReportStatusRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var allowedStatuses = new[] { "investigating", "resolved", "dismissed", "pending" };
            if (string.IsNullOrWhiteSpace(request.Status) || !allowedStatuses.Contains(request.Status.ToLowerInvariant()))
                return BadRequest(new { Error = $"Invalid status. Allowed: {string.Join(", ", allowedStatuses)}" });

            var success = await _reportsServiceClient.UpdateReportStatusAsync(
                id, request.Status.ToLowerInvariant(), request.Resolution, null, ct);

            if (!success)
                return NotFound(new { Error = $"Report {id} not found or update failed" });

            return Ok(new { Success = true, Message = "Report status updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report {ReportId} status", id);
            return StatusCode(500, new { Error = "Failed to update report status" });
        }
    }

    /// <summary>
    /// Resolve a user report — updates status via ReportsService and
    /// sends notification via MediatR.
    /// </summary>
    [HttpPost("{reportId:guid}/resolve")]
    public async Task<IActionResult> ResolveReport(
        Guid reportId,
        [FromBody] ResolveReportRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Resolution))
                return BadRequest(new { Error = "Resolution is required" });

            // Use JWT claim for reviewer identity — never trust client body
            var resolvedBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(resolvedBy))
                return Unauthorized(new { Error = "Unable to identify admin user" });

            // Update status to Resolved via ReportsService
            var success = await _reportsServiceClient.UpdateReportStatusAsync(
                reportId, "resolved", request.Resolution, resolvedBy, ct);

            if (!success)
                return NotFound(new { Error = $"Report {reportId} not found or resolve failed" });

            // Send notification via MediatR
            var command = new ResolveReportCommand(
                reportId,
                resolvedBy,
                request.Resolution,
                request.ReporterEmail,
                request.ReportSubject);

            await _mediator.Send(command, ct);

            return Ok(new { Success = true, Message = "Report resolved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving report {ReportId}", reportId);
            return StatusCode(500, new { Error = "Failed to resolve report" });
        }
    }
}

public record ResolveReportRequest(
    string Resolution,
    string ReporterEmail,
    string ReportSubject);

public record UpdateReportStatusRequest(
    string Status,
    string? Resolution = null);
