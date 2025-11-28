using AuthService.Infrastructure.Services.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Services;

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

        // Register Queue Service
        services.AddScoped<NotificationQueueService>();

        // Background Services
        services.AddHostedService<RabbitMQNotificationConsumer>(); // Para mensajes externos de RabbitMQ
        services.AddHostedService<NotificationQueueBackgroundService>(); // Para procesar colas internas

        return services;
    }
}