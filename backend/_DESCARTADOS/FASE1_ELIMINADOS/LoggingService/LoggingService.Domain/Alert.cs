namespace LoggingService.Domain;

/// <summary>
/// Represents a triggered alert instance
/// </summary>
public class Alert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RuleId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public AlertStatus Status { get; set; } = AlertStatus.Open;
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
    public List<string> RelatedLogIds { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
    public List<AlertActionResult> ActionResults { get; set; } = new();

    public bool IsOpen() => Status == AlertStatus.Open;

    public bool IsAcknowledged() => Status == AlertStatus.Acknowledged;

    public bool IsResolved() => Status == AlertStatus.Resolved;

    public TimeSpan GetAge() => DateTime.UtcNow - TriggeredAt;

    public TimeSpan? GetTimeToAcknowledge()
    {
        return AcknowledgedAt.HasValue ? AcknowledgedAt.Value - TriggeredAt : null;
    }

    public TimeSpan? GetTimeToResolve()
    {
        return ResolvedAt.HasValue ? ResolvedAt.Value - TriggeredAt : null;
    }

    public void Acknowledge(string userId)
    {
        Status = AlertStatus.Acknowledged;
        AcknowledgedAt = DateTime.UtcNow;
        AcknowledgedBy = userId;
    }

    public void Resolve(string userId, string? notes = null)
    {
        Status = AlertStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = userId;
        ResolutionNotes = notes;
    }
}

/// <summary>
/// Alert severity levels
/// </summary>
public enum AlertSeverity
{
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

/// <summary>
/// Alert status
/// </summary>
public enum AlertStatus
{
    Open,
    Acknowledged,
    Resolved,
    Suppressed
}

/// <summary>
/// Result of executing an alert action
/// </summary>
public class AlertActionResult
{
    public string ActionId { get; set; } = string.Empty;
    public ActionType ActionType { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
