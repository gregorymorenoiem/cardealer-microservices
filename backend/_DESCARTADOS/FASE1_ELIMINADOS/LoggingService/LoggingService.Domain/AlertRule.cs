namespace LoggingService.Domain;

/// <summary>
/// Represents an alert rule configuration
/// </summary>
public class AlertRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public RuleCondition Condition { get; set; } = new();
    public List<AlertAction> Actions { get; set; } = new();
    public TimeSpan EvaluationWindow { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan CooldownPeriod { get; set; } = TimeSpan.FromMinutes(15);
    public DateTime? LastTriggered { get; set; }
    public int TriggerCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool CanTrigger()
    {
        if (!IsEnabled) return false;
        if (LastTriggered == null) return true;
        return DateTime.UtcNow - LastTriggered.Value >= CooldownPeriod;
    }

    public void MarkTriggered()
    {
        LastTriggered = DateTime.UtcNow;
        TriggerCount++;
    }
}

/// <summary>
/// Rule condition for triggering alerts
/// </summary>
public class RuleCondition
{
    public ConditionType Type { get; set; }
    public string? ServiceName { get; set; }
    public LogLevel? MinLevel { get; set; }
    public int? ErrorCountThreshold { get; set; }
    public double? ErrorRateThreshold { get; set; }
    public string? MessagePattern { get; set; }
    public string? ExceptionPattern { get; set; }
    public Dictionary<string, object> CustomConditions { get; set; } = new();
}

/// <summary>
/// Types of alert conditions
/// </summary>
public enum ConditionType
{
    ErrorCount,           // Error count exceeds threshold
    ErrorRate,            // Error rate exceeds percentage
    SpecificError,        // Specific error pattern matches
    ServiceDown,          // Service appears to be down
    PerformanceDegradation, // Performance below threshold
    AnomalyDetected,      // Anomaly detected by ML
    PatternMatch,         // Custom pattern matches
    Threshold            // Generic threshold condition
}

/// <summary>
/// Action to take when alert is triggered
/// </summary>
public class AlertAction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ActionType Type { get; set; }
    public Dictionary<string, string> Configuration { get; set; } = new();
    public int Priority { get; set; } = 1; // 1-5, 5 is highest
}

/// <summary>
/// Types of alert actions
/// </summary>
public enum ActionType
{
    Email,              // Send email notification
    Webhook,            // Call webhook URL
    Slack,              // Send Slack message
    Teams,              // Send Teams message
    PagerDuty,          // Create PagerDuty incident
    SMS,                // Send SMS
    CreateTicket,       // Create ticket in issue tracker
    RunScript,          // Execute script
    ScaleService        // Trigger auto-scaling
}
