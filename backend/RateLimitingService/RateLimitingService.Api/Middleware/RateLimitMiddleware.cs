using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;

namespace RateLimitingService.Api.Middleware;

/// <summary>
/// Middleware for transparent rate limiting
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMiddleware> _logger;

    public RateLimitMiddleware(
        RequestDelegate next,
        ILogger<RateLimitMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService)
    {
        // Skip rate limiting for health checks and internal endpoints
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/metrics"))
        {
            await _next(context);
            return;
        }

        try
        {
            var request = BuildRateLimitRequest(context);
            var result = await rateLimitService.CheckAsync(request);

            // Add rate limit headers
            context.Response.Headers.Append("X-RateLimit-Limit", result.Limit.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", result.Remaining.ToString());
            context.Response.Headers.Append("X-RateLimit-Reset", result.ResetAt.ToString());

            if (!result.IsAllowed)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (result.RetryAfterSeconds > 0)
                {
                    context.Response.Headers.Append("Retry-After", result.RetryAfterSeconds.ToString());
                }

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests",
                    message = "Rate limit exceeded. Please try again later.",
                    retryAfter = result.RetryAfterSeconds,
                    limit = result.Limit,
                    remaining = result.Remaining,
                    resetAt = result.ResetAt
                });

                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in rate limit middleware");
            // Fail open - allow request if rate limiting fails
            await _next(context);
        }
    }

    private static RateLimitCheckRequest BuildRateLimitRequest(HttpContext context)
    {
        // Try to extract identifier from different sources
        var identifier = ExtractIdentifier(context);
        var identifierType = DetermineIdentifierType(context);

        return new RateLimitCheckRequest
        {
            Identifier = identifier,
            IdentifierType = identifierType,
            Endpoint = context.Request.Path.Value ?? "/",
            Cost = 1,
            Context = new Dictionary<string, string>
            {
                { "Method", context.Request.Method },
                { "UserAgent", context.Request.Headers.UserAgent.ToString() },
                { "IP", context.Connection.RemoteIpAddress?.ToString() ?? "unknown" }
            }
        };
    }

    private static string ExtractIdentifier(HttpContext context)
    {
        // Priority: API Key > User ID > IP Address

        // Check for API Key
        if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey) && !string.IsNullOrEmpty(apiKey))
        {
            return apiKey.ToString();
        }

        // Check for authenticated user
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst("sub")?.Value ??
                        context.User.FindFirst("userId")?.Value ??
                        context.User.Identity.Name;

            if (!string.IsNullOrEmpty(userId))
            {
                return userId;
            }
        }

        // Fall back to IP address
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static RateLimitIdentifierType DetermineIdentifierType(HttpContext context)
    {
        // Check for API Key
        if (context.Request.Headers.ContainsKey("X-API-Key"))
        {
            return RateLimitIdentifierType.ApiKey;
        }

        // Check for authenticated user
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            return RateLimitIdentifierType.UserId;
        }

        // Default to IP address
        return RateLimitIdentifierType.IpAddress;
    }
}

/// <summary>
/// Extension methods for registering rate limit middleware
/// </summary>
public static class RateLimitMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitMiddleware>();
    }
}
