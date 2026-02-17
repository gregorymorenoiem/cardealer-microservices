using CarDealer.Shared.Resilience.Middleware;
using Microsoft.AspNetCore.Builder;

namespace CarDealer.Shared.Resilience.Extensions;

/// <summary>
/// Extensiones para registrar middlewares de resiliencia en el pipeline HTTP
/// </summary>
public static class ResilienceMiddlewareExtensions
{
    /// <summary>
    /// Agrega el middleware de degradación graceful.
    /// Intercepta errores de circuit breaker y timeouts para devolver respuestas degradadas
    /// en lugar de errores 500 al cliente.
    /// 
    /// ⚠️ DEBE colocarse ANTES de Ocelot o del pipeline de routing principal.
    /// </summary>
    public static IApplicationBuilder UseGracefulDegradation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GracefulDegradationMiddleware>();
    }
}
