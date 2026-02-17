// ComplianceReportingService - Compliance Controller
// Dashboard y métricas de cumplimiento regulatorio

namespace ComplianceReportingService.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ComplianceReportingService.Application.DTOs;
using ComplianceReportingService.Application.Features.Commands;
using ComplianceReportingService.Application.Features.Queries;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplianceController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComplianceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene el dashboard de cumplimiento
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<ComplianceDashboardDto>> GetDashboard()
    {
        var result = await _mediator.Send(new GetComplianceDashboardQuery(DateTime.UtcNow));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene estadísticas de reportería
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<ReportingStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;
        var result = await _mediator.Send(new GetReportingStatisticsQuery(from, to));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene métricas actuales de cumplimiento
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult<List<ComplianceMetricDto>>> GetMetrics(
        [FromQuery] string? category = null)
    {
        var result = await _mediator.Send(new GetCurrentMetricsQuery(category));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene alertas activas
    /// </summary>
    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<List<ComplianceMetricDto>>> GetAlerts()
    {
        var result = await _mediator.Send(new GetActiveAlertsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Registra una nueva métrica de cumplimiento
    /// </summary>
    [HttpPost("metrics")]
    [Authorize(Roles = "Admin,Compliance,System")]
    public async Task<ActionResult<ComplianceMetricDto>> RecordMetric([FromBody] RecordMetricRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new RecordMetricCommand(
            request.MetricCode, request.MetricName, request.Category,
            request.Value, request.Threshold, request.Unit, userId));
        return Created($"/api/compliance/metrics/{result.Id}", result);
    }

    /// <summary>
    /// Obtiene historial de una métrica
    /// </summary>
    [HttpGet("metrics/{metricCode}/history")]
    public async Task<ActionResult<List<ComplianceMetricDto>>> GetMetricHistory(
        string metricCode,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-3);
        var to = toDate ?? DateTime.UtcNow;
        var result = await _mediator.Send(new GetMetricHistoryQuery(metricCode, from, to));
        return Ok(result);
    }

    /// <summary>
    /// Genera alertas de cumplimiento
    /// </summary>
    [HttpPost("generate-alerts")]
    [Authorize(Roles = "Admin,Compliance,System")]
    public async Task<ActionResult<List<ComplianceMetricDto>>> GenerateAlerts()
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new GenerateComplianceAlertsCommand(DateTime.UtcNow, userId));
        return Ok(result);
    }
}

public record RecordMetricRequest(
    string MetricCode,
    string MetricName,
    string Category,
    decimal Value,
    decimal? Threshold,
    string? Unit);
