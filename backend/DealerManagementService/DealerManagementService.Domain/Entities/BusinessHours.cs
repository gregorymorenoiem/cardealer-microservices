namespace DealerManagementService.Domain.Entities;

public class BusinessHours
{
    public Guid Id { get; set; }
    public Guid DealerLocationId { get; set; }
    public DealerLocation DealerLocation { get; set; } = null!;
    
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsOpen { get; set; } = true;
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public TimeOnly? BreakStartTime { get; set; }
    public TimeOnly? BreakEndTime { get; set; }
    
    // Special notes
    public string? Notes { get; set; } // e.g., "Cerrado por almuerzo 12:00-13:00"
    
    // Helper Methods
    public bool IsOpenAt(TimeOnly time)
    {
        if (!IsOpen || OpenTime == null || CloseTime == null)
            return false;
        
        // Check if within break time
        if (BreakStartTime.HasValue && BreakEndTime.HasValue)
        {
            if (time >= BreakStartTime.Value && time <= BreakEndTime.Value)
                return false;
        }
        
        return time >= OpenTime.Value && time <= CloseTime.Value;
    }
    
    public string GetFormattedHours()
    {
        if (!IsOpen)
            return "Cerrado";
        
        if (OpenTime == null || CloseTime == null)
            return "N/A";
        
        var hours = $"{OpenTime.Value:HH\\:mm} - {CloseTime.Value:HH\\:mm}";
        
        if (BreakStartTime.HasValue && BreakEndTime.HasValue)
        {
            hours += $" (Almuerzo: {BreakStartTime.Value:HH\\:mm} - {BreakEndTime.Value:HH\\:mm})";
        }
        
        return hours;
    }
}
