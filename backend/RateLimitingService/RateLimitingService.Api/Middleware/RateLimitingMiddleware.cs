using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;

namespace RateLimitingService.Api.Middleware;

/// <summary>
/// Middleware for rate limiting incoming requests
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IOptions<RateLimitOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitingService rateLimitingService)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;

        // Check if path is excluded
        if (IsExcludedPath(path))
        {
            await _next(context);
            return;
        }

        // Get client identifier
        var clientId = GetClientId(context);
        var tier = GetUserTier(context);
        var endpoint = $"{context.Request.Method}:{path}";

        try
        {
            var result = await rateLimitingService.CheckRateLimitAsync(clientId, endpoint, tier);

            // Add rate limit headers
            if (_options.IncludeHeaders)
            {
                foreach (var header in result.GetHeaders())
                {
                    context.Response.Headers[header.Key] = header.Value;
                }
            }

            if (!result.IsAllowed)
            {
                _logger.LogWarning(
                    "Rate limit exceeded for client {ClientId} on {Endpoint}",
                    clientId, endpoint);

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Too Many Requests",
                    message = _options.RateLimitExceededMessage,
                    retryAfter = result.RetryAfter.TotalSeconds
                };

                await context.Response.WriteAsJsonAsync(response);
                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in rate limiting middleware");
            // Fail open - continue processing on error
            await _next(context);
        }
    }

    private bool IsExcludedPath(string path)
    {
        return _options.ExcludedPaths.Any(excluded =>
            path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase));
    }

    private string GetClientId(HttpContext context)
    {
        // Try to get from custom header
        if (context.Request.Headers.TryGetValue(_options.ClientIdHeader, out var clientIdHeader) &&
            !string.IsNullOrEmpty(clientIdHeader))
        {
            return clientIdHeader!;
        }

        // Try Authorization header (extract token hash)
        if (context.Request.Headers.TryGetValue("Authorization", out var auth) &&
            !string.IsNullOrEmpty(auth))
        {
            var token = auth.ToString().Replace("Bearer ", "");
            return $"auth:{token.GetHashCode():X}";
        }

        // Fall back to IP address
        if (_options.UseIpAsFallback)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Check for forwarded header
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) &&
                !string.IsNullOrEmpty(forwardedFor))
            {
                ip = forwardedFor.ToString().Split(',').First().Trim();
            }

            return $"ip:{ip}";
        }

        return "anonymous";
    }

    private string? GetUserTier(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(_options.UserTierHeader, out var tierHeader) &&
            !string.IsNullOrEmpty(tierHeader))
        {
            return tierHeader!;
        }

        return null;
    }
}

/// <summary>
/// Extension methods for RateLimitingMiddleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimitingMiddleware>();
    }
}
