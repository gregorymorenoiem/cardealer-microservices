using CarDealer.Contracts.Events.Error;
using ErrorService.Domain.Interfaces;
using ErrorService.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Text.Json;

namespace ErrorService.Shared.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IErrorReporter _errorReporter;
        private readonly IEventPublisher? _eventPublisher;
        private readonly string _serviceName;

        public ErrorHandlingMiddleware(
            RequestDelegate next, 
            IErrorReporter errorReporter,
            string serviceName = "UnknownService",
            IEventPublisher? eventPublisher = null)
        {
            _next = next;
            _errorReporter = errorReporter;
            _serviceName = serviceName;
            _eventPublisher = eventPublisher;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
                
                if (context.Response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    await CaptureValidationErrorAsync(context);
                }
            }
            catch (AppException appEx)
            {
                await HandleExceptionAsync(appEx, context, appEx.StatusCode);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex, context, StatusCodes.Status500InternalServerError);
            }
        }

        private async Task HandleExceptionAsync(Exception exception, HttpContext context, int statusCode)
        {
            Log.Error(exception, "Unhandled exception in {ServiceName}", _serviceName);

            await StoreErrorInDatabase(exception, context, statusCode);

            // Publish critical error event to RabbitMQ if status >= 500
            if (statusCode >= 500 && _eventPublisher != null)
            {
                await PublishCriticalErrorEventAsync(exception, context, statusCode);
            }

            context.Response.StatusCode = statusCode;
            
            var response = new 
            { 
                success = false, 
                error = exception is AppException ? exception.Message : "Internal server error.",
                traceId = context.TraceIdentifier
            };
            
            await context.Response.WriteAsJsonAsync(response);
        }

        private async Task CaptureValidationErrorAsync(HttpContext context)
        {
            try
            {
                if (context.Items.TryGetValue("ResponseBody", out var responseBodyObj) && 
                    responseBodyObj is string responseBody)
                {
                    var validationDetails = TryParseValidationResponse(responseBody);

                    var errorRequest = new ErrorReport
                    {
                        ServiceName = _serviceName,
                        ExceptionType = "ValidationError",
                        Message = "Request validation failed",
                        StackTrace = null,
                        OccurredAt = DateTime.UtcNow,
                        Endpoint = context.Request.Path,
                        HttpMethod = context.Request.Method,
                        StatusCode = StatusCodes.Status400BadRequest,
                        UserId = context.User?.FindFirst("sub")?.Value,
                        Metadata = new Dictionary<string, object>
                        {
                            ["RequestId"] = context.TraceIdentifier,
                            ["RequestQuery"] = context.Request.QueryString.ToString(),
                            ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
                            ["ClientIp"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                            ["ValidationDetails"] = validationDetails,
                            ["ErrorType"] = "FluentValidation",
                            ["ServiceName"] = _serviceName
                        }
                    };

                    await _errorReporter.ReportErrorAsync(errorRequest);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to capture validation error");
            }
        }

        private string TryParseValidationResponse(string responseBody)
        {
            try
            {
                using var document = JsonDocument.Parse(responseBody);
                if (document.RootElement.TryGetProperty("errors", out var errors))
                {
                    return errors.ToString();
                }
                if (document.RootElement.TryGetProperty("error", out var error))
                {
                    return error.GetString() ?? responseBody;
                }
            }
            catch
            {
                // Si no se puede parsear, devolver el cuerpo original
            }
            return responseBody;
        }

        private async Task StoreErrorInDatabase(Exception exception, HttpContext context, int statusCode)
        {
            try
            {
                var errorRequest = new ErrorReport
                {
                    ServiceName = _serviceName,
                    ExceptionType = exception.GetType().Name,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    OccurredAt = DateTime.UtcNow,
                    Endpoint = context.Request.Path,
                    HttpMethod = context.Request.Method,
                    StatusCode = statusCode,
                    UserId = context.User?.FindFirst("sub")?.Value,
                    Metadata = new Dictionary<string, object>
                    {
                        ["RequestId"] = context.TraceIdentifier,
                        ["RequestQuery"] = context.Request.QueryString.ToString(),
                        ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
                        ["ClientIp"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        ["ServiceName"] = _serviceName
                    }
                };

                await _errorReporter.ReportErrorAsync(errorRequest);
            }
            catch (Exception logEx)
            {
                Log.Error(logEx, "Failed to store error in ErrorService database");
            }
        }

        private async Task PublishCriticalErrorEventAsync(Exception exception, HttpContext context, int statusCode)
        {
            try
            {
                var criticalEvent = new ErrorCriticalEvent
                {
                    ErrorId = Guid.NewGuid(),
                    ServiceName = _serviceName,
                    ExceptionType = exception.GetType().Name,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    StatusCode = statusCode,
                    Endpoint = context.Request.Path,
                    UserId = context.User?.FindFirst("sub")?.Value,
                    Metadata = new Dictionary<string, object>
                    {
                        ["RequestId"] = context.TraceIdentifier,
                        ["HttpMethod"] = context.Request.Method,
                        ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
                        ["ClientIp"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        ["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
                    }
                };

                await _eventPublisher!.PublishAsync(criticalEvent);
                
                Log.Information(
                    "Published ErrorCriticalEvent {EventId} for {ExceptionType} in {ServiceName}",
                    criticalEvent.EventId, exception.GetType().Name, _serviceName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to publish ErrorCriticalEvent");
            }
        }
    }
}