using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RoleService.Domain.Interfaces;

namespace RoleService.Infrastructure.Services;

/// <summary>
/// Implementación de IErrorReporter que envía errores al ErrorService via HTTP
/// </summary>
public class HttpErrorReporter : IErrorReporter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpErrorReporter> _logger;

    public HttpErrorReporter(HttpClient httpClient, ILogger<HttpErrorReporter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Guid> ReportErrorAsync(ErrorReport request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/errors", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ErrorReportResponse>();
                return result?.Id ?? Guid.NewGuid();
            }

            _logger.LogWarning("Failed to report error to ErrorService. Status: {StatusCode}", response.StatusCode);
            return Guid.NewGuid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send error report to ErrorService");
            return Guid.NewGuid();
        }
    }

    private class ErrorReportResponse
    {
        public Guid Id { get; set; }
    }
}

/// <summary>
/// Implementación noop de IErrorReporter para cuando ErrorService no está disponible
/// </summary>
public class NoOpErrorReporter : IErrorReporter
{
    private readonly ILogger<NoOpErrorReporter> _logger;

    public NoOpErrorReporter(ILogger<NoOpErrorReporter> logger)
    {
        _logger = logger;
    }

    public Task<Guid> ReportErrorAsync(ErrorReport request)
    {
        _logger.LogWarning("Error reported locally (ErrorService not configured): {Message}", request.Message);
        return Task.FromResult(Guid.NewGuid());
    }
}
