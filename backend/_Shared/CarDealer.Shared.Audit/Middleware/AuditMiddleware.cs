using System.Diagnostics;
using CarDealer.Shared.Audit.Configuration;
using CarDealer.Shared.Audit.Interfaces;
using CarDealer.Shared.Audit.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarDealer.Shared.Audit.Middleware;

/// <summary>
/// Middleware para auditoría automática de requests HTTP
/// </summary>
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;
    private readonly AuditOptions _options;

    public AuditMiddleware(
        RequestDelegate next,
        ILogger<AuditMiddleware> logger,
        IOptions<AuditOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, IAuditPublisher auditPublisher)
    {
        if (!_options.Enabled || !_options.AutoAudit.HttpRequests)
        {
            await _next(context);
            return;
        }

        // Verificar si la ruta debe ser excluida
        var path = context.Request.Path.Value ?? string.Empty;
        if (_options.AutoAudit.ExcludePaths.Any(p =>
            path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Verificar si solo queremos auditar mutaciones
        if (_options.AutoAudit.OnlyMutations &&
            context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            await PublishAuditEventAsync(context, stopwatch.ElapsedMilliseconds, exception, auditPublisher);
        }
    }

    private async Task PublishAuditEventAsync(
        HttpContext context,
        long durationMs,
        Exception? exception,
        IAuditPublisher auditPublisher)
    {
        try
        {
            var auditEvent = new AuditEvent
            {
                EventType = DetermineEventType(context),
                Action = DetermineAction(context.Request.Method),
                RequestPath = context.Request.Path,
                HttpMethod = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                UserId = context.User?.FindFirst("sub")?.Value ??
                         context.User?.FindFirst("id")?.Value,
                UserEmail = context.User?.FindFirst("email")?.Value,
                CorrelationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ??
                               context.TraceIdentifier,
                TraceId = Activity.Current?.TraceId.ToString(),
                SpanId = Activity.Current?.SpanId.ToString(),
                Success = exception == null && context.Response.StatusCode < 400,
                ErrorMessage = exception?.Message,
                DurationMs = durationMs,
                Severity = DetermineSeverity(context.Response.StatusCode, exception),
                Metadata = new Dictionary<string, object>
                {
                    ["query"] = context.Request.QueryString.Value ?? string.Empty,
                    ["contentType"] = context.Request.ContentType ?? string.Empty
                }
            };

            // Extraer ResourceId del path si existe
            var segments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments?.Length >= 3 && Guid.TryParse(segments.LastOrDefault(), out _))
            {
                auditEvent.ResourceId = segments.Last();
                auditEvent.ResourceType = segments.Length > 1 ? segments[^2] : null;
            }

            await auditPublisher.PublishAsync(auditEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish audit event for request {Path}", context.Request.Path);
        }
    }

    private static string DetermineEventType(HttpContext context)
    {
        var method = context.Request.Method.ToUpper();
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        // Determinar tipo basado en el path
        if (path.Contains("/auth/login")) return "UserLogin";
        if (path.Contains("/auth/logout")) return "UserLogout";
        if (path.Contains("/auth/register")) return "UserRegistered";
        if (path.Contains("/payments")) return $"Payment{GetActionSuffix(method)}";
        if (path.Contains("/vehicles")) return $"Vehicle{GetActionSuffix(method)}";
        if (path.Contains("/users")) return $"User{GetActionSuffix(method)}";

        return $"Http{method}";
    }

    private static string GetActionSuffix(string method) => method.ToUpper() switch
    {
        "POST" => "Created",
        "PUT" or "PATCH" => "Updated",
        "DELETE" => "Deleted",
        _ => "Accessed"
    };

    private static string DetermineAction(string method) => method.ToUpper() switch
    {
        "GET" => "Read",
        "POST" => "Create",
        "PUT" or "PATCH" => "Update",
        "DELETE" => "Delete",
        _ => method
    };

    private static AuditSeverity DetermineSeverity(int statusCode, Exception? exception)
    {
        if (exception != null) return AuditSeverity.Error;

        return statusCode switch
        {
            >= 500 => AuditSeverity.Error,
            >= 400 => AuditSeverity.Warning,
            _ => AuditSeverity.Info
        };
    }
}
