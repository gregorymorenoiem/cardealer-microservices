// NotificationService.Infrastructure\Services\ErrorServiceClient.cs
using System.Net.Http.Json; // ✅ Agregar este using
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public class ErrorServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ErrorServiceClient> _logger;

    public ErrorServiceClient(HttpClient httpClient, ILogger<ErrorServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task ReportErrorAsync(string serviceName, string errorType, string message,
        string? stackTrace = null, Dictionary<string, object>? metadata = null)
    {
        try
        {
            var errorRequest = new
            {
                ServiceName = serviceName,
                ExceptionType = errorType,
                Message = message,
                StackTrace = stackTrace,
                OccurredAt = DateTime.UtcNow,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            // ✅ Ahora PostAsJsonAsync estará disponible
            var response = await _httpClient.PostAsJsonAsync("/api/errors", errorRequest);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to report error to ErrorService: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with ErrorService");
        }
    }
}