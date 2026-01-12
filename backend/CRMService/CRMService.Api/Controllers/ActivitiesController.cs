using CRMService.Application.DTOs;
using CRMService.Domain.Entities;
using CRMService.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Api.Controllers;

[ApiController]
[Route("api/crm/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityRepository _activityRepository;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(IActivityRepository activityRepository, ILogger<ActivitiesController> logger)
    {
        _activityRepository = activityRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var activities = await _activityRepository.GetAllAsync(cancellationToken);
        return Ok(activities.Select(ActivityDto.FromEntity));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ActivityDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);
        if (activity == null)
            return NotFound();

        return Ok(ActivityDto.FromEntity(activity));
    }

    [HttpGet("lead/{leadId:guid}")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByLeadId(
        Guid leadId,
        CancellationToken cancellationToken = default)
    {
        var activities = await _activityRepository.GetByLeadAsync(leadId, cancellationToken);
        return Ok(activities.Select(ActivityDto.FromEntity));
    }

    [HttpGet("deal/{dealId:guid}")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByDealId(
        Guid dealId,
        CancellationToken cancellationToken = default)
    {
        var activities = await _activityRepository.GetByDealAsync(dealId, cancellationToken);
        return Ok(activities.Select(ActivityDto.FromEntity));
    }

    [HttpGet("upcoming/{days:int}")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetUpcoming(
        int days,
        CancellationToken cancellationToken = default)
    {
        var activities = await _activityRepository.GetUpcomingAsync(days, cancellationToken);
        return Ok(activities.Select(ActivityDto.FromEntity));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetOverdue(CancellationToken cancellationToken = default)
    {
        var activities = await _activityRepository.GetOverdueAsync(cancellationToken);
        return Ok(activities.Select(ActivityDto.FromEntity));
    }

    [HttpGet("assigned/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByAssignedUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var activities = await _activityRepository.GetByAssignedUserAsync(userId, cancellationToken);
        return Ok(activities.Select(ActivityDto.FromEntity));
    }

    [HttpGet("pending-count")]
    public async Task<ActionResult<int>> GetPendingCount(
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var count = await _activityRepository.GetPendingCountAsync(userId, cancellationToken);
        return Ok(count);
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(
        [FromBody] CreateActivityRequest request,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        [FromHeader(Name = "X-User-Id")] Guid createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var activityType = Enum.TryParse<ActivityType>(request.Type, out var type) ? type : ActivityType.Task;

        var activity = new Activity(
            dealerId,
            activityType,
            request.Subject,
            createdByUserId,
            request.LeadId,
            request.DealId,
            request.ContactId
        );

        if (request.DueDate.HasValue)
        {
            activity.SetDueDate(request.DueDate.Value);
        }

        if (request.AssignedToUserId.HasValue)
        {
            activity.AssignTo(request.AssignedToUserId.Value);
        }

        await _activityRepository.AddAsync(activity, cancellationToken);

        _logger.LogInformation("Activity {ActivityId} created for dealer {DealerId}", activity.Id, dealerId);

        return CreatedAtAction(nameof(GetById), new { id = activity.Id }, ActivityDto.FromEntity(activity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ActivityDto>> Update(
        Guid id,
        [FromBody] UpdateActivityRequest request,
        CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);
        if (activity == null)
            return NotFound();

        var priority = Enum.TryParse<ActivityPriority>(request.Priority, out var p) ? p : ActivityPriority.Normal;
        activity.Update(request.Subject, request.Description, priority);

        if (request.DueDate.HasValue)
        {
            activity.SetDueDate(request.DueDate.Value);
        }

        await _activityRepository.UpdateAsync(activity, cancellationToken);

        _logger.LogInformation("Activity {ActivityId} updated", id);

        return Ok(ActivityDto.FromEntity(activity));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ActivityDto>> Complete(
        Guid id,
        [FromBody] CompleteActivityRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);
        if (activity == null)
            return NotFound();

        activity.MarkAsCompleted(request?.Outcome, request?.DurationMinutes);
        await _activityRepository.UpdateAsync(activity, cancellationToken);

        _logger.LogInformation("Activity {ActivityId} completed", id);

        return Ok(ActivityDto.FromEntity(activity));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);
        if (activity == null)
            return NotFound();

        await _activityRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Activity {ActivityId} deleted", id);

        return NoContent();
    }
}
