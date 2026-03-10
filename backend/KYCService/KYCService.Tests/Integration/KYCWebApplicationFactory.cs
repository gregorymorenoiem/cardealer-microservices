using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KYCService.Infrastructure.Persistence;
using KYCService.Domain.Interfaces;
using KYCService.Application.Clients;
using Moq;
using CarDealer.Shared.Configuration;

namespace KYCService.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for KYCService integration tests.
/// Uses InMemory database and mocks for all external dependencies
/// (AuditService, IdempotencyService, MediaService, RabbitMQ, ConfigurationService).
/// </summary>
public class KYCWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // ── Replace PostgreSQL with InMemory database ──────────────
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<KYCDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            // Remove the DbContext registration itself
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(KYCDbContext));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Remove any DbContextOptions
            var optionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions));
            if (optionsDescriptor != null)
                services.Remove(optionsDescriptor);

            services.AddDbContext<KYCDbContext>(options =>
                options.UseInMemoryDatabase($"KYCTestDb_{Guid.NewGuid()}"));

            // ── Mock external service clients ──────────────────────────
            ReplaceServiceWithMock<IAuditServiceClient>(services);
            ReplaceServiceWithMock<IIdempotencyServiceClient>(services);
            ReplaceServiceWithMock<IMediaServiceClient>(services);
            ReplaceServiceWithMock<IKYCEventPublisher>(services);
            ReplaceServiceWithMock<IErrorServiceClient>(services);

            // ── Mock ConfigurationService client ───────────────────────
            var configMock = new Mock<IConfigurationServiceClient>();
            configMock.Setup(c => c.GetIntAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((string _, int defaultValue, CancellationToken __) => defaultValue);
            configMock.Setup(c => c.IsEnabledAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((string _, bool defaultValue, CancellationToken __) => defaultValue);
            ReplaceService(services, configMock.Object);
        });
    }

    private static void ReplaceServiceWithMock<T>(IServiceCollection services) where T : class
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
            services.Remove(descriptor);

        services.AddSingleton(new Mock<T>().Object);
    }

    private static void ReplaceService<T>(IServiceCollection services, T implementation) where T : class
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
            services.Remove(descriptor);

        services.AddSingleton(implementation);
    }
}
