using Microsoft.Extensions.DependencyInjection;

namespace ChatbotService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        
        return services;
    }
}
