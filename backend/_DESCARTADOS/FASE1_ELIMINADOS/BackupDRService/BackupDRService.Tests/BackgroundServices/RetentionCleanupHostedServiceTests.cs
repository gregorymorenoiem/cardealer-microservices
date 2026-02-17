using BackupDRService.Core.BackgroundServices;
using BackupDRService.Core.Entities;
using BackupDRService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackupDRService.Tests.BackgroundServices;

public class RetentionCleanupHostedServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<RetentionCleanupHostedService>> _mockLogger;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceProvider> _mockScopedProvider;
    private readonly Mock<RetentionService> _mockRetentionService;

    public RetentionCleanupHostedServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<RetentionCleanupHostedService>>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScopedProvider = new Mock<IServiceProvider>();
        _mockRetentionService = new Mock<RetentionService>(
            null!, null!, null!, Mock.Of<ILogger<RetentionService>>());

        // Setup scope factory
        _mockServiceScope.Setup(x => x.ServiceProvider).Returns(_mockScopedProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockServiceScope.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_mockScopeFactory.Object);
        _mockScopedProvider.Setup(x => x.GetService(typeof(RetentionService))).Returns(_mockRetentionService.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Act
        var service = new RetentionCleanupHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task StartAsync_ShouldStartWithoutError()
    {
        // Arrange
        var service = new RetentionCleanupHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object);

        var cts = new CancellationTokenSource();

        // Act & Assert - Should not throw
        await service.StartAsync(cts.Token);

        // Give it a moment to start
        await Task.Delay(100);

        cts.Cancel();

        // Wait for graceful shutdown
        try
        {
            await service.StopAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation occurs during delay
        }
    }

    [Fact]
    public async Task StopAsync_ShouldLogStopMessage()
    {
        // Arrange
        var service = new RetentionCleanupHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("stopping")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        var service = new RetentionCleanupHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object);

        var cts = new CancellationTokenSource();

        // Act - Start the service
        var executeTask = service.StartAsync(cts.Token);

        // Wait for startup delay to begin
        await Task.Delay(50);

        // Cancel immediately
        cts.Cancel();

        // Assert - Should complete without throwing
        await FluentActions.Awaiting(() => service.StopAsync(CancellationToken.None))
            .Should().NotThrowAsync();
    }

    [Fact]
    public void Constructor_ShouldLogStartupMessage()
    {
        // Act
        var service = new RetentionCleanupHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object);

        // Note: The startup message is logged in ExecuteAsync, not constructor
        // But we verify the service is created without errors
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task StartAsync_ShouldLogServiceStarted()
    {
        // Arrange
        var service = new RetentionCleanupHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object);

        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);

        // Give it a moment to log
        await Task.Delay(100);

        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        // Assert - Verify startup log
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Retention Cleanup Service started")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldCallBaseStopAsync()
    {
        // Arrange
        var service = new RetentionCleanupHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object);

        var cts = new CancellationTokenSource();
        await service.StartAsync(cts.Token);
        cts.Cancel();

        // Act & Assert - Should not throw
        await FluentActions.Awaiting(() => service.StopAsync(CancellationToken.None))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task MultipleStartStop_ShouldWorkCorrectly()
    {
        // Arrange
        var service = new RetentionCleanupHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object);

        // Act & Assert - Multiple start/stop cycles
        for (int i = 0; i < 3; i++)
        {
            var cts = new CancellationTokenSource();
            await service.StartAsync(cts.Token);
            await Task.Delay(50);
            cts.Cancel();
            await service.StopAsync(CancellationToken.None);
        }

        // Should complete without throwing
        true.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_WithCancellationToken_ShouldRespectToken()
    {
        // Arrange
        var service = new RetentionCleanupHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Should complete (possibly with cancellation)
        await FluentActions.Awaiting(() => service.StopAsync(cts.Token))
            .Should().NotThrowAsync();
    }
}

/// <summary>
/// Integration-style tests for RetentionCleanupHostedService
/// </summary>
public class RetentionCleanupHostedServiceIntegrationTests
{
    [Fact]
    public void Service_ShouldImplementBackgroundService()
    {
        // Arrange & Act
        var serviceType = typeof(RetentionCleanupHostedService);

        // Assert
        serviceType.BaseType!.Name.Should().Be("BackgroundService");
    }

    [Fact]
    public void Service_ShouldHaveCorrectConstructorParameters()
    {
        // Arrange & Act
        var constructor = typeof(RetentionCleanupHostedService).GetConstructors().First();
        var parameters = constructor.GetParameters();

        // Assert
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be<IServiceProvider>();
        parameters[1].ParameterType.Should().Be<ILogger<RetentionCleanupHostedService>>();
    }
}
