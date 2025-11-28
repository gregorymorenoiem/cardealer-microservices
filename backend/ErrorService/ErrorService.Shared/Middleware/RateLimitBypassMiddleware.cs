using ErrorService.Shared.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ErrorService.Shared.Middleware
{
    /// <summary>
    /// Middleware para manejar el bypass de rate limiting
    /// </summary>
    public class RateLimitBypassMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitBypassMiddleware> _logger;

        public RateLimitBypassMiddleware(RequestDelegate next, ILogger<RateLimitBypassMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Obtener el endpoint
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                // Verificar si el endpoint tiene el atributo AllowRateLimitBypass
                var allowBypass = endpoint.Metadata.GetOrderedMetadata<AllowRateLimitBypassAttribute>()
                    .FirstOrDefault() != null;

                if (allowBypass)
                {
                    // Marcar en el contexto que este request puede bypasear el rate limiting
                    context.Items["BypassRateLimit"] = true;
                    _logger.LogDebug("Rate limit bypass habilitado para {Method} {Path}",
                        context.Request.Method, context.Request.Path);
                }
            }

            await _next(context);
        }
    }
}
