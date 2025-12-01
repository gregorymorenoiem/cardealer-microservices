using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AdminService.Application.Interfaces;

namespace AdminService.Infrastructure.External
{
    public class ErrorServiceClient : IErrorServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ErrorServiceClient> _logger;

        public ErrorServiceClient(HttpClient httpClient, ILogger<ErrorServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task LogErrorAsync(string exceptionType, string message, string? stackTrace = null, string? endpoint = null, int? statusCode = null)
        {
            try
            {
                var payload = new
                {
                    ServiceName = "AdminService",
                    ExceptionType = exceptionType,
                    Message = message,
                    StackTrace = stackTrace,
                    Endpoint = endpoint,
                    StatusCode = statusCode,
                    OccurredAt = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync("/api/errors", payload);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Error logged to ErrorService: {ExceptionType}", exceptionType);
            }
            catch (Exception ex)
            {
                // Log locally only - avoid infinite loop
                _logger.LogError(ex, "Failed to send error to ErrorService: {ExceptionType}", exceptionType);
            }
        }
    }
}
