namespace AppointmentService.Application.DTOs;

public record TimeSlotDto(
    Guid Id,
    string DayOfWeek,
    string StartTime,
    string EndTime,
    int SlotDurationMinutes,
    int MaxConcurrentAppointments,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateTimeSlotRequest(
    string DayOfWeek,
    string StartTime,
    string EndTime,
    int SlotDurationMinutes = 60,
    int MaxConcurrentAppointments = 1
);

public record UpdateTimeSlotRequest(
    string StartTime,
    string EndTime,
    int SlotDurationMinutes,
    int MaxConcurrentAppointments
);

public record AvailableSlotDto(
    DateTime Date,
    string StartTime,
    string EndTime,
    int RemainingCapacity
);
