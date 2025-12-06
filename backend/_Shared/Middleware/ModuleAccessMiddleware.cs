using System;
using System.Linq;
using System.Threading.Tasks;
using CarDealer.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Middleware
{
    /// <summary>
    /// Middleware para verificar acceso a módulos add-on
    /// Aplicar en microservicios que implementan features vendibles
    /// 
    /// Ejemplo de uso en CRMService:
    /// app.UseModuleAccess("crm-advanced");
    /// </summary>
    public class ModuleAccessMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _requiredModuleCode;
        private readonly ILogger<ModuleAccessMiddleware> _logger;

        public ModuleAccessMiddleware(
            RequestDelegate next,
            string requiredModuleCode,
            ILogger<ModuleAccessMiddleware> logger)
        {
            _next = next;
            _requiredModuleCode = requiredModuleCode;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IModuleAccessService moduleAccessService)
        {
            // Endpoints que NO requieren verificación de módulo
            var publicPaths = new[]
            {
                "/health",
                "/swagger",
                "/api-docs",
                "/metrics"
            };

            if (publicPaths.Any(p => context.Request.Path.StartsWithSegments(p)))
            {
                await _next(context);
                return;
            }

            // Extraer dealerId del token JWT
            var dealerIdClaim = context.User.FindFirst("dealerId")?.Value;

            if (string.IsNullOrEmpty(dealerIdClaim))
            {
                _logger.LogWarning("Request to {Path} without dealerId claim", context.Request.Path);
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Forbidden",
                    message = "Dealer ID required"
                });
                return;
            }

            if (!Guid.TryParse(dealerIdClaim, out var dealerId))
            {
                _logger.LogWarning("Invalid dealerId format: {DealerId}", dealerIdClaim);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Bad Request",
                    message = "Invalid dealer ID format"
                });
                return;
            }

            // Verificar si el dealer tiene acceso al módulo
            var hasAccess = await moduleAccessService.HasModuleAccessAsync(dealerId, _requiredModuleCode);

            if (!hasAccess)
            {
                _logger.LogWarning(
                    "Dealer {DealerId} attempted to access {Path} without module {ModuleCode}",
                    dealerId, context.Request.Path, _requiredModuleCode);

                context.Response.StatusCode = 402; // 402 Payment Required
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Payment Required",
                    message = $"This feature requires the '{_requiredModuleCode}' module",
                    moduleCode = _requiredModuleCode,
                    upgradeUrl = $"/dealer/billing/modules/{_requiredModuleCode}"
                });
                return;
            }

            _logger.LogDebug("Dealer {DealerId} has access to module {ModuleCode}", dealerId, _requiredModuleCode);

            await _next(context);
        }
    }

    /// <summary>
    /// Extension methods para registrar el middleware fácilmente
    /// </summary>
    public static class ModuleAccessMiddlewareExtensions
    {
        /// <summary>
        /// Usa el middleware de verificación de módulos
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="moduleCode">Código del módulo requerido (ej: "crm-advanced")</param>
        public static IApplicationBuilder UseModuleAccess(this IApplicationBuilder app, string moduleCode)
        {
            return app.UseMiddleware<ModuleAccessMiddleware>(moduleCode);
        }
    }

    /// <summary>
    /// Atributo para proteger endpoints específicos (alternativa al middleware global)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireModuleAttribute : Attribute
    {
        public string ModuleCode { get; }

        public RequireModuleAttribute(string moduleCode)
        {
            ModuleCode = moduleCode;
        }
    }
}
