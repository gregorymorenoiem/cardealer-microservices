using IdempotencyService.Core.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IdempotencyService.Api.Extensions;

/// <summary>
/// Swagger operation filter that adds idempotency header documentation
/// </summary>
public class IdempotencyHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the endpoint has IdempotentAttribute
        var idempotentAttribute = context.MethodInfo
            .GetCustomAttributes(typeof(IdempotentAttribute), true)
            .Cast<IdempotentAttribute>()
            .FirstOrDefault();

        if (idempotentAttribute == null)
        {
            // Check controller level
            idempotentAttribute = context.MethodInfo.DeclaringType?
                .GetCustomAttributes(typeof(IdempotentAttribute), true)
                .Cast<IdempotentAttribute>()
                .FirstOrDefault();
        }

        // Check for skip attribute
        var skipAttribute = context.MethodInfo
            .GetCustomAttributes(typeof(SkipIdempotencyAttribute), true)
            .FirstOrDefault();

        if (skipAttribute != null)
        {
            // Endpoint explicitly skips idempotency
            return;
        }

        if (idempotentAttribute != null)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var headerName = idempotentAttribute.HeaderName ?? "X-Idempotency-Key";

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = headerName,
                In = ParameterLocation.Header,
                Required = idempotentAttribute.RequireKey,
                Description = idempotentAttribute.RequireKey
                    ? "Idempotency key (required). Use the same key to safely retry the request."
                    : "Idempotency key (optional). Provide to make this request idempotent.",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Example = new Microsoft.OpenApi.Any.OpenApiString(Guid.NewGuid().ToString())
                }
            });

            // Add response header
            operation.Responses ??= new OpenApiResponses();
            foreach (var response in operation.Responses.Values)
            {
                response.Headers ??= new Dictionary<string, OpenApiHeader>();
                response.Headers["X-Idempotency-Replayed"] = new OpenApiHeader
                {
                    Description = "Indicates if this response was replayed from cache",
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = new List<Microsoft.OpenApi.Any.IOpenApiAny>
                        {
                            new Microsoft.OpenApi.Any.OpenApiString("true"),
                            new Microsoft.OpenApi.Any.OpenApiString("false")
                        }
                    }
                };
            }

            // Update operation description
            var idempotencyInfo = idempotentAttribute.RequireKey
                ? "🔒 **This endpoint requires an idempotency key.**"
                : "🔓 **This endpoint supports optional idempotency.**";

            var ttlInfo = idempotentAttribute.TtlSeconds > 0
                ? $" Cached for {idempotentAttribute.TtlSeconds} seconds."
                : "";

            operation.Description = $"{operation.Description}\n\n{idempotencyInfo}{ttlInfo}";
        }
    }
}
