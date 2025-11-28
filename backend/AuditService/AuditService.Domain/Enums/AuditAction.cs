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