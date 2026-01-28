using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Infrastructure.Persistence;
using ChatbotService.Infrastructure.Persistence.Repositories;
using ChatbotService.Infrastructure.Services;

namespace ChatbotService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ChatbotDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            }));

        // Settings
        services.Configure<DialogflowSettings>(configuration.GetSection("Dialogflow"));
        services.Configure<InventorySyncSettings>(configuration.GetSection("InventorySync"));
        services.Configure<ReportingSettings>(configuration.GetSection("Reporting"));

        // Repositories
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IChatLeadRepository, ChatLeadRepository>();
        services.AddScoped<IChatbotConfigurationRepository, ChatbotConfigurationRepository>();
        services.AddScoped<IInteractionUsageRepository, InteractionUsageRepository>();
        services.AddScoped<IMaintenanceTaskRepository, MaintenanceTaskRepository>();
        services.AddScoped<IQuickResponseRepository, QuickResponseRepository>();
        services.AddScoped<IChatbotVehicleRepository, ChatbotVehicleRepository>();
        services.AddScoped<IUnansweredQuestionRepository, UnansweredQuestionRepository>();

        // Services
        services.AddScoped<IDialogflowService, DialogflowService>();
        services.AddScoped<IAutoLearningService, AutoLearningService>();
        services.AddScoped<IHealthMonitoringService, HealthMonitoringService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IInventorySyncService, InventorySyncService>();

        // HTTP Clients with Polly resilience
        services.AddHttpClient("VehiclesApi", client =>
        {
            client.BaseAddress = new Uri(configuration["InventorySync:VehiclesApiUrl"] ?? "http://vehiclessaleservice:8080");
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddHttpClient("NotificationService", client =>
        {
            client.BaseAddress = new Uri(configuration["Reporting:NotificationServiceUrl"] ?? "http://notificationservice:8080");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
