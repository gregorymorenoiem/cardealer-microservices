using Microsoft.AspNetCore.Mvc;
using AppointmentService.Application.DTOs;
using AppointmentService.Domain.Entities;
using AppointmentService.Domain.Interfaces;

namespace AppointmentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentRepository appointmentRepository,
        ILogger<AppointmentsController> logger)
    {
        _appointmentRepository = appointmentRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll(CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        return Ok(appointments.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            return NotFound();

        return Ok(MapToDto(appointment));
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return Ok(appointments.Select(MapToDto));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return Ok(appointments.Select(MapToDto));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByStatus(AppointmentStatus status, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetByStatusAsync(status, cancellationToken);
        return Ok(appointments.Select(MapToDto));
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByType(AppointmentType type, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetByTypeAsync(type, cancellationToken);
        return Ok(appointments.Select(MapToDto));
    }

    [HttpGet("assigned/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByAssignedUser(Guid userId, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetByAssignedUserAsync(userId, cancellationToken);
        return Ok(appointments.Select(MapToDto));
    }

    [HttpGet("upcoming/{days:int}")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetUpcoming(int days, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetUpcomingAsync(days, cancellationToken);
        return Ok(appointments.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create(
        [FromBody] CreateAppointmentRequest request,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AppointmentType>(request.Type, true, out var appointmentType))
            return BadRequest("Invalid appointment type");

        if (!TimeOnly.TryParse(request.StartTime, out var startTime))
            return BadRequest("Invalid start time format");

        // Check for conflicts
        var endTime = startTime.AddMinutes(request.DurationMinutes);
        var hasConflict = await _appointmentRepository.HasConflictAsync(
            request.ScheduledDate, startTime, endTime, null, cancellationToken);

        if (hasConflict)
            return Conflict("There is a scheduling conflict with an existing appointment");

        var appointment = new Appointment(
            dealerId,
            request.CustomerId,
            request.CustomerName,
            request.CustomerEmail,
            appointmentType,
            request.ScheduledDate,
            startTime,
            request.DurationMinutes,
            userId,
            request.CustomerPhone);

        if (request.VehicleId.HasValue && !string.IsNullOrEmpty(request.VehicleDescription))
            appointment.SetVehicle(request.VehicleId.Value, request.VehicleDescription);

        if (request.AssignedToUserId.HasValue && !string.IsNullOrEmpty(request.AssignedToUserName))
            appointment.AssignTo(request.AssignedToUserId.Value, request.AssignedToUserName);

        if (!string.IsNullOrEmpty(request.Location))
            appointment.SetLocation(request.Location);

        if (!string.IsNullOrEmpty(request.Notes))
            appointment.AddNotes(request.Notes);

        var created = await _appointmentRepository.AddAsync(appointment, cancellationToken);
        _logger.LogInformation("Appointment {AppointmentId} created for dealer {DealerId}", created.Id, dealerId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AppointmentDto>> Update(Guid id, [FromBody] UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            return NotFound();

        if (request.VehicleId.HasValue && !string.IsNullOrEmpty(request.VehicleDescription))
            appointment.SetVehicle(request.VehicleId.Value, request.VehicleDescription);

        if (request.AssignedToUserId.HasValue && !string.IsNullOrEmpty(request.AssignedToUserName))
            appointment.AssignTo(request.AssignedToUserId.Value, request.AssignedToUserName);

        if (!string.IsNullOrEmpty(request.Location))
            appointment.SetLocation(request.Location);

        if (!string.IsNullOrEmpty(request.Notes))
            appointment.AddNotes(request.Notes);

        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        _logger.LogInformation("Appointment {AppointmentId} updated", id);

        return Ok(MapToDto(appointment));
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<AppointmentDto>> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            return NotFound();

        appointment.Confirm();
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        _logger.LogInformation("Appointment {AppointmentId} confirmed", id);

        return Ok(MapToDto(appointment));
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<AppointmentDto>> Start(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            return NotFound();

        appointment.Start();
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        _logger.LogInformation("Appointment {AppointmentId} started", id);

        return Ok(MapToDto(appointment));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<AppointmentDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            return NotFound();

        appointment.Complete();
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        _logger.LogInformation("Appointment {AppointmentId} completed", id);

        return Ok(MapToDto(appointment));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<AppointmentDto>> Cancel(Guid id, [FromBody] string reason, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            return NotFound();

        appointment.Cancel(reason);
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        _logger.LogInformation("Appointment {AppointmentId} cancelled", id);

        return Ok(MapToDto(appointment));
    }

    [HttpPost("{id:guid}/no-show")]
    public async Task<ActionResult<AppointmentDto>> MarkNoShow(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            return NotFound();

        appointment.MarkNoShow();
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        _logger.LogInformation("Appointment {AppointmentId} marked as no-show", id);

        return Ok(MapToDto(appointment));
    }

    [HttpPost("{id:guid}/reschedule")]
    public async Task<ActionResult<AppointmentDto>> Reschedule(Guid id, [FromBody] RescheduleAppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment == null)
            return NotFound();

        if (!TimeOnly.TryParse(request.StartTime, out var startTime))
            return BadRequest("Invalid start time format");

        // Check for conflicts
        var endTime = startTime.AddMinutes(request.DurationMinutes);
        var hasConflict = await _appointmentRepository.HasConflictAsync(
            request.ScheduledDate, startTime, endTime, id, cancellationToken);

        if (hasConflict)
            return Conflict("There is a scheduling conflict with an existing appointment");

        appointment.Reschedule(request.ScheduledDate, startTime, request.DurationMinutes);
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
        _logger.LogInformation("Appointment {AppointmentId} rescheduled", id);

        return Ok(MapToDto(appointment));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _appointmentRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            return NotFound();

        await _appointmentRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Appointment {AppointmentId} deleted", id);

        return NoContent();
    }

    private static AppointmentDto MapToDto(Appointment appointment) => new(
        appointment.Id,
        appointment.CustomerId,
        appointment.CustomerName,
        appointment.CustomerEmail,
        appointment.CustomerPhone,
        appointment.Type.ToString(),
        appointment.Status.ToString(),
        appointment.ScheduledDate,
        appointment.StartTime.ToString("HH:mm"),
        appointment.EndTime.ToString("HH:mm"),
        appointment.DurationMinutes,
        appointment.VehicleId,
        appointment.VehicleDescription,
        appointment.AssignedToUserId,
        appointment.AssignedToUserName,
        appointment.Notes,
        appointment.Location,
        appointment.ConfirmedAt,
        appointment.CompletedAt,
        appointment.CancelledAt,
        appointment.CancellationReason,
        appointment.CreatedAt
    );
}
