using EventTrackingService.Application.DTOs;
using EventTrackingService.Application.Features.Events.Commands;
using EventTrackingService.Application.Features.Events.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventTrackingService.Api.Controllers;

/// <summary>
/// Controller para ingesta y consulta de eventos de tracking
/// </summary>
[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IMediator mediator, ILogger<EventsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Ingerir un solo evento (punto de entrada principal para frontend SDK)
    /// </summary>
    /// <param name="eventDto">Datos del evento a trackear</param>
    /// <returns>Respuesta de ingesta</returns>
    [HttpPost("track")]
    [ProducesResponseType(typeof(EventIngestionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventIngestionResponseDto>> TrackEvent([FromBody] CreateTrackedEventDto eventDto)
    {
        if (string.IsNullOrEmpty(eventDto.EventType))
        {
            return BadRequest(new { message = "EventType is required" });
        }

        var command = new IngestEventCommand { Event = eventDto };
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to ingest event: {Message}", result.Message);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Ingerir múltiples eventos en batch (alta performance)
    /// Usado para enviar eventos en lotes desde el frontend
    /// </summary>
    /// <param name="batchDto">Lista de eventos a trackear</param>
    /// <returns>Respuesta de ingesta en batch</returns>
    [HttpPost("track/batch")]
    [ProducesResponseType(typeof(BatchIngestionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchIngestionResponseDto>> TrackBatch([FromBody] BatchEventsDto batchDto)
    {
        if (batchDto.Events == null || !batchDto.Events.Any())
        {
            return BadRequest(new { message = "Events list cannot be empty" });
        }

        var command = new IngestBatchCommand { Events = batchDto.Events };
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Obtener eventos por tipo
    /// </summary>
    [HttpGet("type/{eventType}")]
    [ProducesResponseType(typeof(List<TrackedEventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TrackedEventDto>>> GetEventsByType(
        string eventType,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int limit = 100)
    {
        var query = new GetEventsByTypeQuery
        {
            EventType = eventType,
            StartDate = startDate,
            EndDate = endDate,
            Limit = limit
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtener resumen completo de analytics
    /// </summary>
    [HttpGet("analytics/summary")]
    [ProducesResponseType(typeof(AnalyticsSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetAnalyticsSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int topN = 10)
    {
        var query = new GetAnalyticsSummaryQuery
        {
            StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
            EndDate = endDate ?? DateTime.UtcNow,
            TopN = topN
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtener top búsquedas
    /// </summary>
    [HttpGet("analytics/top-searches")]
    [ProducesResponseType(typeof(List<TopSearchQueryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TopSearchQueryDto>>> GetTopSearches(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int topN = 20)
    {
        var query = new GetTopSearchQueriesQuery
        {
            StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
            EndDate = endDate ?? DateTime.UtcNow,
            TopN = topN
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtener vehículos más vistos
    /// </summary>
    [HttpGet("analytics/most-viewed-vehicles")]
    [ProducesResponseType(typeof(List<MostViewedVehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MostViewedVehicleDto>>> GetMostViewedVehicles(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int topN = 20)
    {
        var query = new GetMostViewedVehiclesQuery
        {
            StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
            EndDate = endDate ?? DateTime.UtcNow,
            TopN = topN
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Endpoint de imagen 1x1 pixel para tracking sin JS (tracking pixel)
    /// GET /api/events/pixel.gif?type=PageView&url=...
    /// </summary>
    [HttpGet("pixel.gif")]
    public async Task<IActionResult> TrackingPixel(
        [FromQuery] string type = "PageView",
        [FromQuery] string? url = null,
        [FromQuery] string? ref = null)
    {
        try
        {
            // Extraer info del request
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var sessionId = HttpContext.Request.Cookies["session_id"] ?? Guid.NewGuid().ToString();

            var eventDto = new CreateTrackedEventDto
            {
                EventType = type,
                SessionId = sessionId,
                IpAddress = ip,
                UserAgent = userAgent,
                CurrentUrl = url ?? ref ?? "unknown",
                Referrer = ref,
                DeviceType = "unknown",
                Browser = "unknown",
                OperatingSystem = "unknown"
            };

            var command = new IngestEventCommand { Event = eventDto };
            await _mediator.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking pixel");
        }

        // Retornar 1x1 transparent GIF
        var pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
        return File(pixel, "image/gif");
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("/health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", service = "EventTrackingService", timestamp = DateTime.UtcNow });
    }
}
