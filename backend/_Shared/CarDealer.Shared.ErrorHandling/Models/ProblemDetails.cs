using System.Net;
using System.Text.Json;

namespace CarDealer.Shared.ErrorHandling.Models;

/// <summary>
/// Standardized API error response following RFC 7807 Problem Details
/// </summary>
public class ProblemDetails
{
    /// <summary>
    /// A URI reference that identifies the problem type
    /// </summary>
    public string Type { get; set; } = "about:blank";
    
    /// <summary>
    /// A short, human-readable summary of the problem type
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int Status { get; set; }
    
    /// <summary>
    /// A human-readable explanation specific to this occurrence
    /// </summary>
    public string? Detail { get; set; }
    
    /// <summary>
    /// A URI reference that identifies the specific occurrence
    /// </summary>
    public string? Instance { get; set; }
    
    /// <summary>
    /// Trace ID for correlation
    /// </summary>
    public string? TraceId { get; set; }
    
    /// <summary>
    /// Error code for programmatic handling
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// Validation errors (for 400 responses)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }
    
    /// <summary>
    /// Additional extension properties
    /// </summary>
    public Dictionary<string, object>? Extensions { get; set; }

    /// <summary>
    /// Creates a ProblemDetails for validation errors
    /// </summary>
    public static ProblemDetails ValidationError(
        Dictionary<string, string[]> errors, 
        string? traceId = null,
        string? instance = null)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.BadRequest,
            Errors = errors,
            TraceId = traceId,
            Instance = instance,
            ErrorCode = "VALIDATION_ERROR"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails for not found errors
    /// </summary>
    public static ProblemDetails NotFound(
        string? detail = null,
        string? traceId = null,
        string? instance = null)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Resource not found.",
            Status = (int)HttpStatusCode.NotFound,
            Detail = detail,
            TraceId = traceId,
            Instance = instance,
            ErrorCode = "NOT_FOUND"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails for unauthorized errors
    /// </summary>
    public static ProblemDetails Unauthorized(
        string? detail = null,
        string? traceId = null)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Title = "Unauthorized.",
            Status = (int)HttpStatusCode.Unauthorized,
            Detail = detail ?? "Authentication is required to access this resource.",
            TraceId = traceId,
            ErrorCode = "UNAUTHORIZED"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails for forbidden errors
    /// </summary>
    public static ProblemDetails Forbidden(
        string? detail = null,
        string? traceId = null)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Title = "Forbidden.",
            Status = (int)HttpStatusCode.Forbidden,
            Detail = detail ?? "You do not have permission to access this resource.",
            TraceId = traceId,
            ErrorCode = "FORBIDDEN"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails for conflict errors
    /// </summary>
    public static ProblemDetails Conflict(
        string? detail = null,
        string? traceId = null)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            Title = "Conflict.",
            Status = (int)HttpStatusCode.Conflict,
            Detail = detail,
            TraceId = traceId,
            ErrorCode = "CONFLICT"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails for internal server errors
    /// </summary>
    public static ProblemDetails InternalServerError(
        string? detail = null,
        string? traceId = null,
        bool includeDetail = false)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "An unexpected error occurred.",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = includeDetail ? detail : "An error occurred while processing your request.",
            TraceId = traceId,
            ErrorCode = "INTERNAL_ERROR"
        };
    }

    /// <summary>
    /// Creates a ProblemDetails for rate limit exceeded
    /// </summary>
    public static ProblemDetails TooManyRequests(
        int retryAfterSeconds = 60,
        string? traceId = null)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc6585#section-4",
            Title = "Too many requests.",
            Status = (int)HttpStatusCode.TooManyRequests,
            Detail = $"Rate limit exceeded. Try again in {retryAfterSeconds} seconds.",
            TraceId = traceId,
            ErrorCode = "RATE_LIMIT_EXCEEDED",
            Extensions = new Dictionary<string, object>
            {
                ["retryAfter"] = retryAfterSeconds
            }
        };
    }

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes to JSON using cached serializer options for performance.
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, s_jsonOptions);
    }
}
