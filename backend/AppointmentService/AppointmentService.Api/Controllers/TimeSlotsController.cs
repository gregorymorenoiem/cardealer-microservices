using Microsoft.AspNetCore.Mvc;
using AppointmentService.Application.DTOs;
using AppointmentService.Domain.Entities;
using AppointmentService.Domain.Interfaces;

namespace AppointmentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimeSlotsController : ControllerBase
{
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly ILogger<TimeSlotsController> _logger;

    public TimeSlotsController(
        ITimeSlotRepository timeSlotRepository,
        ILogger<TimeSlotsController> logger)
    {
        _timeSlotRepository = timeSlotRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimeSlotDto>>> GetAll(CancellationToken cancellationToken)
    {
        var timeSlots = await _timeSlotRepository.GetAllAsync(cancellationToken);
        return Ok(timeSlots.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TimeSlotDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var timeSlot = await _timeSlotRepository.GetByIdAsync(id, cancellationToken);
        if (timeSlot == null)
            return NotFound();

        return Ok(MapToDto(timeSlot));
    }

    [HttpGet("day/{dayOfWeek}")]
    public async Task<ActionResult<IEnumerable<TimeSlotDto>>> GetByDayOfWeek(DayOfWeek dayOfWeek, CancellationToken cancellationToken)
    {
        var timeSlots = await _timeSlotRepository.GetByDayOfWeekAsync(dayOfWeek, cancellationToken);
        return Ok(timeSlots.Select(MapToDto));
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<TimeSlotDto>>> GetActive(CancellationToken cancellationToken)
    {
        var timeSlots = await _timeSlotRepository.GetActiveAsync(cancellationToken);
        return Ok(timeSlots.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<TimeSlotDto>> Create(
        [FromBody] CreateTimeSlotRequest request,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<DayOfWeek>(request.DayOfWeek, true, out var dayOfWeek))
            return BadRequest("Invalid day of week");

        if (!TimeOnly.TryParse(request.StartTime, out var startTime))
            return BadRequest("Invalid start time format");

        if (!TimeOnly.TryParse(request.EndTime, out var endTime))
            return BadRequest("Invalid end time format");

        var timeSlot = new TimeSlot(
            dealerId,
            dayOfWeek,
            startTime,
            endTime,
            request.SlotDurationMinutes,
            request.MaxConcurrentAppointments);

        var created = await _timeSlotRepository.AddAsync(timeSlot, cancellationToken);
        _logger.LogInformation("TimeSlot {TimeSlotId} created for dealer {DealerId}", created.Id, dealerId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TimeSlotDto>> Update(Guid id, [FromBody] UpdateTimeSlotRequest request, CancellationToken cancellationToken)
    {
        var timeSlot = await _timeSlotRepository.GetByIdAsync(id, cancellationToken);
        if (timeSlot == null)
            return NotFound();

        if (!TimeOnly.TryParse(request.StartTime, out var startTime))
            return BadRequest("Invalid start time format");

        if (!TimeOnly.TryParse(request.EndTime, out var endTime))
            return BadRequest("Invalid end time format");

        timeSlot.Update(startTime, endTime, request.SlotDurationMinutes, request.MaxConcurrentAppointments);
        await _timeSlotRepository.UpdateAsync(timeSlot, cancellationToken);
        _logger.LogInformation("TimeSlot {TimeSlotId} updated", id);

        return Ok(MapToDto(timeSlot));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<TimeSlotDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var timeSlot = await _timeSlotRepository.GetByIdAsync(id, cancellationToken);
        if (timeSlot == null)
            return NotFound();

        timeSlot.Activate();
        await _timeSlotRepository.UpdateAsync(timeSlot, cancellationToken);
        _logger.LogInformation("TimeSlot {TimeSlotId} activated", id);

        return Ok(MapToDto(timeSlot));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<TimeSlotDto>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var timeSlot = await _timeSlotRepository.GetByIdAsync(id, cancellationToken);
        if (timeSlot == null)
            return NotFound();

        timeSlot.Deactivate();
        await _timeSlotRepository.UpdateAsync(timeSlot, cancellationToken);
        _logger.LogInformation("TimeSlot {TimeSlotId} deactivated", id);

        return Ok(MapToDto(timeSlot));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _timeSlotRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            return NotFound();

        await _timeSlotRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("TimeSlot {TimeSlotId} deleted", id);

        return NoContent();
    }

    private static TimeSlotDto MapToDto(TimeSlot timeSlot) => new(
        timeSlot.Id,
        timeSlot.DayOfWeek.ToString(),
        timeSlot.StartTime.ToString("HH:mm"),
        timeSlot.EndTime.ToString("HH:mm"),
        timeSlot.SlotDurationMinutes,
        timeSlot.MaxConcurrentAppointments,
        timeSlot.IsActive,
        timeSlot.CreatedAt
    );
}
