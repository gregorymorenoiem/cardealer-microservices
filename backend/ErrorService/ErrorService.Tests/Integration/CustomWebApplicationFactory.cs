using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ErrorService.Infrastructure.Persistence;
using ErrorService.Domain.Interfaces;
using ErrorService.Infrastructure.Messaging;
using ErrorService.Infrastructure.Services.Messaging;
using CarDealer.Contracts.Abstractions;

namespace ErrorService.Tests.Integration
{
    /// <summary>
    /// Custom WebApplicationFactory for E2E integration tests.
    /// Configures the application to use correct PostgreSQL port (25432) and other test settings.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Override connection string to use correct PostgreSQL port
                var connectionString = "Host=localhost;Port=25432;Database=errorservice;Username=postgres;Password=password;Pooling=true;";

                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = connectionString,

                    // JWT Configuration - match appsettings.Development.json
                    ["Jwt:Issuer"] = "cardealer-auth-dev",
                    ["Jwt:Audience"] = "cardealer-services-dev",
                    ["Jwt:SecretKey"] = "development-jwt-secret-key-minimum-32-chars-long-for-testing!",

                    // Disable rate limiting for tests (avoid 429 Too Many Requests)
                    ["RateLimiting:WindowSeconds"] = "3600",
                    ["RateLimiting:MaxRequests"] = "10000",

                    // Use simplified logging for tests
                    ["Logging:LogLevel:Default"] = "Warning",
                    ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
                    ["Logging:LogLevel:ErrorService"] = "Information"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Use InMemory database for integration tests (avoids PostgreSQL connection issues)
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ErrorServiceTestDb");
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Ensure database is created for integration tests
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

                try
                {
                    // Ensure in-memory database is created
                    db.Database.EnsureCreated();
                    logger.LogInformation("✅ In-memory database created successfully for integration tests");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "❌ An error occurred while creating in-memory database for integration tests");
                    throw;
                }

                // Remove real RabbitMQ publishers and register no-op implementations
                // IMPORTANTE: Remover tanto la interfaz como la implementación concreta
                var eventPublisherDescriptors = services.Where(d => d.ServiceType == typeof(IEventPublisher)).ToList();
                foreach (var epDescriptor in eventPublisherDescriptors)
                {
                    services.Remove(epDescriptor);
                }

                // Remover el RabbitMqEventPublisher concreto (evita que intente conectar a RabbitMQ)
                var rabbitMqDescriptors = services.Where(d =>
                    d.ServiceType == typeof(ErrorService.Infrastructure.Messaging.RabbitMqEventPublisher) ||
                    d.ImplementationType == typeof(ErrorService.Infrastructure.Messaging.RabbitMqEventPublisher)).ToList();
                foreach (var rmqDescriptor in rabbitMqDescriptors)
                {
                    services.Remove(rmqDescriptor);
                }

                var deadLetterDescriptors = services.Where(d => d.ServiceType == typeof(IDeadLetterQueue)).ToList();
                foreach (var dlDescriptor in deadLetterDescriptors)
                {
                    services.Remove(dlDescriptor);
                }

                // Remover el DeadLetterQueueProcessor (HostedService que depende de RabbitMqEventPublisher)
                var dlqProcessorDescriptors = services.Where(d =>
                    d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                    d.ImplementationType == typeof(ErrorService.Infrastructure.Messaging.DeadLetterQueueProcessor)).ToList();
                foreach (var dlqDescriptor in dlqProcessorDescriptors)
                {
                    services.Remove(dlqDescriptor);
                }

                // Remover el RabbitMQErrorConsumer (BackgroundService que conecta a RabbitMQ en el constructor)
                var rabbitMqConsumerDescriptors = services.Where(d =>
                    d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                    d.ImplementationType == typeof(ErrorService.Infrastructure.Services.Messaging.RabbitMQErrorConsumer)).ToList();
                foreach (var consumerDescriptor in rabbitMqConsumerDescriptors)
                {
                    services.Remove(consumerDescriptor);
                }

                services.AddSingleton<IEventPublisher, NoOpEventPublisher>();
                services.AddSingleton<IDeadLetterQueue, NoOpDeadLetterQueue>();
            });
        }
    }

    #region Test Mocks (No-Op implementations)

    /// <summary>
    /// No-op implementation of IEventPublisher for integration tests (avoids RabbitMQ connection).
    /// </summary>
    internal class NoOpEventPublisher : IEventPublisher
    {
        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IEvent => Task.CompletedTask;
    }

    /// <summary>
    /// No-op implementation of IDeadLetterQueue for integration tests.
    /// </summary>
    internal class NoOpDeadLetterQueue : IDeadLetterQueue
    {
        public void Enqueue(FailedEvent failedEvent) { }
        public IEnumerable<FailedEvent> GetEventsReadyForRetry() => Array.Empty<FailedEvent>();
        public void Remove(Guid eventId) { }
        public void MarkAsFailed(Guid eventId, string error) { }
        public (int TotalEvents, int ReadyForRetry, int MaxRetries) GetStats() => (0, 0, 0);
    }

    #endregion
}
