namespace CarDealer.Contracts.DTOs.Common;

/// <summary>
/// Standard error details DTO used across all microservices.
/// </summary>
public class ErrorDetailsDto
{
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? StackTrace { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
