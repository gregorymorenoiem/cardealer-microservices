using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
                var testConfig = new Dictionary<string, string>
                {
                    ["RabbitMQ:Host"] = _rabbitMqContainer.Hostname,
                    ["RabbitMQ:HostName"] = _rabbitMqContainer.Hostname,
                    ["RabbitMQ:Port"] = _rabbitMqContainer.GetMappedPublicPort(5672).ToString(),
                    ["RabbitMQ:UserName"] = "testuser",
                    ["RabbitMQ:Password"] = "testpass",
                    ["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString(),
                    ["Database:Host"] = _postgresContainer.Hostname,
                    ["Database:Port"] = _postgresContainer.GetMappedPublicPort(5432).ToString(),
                    ["Database:Database"] = "mediaservice_test",
                    ["Database:Username"] = "testuser",
                    ["Database:Password"] = "testpass",
                    ["Database:Provider"] = "PostgreSQL"
                };

                config.AddInMemoryCollection(testConfig!);
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(_postgresContainer.GetConnectionString());
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });
            });

            builder.UseEnvironment("Testing");
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            return host;
        }
    }
}
