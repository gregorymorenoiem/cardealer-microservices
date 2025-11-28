using System.ComponentModel;

namespace AuditService.Domain.Enums;

// QUITAMOS el enum AuditSeverity de aquí - se movió a Shared

/// <summary>
/// Common audit action types
/// </summary>
public static class AuditActions
{
    // Authentication actions
    public const string Login = "LOGIN";
    public const string Register = "REGISTER";
    public const string Logout = "LOGOUT";
    public const string RefreshToken = "REFRESH_TOKEN";
    public const string ForgotPassword = "FORGOT_PASSWORD";
    public const string ResetPassword = "RESET_PASSWORD";
    public const string VerifyEmail = "VERIFY_EMAIL";
    public const string ChangePassword = "CHANGE_PASSWORD";

    // User management actions
    public const string CreateUser = "CREATE_USER";
    public const string UpdateUser = "UPDATE_USER";
    public const string DeleteUser = "DELETE_USER";
    public const string EnableUser = "ENABLE_USER";
    public const string DisableUser = "DISABLE_USER";
    public const string LockUser = "LOCK_USER";
    public const string UnlockUser = "UNLOCK_USER";

    // Profile actions
    public const string UpdateProfile = "UPDATE_PROFILE";
    public const string UploadAvatar = "UPLOAD_AVATAR";
    public const string DeleteAvatar = "DELETE_AVATAR";

    // Two-factor authentication actions
    public const string Enable2FA = "ENABLE_2FA";
    public const string Disable2FA = "DISABLE_2FA";
    public const string Verify2FA = "VERIFY_2FA";
    public const string GenerateRecoveryCodes = "GENERATE_RECOVERY_CODES";
    public const string UseRecoveryCode = "USE_RECOVERY_CODE";

    // External authentication actions
    public const string ExternalLogin = "EXTERNAL_LOGIN";
    public const string LinkExternalAccount = "LINK_EXTERNAL_ACCOUNT";
    public const string UnlinkExternalAccount = "UNLINK_EXTERNAL_ACCOUNT";

    // Role and permission actions
    public const string CreateRole = "CREATE_ROLE";
    public const string UpdateRole = "UPDATE_ROLE";
    public const string DeleteRole = "DELETE_ROLE";
    public const string AssignRole = "ASSIGN_ROLE";
    public const string RemoveRole = "REMOVE_ROLE";

    // System actions
    public const string SystemStartup = "SYSTEM_STARTUP";
    public const string SystemShutdown = "SYSTEM_SHUTDOWN";
    public const string Cleanup = "CLEANUP";
    public const string Backup = "BACKUP";
    public const string Restore = "RESTORE";

    // Audit-specific actions
    public const string ViewAuditLogs = "VIEW_AUDIT_LOGS";
    public const string ExportAuditLogs = "EXPORT_AUDIT_LOGS";
    public const string PurgeAuditLogs = "PURGE_AUDIT_LOGS";

    // Security actions
    public const string FailedLoginAttempt = "FAILED_LOGIN_ATTEMPT";
    public const string AccountLocked = "ACCOUNT_LOCKED";
    public const string SuspiciousActivity = "SUSPICIOUS_ACTIVITY";
    public const string PasswordSprayAttempt = "PASSWORD_SPRAY_ATTEMPT";
    public const string BruteForceAttempt = "BRUTE_FORCE_ATTEMPT";

    // API actions
    public const string ApiCall = "API_CALL";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    public const string InvalidToken = "INVALID_TOKEN";
    public const string TokenExpired = "TOKEN_EXPIRED";

    // Notification actions
    public const string SendEmail = "SEND_EMAIL";
    public const string SendSMS = "SEND_SMS";
    public const string SendPush = "SEND_PUSH";

    // Business-specific actions
    public const string CreateDeal = "CREATE_DEAL";
    public const string UpdateDeal = "UPDATE_DEAL";
    public const string DeleteDeal = "DELETE_DEAL";
    public const string ViewDeal = "VIEW_DEAL";
    public const string SearchDeals = "SEARCH_DEALS";
    public const string ExportDeals = "EXPORT_DEALS";

    // Vehicle management actions
    public const string CreateVehicle = "CREATE_VEHICLE";
    public const string UpdateVehicle = "UPDATE_VEHICLE";
    public const string DeleteVehicle = "DELETE_VEHICLE";
    public const string ImportVehicles = "IMPORT_VEHICLES";
    public const string ExportVehicles = "EXPORT_VEHICLES";

    // Customer management actions
    public const string CreateCustomer = "CREATE_CUSTOMER";
    public const string UpdateCustomer = "UPDATE_CUSTOMER";
    public const string DeleteCustomer = "DELETE_CUSTOMER";
    public const string ImportCustomers = "IMPORT_CUSTOMERS";
    public const string ExportCustomers = "EXPORT_CUSTOMERS";

    // File operations
    public const string UploadFile = "UPLOAD_FILE";
    public const string DownloadFile = "DOWNLOAD_FILE";
    public const string DeleteFile = "DELETE_FILE";
    public const string ViewFile = "VIEW_FILE";

    // Report actions
    public const string GenerateReport = "GENERATE_REPORT";
    public const string ViewReport = "VIEW_REPORT";
    public const string ExportReport = "EXPORT_REPORT";
    public const string ScheduleReport = "SCHEDULE_REPORT";

    // Settings actions
    public const string UpdateSettings = "UPDATE_SETTINGS";
    public const string ViewSettings = "VIEW_SETTINGS";
    public const string ResetSettings = "RESET_SETTINGS";

