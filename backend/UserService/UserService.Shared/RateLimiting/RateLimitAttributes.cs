using Microsoft.AspNetCore.Mvc.Filters;

namespace UserService.Shared.RateLimiting
{
    /// <summary>
    /// Atributo para aplicar rate limiting específico a un endpoint
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RateLimitAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _maxRequests;
        private readonly int _windowSeconds;
        private readonly string? _policy;

        /// <summary>
        /// Inicializa una nueva instancia del atributo RateLimitAttribute
        /// </summary>
        /// <param name="maxRequests">Número máximo de requests permitidos</param>
        /// <param name="windowSeconds">Duración de la ventana de tiempo en segundos</param>
        /// <param name="policy">Nombre de la política (opcional)</param>
        public RateLimitAttribute(int maxRequests = 100, int windowSeconds = 60, string? policy = null)
        {
            _maxRequests = maxRequests;
            _windowSeconds = windowSeconds;
            _policy = policy;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Agregar metadatos al contexto para que el middleware pueda procesarlo
            context.HttpContext.Items["RateLimit_MaxRequests"] = _maxRequests;
            context.HttpContext.Items["RateLimit_WindowSeconds"] = _windowSeconds;
            if (_policy != null)
                context.HttpContext.Items["RateLimit_Policy"] = _policy;

            await next();
        }
    }

    /// <summary>
    /// Atributo para exentar un endpoint del rate limiting
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowRateLimitBypassAttribute : Attribute
    {
        public AllowRateLimitBypassAttribute() { }
    }

    /// <summary>
    /// Atributo para aplicar rate limiting diferenciado por cliente
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ClientRateLimitAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _maxRequests;
        private readonly int _windowSeconds;

        /// <summary>
        /// Inicializa una nueva instancia del atributo ClientRateLimitAttribute
        /// </summary>
        /// <param name="maxRequests">Número máximo de requests permitidos por cliente</param>
        /// <param name="windowSeconds">Duración de la ventana de tiempo en segundos</param>
        public ClientRateLimitAttribute(int maxRequests = 50, int windowSeconds = 60)
        {
            _maxRequests = maxRequests;
            _windowSeconds = windowSeconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Marcar que este endpoint requiere rate limiting por cliente
            context.HttpContext.Items["ClientRateLimit_MaxRequests"] = _maxRequests;
            context.HttpContext.Items["ClientRateLimit_WindowSeconds"] = _windowSeconds;

            await next();
        }
    }
}
