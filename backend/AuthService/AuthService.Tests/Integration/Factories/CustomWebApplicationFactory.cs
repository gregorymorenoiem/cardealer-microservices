using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
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
        public Task InitializeAsync() => Task.CompletedTask;

        public new Task DisposeAsync() => Task.CompletedTask;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContextOptions
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

                // Use InMemory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("AuthServiceTestDb");
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

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