    // Integration actions
    public const string ApiIntegration = "API_INTEGRATION";
    public const string WebhookCall = "WEBHOOK_CALL";
    public const string SyncData = "SYNC_DATA";
    public const string ImportData = "IMPORT_DATA";
    public const string ExportData = "EXPORT_DATA";
}

/// <summary>
/// Common audit resource types
/// </summary>
public static class AuditResources
{
    public const string User = "USER";
    public const string Auth = "AUTH";
    public const string Token = "TOKEN";
    public const string TwoFactor = "TWO_FACTOR";
    public const string ExternalAuth = "EXTERNAL_AUTH";
    public const string Role = "ROLE";
    public const string Permission = "PERMISSION";
    public const string System = "SYSTEM";
    public const string Audit = "AUDIT";
    public const string Security = "SECURITY";
    public const string API = "API";
    public const string Notification = "NOTIFICATION";
    public const string Profile = "PROFILE";

    // Business-specific resources
    public const string Deal = "DEAL";
    public const string Vehicle = "VEHICLE";
    public const string Customer = "CUSTOMER";
    public const string Inventory = "INVENTORY";
    public const string Sales = "SALES";
    public const string Finance = "FINANCE";

    // File resources
    public const string File = "FILE";
    public const string Document = "DOCUMENT";
    public const string Image = "IMAGE";
    public const string Report = "REPORT";

    // Settings resources
    public const string Settings = "SETTINGS";
    public const string Configuration = "CONFIGURATION";

    // Integration resources
    public const string Integration = "INTEGRATION";
    public const string Webhook = "WEBHOOK";
    public const string Data = "DATA";
}

/// <summary>
/// Extension methods for audit enums
/// </summary>
public static class AuditEnumExtensions
{
    /// <summary>
    /// Gets the display name for an audit severity
    /// </summary>
    public static string GetDisplayName(this Shared.Enums.AuditSeverity severity)
    {
        return severity switch
        {
            Shared.Enums.AuditSeverity.Debug => "Debug",
            Shared.Enums.AuditSeverity.Information => "Information",
            Shared.Enums.AuditSeverity.Warning => "Warning",
            Shared.Enums.AuditSeverity.Error => "Error",
            Shared.Enums.AuditSeverity.Critical => "Critical",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets the CSS class for an audit severity (for UI display)
    /// </summary>
    public static string GetCssClass(this Shared.Enums.AuditSeverity severity)
    {
        return severity switch
        {
            Shared.Enums.AuditSeverity.Debug => "text-muted",
            Shared.Enums.AuditSeverity.Information => "text-info",
            Shared.Enums.AuditSeverity.Warning => "text-warning",
            Shared.Enums.AuditSeverity.Error => "text-danger",
            Shared.Enums.AuditSeverity.Critical => "text-danger font-weight-bold",
            _ => "text-muted"
        };
    }

    /// <summary>
    /// Gets the Bootstrap badge class for an audit severity
    /// </summary>
    public static string GetBadgeClass(this Shared.Enums.AuditSeverity severity)
    {
        return severity switch
        {
            Shared.Enums.AuditSeverity.Debug => "bg-secondary",
            Shared.Enums.AuditSeverity.Information => "bg-info",
            Shared.Enums.AuditSeverity.Warning => "bg-warning",
            Shared.Enums.AuditSeverity.Error => "bg-danger",
            Shared.Enums.AuditSeverity.Critical => "bg-dark",
            _ => "bg-secondary"
        };
    }

    /// <summary>
    /// Gets the color code for an audit severity
    /// </summary>
    public static string GetColorCode(this Shared.Enums.AuditSeverity severity)
    {
        return severity switch
        {
            Shared.Enums.AuditSeverity.Debug => "#6c757d",      // Gray
            Shared.Enums.AuditSeverity.Information => "#17a2b8", // Teal
            Shared.Enums.AuditSeverity.Warning => "#ffc107",     // Yellow
            Shared.Enums.AuditSeverity.Error => "#dc3545",       // Red
            Shared.Enums.AuditSeverity.Critical => "#721c24",    // Dark Red
            _ => "#6c757d"
        };
    }

    /// <summary>
    /// Checks if the severity represents an error condition
    /// </summary>
    public static bool IsError(this Shared.Enums.AuditSeverity severity)
    {
        return severity == Shared.Enums.AuditSeverity.Error || severity == Shared.Enums.AuditSeverity.Critical;
    }

    /// <summary>
    /// Checks if the severity represents a warning or higher
    /// </summary>
    public static bool IsWarningOrHigher(this Shared.Enums.AuditSeverity severity)
    {
        return severity >= Shared.Enums.AuditSeverity.Warning;
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
    public static Shared.Enums.AuditSeverity ToAuditSeverity(this string severity)
    {
        if (Enum.TryParse<Shared.Enums.AuditSeverity>(severity, true, out var result))
        {
            return result;
        }

        // Try to parse by number
        if (int.TryParse(severity, out var severityNumber) &&
            Enum.IsDefined(typeof(Shared.Enums.AuditSeverity), severityNumber))
        {
            return (Shared.Enums.AuditSeverity)severityNumber;
        }

        return Shared.Enums.AuditSeverity.Information; // Default fallback
    }

    /// <summary>
    /// Gets all available audit actions
    /// </summary>
    public static List<string> GetAllActions()
    {
        return typeof(AuditActions)
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
        return typeof(AuditResources)
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