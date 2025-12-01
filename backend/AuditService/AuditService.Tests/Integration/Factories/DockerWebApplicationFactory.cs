using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AuditService.Infrastructure.Persistence;
using AuditService.Infrastructure.Persistence.Repositories;
using AuditService.Infrastructure.Repositories;
using AuditService.Domain.Interfaces;
using AuditService.Domain.Interfaces.Repositories;
using AuditService.Shared;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace AuditService.Tests.Integration.Factories
{
    /// <summary>
    /// Test factory for E2E tests using real Docker containers (PostgreSQL + RabbitMQ)
    /// </summary>
    public class DockerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("auditservice_test")
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
                    ["RabbitMQ:ExchangeName"] = "audit_events",
                    ["RabbitMQ:RetryCount"] = "3",
                    ["ErrorService:RabbitMQ:HostName"] = _rabbitMqContainer.Hostname,
                    ["ErrorService:RabbitMQ:Port"] = _rabbitMqContainer.GetMappedPublicPort(5672).ToString(),
                    ["ErrorService:RabbitMQ:UserName"] = "testuser",
                    ["ErrorService:RabbitMQ:Password"] = "testpass",
                    ["ErrorService:RabbitMQ:VirtualHost"] = "/",
                    ["ErrorService:RabbitMQ:QueueName"] = "error_queue",
                    ["ErrorService:RabbitMQ:ExchangeName"] = "error_exchange",
                    ["ErrorService:RabbitMQ:RoutingKey"] = "error"
                };

                config.AddInMemoryCollection(rabbitMqConfig!);
            });

            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContextOptions
                services.RemoveAll(typeof(DbContextOptions<AuditDbContext>));

                // Use PostgreSQL container for testing
                services.AddDbContext<AuditDbContext>(options =>
                {
                    options.UseNpgsql(_postgresContainer.GetConnectionString());
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Register MediatR from Application assembly
                var applicationAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "AuditService.Application");

                if (applicationAssembly != null)
                {
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
                }

                // Register repositories
                services.AddScoped<IAuditLogRepository, AuditLogRepository>();
                services.AddScoped<IAuditRepository, AuditRepository>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                // Ensure database is created and migrated
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
                db.Database.EnsureCreated(); // Use EnsureCreated instead of Migrate for tests
            });

            builder.UseEnvironment("Testing");
        }
    }
}
