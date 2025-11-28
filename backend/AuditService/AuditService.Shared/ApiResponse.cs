using System.Text.Json;

namespace AuditService.Shared;

/// <summary>
/// Generic wrapper for API responses
/// </summary>
/// <typeparam name="T">Type of the data payload</typeparam>
public class ApiResponse<T>
{
    /// <summary>Indicates if the operation was successful</summary>
    public bool Success { get; set; }

    /// <summary>Data returned when Success is true</summary>
    public T? Data { get; set; }

    /// <summary>Error message when Success is false</summary>
    public string? Error { get; set; }

    /// <summary>Additional metadata about the response</summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>Timestamp when the response was generated</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Creates a successful response with the provided data</summary>
    public static ApiResponse<T> Ok(T data, Dictionary<string, object>? metadata = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Metadata = metadata,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>Creates a failed response with the provided error message</summary>
    public static ApiResponse<T> Fail(string errorMessage, Dictionary<string, object>? metadata = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = errorMessage,
            Metadata = metadata,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>Creates a failed response with validation errors</summary>
    public static ApiResponse<T> ValidationFail(Dictionary<string, string[]> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = "Validation failed",
            Metadata = new Dictionary<string, object> { ["validationErrors"] = errors },
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Non-generic API response for void operations
/// </summary>
public class ApiResponse
{
    /// <summary>Indicates if the operation was successful</summary>
    public bool Success { get; set; }

    /// <summary>Error message when Success is false</summary>
    public string? Error { get; set; }

    /// <summary>Additional metadata about the response</summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>Timestamp when the response was generated</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Creates a successful response</summary>
    public static ApiResponse Ok(Dictionary<string, object>? metadata = null)
    {
        return new ApiResponse
        {
            Success = true,
            Metadata = metadata,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>Creates a failed response with the provided error message</summary>
    public static ApiResponse Fail(string errorMessage, Dictionary<string, object>? metadata = null)
    {
        return new ApiResponse
        {
            Success = false,
            Error = errorMessage,
            Metadata = metadata,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Paginated result wrapper
/// </summary>
/// <typeparam name="T">Type of the items</typeparam>
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;

    public PaginatedResult() { }

    public PaginatedResult(List<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    public static PaginatedResult<T> Create(List<T> items, int totalCount, int page, int pageSize)
    {
        return new PaginatedResult<T>(items, totalCount, page, pageSize);
    }

    public static PaginatedResult<T> Empty(int page = 1, int pageSize = 50)
    {
        return new PaginatedResult<T>(new List<T>(), 0, page, pageSize);
    }
}

/// <summary>
/// Extension methods for ApiResponse to handle paginated results
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Creates a successful response with paginated data
    /// </summary>
    public static ApiResponse<PaginatedResult<T>> OkPaginated<T>(
        PaginatedResult<T> result,
        Dictionary<string, object>? metadata = null,
        string? correlationId = null)
    {
        var response = new ApiResponse<PaginatedResult<T>>
        {
            Success = true,
            Data = result,
            Metadata = metadata ?? new Dictionary<string, object>(),
            Timestamp = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(correlationId))
        {
            response.Metadata["correlationId"] = correlationId;
        }

        // Add pagination metadata
        response.Metadata["pagination"] = new Dictionary<string, object>
        {
            ["currentPage"] = result.Page,
            ["pageSize"] = result.PageSize,
            ["totalCount"] = result.TotalCount,
            ["totalPages"] = result.TotalPages,
            ["hasPreviousPage"] = result.HasPreviousPage,
            ["hasNextPage"] = result.HasNextPage
        };

        return response;
    }

    /// <summary>
    /// Creates a successful response with paginated data (alternative method)
    /// </summary>
    public static ApiResponse<PaginatedResult<T>> Ok<T>(
        PaginatedResult<T> result,
        Dictionary<string, object>? metadata = null)
    {
        return OkPaginated(result, metadata);
    }
}