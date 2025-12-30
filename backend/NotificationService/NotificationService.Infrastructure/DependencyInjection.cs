// NotificationService.Infrastructure\DependencyInjection.cs
using CarDealer.Shared.MultiTenancy;
using CarDealer.Shared.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Interfaces.External;
using NotificationService.Domain.Interfaces.Repositories;
using NotificationService.Infrastructure.Configuration;
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
        // Secret Provider (reads from ENV vars and Docker secrets)
        services.AddSecretProvider();
        
        // Notification Secrets Configuration (overrides settings from secrets)
        services.AddNotificationSecretsConfiguration(configuration);

        // Multi-tenancy support
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();

        // Database Context - connection string from secrets has priority
        var secretProvider = CompositeSecretProvider.CreateDefault();
        var connectionString = secretProvider.GetSecret(SecretKeys.DatabaseConnectionString)
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["Database:ConnectionStrings:PostgreSQL"];
            
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
        
        // Firebase Credential Provider
        services.AddSingleton<FirebaseCredentialProvider>();

        // Infrastructure Services (Domain Interfaces)
        services.AddScoped<ITemplateEngine, TemplateService>();
        services.AddScoped<NotificationQueueService>();

        // ✅ HTTP CLIENT PARA ERROR SERVICE
        services.AddHttpClient<ErrorServiceClient>(client =>
        {
            client.BaseAddress = new Uri("http://errorservice:80");
            client.DefaultRequestHeaders.Add("User-Agent", "NotificationService");
        });

        // Message Bus (Domain Interface) - using secrets for RabbitMQ
        services.AddSingleton<IMessageBus>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var secrets = sp.GetRequiredService<ISecretProvider>();
            
            var host = secrets.GetSecret(SecretKeys.RabbitMqHost)
                ?? config["RabbitMQ:HostName"] 
                ?? "localhost";
            var user = secrets.GetSecret(SecretKeys.RabbitMqUser)
                ?? config["RabbitMQ:UserName"] 
                ?? "guest";
            var password = secrets.GetSecret(SecretKeys.RabbitMqPassword)
                ?? config["RabbitMQ:Password"] 
                ?? "guest";
                
            return new RabbitMQMessageBus(host, user, password);
        });

        return services;
    }
}