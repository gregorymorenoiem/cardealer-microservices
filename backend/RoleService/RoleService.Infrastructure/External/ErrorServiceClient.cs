using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RoleService.Application.Interfaces;

namespace RoleService.Infrastructure.External
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

        public async Task LogErrorAsync(string exceptionType, string message, string stackTrace, string endpoint = null, int? statusCode = null)
        {
            try
            {
                var errorLog = new
                {
                    ServiceName = "RoleService",
                    ExceptionType = exceptionType,
                    Message = message,
                    StackTrace = stackTrace,
                    Endpoint = endpoint,
                    StatusCode = statusCode,
                    OccurredAt = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync("/api/errors", errorLog);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Error log sent to ErrorService: {ExceptionType}", exceptionType);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send error log to ErrorService");
            }
        }
    }
}
