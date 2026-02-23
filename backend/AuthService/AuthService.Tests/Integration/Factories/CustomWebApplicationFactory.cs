using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Identity;
using AuthService.Infrastructure.Persistence;
using AuthService.Domain.Interfaces.Services;
using AuthService.Infrastructure.Services.Messaging;
using AuthService.Infrastructure.External;
using AuthService.Domain.Interfaces;
using Xunit;
using Moq;

namespace AuthService.Tests.Integration.Factories
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        // Use a static DB name so all tests share the same database within a collection
        private static readonly string DatabaseName = $"AuthServiceTestDb_{Guid.NewGuid():N}";

        public Task InitializeAsync() => Task.CompletedTask;

        public new Task DisposeAsync() => Task.CompletedTask;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Set required environment variables for JWT
            builder.UseSetting("Jwt:Key", "ThisIsATestSecretKeyForAuthServiceTestingPurposesOnly12345");
            builder.UseSetting("Jwt:Issuer", "AuthService.Tests");
            builder.UseSetting("Jwt:Audience", "AuthService.Tests");
            builder.UseSetting("Jwt:ExpirationMinutes", "60");
            builder.UseSetting("Database:AutoMigrate", "false"); // Disable migrations for InMemory
            builder.UseSetting("Database:SeedDefaultAdmin", "false"); // Disable admin seeding (no ADMIN_SEED_PASSWORD in tests)
            builder.UseSetting("Cache:EnableDistributedCache", "false"); // Use in-memory cache instead of Redis

            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContextOptions
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

                // Use InMemory database for testing - shared across all tests in factory
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(DatabaseName);
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Replace Redis distributed cache with in-memory cache for tests
                services.RemoveAll<IDistributedCache>();
                services.AddDistributedMemoryCache();

                // Mock RabbitMQ services
                services.RemoveAll<IErrorEventProducer>();
                services.RemoveAll<INotificationEventProducer>();
                services.RemoveAll<IEventPublisher>();
                services.RemoveAll<NotificationServiceClient>();
                services.AddSingleton(_ => Mock.Of<IErrorEventProducer>());
                services.AddSingleton(_ => Mock.Of<INotificationEventProducer>());
                services.AddSingleton(_ => Mock.Of<IEventPublisher>());
                services.AddSingleton(_ => Mock.Of<NotificationServiceClient>());

                // Mock IAuthNotificationService
                services.RemoveAll<IAuthNotificationService>();
                services.AddSingleton(_ => Mock.Of<IAuthNotificationService>());

                // Ensure IPasswordHasher<object> is registered
                if (!services.Any(x => x.ServiceType == typeof(IPasswordHasher<object>)))
                {
                    services.AddScoped<IPasswordHasher<object>, PasswordHasher<object>>();
                }

                // Ensure database is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            });

            builder.UseEnvironment("Testing");
        }
    }
}
