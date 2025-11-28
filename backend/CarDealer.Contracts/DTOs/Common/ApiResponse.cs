namespace CarDealer.Contracts.DTOs.Common;

/// <summary>
/// Standard API response wrapper used across all microservices.
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public ErrorDetailsDto? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }
    
    public static ApiResponse<T> ErrorResponse(string message, ErrorDetailsDto? error = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = error
        };
    }
}
