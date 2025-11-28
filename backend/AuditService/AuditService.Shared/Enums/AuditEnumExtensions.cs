using AuditService.Shared.Enums;

namespace AuditService.Shared.Enums;

public static class AuditEnumExtensions
{
    /// <summary>
    /// Gets the display name for an audit severity
    /// </summary>
    public static string GetDisplayName(this AuditSeverity severity)
    {
        return severity switch
        {
            AuditSeverity.Debug => "Debug",
            AuditSeverity.Information => "Information",
            AuditSeverity.Warning => "Warning",
            AuditSeverity.Error => "Error",
            AuditSeverity.Critical => "Critical",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets the CSS class for an audit severity (for UI display)
    /// </summary>
    public static string GetCssClass(this AuditSeverity severity)
    {
        return severity switch
        {
            AuditSeverity.Debug => "text-muted",
            AuditSeverity.Information => "text-info",
            AuditSeverity.Warning => "text-warning",
            AuditSeverity.Error => "text-danger",
            AuditSeverity.Critical => "text-danger font-weight-bold",
            _ => "text-muted"
        };
    }

    /// <summary>
    /// Gets the Bootstrap badge class for an audit severity
    /// </summary>
    public static string GetBadgeClass(this AuditSeverity severity)
    {
        return severity switch
        {
            AuditSeverity.Debug => "bg-secondary",
            AuditSeverity.Information => "bg-info",
            AuditSeverity.Warning => "bg-warning",
            AuditSeverity.Error => "bg-danger",
            AuditSeverity.Critical => "bg-dark",
            _ => "bg-secondary"
        };
    }

    /// <summary>
    /// Gets the color code for an audit severity
    /// </summary>
    public static string GetColorCode(this AuditSeverity severity)
    {
        return severity switch
        {
            AuditSeverity.Debug => "#6c757d",      // Gray
            AuditSeverity.Information => "#17a2b8", // Teal
            AuditSeverity.Warning => "#ffc107",     // Yellow
            AuditSeverity.Error => "#dc3545",       // Red
            AuditSeverity.Critical => "#721c24",    // Dark Red
            _ => "#6c757d"
        };
    }

    /// <summary>
    /// Checks if the severity represents an error condition
    /// </summary>
    public static bool IsError(this AuditSeverity severity)
    {
        return severity == AuditSeverity.Error || severity == AuditSeverity.Critical;
    }

    /// <summary>
    /// Checks if the severity represents a warning or higher
    /// </summary>
    public static bool IsWarningOrHigher(this AuditSeverity severity)
    {
        return severity >= AuditSeverity.Warning;
    }

    /// <summary>
    /// Gets the action category for grouping purposes
    /// </summary>
    public static string GetActionCategory(string action)
    {
        return action switch
        {
            var a when a.StartsWith("LOGIN") || a.StartsWith("LOGOUT") || a.StartsWith("REFRESH") || a.Contains("AUTH") => "Authentication",
            var a when a.Contains("PASSWORD") || a.Contains("2FA") || a.Contains("RECOVERY") => "Security",
            var a when a.StartsWith("CREATE_") || a.StartsWith("UPDATE_") || a.StartsWith("DELETE_") => "CRUD Operations",
            var a when a.StartsWith("ENABLE_") || a.StartsWith("DISABLE_") || a.StartsWith("LOCK") || a.StartsWith("UNLOCK") => "User Management",
            var a when a.Contains("ROLE") || a.Contains("PERMISSION") => "Authorization",
            var a when a.StartsWith("SYSTEM_") || a.Contains("BACKUP") || a.Contains("RESTORE") => "System",
            var a when a.Contains("AUDIT") => "Audit",
            var a when a.Contains("API") => "API",
            var a when a.Contains("NOTIFICATION") || a.Contains("SEND_") => "Notifications",
            var a when a.Contains("DEAL") || a.Contains("VEHICLE") || a.Contains("CUSTOMER") => "Business Operations",
            var a when a.Contains("FILE") || a.Contains("DOCUMENT") || a.Contains("IMAGE") => "File Operations",
            var a when a.Contains("REPORT") => "Reporting",
            var a when a.Contains("SETTING") || a.Contains("CONFIG") => "Settings",
            var a when a.Contains("INTEGRATION") || a.Contains("WEBHOOK") || a.Contains("SYNC") => "Integration",
            _ => "Other"
        };
    }

    /// <summary>
    /// Gets the icon for an audit action (for UI display)
    /// </summary>
    public static string GetActionIcon(string action)
    {
        return action switch
        {
            var a when a.Contains("LOGIN") => "fas fa-sign-in-alt",
            var a when a.Contains("LOGOUT") => "fas fa-sign-out-alt",
            var a when a.Contains("CREATE") => "fas fa-plus-circle",
            var a when a.Contains("UPDATE") => "fas fa-edit",
            var a when a.Contains("DELETE") => "fas fa-trash-alt",
            var a when a.Contains("PASSWORD") => "fas fa-key",
            var a when a.Contains("EMAIL") => "fas fa-envelope",
            var a when a.Contains("LOCK") => "fas fa-lock",
            var a when a.Contains("UNLOCK") => "fas fa-unlock",
            var a when a.Contains("2FA") => "fas fa-mobile-alt",
            var a when a.Contains("ROLE") => "fas fa-user-tag",
            var a when a.Contains("SYSTEM") => "fas fa-cog",
            var a when a.Contains("AUDIT") => "fas fa-clipboard-list",
            var a when a.Contains("NOTIFICATION") => "fas fa-bell",
            var a when a.Contains("DEAL") => "fas fa-handshake",
            var a when a.Contains("VEHICLE") => "fas fa-car",
            var a when a.Contains("CUSTOMER") => "fas fa-users",
            var a when a.Contains("FILE") || a.Contains("DOCUMENT") => "fas fa-file",
            var a when a.Contains("REPORT") => "fas fa-chart-bar",
            var a when a.Contains("SETTING") => "fas fa-cogs",
            var a when a.Contains("INTEGRATION") => "fas fa-plug",
            _ => "fas fa-history"
        };
    }

    /// <summary>
    /// Gets the resource icon for an audit resource (for UI display)
    /// </summary>
    public static string GetResourceIcon(string resource)
    {
        return resource switch
        {
            var r when r.Contains("USER") => "fas fa-user",
            var r when r.Contains("AUTH") => "fas fa-shield-alt",
            var r when r.Contains("TOKEN") => "fas fa-key",
            var r when r.Contains("ROLE") => "fas fa-user-tag",
            var r when r.Contains("SYSTEM") => "fas fa-cog",
            var r when r.Contains("AUDIT") => "fas fa-clipboard-list",
            var r when r.Contains("SECURITY") => "fas fa-shield-alt",
            var r when r.Contains("API") => "fas fa-code",
            var r when r.Contains("NOTIFICATION") => "fas fa-bell",
            var r when r.Contains("PROFILE") => "fas fa-id-card",
            var r when r.Contains("DEAL") => "fas fa-handshake",
            var r when r.Contains("VEHICLE") => "fas fa-car",
            var r when r.Contains("CUSTOMER") => "fas fa-users",
            var r when r.Contains("FILE") => "fas fa-file",
            var r when r.Contains("REPORT") => "fas fa-chart-bar",
            var r when r.Contains("SETTING") => "fas fa-cogs",
            var r when r.Contains("INTEGRATION") => "fas fa-plug",
            _ => "fas fa-cube"
        };
    }

    /// <summary>
    /// Converts string to AuditSeverity enum
    /// </summary>
    public static AuditSeverity ToAuditSeverity(this string severity)
    {
        if (Enum.TryParse<AuditSeverity>(severity, true, out var result))
        {
            return result;
        }

        // Try to parse by number
        if (int.TryParse(severity, out var severityNumber) &&
            Enum.IsDefined(typeof(AuditSeverity), severityNumber))
        {
            return (AuditSeverity)severityNumber;
        }

        return AuditSeverity.Information; // Default fallback
    }

    /// <summary>
    /// Gets all available audit actions
    /// </summary>
    public static List<string> GetAllActions()
    {
        return typeof(Domain.Enums.AuditActions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .Select(f => f.GetValue(null)?.ToString() ?? string.Empty)
            .Where(value => !string.IsNullOrEmpty(value))
            .ToList();
    }

    /// <summary>
    /// Gets all available audit resources
    /// </summary>
    public static List<string> GetAllResources()
    {
        return typeof(Domain.Enums.AuditResources)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .Select(f => f.GetValue(null)?.ToString() ?? string.Empty)
            .Where(value => !string.IsNullOrEmpty(value))
            .ToList();
    }

    /// <summary>
    /// Validates if an action is a known audit action
    /// </summary>
    public static bool IsKnownAction(string action)
    {
        return GetAllActions().Contains(action);
    }

    /// <summary>
    /// Validates if a resource is a known audit resource
    /// </summary>
    public static bool IsKnownResource(string resource)
    {
        return GetAllResources().Contains(resource);
    }
}