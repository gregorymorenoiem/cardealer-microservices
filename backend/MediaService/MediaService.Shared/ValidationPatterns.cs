using System.Text.RegularExpressions;

namespace MediaService.Shared;

/// <summary>
/// Regular expression patterns for validation
/// </summary>
public static class ValidationPatterns
{
    /// <summary>
    /// Pattern for email validation
    /// </summary>
    public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    /// <summary>
    /// Pattern for GUID validation
    /// </summary>
    public const string Guid = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";

    /// <summary>
    /// Pattern for IP address validation (IPv4 and IPv6)
    /// </summary>
    public const string IpAddress = @"^([0-9]{1,3}\.){3}[0-9]{1,3}$|^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$";

    /// <summary>
    /// Pattern for user agent validation (basic pattern)
    /// </summary>
    public const string UserAgent = @"^.{1,500}$";

    /// <summary>
    /// Pattern for media ID validation (alphanumeric and underscores)
    /// </summary>
    public const string MediaId = @"^[a-zA-Z0-9_]{1,50}$";

    /// <summary>
    /// Pattern for file name validation
    /// </summary>
    public const string FileName = @"^[a-zA-Z0-9_\-. ]{1,255}$";

    /// <summary>
    /// Pattern for content type validation
    /// </summary>
    public const string ContentType = @"^[a-zA-Z0-9_\-+/.]{1,100}$";

    /// <summary>
    /// Pattern for context validation (alphanumeric, underscores, and hyphens)
    /// </summary>
    public const string Context = @"^[a-zA-Z0-9_\-]{1,50}$";

    /// <summary>
    /// Pattern for correlation ID validation (alphanumeric and hyphens)
    /// </summary>
    public const string CorrelationId = @"^[a-zA-Z0-9-]{1,100}$";

    /// <summary>
    /// Pattern for service name validation (alphanumeric and hyphens)
    /// </summary>
    public const string ServiceName = @"^[a-zA-Z0-9-]{1,50}$";

    /// <summary>
    /// Pattern for sorting parameter validation (alphanumeric and underscores)
    /// </summary>
    public const string SortBy = @"^[a-zA-Z0-9_]{1,50}$";

    /// <summary>
    /// Pattern for storage key validation
    /// </summary>
    public const string StorageKey = @"^[a-zA-Z0-9_\-/.]{1,1024}$";

    /// <summary>
    /// Validates an email address
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return Regex.IsMatch(email, Email, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Validates a GUID string
    /// </summary>
    public static bool IsValidGuid(string guid)
    {
        if (string.IsNullOrWhiteSpace(guid))
            return false;

        return Regex.IsMatch(guid, Guid, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Validates an IP address
    /// </summary>
    public static bool IsValidIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        return Regex.IsMatch(ipAddress, IpAddress);
    }

    /// <summary>
    /// Validates a user agent string
    /// </summary>
    public static bool IsValidUserAgent(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return false;

        return Regex.IsMatch(userAgent, UserAgent);
    }

    /// <summary>
    /// Validates a media ID
    /// </summary>
    public static bool IsValidMediaId(string mediaId)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
            return false;

        return Regex.IsMatch(mediaId, MediaId);
    }

    /// <summary>
    /// Validates a file name
    /// </summary>
    public static bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        return Regex.IsMatch(fileName, FileName);
    }

    /// <summary>
    /// Validates a content type
    /// </summary>
    public static bool IsValidContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return Regex.IsMatch(contentType, ContentType);
    }

    /// <summary>
    /// Validates a context
    /// </summary>
    public static bool IsValidContext(string context)
    {
        if (string.IsNullOrWhiteSpace(context))
            return false;

        return Regex.IsMatch(context, Context);
    }

    /// <summary>
    /// Validates a correlation ID
    /// </summary>
    public static bool IsValidCorrelationId(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            return false;

        return Regex.IsMatch(correlationId, CorrelationId);
    }

    /// <summary>
    /// Validates a service name
    /// </summary>
    public static bool IsValidServiceName(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return false;

        return Regex.IsMatch(serviceName, ServiceName);
    }

    /// <summary>
    /// Validates a sort by parameter
    /// </summary>
    public static bool IsValidSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return false;

        return Regex.IsMatch(sortBy, SortBy);
    }

    /// <summary>
    /// Validates a storage key
    /// </summary>
    public static bool IsValidStorageKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            return false;

        return Regex.IsMatch(storageKey, StorageKey);
    }

    /// <summary>
    /// Sanitizes a string for safe logging (removes sensitive data)
    /// </summary>
    public static string SanitizeForLogging(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove potential sensitive data patterns
        var patterns = new Dictionary<string, string>
        {
            { @"password[^=]*=([^&]*)", "password=***" },
            { @"token[^=]*=([^&]*)", "token=***" },
            { @"authorization[^:]*:\s*([^,\s]*)", "authorization: ***" },
            { @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", "***@***.***" },
            { @"\b\d{4}[- ]?\d{4}[- ]?\d{4}[- ]?\d{4}\b", "****-****-****-****" } // Credit card
        };

        var sanitized = input;
        foreach (var pattern in patterns)
        {
            sanitized = Regex.Replace(sanitized, pattern.Key, pattern.Value, RegexOptions.IgnoreCase);
        }

        return sanitized;
    }

    /// <summary>
    /// Validates a page number
    /// </summary>
    public static bool IsValidPage(int page)
    {
        return page >= 1;
    }

    /// <summary>
    /// Validates a page size
    /// </summary>
    public static bool IsValidPageSize(int pageSize)
    {
        return pageSize >= 1 && pageSize <= Constants.Pagination.MaxPageSize;
    }

    /// <summary>
    /// Validates a date range
    /// </summary>
    public static bool IsValidDateRange(DateTime fromDate, DateTime toDate)
    {
        return fromDate <= toDate && toDate <= DateTime.UtcNow;
    }

    /// <summary>
    /// Validates file size against maximum allowed
    /// </summary>
    public static bool IsValidFileSize(long fileSize, long maxSize = Constants.FileSizes.HundredMB)
    {
        return fileSize > 0 && fileSize <= maxSize;
    }

    /// <summary>
    /// Validates if content type is allowed
    /// </summary>
    public static bool IsAllowedContentType(string contentType, string[] allowedTypes)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return allowedTypes.Any(pattern =>
            pattern.EndsWith("/*") ? contentType.StartsWith(pattern[..^1]) : contentType == pattern);
    }
}