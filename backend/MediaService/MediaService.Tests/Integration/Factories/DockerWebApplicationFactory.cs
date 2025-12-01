using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MediaService.Infrastructure.Persistence;
using MediaService.Shared;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace MediaService.Tests.Integration.Factories
{
    public class DockerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("mediaservice_test")
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
                var rabbitMqConfig = new Dictionary<string, string>
                {
                    ["RabbitMQ:HostName"] = _rabbitMqContainer.Hostname,
                    ["RabbitMQ:Port"] = _rabbitMqContainer.GetMappedPublicPort(5672).ToString(),
                    ["RabbitMQ:UserName"] = "testuser",
                    ["RabbitMQ:Password"] = "testpass"
                };

                config.AddInMemoryCollection(rabbitMqConfig!);
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<MediaDbContext>));
                services.AddDbContext<MediaDbContext>(options =>
                {
                    options.UseNpgsql(_postgresContainer.GetConnectionString());
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
                db.Database.EnsureCreated();
            });

            builder.UseEnvironment("Testing");
        }
    }
}
