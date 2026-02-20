using AdvertisingService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace AdvertisingService.Tests.DI;

/// <summary>
/// DI startup validation test.
/// Overrides external dependencies (PostgreSQL, Redis, RabbitMQ) with in-memory alternatives
/// so this test runs without infrastructure.
/// </summary>
public class StartupTests
{
    [Fact]
    public async Task Application_Starts_And_DI_Resolves_All_Services()
    {
        await using var app = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services =>
                {
                    // Replace PostgreSQL with InMemory DB
                    var dbDescriptors = services
                        .Where(d => d.ServiceType == typeof(DbContextOptions<AdvertisingDbContext>)
                                 || d.ServiceType == typeof(DbContextOptions))
                        .ToList();
                    foreach (var d in dbDescriptors) services.Remove(d);

                    services.AddDbContext<AdvertisingDbContext>(options =>
                        options.UseInMemoryDatabase("DI_Test_Db"));

                    // Replace Redis with in-memory distributed cache
                    var redisDescriptors = services
                        .Where(d => d.ServiceType.FullName?.Contains("IDistributedCache") == true
                                 || d.ServiceType.FullName?.Contains("IConnectionMultiplexer") == true)
                        .ToList();
                    foreach (var d in redisDescriptors) services.Remove(d);

                    services.AddDistributedMemoryCache();

                    // Register mock IConnectionMultiplexer for services that inject it directly
                    // (HomepageRotationCacheService, RecordImpressionCommandHandler)
                    var mockMultiplexer = new Mock<IConnectionMultiplexer>();
                    mockMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                        .Returns(Mock.Of<IDatabase>());
                    services.AddSingleton(mockMultiplexer.Object);
                });
            });

        using var client = app.CreateClient();
        // /health/live only checks if the process is alive — no external dependency checks
        var response = await client.GetAsync("/health/live");
        // If DI fails to resolve any service, the app won't start and this throws
        response.EnsureSuccessStatusCode();
    }
}
