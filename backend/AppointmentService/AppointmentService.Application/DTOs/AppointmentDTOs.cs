namespace AppointmentService.Application.DTOs;

public record AppointmentDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string Type,
    string Status,
    DateTime ScheduledDate,
    string StartTime,
    string EndTime,
    int DurationMinutes,
    Guid? VehicleId,
    string? VehicleDescription,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    string? Notes,
    string? Location,
    DateTime? ConfirmedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    DateTime CreatedAt
);

public record CreateAppointmentRequest(
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string Type,
    DateTime ScheduledDate,
    string StartTime,
    int DurationMinutes,
    string? CustomerPhone = null,
    Guid? VehicleId = null,
    string? VehicleDescription = null,
    Guid? AssignedToUserId = null,
    string? AssignedToUserName = null,
    string? Notes = null,
    string? Location = null
);

public record UpdateAppointmentRequest(
    Guid? VehicleId = null,
    string? VehicleDescription = null,
    Guid? AssignedToUserId = null,
    string? AssignedToUserName = null,
    string? Notes = null,
    string? Location = null
);

public record RescheduleAppointmentRequest(
    DateTime ScheduledDate,
    string StartTime,
    int DurationMinutes
);
