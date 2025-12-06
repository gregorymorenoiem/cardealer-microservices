using Microsoft.AspNetCore.Mvc;
using IntegrationService.Application.DTOs;
using IntegrationService.Domain.Entities;
using IntegrationService.Domain.Interfaces;

namespace IntegrationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncJobsController : ControllerBase
{
    private readonly ISyncJobRepository _syncJobRepository;

    public SyncJobsController(ISyncJobRepository syncJobRepository)
    {
        _syncJobRepository = syncJobRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SyncJobDto>>> GetAll(CancellationToken cancellationToken)
    {
        var jobs = await _syncJobRepository.GetAllAsync(cancellationToken);
        return Ok(jobs.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SyncJobDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var job = await _syncJobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
            return NotFound();
        return Ok(MapToDto(job));
    }

    [HttpGet("integration/{integrationId:guid}")]
    public async Task<ActionResult<IEnumerable<SyncJobDto>>> GetByIntegration(Guid integrationId, CancellationToken cancellationToken)
    {
        var jobs = await _syncJobRepository.GetByIntegrationIdAsync(integrationId, cancellationToken);
        return Ok(jobs.Select(MapToDto));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<SyncJobDto>>> GetByStatus(string status, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<SyncStatus>(status, true, out var syncStatus))
            return BadRequest($"Invalid sync status: {status}");

        var jobs = await _syncJobRepository.GetByStatusAsync(syncStatus, cancellationToken);
        return Ok(jobs.Select(MapToDto));
    }

    [HttpGet("scheduled")]
    public async Task<ActionResult<IEnumerable<SyncJobDto>>> GetScheduled(CancellationToken cancellationToken)
    {
        var jobs = await _syncJobRepository.GetScheduledAsync(cancellationToken);
        return Ok(jobs.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<SyncJobDto>> Create([FromBody] CreateSyncJobRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<SyncDirection>(request.Direction, true, out var direction))
            return BadRequest($"Invalid sync direction: {request.Direction}");

        var dealerId = GetCurrentDealerId();
        var userId = GetCurrentUserId();

        var job = new SyncJob(
            dealerId,
            request.IntegrationId,
            request.Name,
            request.EntityType,
            direction,
            userId
        );

        if (!string.IsNullOrEmpty(request.FilterCriteria))
            job.SetFilter(request.FilterCriteria);

        if (request.ScheduledAt.HasValue)
        {
            try
            {
                job.Schedule(request.ScheduledAt.Value);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        await _syncJobRepository.AddAsync(job, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = job.Id }, MapToDto(job));
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<SyncJobDto>> Start(Guid id, [FromQuery] int totalRecords, CancellationToken cancellationToken)
    {
        var job = await _syncJobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
            return NotFound();

        job.Start(totalRecords);
        await _syncJobRepository.UpdateAsync(job, cancellationToken);
        return Ok(MapToDto(job));
    }

    [HttpPut("{id:guid}/progress")]
    public async Task<ActionResult<SyncJobDto>> UpdateProgress(Guid id, [FromBody] UpdateSyncJobProgressRequest request, CancellationToken cancellationToken)
    {
        var job = await _syncJobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
            return NotFound();

        job.UpdateProgress(request.ProcessedRecords, request.SuccessCount, request.ErrorCount);
        await _syncJobRepository.UpdateAsync(job, cancellationToken);
        return Ok(MapToDto(job));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<SyncJobDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var job = await _syncJobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
            return NotFound();

        job.Complete();
        await _syncJobRepository.UpdateAsync(job, cancellationToken);
        return Ok(MapToDto(job));
    }

    [HttpPost("{id:guid}/fail")]
    public async Task<ActionResult<SyncJobDto>> Fail(Guid id, [FromBody] string errorLog, CancellationToken cancellationToken)
    {
        var job = await _syncJobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
            return NotFound();

        job.Fail(errorLog);
        await _syncJobRepository.UpdateAsync(job, cancellationToken);
        return Ok(MapToDto(job));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<SyncJobDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var job = await _syncJobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
            return NotFound();

        job.Cancel();
        await _syncJobRepository.UpdateAsync(job, cancellationToken);
        return Ok(MapToDto(job));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!await _syncJobRepository.ExistsAsync(id, cancellationToken))
            return NotFound();

        await _syncJobRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentDealerId()
    {
        var dealerIdClaim = User.FindFirst("dealer_id")?.Value;
        return dealerIdClaim != null ? Guid.Parse(dealerIdClaim) : Guid.Empty;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    private static SyncJobDto MapToDto(SyncJob j) => new(
        j.Id,
        j.IntegrationId,
        j.Name,
        j.EntityType,
        j.Direction.ToString(),
        j.Status.ToString(),
        j.FilterCriteria,
        j.ScheduledAt,
        j.StartedAt,
        j.CompletedAt,
        j.TotalRecords,
        j.ProcessedRecords,
        j.SuccessCount,
        j.ErrorCount,
        j.CreatedAt
    );
}
