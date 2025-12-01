using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Shared;
using Xunit;

namespace NotificationService.Tests.Integration.Factories
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;
        public new Task DisposeAsync() => Task.CompletedTask;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<NotificationDbContext>));
                services.AddDbContext<NotificationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("NotificationServiceTestDb_" + Guid.NewGuid());
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                db.Database.EnsureCreated();
            });

            builder.UseEnvironment("Testing");
        }
    }
}
