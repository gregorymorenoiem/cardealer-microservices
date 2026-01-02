using AuthService.Shared.ErrorMessages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AuthService.Shared.Exceptions;
using AuthService.Infrastructure.Services.Messaging;

namespace AuthService.Infrastructure.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IErrorEventProducer _errorEventProducer;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IErrorEventProducer errorEventProducer)
    {
        _next = next;
        _logger = logger;
        _errorEventProducer = errorEventProducer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorCode = GetErrorCode(exception);
        var statusCode = GetStatusCode(exception);

        // Extraer informaci�n del contexto
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var endpoint = context.Request.Path;
        var httpMethod = context.Request.Method;

        // Crear evento de error
        var errorEvent = new RabbitMQErrorEvent
        {
            ErrorCode = errorCode,
            ErrorMessage = exception.Message,
            StackTrace = exception.StackTrace,
            UserId = userId,
            Endpoint = endpoint.ToString(),
            HttpMethod = httpMethod,
            StatusCode = statusCode,
            Metadata = new Dictionary<string, object>
            {
                ["RequestId"] = context.TraceIdentifier,
                ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
                ["ClientIp"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                ["ExceptionType"] = exception.GetType().Name
            }
        };

        // Publicar error as�ncronamente (no esperar)
        _ = _errorEventProducer.PublishErrorAsync(errorEvent);

        // Log local
        _logger.LogError(exception, "Error occurred: {ErrorCode} at {Endpoint}", errorCode, endpoint);

        // Responder al cliente
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        // In development, show detailed error; in production, hide internal details
        var environment = context.RequestServices.GetService<IWebHostEnvironment>();
        var isDevelopment = environment?.EnvironmentName == "Development";

        var response = new
        {
            success = false,
            error = (exception is AppException || isDevelopment) ? exception.Message : "An error occurred",
            errorCode = errorCode,
            traceId = context.TraceIdentifier,
            details = isDevelopment ? exception.ToString() : null
        };

        await context.Response.WriteAsJsonAsync(response);
    }

    private static string GetErrorCode(Exception exception)
    {
        return exception switch
        {
            AppException appEx => appEx.GetType().Name.Replace("Exception", ""),
            _ => "INTERNAL_ERROR"
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            NotFoundException => StatusCodes.Status404NotFound,
            BadRequestException => StatusCodes.Status400BadRequest,
            ConflictException => StatusCodes.Status409Conflict,
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
