using BackupDRService.Core.BackgroundServices;
using BackupDRService.Core.Entities;
using BackupDRService.Core.Models;
using BackupDRService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BackupDRService.Tests.BackgroundServices;

public class BackupSchedulerHostedServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<BackupSchedulerHostedService>> _mockLogger;
    private readonly Mock<IOptions<BackupOptions>> _mockOptions;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;

    public BackupSchedulerHostedServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<BackupSchedulerHostedService>>();
        _mockOptions = new Mock<IOptions<BackupOptions>>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();

        // Setup default options
        _mockOptions.Setup(x => x.Value).Returns(new BackupOptions
        {
            MaxConcurrentJobs = 2
        });

        // Setup scope factory
        _mockServiceScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockServiceScope.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_mockScopeFactory.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Act
        var service = new BackupSchedulerHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockOptions.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldLogInitialization()
    {
        // Act
        var service = new BackupSchedulerHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockOptions.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Backup Scheduler initialized")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_ShouldStartWithoutError()
    {
        // Arrange
        var service = new BackupSchedulerHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockOptions.Object);

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
        var service = new BackupSchedulerHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockOptions.Object);

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

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Constructor_WithDifferentConcurrencyLevels_ShouldInitialize(int maxConcurrent)
    {
        // Arrange
        _mockOptions.Setup(x => x.Value).Returns(new BackupOptions
        {
            MaxConcurrentJobs = maxConcurrent
        });

        // Act
        var service = new BackupSchedulerHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockOptions.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        var service = new BackupSchedulerHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockOptions.Object);

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
    public void MaxConcurrentBackups_ShouldBeConfigurable()
    {
        // Arrange
        var options = new BackupOptions { MaxConcurrentJobs = 8 };
        _mockOptions.Setup(x => x.Value).Returns(options);

        // Act
        var service = new BackupSchedulerHostedService(
            _mockServiceProvider.Object,
            _mockLogger.Object,
            _mockOptions.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("8")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

/// <summary>
/// Tests for BackupSchedulerHostedService internal methods using reflection
/// </summary>
public class BackupSchedulerHostedServiceInternalTests
{
    [Theory]
    [InlineData("full", BackupType.Full)]
    [InlineData("Full", BackupType.Full)]
    [InlineData("FULL", BackupType.Full)]
    [InlineData("incremental", BackupType.Incremental)]
    [InlineData("differential", BackupType.Differential)]
    [InlineData("unknown", BackupType.Full)] // Default
    public void ParseBackupType_ShouldParseCorrectly(string input, BackupType expected)
    {
        // Use reflection to test private static method
        var method = typeof(BackupSchedulerHostedService)
            .GetMethod("ParseBackupType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method.Should().NotBeNull();

        var result = method!.Invoke(null, new object[] { input });

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("local", StorageType.Local)]
    [InlineData("Local", StorageType.Local)]
    [InlineData("azureblob", StorageType.AzureBlob)]
    [InlineData("azure", StorageType.AzureBlob)]
    [InlineData("s3", StorageType.S3)]
    [InlineData("ftp", StorageType.Ftp)]
    [InlineData("unknown", StorageType.Local)] // Default
    public void ParseStorageType_ShouldParseCorrectly(string input, StorageType expected)
    {
        // Use reflection to test private static method
        var method = typeof(BackupSchedulerHostedService)
            .GetMethod("ParseStorageType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method.Should().NotBeNull();

        var result = method!.Invoke(null, new object[] { input });

        result.Should().Be(expected);
    }
}
