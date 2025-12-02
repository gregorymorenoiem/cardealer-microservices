using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchedulerService.Application.Commands;
using SchedulerService.Application.Queries;
using SchedulerService.Domain.Entities;

namespace SchedulerService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IMediator mediator, ILogger<JobsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all jobs
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Job>>> GetAll()
    {
        var jobs = await _mediator.Send(new GetAllJobsQuery());
        return Ok(jobs);
    }

    /// <summary>
    /// Get active jobs only
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<List<Job>>> GetActive()
    {
        var jobs = await _mediator.Send(new GetActiveJobsQuery());
        return Ok(jobs);
    }

    /// <summary>
    /// Get job by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Job>> GetById(Guid id)
    {
        var job = await _mediator.Send(new GetJobByIdQuery(id));

        if (job == null)
            return NotFound(new { message = $"Job with ID {id} not found" });

        return Ok(job);
    }

    /// <summary>
    /// Create a new job
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Job>> Create([FromBody] CreateJobCommand command)
    {
        try
        {
            var job = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing job
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Job>> Update(Guid id, [FromBody] UpdateJobCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { message = "ID mismatch" });

        try
        {
            var job = await _mediator.Send(command);
            return Ok(job);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job {JobId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a job
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteJobCommand(id));

        if (!result)
            return NotFound(new { message = $"Job with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Enable a job
    /// </summary>
    [HttpPost("{id}/enable")]
    public async Task<ActionResult<Job>> Enable(Guid id)
    {
        try
        {
            var job = await _mediator.Send(new EnableJobCommand(id));
            return Ok(job);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Disable a job
    /// </summary>
    [HttpPost("{id}/disable")]
    public async Task<ActionResult<Job>> Disable(Guid id)
    {
        try
        {
            var job = await _mediator.Send(new DisableJobCommand(id));
            return Ok(job);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Pause a job
    /// </summary>
    [HttpPost("{id}/pause")]
    public async Task<ActionResult<Job>> Pause(Guid id)
    {
        try
        {
            var job = await _mediator.Send(new PauseJobCommand(id));
            return Ok(job);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Trigger immediate execution of a job
    /// </summary>
    [HttpPost("{id}/trigger")]
    public async Task<ActionResult<object>> Trigger(Guid id)
    {
        try
        {
            var executionId = await _mediator.Send(new TriggerJobCommand(id));
            return Ok(new { executionId, message = "Job triggered successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
