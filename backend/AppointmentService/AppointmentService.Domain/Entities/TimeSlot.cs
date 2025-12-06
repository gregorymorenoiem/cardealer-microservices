using CarDealer.Shared.MultiTenancy;

namespace AppointmentService.Domain.Entities;

public class TimeSlot : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }

    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int SlotDurationMinutes { get; private set; }
    public int MaxConcurrentAppointments { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private TimeSlot() { }

    public TimeSlot(
        Guid dealerId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int slotDurationMinutes = 60,
        int maxConcurrentAppointments = 1)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time");

        if (slotDurationMinutes <= 0)
            throw new ArgumentException("Slot duration must be positive", nameof(slotDurationMinutes));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        SlotDurationMinutes = slotDurationMinutes;
        MaxConcurrentAppointments = maxConcurrentAppointments;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(TimeOnly startTime, TimeOnly endTime, int slotDurationMinutes, int maxConcurrentAppointments)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time");

        StartTime = startTime;
        EndTime = endTime;
        SlotDurationMinutes = slotDurationMinutes;
        MaxConcurrentAppointments = maxConcurrentAppointments;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
