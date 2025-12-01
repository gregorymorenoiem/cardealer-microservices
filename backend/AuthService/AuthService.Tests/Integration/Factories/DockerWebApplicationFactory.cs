using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AuthService.Infrastructure.Persistence;
using AuthService.Shared;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace AuthService.Tests.Integration.Factories
{
    public class DockerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("authservice_test")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();

        private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();
            await _rabbitMqContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgresContainer.StopAsync();
            await _rabbitMqContainer.StopAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Add in-memory configuration for RabbitMQ
                var rabbitMqConfig = new Dictionary<string, string>
                {
                    ["RabbitMQ:HostName"] = _rabbitMqContainer.Hostname,
                    ["RabbitMQ:Port"] = _rabbitMqContainer.GetMappedPublicPort(5672).ToString(),
                    ["RabbitMQ:UserName"] = "testuser",
                    ["RabbitMQ:Password"] = "testpass",
                    ["RabbitMQ:VirtualHost"] = "/",
                    ["RabbitMQ:ExchangeName"] = "auth_events",
                    ["RabbitMQ:RetryCount"] = "3",
                    ["ErrorService:RabbitMQ:HostName"] = _rabbitMqContainer.Hostname,
                    ["ErrorService:RabbitMQ:Port"] = _rabbitMqContainer.GetMappedPublicPort(5672).ToString(),
                    ["ErrorService:RabbitMQ:UserName"] = "testuser",
                    ["ErrorService:RabbitMQ:Password"] = "testpass",
                    ["ErrorService:RabbitMQ:VirtualHost"] = "/",
                    ["ErrorService:RabbitMQ:QueueName"] = "error_queue",
                    ["ErrorService:RabbitMQ:ExchangeName"] = "error_exchange",
                    ["ErrorService:RabbitMQ:RoutingKey"] = "error",
                    ["NotificationService:RabbitMQ:HostName"] = _rabbitMqContainer.Hostname,
                    ["NotificationService:RabbitMQ:Port"] = _rabbitMqContainer.GetMappedPublicPort(5672).ToString(),
                    ["NotificationService:RabbitMQ:UserName"] = "testuser",
                    ["NotificationService:RabbitMQ:Password"] = "testpass",
                    ["NotificationService:RabbitMQ:VirtualHost"] = "/",
                    ["NotificationService:RabbitMQ:QueueName"] = "notification_queue",
                    ["NotificationService:RabbitMQ:ExchangeName"] = "notification_exchange",
                    ["NotificationService:RabbitMQ:RoutingKey"] = "notification"
                };

                config.AddInMemoryCollection(rabbitMqConfig!);
            });

            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContextOptions
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

                // Use PostgreSQL container for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(_postgresContainer.GetConnectionString());
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Ensure IPasswordHasher<object> is registered
                if (!services.Any(x => x.ServiceType == typeof(IPasswordHasher<object>)))
                {
                    services.AddScoped<IPasswordHasher<object>, PasswordHasher<object>>();
                }

                // Ensure database is created and migrated
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated(); // Use EnsureCreated instead of Migrate for tests
            });

            builder.UseEnvironment("Testing");
        }
    }
}
