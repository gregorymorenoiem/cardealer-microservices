using ApiDocsService.Core.Interfaces;
using ApiDocsService.Core.Models;
using ApiDocsService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiDocsService.Tests;

public class VersionServiceTests
{
    private readonly Mock<IApiAggregatorService> _aggregatorMock;
    private readonly Mock<ILogger<VersionService>> _loggerMock;
    private readonly IVersionService _versionService;

    public VersionServiceTests()
    {
        _aggregatorMock = new Mock<IApiAggregatorService>();
        _loggerMock = new Mock<ILogger<VersionService>>();
        _versionService = new VersionService(_aggregatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllVersionedServicesAsync_ShouldReturnServices()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new()
            {
                Name = "TestService",
                DisplayName = "Test Service",
                Description = "Test description",
                Version = "v1"
            }
        };

        _aggregatorMock.Setup(x => x.GetAllServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _versionService.GetAllVersionedServicesAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result[0].ServiceName.Should().Be("TestService");
        result[0].Versions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetServiceVersionsAsync_WithValidService_ShouldReturnVersionInfo()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new()
            {
                Name = "TestService",
                DisplayName = "Test Service",
                Description = "Test description",
                Version = "v1"
            }
        };

        _aggregatorMock.Setup(x => x.GetAllServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _versionService.GetServiceVersionsAsync("TestService");

        // Assert
        result.Should().NotBeNull();
        result!.ServiceName.Should().Be("TestService");
        result.CurrentVersion.Should().Be("v1");
        result.Versions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetServiceVersionsAsync_WithInvalidService_ShouldReturnNull()
    {
        // Arrange
        var services = new List<ServiceInfo>();
        _aggregatorMock.Setup(x => x.GetAllServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _versionService.GetServiceVersionsAsync("NonExistentService");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDeprecatedApisAsync_ShouldReturnOnlyDeprecated()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new()
            {
                Name = "Service1",
                DisplayName = "Service 1",
                Description = "Test",
                Version = "v1"
            },
            new()
            {
                Name = "Service2",
                DisplayName = "Service 2",
                Description = "Test",
                Version = "v2"
            }
        };

        _aggregatorMock.Setup(x => x.GetAllServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _versionService.GetDeprecatedApisAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty(); // No deprecated APIs initially
    }

    [Fact]
    public async Task IsVersionDeprecatedAsync_WithNonDeprecatedVersion_ShouldReturnFalse()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new()
            {
                Name = "TestService",
                DisplayName = "Test Service",
                Description = "Test",
                Version = "v1"
            }
        };

        _aggregatorMock.Setup(x => x.GetAllServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _versionService.IsVersionDeprecatedAsync("TestService", "v1");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetMigrationPathAsync_WithValidVersions_ShouldReturnPath()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new()
            {
                Name = "TestService",
                DisplayName = "Test Service",
                Description = "Test",
                Version = "v1"
            }
        };

        _aggregatorMock.Setup(x => x.GetAllServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _versionService.GetMigrationPathAsync("TestService", "v1", "v1");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Should().Be("v1");
    }

    [Fact]
    public async Task CompareVersionsAsync_WithInvalidService_ShouldReturnNull()
    {
        // Arrange
        var services = new List<ServiceInfo>();
        _aggregatorMock.Setup(x => x.GetAllServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _versionService.CompareVersionsAsync("NonExistent", "v1", "v2");

        // Assert
        result.Should().BeNull();
    }
}
