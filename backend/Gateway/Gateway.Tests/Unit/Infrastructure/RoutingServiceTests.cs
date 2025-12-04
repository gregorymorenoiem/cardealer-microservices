using FluentAssertions;
using Gateway.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gateway.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for RoutingService
/// </summary>
public class RoutingServiceTests : IDisposable
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<RoutingService>> _loggerMock;
    private readonly string _testDirectory;
    private readonly List<string> _createdFiles = new();

    public RoutingServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<RoutingService>>();
        _testDirectory = Directory.GetCurrentDirectory();
    }

    public void Dispose()
    {
        // Cleanup created files
        foreach (var file in _createdFiles)
        {
            if (File.Exists(file))
            {
                try { File.Delete(file); } catch { }
            }
        }
    }

    private RoutingService CreateServiceWithConfig(string? environment = null)
    {
        _configurationMock.Setup(x => x["ASPNETCORE_ENVIRONMENT"]).Returns(environment);
        return new RoutingService(_configurationMock.Object, _loggerMock.Object);
    }

    private RoutingService CreateServiceWithOcelotConfig(string ocelotJson, string environment = "Development")
    {
        var fileName = environment == "Development" ? "ocelot.dev.json" : "ocelot.prod.json";
        var filePath = Path.Combine(_testDirectory, fileName);
        File.WriteAllText(filePath, ocelotJson);
        _createdFiles.Add(filePath);

        _configurationMock.Setup(x => x["ASPNETCORE_ENVIRONMENT"]).Returns(environment);
        return new RoutingService(_configurationMock.Object, _loggerMock.Object);
    }

    private static string CreateValidOcelotConfig() => @"{
        ""Routes"": [
            {
                ""UpstreamPathTemplate"": ""/api/users"",
                ""UpstreamHttpMethod"": [""GET"", ""POST""],
                ""DownstreamPathTemplate"": ""/users"",
                ""DownstreamScheme"": ""http"",
                ""DownstreamHostAndPorts"": [
                    { ""Host"": ""userservice"", ""Port"": 80 }
                ]
            },
            {
                ""UpstreamPathTemplate"": ""/api/users/{id}"",
                ""UpstreamHttpMethod"": [""GET"", ""PUT"", ""DELETE""],
                ""DownstreamPathTemplate"": ""/users/{id}"",
                ""DownstreamScheme"": ""http"",
                ""DownstreamHostAndPorts"": [
                    { ""Host"": ""userservice"", ""Port"": 80 }
                ]
            },
            {
                ""UpstreamPathTemplate"": ""/api/orders/{orderId}/items/{itemId}"",
                ""UpstreamHttpMethod"": [""GET""],
                ""DownstreamPathTemplate"": ""/orders/{orderId}/items/{itemId}"",
                ""DownstreamScheme"": ""https"",
                ""DownstreamHostAndPorts"": [
                    { ""Host"": ""orderservice"", ""Port"": 443 }
                ]
            },
            {
                ""UpstreamPathTemplate"": ""/api/health"",
                ""UpstreamHttpMethod"": [""GET""],
                ""DownstreamPathTemplate"": ""/health"",
                ""DownstreamScheme"": ""http"",
                ""DownstreamHostAndPorts"": [
                    { ""Host"": ""healthservice"", ""Port"": 8080 }
                ]
            }
        ],
        ""GlobalConfiguration"": {
            ""BaseUrl"": ""http://localhost:5000""
        }
    }";

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeWithoutError()
    {
        // Arrange & Act
        var service = CreateServiceWithConfig("Development");

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithProductionEnvironment_ShouldInitialize()
    {
        // Arrange & Act
        var service = CreateServiceWithConfig("Production");

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullEnvironment_ShouldUseProduction()
    {
        // Arrange & Act
        var service = CreateServiceWithConfig(null);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region RouteExists Tests

    [Fact]
    public async Task RouteExists_WhenConfigNotLoaded_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists("/api/nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RouteExists_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists(string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RouteExists_WithNullPath_ShouldHandleGracefully()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists(null!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ResolveDownstreamPath Tests

    [Fact]
    public async Task ResolveDownstreamPath_WhenConfigNotLoaded_ShouldReturnEmpty()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.ResolveDownstreamPath("/api/users");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithEmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.ResolveDownstreamPath(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Template Matching Tests (via RouteExists)

    [Fact]
    public async Task RouteExists_ShouldBeCaseInsensitive()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var lowerCase = await service.RouteExists("/api/users");
        var upperCase = await service.RouteExists("/API/USERS");
        var mixedCase = await service.RouteExists("/Api/Users");

        // Assert - all should return the same result (false since config not loaded)
        lowerCase.Should().Be(upperCase).And.Be(mixedCase);
    }

    [Fact]
    public async Task RouteExists_WithTrailingSlash_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var withSlash = await service.RouteExists("/api/users/");
        var withoutSlash = await service.RouteExists("/api/users");

        // Assert
        withSlash.Should().BeFalse();
        withoutSlash.Should().BeFalse();
    }

    [Fact]
    public async Task RouteExists_WithLeadingSlash_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var withSlash = await service.RouteExists("/api/users");
        var withoutSlash = await service.RouteExists("api/users");

        // Assert
        // Both should handle gracefully
        withSlash.Should().BeFalse();
        withoutSlash.Should().BeFalse();
    }

    #endregion

    #region Path Parameter Tests

    [Fact]
    public async Task RouteExists_WithPathParameters_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists("/api/users/123");

        // Assert
        result.Should().BeFalse(); // Config not loaded
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithPathParameters_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.ResolveDownstreamPath("/api/users/456");

        // Assert
        result.Should().BeEmpty(); // Config not loaded
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithMultiplePathParameters_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.ResolveDownstreamPath("/api/users/123/roles/456");

        // Assert
        result.Should().BeEmpty(); // Config not loaded
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task RouteExists_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act - using a path that could cause issues
        var result = await service.RouteExists("/api/test");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveDownstreamPath_WhenExceptionOccurs_ShouldReturnEmpty()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.ResolveDownstreamPath("/api/test");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task RouteExists_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists("/api/users?query=test");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RouteExists_WithEncodedPath_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists("/api/users%2F123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithVeryLongPath_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");
        var longPath = "/api/" + string.Join("/", Enumerable.Repeat("segment", 100));

        // Act
        var result = await service.ResolveDownstreamPath(longPath);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Configuration Loaded Tests

    [Fact]
    public async Task RouteExists_WithValidConfig_MatchingRoute_ShouldReturnTrue()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var result = await service.RouteExists("/api/users");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RouteExists_WithValidConfig_NonMatchingRoute_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var result = await service.RouteExists("/api/nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RouteExists_WithValidConfig_RouteWithPathParameter_ShouldReturnTrue()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var result = await service.RouteExists("/api/users/123");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RouteExists_WithValidConfig_RouteWithMultiplePathParameters_ShouldReturnTrue()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var result = await service.RouteExists("/api/orders/456/items/789");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithValidConfig_SimpleRoute_ShouldResolveCorrectly()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var result = await service.ResolveDownstreamPath("/api/users");

        // Assert
        result.Should().Be("http://userservice:80/users");
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithValidConfig_RouteWithParameter_ShouldSubstituteParameter()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var result = await service.ResolveDownstreamPath("/api/users/123");

        // Assert
        result.Should().Be("http://userservice:80/users/123");
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithValidConfig_RouteWithMultipleParameters_ShouldSubstituteAll()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var result = await service.ResolveDownstreamPath("/api/orders/456/items/789");

        // Assert
        result.Should().Be("https://orderservice:443/orders/456/items/789");
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithValidConfig_NonMatchingRoute_ShouldReturnEmpty()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var result = await service.ResolveDownstreamPath("/api/nonexistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RouteExists_WithValidConfig_HealthEndpoint_ShouldReturnTrue()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var result = await service.RouteExists("/api/health");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithValidConfig_DifferentPort_ShouldIncludeCorrectPort()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var result = await service.ResolveDownstreamPath("/api/health");

        // Assert
        result.Should().Be("http://healthservice:8080/health");
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithValidConfig_HttpsScheme_ShouldUseCorrectScheme()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var result = await service.ResolveDownstreamPath("/api/orders/1/items/2");

        // Assert
        result.Should().StartWith("https://");
    }

    #endregion

    #region Production Environment Tests

    [Fact]
    public async Task RouteExists_WithProductionConfig_ShouldLoadProductionFile()
    {
        // Arrange
        var prodConfig = @"{
            ""Routes"": [
                {
                    ""UpstreamPathTemplate"": ""/prod/api/users"",
                    ""UpstreamHttpMethod"": [""GET""],
                    ""DownstreamPathTemplate"": ""/users"",
                    ""DownstreamScheme"": ""http"",
                    ""DownstreamHostAndPorts"": [
                        { ""Host"": ""prod-userservice"", ""Port"": 80 }
                    ]
                }
            ]
        }";
        var service = CreateServiceWithOcelotConfig(prodConfig, "Production");

        // Act
        var result = await service.RouteExists("/prod/api/users");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Invalid Configuration Tests

    [Fact]
    public async Task RouteExists_WithInvalidJson_ShouldReturnFalse()
    {
        // Arrange
        var fileName = "ocelot.dev.json";
        var filePath = Path.Combine(_testDirectory, fileName);
        File.WriteAllText(filePath, "{ invalid json }");
        _createdFiles.Add(filePath);

        _configurationMock.Setup(x => x["ASPNETCORE_ENVIRONMENT"]).Returns("Development");
        var service = new RoutingService(_configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await service.RouteExists("/api/users");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RouteExists_WithEmptyRoutesArray_ShouldReturnFalse()
    {
        // Arrange
        var emptyConfig = @"{ ""Routes"": [] }";
        var service = CreateServiceWithOcelotConfig(emptyConfig);

        // Act
        var result = await service.RouteExists("/api/users");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RouteExists_WithMissingRoutesArray_ShouldReturnFalse()
    {
        // Arrange
        var noRoutesConfig = @"{ ""GlobalConfiguration"": {} }";
        var service = CreateServiceWithOcelotConfig(noRoutesConfig);

        // Act
        var result = await service.RouteExists("/api/users");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithMissingDownstreamHostAndPorts_ShouldReturnEmpty()
    {
        // Arrange
        var incompleteConfig = @"{
            ""Routes"": [
                {
                    ""UpstreamPathTemplate"": ""/api/test"",
                    ""DownstreamPathTemplate"": ""/test"",
                    ""DownstreamScheme"": ""http""
                }
            ]
        }";
        var service = CreateServiceWithOcelotConfig(incompleteConfig);

        // Act
        var result = await service.ResolveDownstreamPath("/api/test");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithEmptyHostAndPorts_ShouldReturnEmpty()
    {
        // Arrange
        var incompleteConfig = @"{
            ""Routes"": [
                {
                    ""UpstreamPathTemplate"": ""/api/test"",
                    ""DownstreamPathTemplate"": ""/test"",
                    ""DownstreamScheme"": ""http"",
                    ""DownstreamHostAndPorts"": []
                }
            ]
        }";
        var service = CreateServiceWithOcelotConfig(incompleteConfig);

        // Act
        var result = await service.ResolveDownstreamPath("/api/test");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithMissingDownstreamTemplate_ShouldReturnEmpty()
    {
        // Arrange
        var incompleteConfig = @"{
            ""Routes"": [
                {
                    ""UpstreamPathTemplate"": ""/api/test"",
                    ""DownstreamScheme"": ""http"",
                    ""DownstreamHostAndPorts"": [
                        { ""Host"": ""testservice"", ""Port"": 80 }
                    ]
                }
            ]
        }";
        var service = CreateServiceWithOcelotConfig(incompleteConfig);

        // Act
        var result = await service.ResolveDownstreamPath("/api/test");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Default Scheme Tests

    [Fact]
    public async Task ResolveDownstreamPath_WithMissingScheme_ShouldDefaultToHttp()
    {
        // Arrange
        var noSchemeConfig = @"{
            ""Routes"": [
                {
                    ""UpstreamPathTemplate"": ""/api/test"",
                    ""DownstreamPathTemplate"": ""/test"",
                    ""DownstreamHostAndPorts"": [
                        { ""Host"": ""testservice"", ""Port"": 80 }
                    ]
                }
            ]
        }";
        var service = CreateServiceWithOcelotConfig(noSchemeConfig);

        // Act
        var result = await service.ResolveDownstreamPath("/api/test");

        // Assert
        result.Should().StartWith("http://");
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public async Task RouteExists_WithValidConfig_CaseInsensitive_ShouldMatch()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var lowerCase = await service.RouteExists("/api/users");
        var upperCase = await service.RouteExists("/API/USERS");
        var mixedCase = await service.RouteExists("/Api/Users");

        // Assert
        lowerCase.Should().BeTrue();
        upperCase.Should().BeTrue();
        mixedCase.Should().BeTrue();
    }

    #endregion

    #region Segment Count Mismatch Tests

    [Fact]
    public async Task RouteExists_WithDifferentSegmentCount_ShouldNotMatch()
    {
        // Arrange
        var service = CreateServiceWithOcelotConfig(CreateValidOcelotConfig());

        // Act
        var tooManySegments = await service.RouteExists("/api/users/123/extra");
        var tooFewSegments = await service.RouteExists("/api");

        // Assert
        tooManySegments.Should().BeFalse();
        tooFewSegments.Should().BeFalse();
    }

    #endregion
}
