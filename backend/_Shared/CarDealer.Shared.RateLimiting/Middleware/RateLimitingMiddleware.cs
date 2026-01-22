using System.Text.Json;
using CarDealer.Shared.RateLimiting.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.RateLimiting.Middleware;

/// <summary>
/// Middleware for rate limiting incoming HTTP requests
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitClient rateLimitClient)
    {
        var result = await rateLimitClient.CheckAsync(context, context.RequestAborted);

        // Add rate limit headers to response
        foreach (var header in result.GetHeaders())
        {
            context.Response.Headers[header.Key] = header.Value;
        }

        if (!result.IsAllowed)
        {
            _logger.LogWarning(
                "Rate limit exceeded for {ClientId} on {Path}. Policy: {Policy}",
                result.ClientIdentifier,
                context.Request.Path,
                result.PolicyName);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                type = "https://httpstatuses.io/429",
                title = "Too Many Requests",
                status = 429,
                detail = $"Rate limit exceeded. Please retry after {result.RetryAfter.TotalSeconds} seconds.",
                instance = context.Request.Path.Value,
                retryAfterSeconds = (int)result.RetryAfter.TotalSeconds,
                limit = result.Limit,
                resetAt = result.ResetAt
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));

            return;
        }

        await _next(context);
    }
}
