namespace MediaService.Shared;

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
        public const string MediaById = "media_{0}";
        public const string UserMediaList = "user_media_{0}";
        public const string UploadUrl = "upload_url_{0}";
        public const string RecentMedia = "recent_media";
        public const string MediaStatistics = "media_statistics";
        public const string ProcessingQueue = "processing_queue";
        public const string DailyStats = "daily_stats_{0}";
    }

    /// <summary>
    /// Policy names for authorization
    /// </summary>
    public static class Policies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireUploadPermission = "RequireUploadPermission";
        public const string RequireViewMedia = "RequireViewMedia";
        public const string RequireDeleteMedia = "RequireDeleteMedia";
        public const string AllowAnonymous = "AllowAnonymous";
    }

    /// <summary>
    /// Role names
    /// </summary>
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string ContentManager = "ContentManager";
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
        public const string UploadToken = "X-Upload-Token";
    }

    /// <summary>
    /// Media action types
    /// </summary>
    public static class MediaActions
    {
        // Upload actions
        public const string Upload = "UPLOAD";
        public const string InitUpload = "INIT_UPLOAD";
        public const string FinalizeUpload = "FINALIZE_UPLOAD";
        public const string CancelUpload = "CANCEL_UPLOAD";

        // Management actions
        public const string View = "VIEW";
        public const string Download = "DOWNLOAD";
        public const string Delete = "DELETE";
        public const string Update = "UPDATE";
        public const string List = "LIST";

        // Processing actions
        public const string Process = "PROCESS";
        public const string Reprocess = "REPROCESS";
        public const string GenerateVariants = "GENERATE_VARIANTS";

        // Sharing actions
        public const string Share = "SHARE";
        public const string Unshare = "UNSHARE";
        public const string GenerateShareLink = "GENERATE_SHARE_LINK";
    }

    /// <summary>
    /// Media resource types
    /// </summary>
    public static class MediaResources
    {
        public const string Image = "IMAGE";
        public const string Video = "VIDEO";
        public const string Document = "DOCUMENT";
        public const string Audio = "AUDIO";
        public const string Thumbnail = "THUMBNAIL";
        public const string Variant = "VARIANT";
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
        public const string FileTooLarge = "FILE_TOO_LARGE";
        public const string UnsupportedFormat = "UNSUPPORTED_FORMAT";
        public const string ProcessingFailed = "PROCESSING_FAILED";
        public const string StorageError = "STORAGE_ERROR";
    }

    /// <summary>
    /// Date and time formats
    /// </summary>
    public static class DateFormats
    {
        public const string Default = "yyyy-MM-ddTHH:mm:ss.fffZ";
        public const string DateOnly = "yyyy-MM-dd";
        public const string HumanReadable = "MMMM dd, yyyy 'at' hh:mm tt";
        public const string FileSafe = "yyyyMMdd_HHmmss";
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
    /// File size limits
    /// </summary>
    public static class FileSizes
    {
        public const long OneKB = 1024;
        public const long OneMB = 1024 * OneKB;
        public const long TenMB = 10 * OneMB;
        public const long HundredMB = 100 * OneMB;
        public const long OneGB = 1024 * OneMB;
    }

    /// <summary>
    /// Configuration section names
    /// </summary>
    public static class ConfigurationSections
    {
        public const string Database = "DatabaseSettings";
        public const string Cache = "CacheSettings";
        public const string Storage = "StorageSettings";
        public const string ImageProcessing = "ImageProcessingSettings";
        public const string VideoTranscode = "VideoTranscodeSettings";
        public const string HealthChecks = "HealthChecks";
        public const string RabbitMQ = "RabbitMQ";
        public const string MediaService = "MediaService";
        public const string Serilog = "Serilog";
    }
}