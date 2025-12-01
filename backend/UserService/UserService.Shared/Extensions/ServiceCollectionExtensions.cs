using UserService.Shared.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace UserService.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddErrorHandling(this IServiceCollection services)
        {
            services.AddScoped<ErrorHandlingMiddleware>();
            return services;
        }

        // public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        // {
        //     app.UseMiddleware<ErrorHandlingMiddleware>();
        //     return app;
        // }
    }
}
