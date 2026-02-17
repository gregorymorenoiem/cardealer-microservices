using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System.Net;
using System.Text.Json;

namespace CarDealer.Shared.Resilience.Middleware;

/// <summary>
/// Middleware de degradación graceful que intercepta errores de circuit breaker
/// y devuelve respuestas degradadas en lugar de errores 500.
/// Permite que el sistema siga funcionando parcialmente cuando un downstream falla.
/// </summary>
public class GracefulDegradationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GracefulDegradationMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GracefulDegradationMiddleware(
        RequestDelegate next,
        ILogger<GracefulDegradationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning(
                "[GracefulDegradation] Circuit breaker open for {Path}. Returning degraded response. Exception: {Message}",
                context.Request.Path, ex.Message);

            await WriteDegradedResponse(context, "Service temporarily unavailable", HttpStatusCode.ServiceUnavailable);
        }
        catch (Polly.Timeout.TimeoutRejectedException ex)
        {
            _logger.LogWarning(
                "[GracefulDegradation] Timeout for {Path}. Returning degraded response. Exception: {Message}",
                context.Request.Path, ex.Message);

            await WriteDegradedResponse(context, "Request timed out", HttpStatusCode.GatewayTimeout);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.BadGateway or HttpStatusCode.GatewayTimeout)
        {
            _logger.LogWarning(
                "[GracefulDegradation] Downstream error {StatusCode} for {Path}. Returning degraded response.",
                ex.StatusCode, context.Request.Path);

            await WriteDegradedResponse(context, "Downstream service unavailable", ex.StatusCode ?? HttpStatusCode.ServiceUnavailable);
        }
    }

    private static async Task WriteDegradedResponse(HttpContext context, string message, HttpStatusCode statusCode)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        context.Response.Headers.Append("X-Degraded-Response", "true");
        context.Response.Headers.Append("Retry-After", "30");

        var response = new
        {
            success = false,
            message,
            degraded = true,
            retryAfterSeconds = 30,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
