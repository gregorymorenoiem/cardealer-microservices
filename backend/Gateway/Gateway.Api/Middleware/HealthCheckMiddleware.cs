namespace Gateway.Api.Middleware;

/// <summary>
/// Middleware to handle health check requests before Ocelot processes them
/// </summary>
public class HealthCheckMiddleware
{
    private readonly RequestDelegate _next;

    public HealthCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Intercept /health requests before Ocelot
        if (context.Request.Path.Equals("/health", StringComparison.OrdinalIgnoreCase))
        {
            // Apply CORS headers if Origin header is present
            if (context.Request.Headers.ContainsKey("Origin"))
            {
                var origin = context.Request.Headers["Origin"].ToString();

                // Allow localhost:5173 for development
                if (origin == "http://localhost:5173" || origin == "https://inelcasrl.com.do")
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

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Gateway is healthy");
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
