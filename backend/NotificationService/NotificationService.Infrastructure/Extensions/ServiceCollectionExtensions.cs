using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Interfaces.Repositories;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Services;
using NotificationService.Infrastructure.Templates;
using NotificationService.Infrastructure.BackgroundServices;

namespace NotificationService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context (ya configurado en Program.cs)
        // No duplicar la configuración del DbContext aquí

        // RabbitMQ Configuration
        services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));
        services.Configure<NotificationServiceRabbitMQSettings>(configuration.GetSection("NotificationService"));

        // ✅ Register Repositories
        services.AddScoped<IScheduledNotificationRepository, EfScheduledNotificationRepository>();

        // ✅ Register Template Engine
        services.AddScoped<ITemplateEngine, TemplateEngine>();

        // ✅ Register Scheduling Service
        services.AddScoped<ISchedulingService, SchedulingService>();

        // Register Queue Service
        services.AddScoped<NotificationQueueService>();

        // ✅ Register Memory Cache
        services.AddMemoryCache();

        // Background Services
        services.AddHostedService<RabbitMQNotificationConsumer>(); // Para mensajes externos de RabbitMQ
        services.AddHostedService<NotificationQueueBackgroundService>(); // Para procesar colas internas
        services.AddHostedService<ScheduledNotificationWorker>(); // ✅ Para procesar notificaciones programadas

        return services;
    }
}