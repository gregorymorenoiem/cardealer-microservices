using RoleService.Shared.Middleware;
using RoleService.Shared.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RoleService.Shared.Extensions
{
    /// <summary>
    /// Servicio de Rate Limiting basado en en-memoria
    /// </summary>
    public class InMemoryRateLimitService : IRateLimitService
    {
        private readonly ConcurrentDictionary<string, ClientRateLimitData> _clientRequests;
        private readonly ILogger<InMemoryRateLimitService> _logger;

        private class ClientRateLimitData
        {
            public int RequestCount { get; set; }
            public DateTime WindowStart { get; set; }
            public DateTime LastRequest { get; set; }
        }

        public InMemoryRateLimitService(ILogger<InMemoryRateLimitService> logger)
        {
            _clientRequests = new ConcurrentDictionary<string, ClientRateLimitData>();
            _logger = logger;
        }

        public async Task<(bool IsAllowed, int Remaining, DateTime ResetTime)> CheckLimitAsync(
            string clientId, int maxRequests, int windowSeconds)
        {
            return await Task.Run(() =>
            {
                var now = DateTime.UtcNow;
                var windowStart = now.AddSeconds(-windowSeconds);

                var data = _clientRequests.AddOrUpdate(clientId, _ =>
                {
                    return new ClientRateLimitData
                    {
                        RequestCount = 1,
                        WindowStart = now,
                        LastRequest = now
                    };
                }, (key, existing) =>
                {
                    // Si la ventana ha expirado, reinicia el contador
                    if (existing.WindowStart < windowStart)
                    {
                        return new ClientRateLimitData
                        {
                            RequestCount = 1,
                            WindowStart = now,
                            LastRequest = now
                        };
                    }

                    existing.RequestCount++;
                    existing.LastRequest = now;
                    return existing;
                });

                bool isAllowed = data.RequestCount <= maxRequests;
                int remaining = Math.Max(0, maxRequests - data.RequestCount);
                var resetTime = data.WindowStart.AddSeconds(windowSeconds);

                if (!isAllowed)
                {
                    _logger.LogWarning(
                        "Rate limit excedido para cliente {ClientId}. " +
                        "Requests: {RequestCount}/{MaxRequests}. " +
                        "Resettea en {ResetTime}",
                        clientId, data.RequestCount, maxRequests, resetTime);
                }

                return (isAllowed, remaining, resetTime);
            });
        }

        public async Task<RateLimitStats> GetStatsAsync()
        {
            return await Task.Run(() =>
            {
                var stats = new RateLimitStats
                {
                    TotalClients = _clientRequests.Count,
                    ActiveClients = _clientRequests.Count(x =>
                        DateTime.UtcNow.Subtract(x.Value.LastRequest).TotalMinutes < 5),
                    TotalRequests = _clientRequests.Values.Sum(x => x.RequestCount)
                };

                return stats;
            });
        }
    }

    /// <summary>
    /// Extensiones para Rate Limiting
    /// </summary>
    public static class RateLimitingExtensionsMethods
    {
        private static readonly Type LogCategory = typeof(RateLimitingExtensionsMethods);

        /// <summary>
        /// Agrega los servicios de Rate Limiting a la colección de servicios
        /// </summary>
        public static IServiceCollection AddCustomRateLimiting(
            this IServiceCollection services,
            RateLimitingConfiguration? configuration = null)
        {
            configuration ??= new RateLimitingConfiguration();

            if (!configuration.Enabled)
                return services;

            // Registrar la configuración y servicios
            services.AddSingleton(configuration);
            services.AddSingleton<IRateLimitService, InMemoryRateLimitService>();
            services.AddMemoryCache();

            return services;
        }

        /// <summary>
        /// Usa el middleware de Rate Limiting en la aplicación
        /// </summary>
        public static IApplicationBuilder UseCustomRateLimiting(
            this IApplicationBuilder app,
            RateLimitingConfiguration? configuration = null)
        {
            configuration ??= app.ApplicationServices.GetService<RateLimitingConfiguration>();

            if (configuration?.Enabled != true)
                return app;

            app.UseMiddleware<RateLimitingMiddleware>(configuration);

            if (configuration.EnableLogging)
            {
                var logFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
                var logger = logFactory.CreateLogger(LogCategory);
                logger.LogInformation(
                    "Rate Limiting habilitado: máximo {MaxRequests} requests en {WindowSeconds} segundos",
                    configuration.MaxRequests, configuration.WindowSeconds);
            }

            return app;
        }

        /// <summary>
        /// Obtiene el cliente ID desde el contexto HTTP
        /// </summary>
        public static string GetClientId(this HttpContext context)
        {
            // Prioridad: X-Client-Id header > API Key > IP
            if (context.Request.Headers.TryGetValue("X-Client-Id", out var clientId))
                return clientId.ToString();

            if (context.Request.Headers.TryGetValue("Authorization", out var auth))
            {
                var token = auth.ToString().Replace("Bearer ", "").Split(' ')[0];
                return token.Substring(0, Math.Min(10, token.Length));
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        /// <summary>
        /// Verifica si la IP está en la whitelist
        /// </summary>
        public static bool IsIpWhitelisted(this HttpContext context, List<string> whitelistedIps)
        {
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "";
            return whitelistedIps.Any(ip => ip.Equals(remoteIp, StringComparison.OrdinalIgnoreCase));
        }
    }
}
