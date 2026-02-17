using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Messaging;

/// <summary>
/// Extension methods for registering shared messaging components.
/// Replaces per-service InMemoryDeadLetterQueue and per-class RabbitMQ connections.
/// </summary>
public static class MessagingExtensions
{
    /// <summary>
    /// Registers a singleton SharedRabbitMqConnection for the service.
    /// All publishers/consumers should inject this instead of creating their own ConnectionFactory.
    /// 
    /// Usage in Program.cs:
    ///   builder.Services.AddSharedRabbitMqConnection(builder.Configuration);
    /// </summary>
    public static IServiceCollection AddSharedRabbitMqConnection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<SharedRabbitMqConnection>(sp =>
            new SharedRabbitMqConnection(
                configuration,
                sp.GetRequiredService<ILogger<SharedRabbitMqConnection>>()));

        return services;
    }

    /// <summary>
    /// Registers a PostgreSQL-backed Dead Letter Queue that persists across pod restarts.
    /// Replaces InMemoryDeadLetterQueue for auto-scaling safety.
    /// 
    /// Usage in Program.cs:
    ///   builder.Services.AddPostgreSqlDeadLetterQueue(builder.Configuration, "AuthService");
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">App configuration (reads ConnectionStrings:DefaultConnection)</param>
    /// <param name="serviceName">Name of the calling service (used for filtering DLQ entries)</param>
    /// <param name="maxRetries">Maximum retry attempts before giving up (default: 5)</param>
    public static IServiceCollection AddPostgreSqlDeadLetterQueue(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        int maxRetries = 5)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["Database:ConnectionStrings:PostgreSQL"]
            ?? throw new InvalidOperationException(
                $"No PostgreSQL connection string found for {serviceName}. " +
                "Set ConnectionStrings:DefaultConnection or Database:ConnectionStrings:PostgreSQL.");

        services.AddSingleton<ISharedDeadLetterQueue>(sp =>
            new PostgreSqlDeadLetterQueue(
                connectionString,
                serviceName,
                sp.GetRequiredService<ILogger<PostgreSqlDeadLetterQueue>>(),
                maxRetries));

        return services;
    }
}
