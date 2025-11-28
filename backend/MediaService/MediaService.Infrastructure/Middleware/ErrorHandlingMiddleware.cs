using System.Net;
using System.Text.Json;
using MediaService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MediaService.Infrastructure.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var error = "An unexpected error occurred.";

        // Mapear diferentes excepciones a diferentes códigos de estado
        if (exception is UnauthorizedAccessException)
        {
            code = HttpStatusCode.Unauthorized;
            error = "Unauthorized access.";
        }
        else if (exception is ArgumentException || exception is InvalidOperationException)
        {
            code = HttpStatusCode.BadRequest;
            error = exception.Message;
        }
        else if (exception is KeyNotFoundException)
        {
            code = HttpStatusCode.NotFound;
            error = "Resource not found.";
        }

        var result = JsonSerializer.Serialize(ApiResponse.Fail(error));
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        await context.Response.WriteAsync(result);
    }
}