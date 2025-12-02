using System.Security.Cryptography;
using System.Text;
using IdempotencyService.Core.Interfaces;
using IdempotencyService.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdempotencyService.Api.Middleware;

/// <summary>
/// Middleware that handles idempotency for HTTP requests
/// </summary>
public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;
    private readonly IdempotencyOptions _options;

    public IdempotencyMiddleware(
        RequestDelegate next,
        ILogger<IdempotencyMiddleware> logger,
        IOptions<IdempotencyOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, IIdempotencyService idempotencyService)
    {
        // Check if this path should be excluded
        if (IsExcludedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Check if this method should be checked for idempotency
        if (!_options.IdempotentMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Get idempotency key from header
        var idempotencyKey = context.Request.Headers[_options.HeaderName].FirstOrDefault();

        if (string.IsNullOrEmpty(idempotencyKey))
        {
            if (_options.RequireIdempotencyKey)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Missing idempotency key",
                    message = $"The {_options.HeaderName} header is required for {context.Request.Method} requests"
                });
                return;
            }

            // No idempotency key provided and not required, proceed normally
            await _next(context);
            return;
        }

        // Read and hash the request body
        var requestBody = await ReadRequestBodyAsync(context.Request);
        var requestHash = ComputeHash(requestBody);

        // Check if we have a cached response
        var checkResult = await idempotencyService.CheckAsync(idempotencyKey, requestHash);

        if (checkResult.Exists)
        {
            if (!checkResult.RequestHashMatches)
            {
                // Conflict: same key, different request body
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Idempotency key conflict",
                    message = checkResult.ErrorMessage
                });
                return;
            }

            if (checkResult.IsProcessing)
            {
                // Request is still being processed
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Request in progress",
                    message = "A request with this idempotency key is currently being processed"
                });
                return;
            }

            if (checkResult.IsCompleted && checkResult.Record != null)
            {
                // Return cached response
                _logger.LogInformation("Returning cached response for idempotency key {Key}", idempotencyKey);

                context.Response.StatusCode = checkResult.Record.ResponseStatusCode;
                context.Response.ContentType = checkResult.Record.ResponseContentType;
                context.Response.Headers["X-Idempotency-Replayed"] = "true";

                await context.Response.WriteAsync(checkResult.Record.ResponseBody);
                return;
            }
        }

        // Create a new idempotency record
        var record = new IdempotencyRecord
        {
            Key = idempotencyKey,
            HttpMethod = context.Request.Method,
            Path = context.Request.Path,
            RequestHash = requestHash,
            ClientId = context.Request.Headers["X-Client-Id"].FirstOrDefault()
        };

        await idempotencyService.StartProcessingAsync(record);

        // Capture the original response body
        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);

            // Read the response
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            responseBodyStream.Seek(0, SeekOrigin.Begin);

            // Complete the idempotency record
            await idempotencyService.CompleteAsync(
                idempotencyKey,
                context.Response.StatusCode,
                responseBody,
                context.Response.ContentType ?? "application/json");

            // Copy the response to the original stream
            await responseBodyStream.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request with idempotency key {Key}", idempotencyKey);
            await idempotencyService.FailAsync(idempotencyKey, ex.Message);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private bool IsExcludedPath(PathString path)
    {
        return _options.ExcludedPaths.Any(excluded =>
            path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return body;
    }

    private static string ComputeHash(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

/// <summary>
/// Extension methods for adding idempotency middleware
/// </summary>
public static class IdempotencyMiddlewareExtensions
{
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IdempotencyMiddleware>();
    }
}
