using ErrorService.Shared.Extensions;
using ErrorService.Shared.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ErrorService.Shared.Middleware
{
    /// <summary>
    /// Interfaz para servicios de Rate Limiting
    /// </summary>
    public interface IRateLimitService
    {
        /// <summary>
        /// Verifica si un cliente ha excedido el límite de requests
        /// </summary>
        Task<(bool IsAllowed, int Remaining, DateTime ResetTime)> CheckLimitAsync(
            string clientId, int maxRequests, int windowSeconds);

        /// <summary>
        /// Obtiene estadísticas de rate limiting
        /// </summary>
        Task<RateLimitStats> GetStatsAsync();
    }

    /// <summary>
    /// Estadísticas de Rate Limiting
    /// </summary>
    public class RateLimitStats
    {
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public long TotalRequests { get; set; }
    }

    /// <summary>
    /// Middleware personalizado para Rate Limiting
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly RateLimitingConfiguration _configuration;
        private readonly IRateLimitService _rateLimitService;

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            RateLimitingConfiguration configuration,
            IRateLimitService rateLimitService)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
            _rateLimitService = rateLimitService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Verificar bypass
            if (context.Items.ContainsKey("BypassRateLimit"))
            {
                await _next(context);
                return;
            }

            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Verificar whitelist de IPs
            if (_configuration.WhitelistedIps.Any(ip => ip.Equals(remoteIp, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogDebug("IP {RemoteIp} está en whitelist, bypass de rate limiting", remoteIp);
                await _next(context);
                return;
            }

            // Obtener endpoint y atributos
            var endpoint = context.GetEndpoint();
            var rateLimitAttr = endpoint?.Metadata.GetOrderedMetadata<RateLimitAttribute>().FirstOrDefault();
            var clientRateLimitAttr = endpoint?.Metadata.GetOrderedMetadata<ClientRateLimitAttribute>().FirstOrDefault();

            // Determinar límites
            int maxRequests = _configuration.MaxRequests;
            int windowSeconds = _configuration.WindowSeconds;

            if (rateLimitAttr != null)
            {
                maxRequests = (int?)context.Items["RateLimit_MaxRequests"] ?? maxRequests;
                windowSeconds = (int?)context.Items["RateLimit_WindowSeconds"] ?? windowSeconds;
            }

            if (clientRateLimitAttr != null)
            {
                maxRequests = (int?)context.Items["ClientRateLimit_MaxRequests"] ?? maxRequests;
                windowSeconds = (int?)context.Items["ClientRateLimit_WindowSeconds"] ?? windowSeconds;
            }

            // Obtener cliente ID
            var clientId = context.GetClientId();

            // Verificar límite
            var (isAllowed, remaining, resetTime) = await _rateLimitService.CheckLimitAsync(
                clientId, maxRequests, windowSeconds);

            // Agregar headers
            context.Response.Headers.Append("X-RateLimit-Limit", maxRequests.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", remaining.ToString());
            context.Response.Headers.Append("X-RateLimit-Reset", ((DateTimeOffset)resetTime).ToUnixTimeSeconds().ToString());

            if (!isAllowed)
            {
                var retryAfter = (int)(resetTime - DateTime.UtcNow).TotalSeconds;
                context.Response.Headers.Append("Retry-After", Math.Max(1, retryAfter).ToString());

                _logger.LogWarning(
                    "Rate limit excedido para cliente {ClientId} ({RemoteIp}). " +
                    "Endpoint: {Method} {Path}. " +
                    "Límite: {MaxRequests}/{WindowSeconds}s",
                    clientId, remoteIp, context.Request.Method, context.Request.Path,
                    maxRequests, windowSeconds);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    timestamp = DateTime.UtcNow.ToString("O"),
                    statusCode = 429,
                    message = "Rate limit exceeded. Maximum allowed requests have been reached.",
                    retryAfter = retryAfter,
                    errors = (string?)null
                };

                await context.Response.WriteAsJsonAsync(response);
                return;
            }

            await _next(context);
        }
    }
}
