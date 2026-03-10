using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gateway.Api.Middleware;

/// <summary>
/// Middleware to handle health check requests before Ocelot processes them.
/// Uses ASP.NET Core HealthCheckService to run actual health probes instead
/// of returning hardcoded "healthy" strings. CORS headers are applied inline
/// because Ocelot would otherwise intercept these paths.
/// </summary>
public class HealthCheckMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> AllowedOrigins = new(StringComparer.OrdinalIgnoreCase)
    {
        "http://localhost:5173",
        "http://localhost:3000",
        "http://localhost:8080",
        "https://okla.com.do",
        "https://www.okla.com.do",
        "https://inelcasrl.com.do"
    };

    public HealthCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Intercept health check requests before Ocelot
        var path = context.Request.Path.Value ?? "";
        if (path.Equals("/health", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/health/ready", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/health/live", StringComparison.OrdinalIgnoreCase))
        {
            // Apply CORS headers if Origin header is present
            if (context.Request.Headers.ContainsKey("Origin"))
            {
                var origin = context.Request.Headers["Origin"].ToString();

                if (AllowedOrigins.Contains(origin))
                {
                    context.Response.Headers["Access-Control-Allow-Origin"] = origin;
                    context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
                    context.Response.Headers["Access-Control-Allow-Headers"] = "*";
                    context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                }

                // Handle preflight OPTIONS request
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.StatusCode = 204; // NoContent
                    return;
                }
            }

            // Use ASP.NET Core HealthCheckService for real probes
            var healthCheckService = context.RequestServices?.GetService<HealthCheckService>();

            if (healthCheckService != null)
            {
                var lowerPath = path.ToLowerInvariant();
                HealthReport report;

                if (lowerPath == "/health")
                {
                    // /health — exclude external checks (⚠️ CRITICAL: per copilot-instructions.md)
                    report = await healthCheckService.CheckHealthAsync(
                        registration => !registration.Tags.Contains("external"),
                        context.RequestAborted);
                }
                else if (lowerPath == "/health/ready")
                {
                    // /health/ready — only checks tagged "ready"
                    report = await healthCheckService.CheckHealthAsync(
                        registration => registration.Tags.Contains("ready"),
                        context.RequestAborted);
                }
                else
                {
                    // /health/live — no checks, just confirm process is running
                    report = new HealthReport(
                        new Dictionary<string, HealthReportEntry>(),
                        TimeSpan.Zero);
                }

                context.Response.StatusCode = report.Status == HealthStatus.Healthy ? 200 : 503;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    status = report.Status.ToString(),
                    service = "Gateway",
                    timestamp = DateTime.UtcNow,
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        duration = e.Value.Duration.TotalMilliseconds
                        // description intentionally omitted — prevent info leakage (CWE-200)
                    })
                };

                await context.Response.WriteAsJsonAsync(response);
            }
            else
            {
                // Fallback if HealthCheckService is not registered
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";

                var fallbackResponse = new
                {
                    status = "Healthy",
                    service = "Gateway",
                    timestamp = DateTime.UtcNow,
                    note = "HealthCheckService not registered — returning basic status"
                };

                await context.Response.WriteAsJsonAsync(fallbackResponse);
            }

            return;
        }

        // Continue to next middleware
        await _next(context);
    }
}

/// <summary>
/// Extension methods for HealthCheckMiddleware
/// </summary>
public static class HealthCheckMiddlewareExtensions
{
    public static IApplicationBuilder UseHealthCheckMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HealthCheckMiddleware>();
    }
}
