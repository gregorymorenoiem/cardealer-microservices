using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AuditService.Infrastructure.Persistence;
using AuditService.Infrastructure.Persistence.Repositories;
using AuditService.Infrastructure.Repositories;
using AuditService.Domain.Interfaces;
using AuditService.Domain.Interfaces.Repositories;
using AuditService.Shared;
using Xunit;
using Moq;

namespace AuditService.Tests.Integration.Factories
{
    /// <summary>
    /// Test factory for integration tests using InMemory database and mocked external dependencies
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public new Task DisposeAsync() => Task.CompletedTask;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContextOptions
                services.RemoveAll(typeof(DbContextOptions<AuditDbContext>));

                // Use InMemory database for testing
                services.AddDbContext<AuditDbContext>(options =>
                {
                    options.UseInMemoryDatabase("AuditServiceTestDb_" + Guid.NewGuid());
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

                // Ensure database is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
                db.Database.EnsureCreated();
            });

            builder.UseEnvironment("Testing");
        }
    }
}
