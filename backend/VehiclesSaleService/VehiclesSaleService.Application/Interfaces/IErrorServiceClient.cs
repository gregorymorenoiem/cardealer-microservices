namespace VehiclesSaleService.Application.Interfaces;

/// <summary>
/// Client for centralized error logging via ErrorService.
/// </summary>
public interface IErrorServiceClient
{
    /// <summary>
    /// Logs an error to the centralized error service.
    /// </summary>
    Task LogErrorAsync(string exceptionType, string message, string? stackTrace, string? endpoint = null, int? statusCode = null);
}
