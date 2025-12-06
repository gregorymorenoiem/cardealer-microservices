// NotificationService.Infrastructure\DependencyInjection.cs
using CarDealer.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Interfaces.External;
using NotificationService.Domain.Interfaces.Repositories;
using NotificationService.Infrastructure.External;
using NotificationService.Infrastructure.MessageBus;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Services;
using NotificationService.Shared;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Multi-tenancy support
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();

        // Database Context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories (Domain Interfaces)
        services.AddScoped<INotificationRepository, EfNotificationRepository>();
        services.AddScoped<INotificationTemplateRepository, EfNotificationTemplateRepository>();
        services.AddScoped<INotificationQueueRepository, EfNotificationQueueRepository>();
        services.AddScoped<INotificationLogRepository, EfNotificationLogRepository>();

        // External Services (Domain Interfaces)
        services.AddScoped<IEmailProvider, SendGridEmailService>();
        services.AddScoped<ISmsProvider, TwilioSmsService>();
        services.AddScoped<IPushNotificationProvider, FirebasePushService>();

        // Infrastructure Services (Domain Interfaces)
        services.AddScoped<ITemplateEngine, TemplateService>();
        services.AddScoped<NotificationQueueService>();

        // ✅ AGREGAR HTTP CLIENT PARA ERROR SERVICE
        services.AddHttpClient<ErrorServiceClient>(client =>
        {
            client.BaseAddress = new Uri("http://errorservice:80");
            client.DefaultRequestHeaders.Add("User-Agent", "NotificationService");
        });

        // Message Bus (Domain Interface)
        services.AddSingleton<IMessageBus>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            return new RabbitMQMessageBus(
                config["RabbitMQ:HostName"] ?? "localhost",
                config["RabbitMQ:UserName"] ?? "guest",
                config["RabbitMQ:Password"] ?? "guest");
        });

        return services;
    }
}