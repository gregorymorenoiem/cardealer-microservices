using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchedulerService.Application.Queries;
using SchedulerService.Domain.Entities;

namespace SchedulerService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExecutionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExecutionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get recent executions across all jobs
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<List<JobExecution>>> GetRecent([FromQuery] int pageSize = 100)
    {
        var executions = await _mediator.Send(new GetRecentExecutionsQuery(pageSize));
        return Ok(executions);
    }

    /// <summary>
    /// Get execution by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<JobExecution>> GetById(Guid id)
    {
        var execution = await _mediator.Send(new GetExecutionByIdQuery(id));

        if (execution == null)
            return NotFound(new { message = $"Execution with ID {id} not found" });

        return Ok(execution);
    }

    /// <summary>
    /// Get executions for a specific job
    /// </summary>
    [HttpGet("job/{jobId}")]
    public async Task<ActionResult<List<JobExecution>>> GetByJobId(Guid jobId, [FromQuery] int pageSize = 50)
    {
        var executions = await _mediator.Send(new GetJobExecutionsQuery(jobId, pageSize));
        return Ok(executions);
    }
}
