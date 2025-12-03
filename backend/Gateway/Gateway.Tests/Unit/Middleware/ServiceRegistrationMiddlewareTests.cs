using Gateway.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Domain.Entities;

namespace Gateway.Tests.Unit.Middleware;

/// <summary>
/// Unit tests for ServiceRegistrationMiddleware
/// </summary>
public class ServiceRegistrationMiddlewareTests
{
    private readonly Mock<IServiceRegistry> _mockServiceRegistry;
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly IConfiguration _configuration;

    public ServiceRegistrationMiddlewareTests()
    {
        _mockServiceRegistry = new Mock<IServiceRegistry>();
        _mockNext = new Mock<RequestDelegate>();

        // Create configuration
        var configDictionary = new Dictionary<string, string?>
        {
            ["Service:Name"] = "Gateway",
            ["Service:Host"] = "localhost",
            ["Service:Port"] = "5008",
            ["Service:HealthCheckUrl"] = "http://localhost:5008/health"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDictionary)
            .Build();
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new ServiceRegistrationMiddleware(_mockNext.Object);

        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceInstance { Id = "test-id", ServiceName = "Gateway" });

        // Act
        await middleware.InvokeAsync(context, _mockServiceRegistry.Object, _configuration);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_RegistersServiceOnce()
    {
        // Arrange
        var context1 = new DefaultHttpContext();
        var context2 = new DefaultHttpContext();
        var middleware = new ServiceRegistrationMiddleware(_mockNext.Object);

        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceInstance { Id = "test-id", ServiceName = "Gateway" });

        // Act
        await middleware.InvokeAsync(context1, _mockServiceRegistry.Object, _configuration);
        await middleware.InvokeAsync(context2, _mockServiceRegistry.Object, _configuration);

        // Assert - Should only register once due to static flag
        _mockServiceRegistry.Verify(
            r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_HandlesRegistrationException_ContinuesProcessing()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new ServiceRegistrationMiddleware(_mockNext.Object);

        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Registration failed"));

        // Act
        var act = async () => await middleware.InvokeAsync(context, _mockServiceRegistry.Object, _configuration);

        // Assert - Should not throw, should continue processing
        await act.Should().NotThrowAsync();
        _mockNext.Verify(next => next(context), Times.Once);
    }
}
