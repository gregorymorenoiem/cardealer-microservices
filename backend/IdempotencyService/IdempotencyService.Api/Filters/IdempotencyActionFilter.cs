using System.Security.Cryptography;
using System.Text;
using IdempotencyService.Core.Attributes;
using IdempotencyService.Core.Interfaces;
using IdempotencyService.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace IdempotencyService.Api.Filters;

/// <summary>
/// Action filter that implements automatic idempotency checking for attributed actions
/// </summary>
public class IdempotencyActionFilter : IAsyncActionFilter
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<IdempotencyActionFilter> _logger;
    private readonly IdempotencyOptions _options;

    public IdempotencyActionFilter(
        IIdempotencyService idempotencyService,
        ILogger<IdempotencyActionFilter> logger,
        IOptions<IdempotencyOptions> options)
    {
        _idempotencyService = idempotencyService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Check if action has SkipIdempotency attribute
        var skipAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<SkipIdempotencyAttribute>()
            .FirstOrDefault();

        if (skipAttribute != null)
        {
            await next();
            return;
        }

        // Check for IdempotentAttribute on action or controller
        var idempotentAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<IdempotentAttribute>()
            .FirstOrDefault();

        if (idempotentAttribute == null)
        {
            // No idempotency required
            await next();
            return;
        }

        // Get idempotency key
        var headerName = idempotentAttribute.HeaderName ?? _options.HeaderName;
        var idempotencyKey = context.HttpContext.Request.Headers[headerName].FirstOrDefault();

        if (string.IsNullOrEmpty(idempotencyKey))
        {
            if (idempotentAttribute.RequireKey)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    error = "Missing idempotency key",
                    message = $"The {headerName} header is required for this operation"
                });
                return;
            }

            // No key and not required, proceed normally
            await next();
            return;
        }

        // Add key prefix if specified
        if (!string.IsNullOrEmpty(idempotentAttribute.KeyPrefix))
        {
            idempotencyKey = $"{idempotentAttribute.KeyPrefix}:{idempotencyKey}";
        }

        // Compute request hash
        var requestHash = await ComputeRequestHashAsync(context.HttpContext.Request, idempotentAttribute);

        // Check for existing record
        var checkResult = await _idempotencyService.CheckAsync(idempotencyKey, requestHash);

        if (checkResult.Exists)
        {
            if (!checkResult.RequestHashMatches)
            {
                context.Result = new ConflictObjectResult(new
                {
                    error = "Idempotency key conflict",
                    message = checkResult.ErrorMessage
                });
                return;
            }

            if (checkResult.IsProcessing)
            {
                context.Result = new ConflictObjectResult(new
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

                context.HttpContext.Response.Headers["X-Idempotency-Replayed"] = "true";
                context.HttpContext.Response.StatusCode = checkResult.Record.ResponseStatusCode;
                context.HttpContext.Response.ContentType = checkResult.Record.ResponseContentType;

                // Parse and return the cached result
                var cachedResult = System.Text.Json.JsonSerializer.Deserialize<object>(checkResult.Record.ResponseBody);
                context.Result = new ObjectResult(cachedResult)
                {
                    StatusCode = checkResult.Record.ResponseStatusCode
                };
                return;
            }
        }

        // Start processing
        var record = new IdempotencyRecord
        {
            Key = idempotencyKey,
            HttpMethod = context.HttpContext.Request.Method,
            Path = context.HttpContext.Request.Path,
            RequestHash = requestHash,
            ClientId = context.HttpContext.Request.Headers["X-Client-Id"].FirstOrDefault()
        };

        await _idempotencyService.StartProcessingAsync(record);

        // Execute the action
        ActionExecutedContext? executedContext = null;
        try
        {
            executedContext = await next();

            if (executedContext.Exception == null && executedContext.Result != null)
            {
                // Get the response
                var responseBody = System.Text.Json.JsonSerializer.Serialize(
                    GetResultValue(executedContext.Result));

                var statusCode = GetStatusCode(executedContext.Result);

                // Complete the idempotency record
                var ttlSeconds = idempotentAttribute.TtlSeconds > 0
                    ? idempotentAttribute.TtlSeconds
                    : _options.DefaultTtlSeconds;

                await _idempotencyService.CompleteAsync(
                    idempotencyKey,
                    statusCode,
                    responseBody,
                    "application/json");
            }
            else if (executedContext.Exception != null)
            {
                await _idempotencyService.FailAsync(idempotencyKey, executedContext.Exception.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in idempotent action with key {Key}", idempotencyKey);
            await _idempotencyService.FailAsync(idempotencyKey, ex.Message);
            throw;
        }
    }

    private async Task<string> ComputeRequestHashAsync(HttpRequest request, IdempotentAttribute attribute)
    {
        var hashBuilder = new StringBuilder();

        // Include request body
        if (attribute.IncludeBodyInHash && request.ContentLength > 0)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            hashBuilder.Append(body);
        }

        // Include query parameters
        if (attribute.IncludeQueryInHash && request.Query.Any())
        {
            foreach (var param in request.Query.OrderBy(q => q.Key))
            {
                hashBuilder.Append($"{param.Key}={param.Value}");
            }
        }

        if (hashBuilder.Length == 0)
        {
            return string.Empty;
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(hashBuilder.ToString()));
        return Convert.ToBase64String(bytes);
    }

    private static object? GetResultValue(IActionResult result)
    {
        return result switch
        {
            JsonResult jsonResult => jsonResult.Value,
            ObjectResult objectResult => objectResult.Value,
            _ => null
        };
    }

    private static int GetStatusCode(IActionResult result)
    {
        return result switch
        {
            StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
            ObjectResult objectResult => objectResult.StatusCode ?? 200,
            _ => 200
        };
    }
}
