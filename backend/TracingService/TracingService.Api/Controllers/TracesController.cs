using MediatR;
using Microsoft.AspNetCore.Mvc;
using TracingService.Application.Queries;

namespace TracingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TracesController> _logger;

    public TracesController(IMediator mediator, ILogger<TracesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get a specific trace by ID
    /// </summary>
    [HttpGet("{traceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTraceById(string traceId)
    {
        _logger.LogInformation("Getting trace {TraceId}", traceId);

        var trace = await _mediator.Send(new GetTraceByIdQuery { TraceId = traceId });

        if (trace == null)
        {
            _logger.LogWarning("Trace {TraceId} not found", traceId);
            return NotFound(new { message = $"Trace {traceId} not found" });
        }

        return Ok(trace);
    }

    /// <summary>
    /// Search for traces with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchTraces(
        [FromQuery] string? serviceName = null,
        [FromQuery] string? operationName = null,
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        [FromQuery] int? minDurationMs = null,
        [FromQuery] int? maxDurationMs = null,
        [FromQuery] bool? hasError = null,
        [FromQuery] int limit = 100)
    {
        _logger.LogInformation("Searching traces with filters: service={Service}, operation={Operation}",
            serviceName, operationName);

        var query = new SearchTracesQuery
        {
            ServiceName = serviceName,
            OperationName = operationName,
            StartTime = startTime,
            EndTime = endTime,
            MinDurationMs = minDurationMs,
            MaxDurationMs = maxDurationMs,
            HasError = hasError,
            Limit = Math.Min(limit, 1000) // Cap at 1000
        };

        var traces = await _mediator.Send(query);

        return Ok(new
        {
            traces,
            count = traces.Count,
            filters = new
            {
                serviceName,
                operationName,
                startTime,
                endTime,
                minDurationMs,
                maxDurationMs,
                hasError,
                limit
            }
        });
    }

    /// <summary>
    /// Get all spans for a specific trace
    /// </summary>
    [HttpGet("{traceId}/spans")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSpansByTraceId(string traceId)
    {
        _logger.LogInformation("Getting spans for trace {TraceId}", traceId);

        var spans = await _mediator.Send(new GetSpansByTraceIdQuery { TraceId = traceId });

        return Ok(new { traceId, spans, count = spans.Count });
    }

    /// <summary>
    /// Get trace statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        [FromQuery] string? serviceName = null)
    {
        _logger.LogInformation("Getting trace statistics");

        var query = new GetTraceStatisticsQuery
        {
            StartTime = startTime,
            EndTime = endTime,
            ServiceName = serviceName
        };

        var statistics = await _mediator.Send(query);

        return Ok(statistics);
    }
}
