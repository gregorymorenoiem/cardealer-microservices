using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Interfaces.Repositories;
using NotificationService.Domain.Interfaces.External;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Services;
using NotificationService.Infrastructure.Templates;
using NotificationService.Infrastructure.BackgroundServices;
using NotificationService.Infrastructure.External;
using NotificationService.Shared;
using CarDealer.Shared.MultiTenancy;

namespace NotificationService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Multi-tenancy support
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();

        // RabbitMQ Configuration
        services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));
        services.Configure<NotificationServiceRabbitMQSettings>(configuration.GetSection("NotificationService"));
        
        // ✅ NotificationSettings Configuration (for Resend, Twilio, Firebase)
        services.Configure<NotificationSettings>(configuration.GetSection("NotificationSettings"));

        // ✅ Register Repositories
        services.AddScoped<INotificationRepository, EfNotificationRepository>();
        services.AddScoped<INotificationTemplateRepository, EfNotificationTemplateRepository>();
        services.AddScoped<INotificationQueueRepository, EfNotificationQueueRepository>();
        services.AddScoped<IScheduledNotificationRepository, EfScheduledNotificationRepository>();
        services.AddScoped<INotificationLogRepository, EfNotificationLogRepository>();

        // ✅ Register External Providers (Email, SMS, Push)
        // Resend for emails (replacing SendGrid)
        services.AddHttpClient("Resend");
        services.AddScoped<IEmailProvider, ResendEmailService>();
        services.AddScoped<ISmsProvider, TwilioSmsService>();
        services.AddScoped<IPushNotificationProvider, FirebasePushService>();
        
        // ✅ Email Service (adapter que usa IEmailProvider)
        services.AddScoped<IEmailService, EmailService>();

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