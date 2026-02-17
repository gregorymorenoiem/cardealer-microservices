using LoggingService.Application.Queries;
using LoggingService.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LoggingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LogEntry>>> GetLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Domain.LogLevel? minLevel,
        [FromQuery] string? serviceName,
        [FromQuery] string? requestId,
        [FromQuery] string? traceId,
        [FromQuery] string? userId,
        [FromQuery] string? searchText,
        [FromQuery] bool? hasException,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var filter = new LogFilter
        {
            StartDate = startDate,
            EndDate = endDate,
            MinLevel = minLevel,
            ServiceName = serviceName,
            RequestId = requestId,
            TraceId = traceId,
            UserId = userId,
            SearchText = searchText,
            HasException = hasException,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        if (!filter.IsValid())
            return BadRequest("Invalid filter parameters");

        var query = new GetLogsQuery(filter);
        var logs = await _mediator.Send(query, cancellationToken);

        return Ok(logs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LogEntry>> GetLogById(
        string id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLogByIdQuery(id);
        var log = await _mediator.Send(query, cancellationToken);

        if (log == null)
            return NotFound($"Log with ID '{id}' not found");

        return Ok(log);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<LogStatistics>> GetStatistics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLogStatisticsQuery(startDate, endDate);
        var statistics = await _mediator.Send(query, cancellationToken);

        return Ok(statistics);
    }
}
