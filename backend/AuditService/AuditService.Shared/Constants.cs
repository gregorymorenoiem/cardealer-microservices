namespace AuditService.Shared;

/// <summary>
/// Application constants
/// </summary>
public static class Constants
{
    /// <summary>
    /// Cache key patterns
    /// </summary>
    public static class CacheKeys
    {
        public const string AuditLogById = "audit_log_{0}";
        public const string UserAuditHistory = "user_audit_history_{0}";
        public const string RecentAudits = "recent_audits";
        public const string AuditStatistics = "audit_statistics";
        public const string TopActions = "top_actions";
        public const string ActiveUsers = "active_users";
        public const string DailyStats = "daily_stats_{0}";
    }

    /// <summary>
    /// Policy names for authorization
    /// </summary>
    public static class Policies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireAuditorRole = "RequireAuditorRole";
        public const string RequireViewAuditLogs = "RequireViewAuditLogs";
        public const string AllowAnonymous = "AllowAnonymous";
    }

    /// <summary>
    /// Role names
    /// </summary>
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string Auditor = "Auditor";
        public const string User = "User";
        public const string System = "System";
    }

    /// <summary>
    /// Claim types
    /// </summary>
    public static class ClaimTypes
    {
        public const string UserId = "sub";
        public const string Email = "email";
        public const string Name = "name";
        public const string Role = "role";
        public const string Permissions = "permissions";
    }

    /// <summary>
    /// HTTP header names
    /// </summary>
    public static class Headers
    {
        public const string ApiKey = "X-API-Key";
        public const string CorrelationId = "X-Correlation-ID";
        public const string UserAgent = "User-Agent";
        public const string ForwardedFor = "X-Forwarded-For";
        public const string RequestId = "X-Request-ID";
    }

    /// <summary>
    /// Audit action types
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

        // User management actions
        public const string CreateUser = "CREATE_USER";
        public const string UpdateUser = "UPDATE_USER";
        public const string DeleteUser = "DELETE_USER";
        public const string EnableUser = "ENABLE_USER";
        public const string DisableUser = "DISABLE_USER";

        // Two-factor authentication actions
        public const string Enable2FA = "ENABLE_2FA";
        public const string Disable2FA = "DISABLE_2FA";
        public const string Verify2FA = "VERIFY_2FA";

        // External authentication actions
        public const string ExternalLogin = "EXTERNAL_LOGIN";
        public const string LinkExternalAccount = "LINK_EXTERNAL_ACCOUNT";
        public const string UnlinkExternalAccount = "UNLINK_EXTERNAL_ACCOUNT";

        // System actions
        public const string SystemStartup = "SYSTEM_STARTUP";
        public const string SystemShutdown = "SYSTEM_SHUTDOWN";
        public const string Cleanup = "CLEANUP";
    }

    /// <summary>
    /// Audit resource types
    /// </summary>
    public static class AuditResources
    {
        public const string User = "USER";
        public const string Auth = "AUTH";
        public const string Token = "TOKEN";
        public const string TwoFactor = "TWO_FACTOR";
        public const string ExternalAuth = "EXTERNAL_AUTH";
        public const string System = "SYSTEM";
    }

    /// <summary>
    /// Error codes
    /// </summary>
    public static class ErrorCodes
    {
        public const string ValidationError = "VALIDATION_ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string Conflict = "CONFLICT";
        public const string InternalError = "INTERNAL_ERROR";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
        public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    }

    /// <summary>
    /// Date and time formats
    /// </summary>
    public static class DateFormats
    {
        public const string Default = "yyyy-MM-ddTHH:mm:ss.fffZ";
        public const string DateOnly = "yyyy-MM-dd";
        public const string HumanReadable = "MMMM dd, yyyy 'at' hh:mm tt";
    }

    /// <summary>
    /// Pagination defaults
    /// </summary>
    public static class Pagination
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 50;
        public const int MaxPageSize = 1000;
    }

    /// <summary>
    /// Configuration section names
    /// </summary>
    public static class ConfigurationSections
    {
        public const string Database = "Database";
        public const string Cache = "Cache";
        public const string HealthChecks = "HealthChecks";
        public const string RabbitMQ = "RabbitMQ";
        public const string AuditService = "AuditService";
        public const string Serilog = "Serilog";
    }
}