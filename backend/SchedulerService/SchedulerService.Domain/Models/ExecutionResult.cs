namespace SchedulerService.Domain.Models;

/// <summary>
/// Result of a job execution
/// </summary>
public class ExecutionResult
{
    public bool Success { get; set; }
    public string? ResultData { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    public static ExecutionResult SuccessResult(string? data = null)
    {
        return new ExecutionResult
        {
            Success = true,
            ResultData = data
        };
    }

    public static ExecutionResult FailureResult(string errorMessage, string? stackTrace = null)
    {
        return new ExecutionResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            StackTrace = stackTrace
        };
    }
}
