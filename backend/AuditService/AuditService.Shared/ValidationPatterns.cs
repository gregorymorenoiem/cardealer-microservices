using System.Text.RegularExpressions;

namespace AuditService.Shared;

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
    /// Pattern for action name validation (alphanumeric and underscores)
    /// </summary>
    public const string ActionName = @"^[a-zA-Z0-9_]{1,100}$";

    /// <summary>
    /// Pattern for resource name validation (alphanumeric, underscores, and dots)
    /// </summary>
    public const string ResourceName = @"^[a-zA-Z0-9_.]{1,255}$";

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
    /// Validates an action name
    /// </summary>
    public static bool IsValidActionName(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return false;

        return Regex.IsMatch(actionName, ActionName);
    }

    /// <summary>
    /// Validates a resource name
    /// </summary>
    public static bool IsValidResourceName(string resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
            return false;

        return Regex.IsMatch(resourceName, ResourceName);
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
    // Actualizar el método IsValidSortBy para aceptar null
    public static bool IsValidSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return false;

        return Regex.IsMatch(sortBy, SortBy);
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
}