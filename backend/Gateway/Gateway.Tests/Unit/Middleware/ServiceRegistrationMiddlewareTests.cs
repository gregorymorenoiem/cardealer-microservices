using Gateway.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Domain.Entities;
using System.Reflection;

namespace Gateway.Tests.Unit.Middleware;

/// <summary>
/// Unit tests for ServiceRegistrationMiddleware
/// </summary>
public class ServiceRegistrationMiddlewareTests : IDisposable
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

        // Reset static state before each test
        ResetStaticState();
    }

    public void Dispose()
    {
        ResetStaticState();
    }

    private static void ResetStaticState()
    {
        var field = typeof(ServiceRegistrationMiddleware)
            .GetField("_isRegistered", BindingFlags.NonPublic | BindingFlags.Static);
        field?.SetValue(null, false);
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

    [Fact]
    public async Task InvokeAsync_WithInvalidPort_UsesDefaultPort()
    {
        // Arrange
        ResetStaticState();
        var context = new DefaultHttpContext();
        var middleware = new ServiceRegistrationMiddleware(_mockNext.Object);

        var configWithInvalidPort = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Service:Name"] = "Gateway",
                ["Service:Host"] = "localhost",
                ["Service:Port"] = "invalid-port",
                ["Service:HealthCheckUrl"] = "http://localhost:5008/health"
            })
            .Build();

        ServiceInstance? capturedInstance = null;
        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceInstance, CancellationToken>((instance, _) => capturedInstance = instance)
            .ReturnsAsync(new ServiceInstance { Id = "test-id", ServiceName = "Gateway" });

        // Act
        await middleware.InvokeAsync(context, _mockServiceRegistry.Object, configWithInvalidPort);

        // Assert - Should use default port 5008
        capturedInstance.Should().NotBeNull();
        capturedInstance!.Port.Should().Be(5008);
    }

    [Fact]
    public async Task InvokeAsync_WithMissingConfig_UsesDefaultValues()
    {
        // Arrange
        ResetStaticState();
        var context = new DefaultHttpContext();
        var middleware = new ServiceRegistrationMiddleware(_mockNext.Object);

        var emptyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        ServiceInstance? capturedInstance = null;
        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceInstance, CancellationToken>((instance, _) => capturedInstance = instance)
            .ReturnsAsync(new ServiceInstance { Id = "test-id", ServiceName = "Gateway" });

        // Act
        await middleware.InvokeAsync(context, _mockServiceRegistry.Object, emptyConfig);

        // Assert
        capturedInstance.Should().NotBeNull();
        capturedInstance!.ServiceName.Should().Be("Gateway");
        capturedInstance.Host.Should().Be("localhost");
        capturedInstance.Port.Should().Be(5008);
    }

    [Fact]
    public async Task InvokeAsync_RegistersWithCorrectTags()
    {
        // Arrange
        ResetStaticState();
        var context = new DefaultHttpContext();
        var middleware = new ServiceRegistrationMiddleware(_mockNext.Object);

        ServiceInstance? capturedInstance = null;
        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceInstance, CancellationToken>((instance, _) => capturedInstance = instance)
            .ReturnsAsync(new ServiceInstance { Id = "test-id", ServiceName = "Gateway" });

        // Act
        await middleware.InvokeAsync(context, _mockServiceRegistry.Object, _configuration);

        // Assert
        capturedInstance.Should().NotBeNull();
        capturedInstance!.Tags.Should().Contain("gateway");
        capturedInstance.Tags.Should().Contain("routing");
        capturedInstance.Tags.Should().Contain("api-gateway");
        capturedInstance.Tags.Should().Contain("consumer");
    }

    [Fact]
    public async Task InvokeAsync_RegistersWithCorrectMetadata()
    {
        // Arrange
        ResetStaticState();
        var context = new DefaultHttpContext();
        var middleware = new ServiceRegistrationMiddleware(_mockNext.Object);

        ServiceInstance? capturedInstance = null;
        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceInstance, CancellationToken>((instance, _) => capturedInstance = instance)
            .ReturnsAsync(new ServiceInstance { Id = "test-id", ServiceName = "Gateway" });

        // Act
        await middleware.InvokeAsync(context, _mockServiceRegistry.Object, _configuration);

        // Assert
        capturedInstance.Should().NotBeNull();
        capturedInstance!.Metadata.Should().ContainKey("version");
        capturedInstance.Metadata["version"].Should().Be("1.0.0");
        capturedInstance.Metadata.Should().ContainKey("ocelot");
        capturedInstance.Metadata["ocelot"].Should().Be("true");
    }

    [Fact]
    public async Task InvokeAsync_RegistersWithHealthCheckSettings()
    {
        // Arrange
        ResetStaticState();
        var context = new DefaultHttpContext();
        var middleware = new ServiceRegistrationMiddleware(_mockNext.Object);

        ServiceInstance? capturedInstance = null;
        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceInstance, CancellationToken>((instance, _) => capturedInstance = instance)
            .ReturnsAsync(new ServiceInstance { Id = "test-id", ServiceName = "Gateway" });

        // Act
        await middleware.InvokeAsync(context, _mockServiceRegistry.Object, _configuration);

        // Assert
        capturedInstance.Should().NotBeNull();
        capturedInstance!.HealthCheckInterval.Should().Be(10);
        capturedInstance.HealthCheckTimeout.Should().Be(5);
        capturedInstance.HealthCheckUrl.Should().Be("http://localhost:5008/health");
    }

    [Fact]
    public async Task InvokeAsync_GeneratesUniqueInstanceId()
    {
        // Arrange
        ResetStaticState();
        var context = new DefaultHttpContext();
        var middleware = new ServiceRegistrationMiddleware(_mockNext.Object);

        ServiceInstance? capturedInstance = null;
        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceInstance, CancellationToken>((instance, _) => capturedInstance = instance)
            .ReturnsAsync(new ServiceInstance { Id = "test-id", ServiceName = "Gateway" });

        // Act
        await middleware.InvokeAsync(context, _mockServiceRegistry.Object, _configuration);

        // Assert
        capturedInstance.Should().NotBeNull();
        capturedInstance!.Id.Should().StartWith("Gateway-");
        capturedInstance.Id.Should().HaveLength("Gateway-".Length + 36); // GUID length
    }

    [Fact]
    public async Task InvokeAsync_WithCustomServiceName_UsesConfiguredName()
    {
        // Arrange
        ResetStaticState();
        var context = new DefaultHttpContext();
        var middleware = new ServiceRegistrationMiddleware(_mockNext.Object);

        var customConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Service:Name"] = "MyCustomGateway",
                ["Service:Host"] = "myhost",
                ["Service:Port"] = "9000"
            })
            .Build();

        ServiceInstance? capturedInstance = null;
        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceInstance, CancellationToken>((instance, _) => capturedInstance = instance)
            .ReturnsAsync(new ServiceInstance { Id = "test-id", ServiceName = "MyCustomGateway" });

        // Act
        await middleware.InvokeAsync(context, _mockServiceRegistry.Object, customConfig);

        // Assert
        capturedInstance.Should().NotBeNull();
        capturedInstance!.ServiceName.Should().Be("MyCustomGateway");
        capturedInstance.Host.Should().Be("myhost");
        capturedInstance.Port.Should().Be(9000);
    }

    [Fact]
    public async Task InvokeAsync_WhenAlreadyRegistered_DoesNotRegisterAgain()
    {
        // Arrange
        ResetStaticState();
        var context = new DefaultHttpContext();
        var middleware1 = new ServiceRegistrationMiddleware(_mockNext.Object);
        var middleware2 = new ServiceRegistrationMiddleware(_mockNext.Object);

        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceInstance { Id = "test-id", ServiceName = "Gateway" });

        // Act
        await middleware1.InvokeAsync(context, _mockServiceRegistry.Object, _configuration);
        await middleware2.InvokeAsync(context, _mockServiceRegistry.Object, _configuration);

        // Assert
        _mockServiceRegistry.Verify(
            r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_BuildsDefaultHealthCheckUrl_WhenNotConfigured()
    {
        // Arrange
        ResetStaticState();
        var context = new DefaultHttpContext();
        var middleware = new ServiceRegistrationMiddleware(_mockNext.Object);

        var configWithoutHealthUrl = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Service:Name"] = "Gateway",
                ["Service:Host"] = "myhost",
                ["Service:Port"] = "8080"
            })
            .Build();

        ServiceInstance? capturedInstance = null;
        _mockServiceRegistry
            .Setup(r => r.RegisterServiceAsync(It.IsAny<ServiceInstance>(), It.IsAny<CancellationToken>()))
            .Callback<ServiceInstance, CancellationToken>((instance, _) => capturedInstance = instance)
            .ReturnsAsync(new ServiceInstance { Id = "test-id", ServiceName = "Gateway" });

        // Act
        await middleware.InvokeAsync(context, _mockServiceRegistry.Object, configWithoutHealthUrl);

        // Assert
        capturedInstance.Should().NotBeNull();
        capturedInstance!.HealthCheckUrl.Should().Be("http://myhost:8080/health");
    }
}
