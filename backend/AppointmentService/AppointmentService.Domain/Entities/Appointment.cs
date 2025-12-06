using CarDealer.Shared.MultiTenancy;

namespace AppointmentService.Domain.Entities;

public enum AppointmentType
{
    TestDrive,
    ServiceAppointment,
    SalesConsultation,
    VehicleDelivery,
    TradeInEvaluation,
    FinancingConsultation,
    Other
}

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    NoShow,
    Rescheduled
}

public class Appointment : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }

    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string? CustomerPhone { get; private set; }

    public AppointmentType Type { get; private set; }
    public AppointmentStatus Status { get; private set; }

    public DateTime ScheduledDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int DurationMinutes { get; private set; }

    public Guid? VehicleId { get; private set; }
    public string? VehicleDescription { get; private set; }

    public Guid? AssignedToUserId { get; private set; }
    public string? AssignedToUserName { get; private set; }

    public string? Notes { get; private set; }
    public string? Location { get; private set; }

    public DateTime? ReminderSentAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    private Appointment() { }

    public Appointment(
        Guid dealerId,
        Guid customerId,
        string customerName,
        string customerEmail,
        AppointmentType type,
        DateTime scheduledDate,
        TimeOnly startTime,
        int durationMinutes,
        Guid createdBy,
        string? customerPhone = null)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required", nameof(customerName));

        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("Customer email is required", nameof(customerEmail));

        if (durationMinutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(durationMinutes));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        Type = type;
        Status = AppointmentStatus.Scheduled;
        ScheduledDate = scheduledDate.Date;
        StartTime = startTime;
        DurationMinutes = durationMinutes;
        EndTime = startTime.AddMinutes(durationMinutes);
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetVehicle(Guid vehicleId, string vehicleDescription)
    {
        VehicleId = vehicleId;
        VehicleDescription = vehicleDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTo(Guid userId, string userName)
    {
        AssignedToUserId = userId;
        AssignedToUserName = userName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLocation(string location)
    {
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        Status = AppointmentStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        Status = AppointmentStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = AppointmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkNoShow()
    {
        Status = AppointmentStatus.NoShow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reschedule(DateTime newDate, TimeOnly newStartTime, int newDurationMinutes)
    {
        ScheduledDate = newDate.Date;
        StartTime = newStartTime;
        DurationMinutes = newDurationMinutes;
        EndTime = newStartTime.AddMinutes(newDurationMinutes);
        Status = AppointmentStatus.Rescheduled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkReminderSent()
    {
        ReminderSentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
