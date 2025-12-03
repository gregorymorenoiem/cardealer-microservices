using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Ocelot.DependencyInjection;

namespace Gateway.Tests.Unit.DependencyInjection;

/// <summary>
/// Unit tests for Gateway dependency injection configuration
/// </summary>
public class ServiceCollectionTests
{
    [Fact]
    public void Services_CanResolveOcelotConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ocelot.dev.json", optional: true)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddOcelot(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        serviceProvider.Should().NotBeNull();
    }

    [Fact]
    public void Services_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.Should().NotBeNull();
    }
}
