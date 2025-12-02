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
using AuditService.Application.Features.Audit.Commands.CreateAudit;
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
        private readonly string _databaseName = $"AuditServiceTestDb_{Guid.NewGuid()}";

        public Task InitializeAsync() => Task.CompletedTask;

        public new Task DisposeAsync() => Task.CompletedTask;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove all existing DbContext registrations
                services.RemoveAll(typeof(DbContextOptions<AuditDbContext>));
                services.RemoveAll(typeof(AuditDbContext));

                // Use InMemory database for testing with a fixed name per factory instance
                services.AddDbContext<AuditDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Register MediatR from Application assembly using a known type from that assembly
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssemblyContaining<CreateAuditCommand>());

                // Register repositories
                services.AddScoped<IAuditLogRepository, AuditLogRepository>();
                services.AddScoped<IAuditRepository, AuditRepository>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
            });

            builder.UseEnvironment("Testing");
        }
    }
}
