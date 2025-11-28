using ErrorService.Domain.Interfaces;
using ErrorService.Shared.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ErrorService.Shared.Extensions
{
    public static class ErrorHandlingExtensions
    {
        public static IServiceCollection AddErrorHandling(this IServiceCollection services, string serviceName)
        {
            services.AddSingleton(new ErrorHandlingMiddlewareOptions { ServiceName = serviceName });
            return services;
        }

        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        {
            var serviceName = app.ApplicationServices.GetService<ErrorHandlingMiddlewareOptions>()?.ServiceName ?? "UnknownService";
            var errorReporter = app.ApplicationServices.GetRequiredService<IErrorReporter>();
            
            return app.UseMiddleware<ErrorHandlingMiddleware>(errorReporter, serviceName);
        }
    }

    public class ErrorHandlingMiddlewareOptions
    {
        public string ServiceName { get; set; } = "UnknownService";
    }
}   