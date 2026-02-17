using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using KYCService.Domain.Interfaces;

namespace KYCService.Infrastructure.External;

/// <summary>
/// HTTP client for centralized error logging via ErrorService.
/// Uses typed HttpClient with base address configured via DI.
/// </summary>
public class ErrorServiceClient : IErrorServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ErrorServiceClient> _logger;

    public ErrorServiceClient(HttpClient httpClient, ILogger<ErrorServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task LogErrorAsync(string exceptionType, string message, string? stackTrace, string? endpoint = null, int? statusCode = null)
    {
        try
        {
            var errorLog = new
            {
                ServiceName = "KYCService",
                ExceptionType = exceptionType,
                Message = message,
                StackTrace = stackTrace,
                Endpoint = endpoint,
                StatusCode = statusCode,
                OccurredAt = DateTime.UtcNow
            };

            await _httpClient.PostAsJsonAsync("/api/errors", errorLog);
            _logger.LogDebug("Error log sent to ErrorService: {ExceptionType}", exceptionType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send error log to ErrorService");
        }
    }
}
