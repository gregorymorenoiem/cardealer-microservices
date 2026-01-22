using System.Diagnostics;
using System.Net;
using System.Text.Json;
using CarDealer.Shared.ErrorHandling.Interfaces;
using CarDealer.Shared.ErrorHandling.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarDealer.Shared.ErrorHandling.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions,
/// returns standardized ProblemDetails responses, and publishes to ErrorService
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly ErrorHandlingOptions _options;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IOptions<ErrorHandlingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, IErrorPublisher errorPublisher)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, errorPublisher);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context, 
        Exception exception,
        IErrorPublisher errorPublisher)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        var spanId = Activity.Current?.SpanId.ToString();
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();

        // Log the exception
        _logger.LogError(exception,
            "Unhandled exception in {Method} {Path}: {Message}",
            context.Request.Method,
            context.Request.Path,
            exception.Message);

        // Publish to ErrorService (non-blocking)
        _ = PublishErrorAsync(context, exception, errorPublisher, traceId, spanId, correlationId);

        // Build the response
        var problemDetails = CreateProblemDetails(exception, traceId, context.Request.Path);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status;

        await context.Response.WriteAsync(problemDetails.ToJson());
    }

    private async Task PublishErrorAsync(
        HttpContext context,
        Exception exception,
        IErrorPublisher errorPublisher,
        string? traceId,
        string? spanId,
        string? correlationId)
    {
        try
        {
            var errorContext = new ErrorContext
            {
                RequestPath = context.Request.Path,
                RequestMethod = context.Request.Method,
                UserId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("userId")?.Value,
                CorrelationId = correlationId,
                TraceId = traceId,
                SpanId = spanId,
                ClientIp = GetClientIp(context),
                UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault()
            };

            await errorPublisher.PublishExceptionAsync(exception, errorContext);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish error to ErrorService");
        }
    }

    private Models.ProblemDetails CreateProblemDetails(Exception exception, string? traceId, string path)
    {
        return exception switch
        {
            ValidationException validationEx => CreateValidationProblemDetails(validationEx, traceId, path),
            UnauthorizedAccessException => Models.ProblemDetails.Unauthorized(
                exception.Message, traceId),
            KeyNotFoundException => Models.ProblemDetails.NotFound(
                exception.Message, traceId, path),
            InvalidOperationException when exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) 
                => Models.ProblemDetails.NotFound(exception.Message, traceId, path),
            OperationCanceledException => new Models.ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Request was cancelled.",
                Status = 499, // Client Closed Request
                Detail = "The request was cancelled by the client.",
                TraceId = traceId,
                ErrorCode = "REQUEST_CANCELLED"
            },
            TimeoutException => new Models.ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.5",
                Title = "Gateway Timeout",
                Status = (int)HttpStatusCode.GatewayTimeout,
                Detail = "The request timed out.",
                TraceId = traceId,
                ErrorCode = "TIMEOUT"
            },
            _ => Models.ProblemDetails.InternalServerError(
                _options.IncludeExceptionDetails ? exception.Message : null,
                traceId,
                _options.IncludeExceptionDetails)
        };
    }

    private static Models.ProblemDetails CreateValidationProblemDetails(
        ValidationException validationEx, 
        string? traceId,
        string path)
    {
        var errors = validationEx.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return Models.ProblemDetails.ValidationError(errors, traceId, path);
    }

    private static string GetClientIp(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
