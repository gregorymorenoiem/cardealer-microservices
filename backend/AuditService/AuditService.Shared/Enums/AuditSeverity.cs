namespace AuditService.Shared.Enums;

/// <summary>
/// Severity levels for audit events
/// </summary>
public enum AuditSeverity
{
    /// <summary>
    /// Debug-level information for development and troubleshooting
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Normal system operations and user actions
    /// </summary>
    Information = 2,

    /// <summary>
    /// Warning events that might indicate potential issues
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Error events that indicate failures but don't break the system
    /// </summary>
    Error = 4,

    /// <summary>
    /// Critical events that require immediate attention
    /// </summary>
    Critical = 5
}