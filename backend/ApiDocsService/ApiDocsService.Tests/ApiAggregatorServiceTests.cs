using ApiDocsService.Core.Interfaces;
using ApiDocsService.Core.Models;
using ApiDocsService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace ApiDocsService.Tests;

public class ApiAggregatorServiceTests
{
    private readonly Mock<ILogger<ApiAggregatorService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IApiAggregatorService _service;

    public ApiAggregatorServiceTests()
    {
        _loggerMock = new Mock<ILogger<ApiAggregatorService>>();
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpHandlerMock.Object);

        var servicesConfig = new ServicesConfiguration
        {
            Services = new List<ServiceInfo>
            {
                new()
                {
                    Name = "TestService",
                    DisplayName = "Test Service",
                    Description = "Test description",
                    BaseUrl = "http://testservice",
                    SwaggerUrl = "http://testservice/swagger/v1/swagger.json",
                    Version = "v1",
                    Category = "Testing",
                    Tags = new List<string> { "test" }
                },
                new()
                {
                    Name = "AnotherService",
                    DisplayName = "Another Service",
                    Description = "Another test service",
                    BaseUrl = "http://anotherservice",
                    SwaggerUrl = "http://anotherservice/swagger/v1/swagger.json",
                    Version = "v1",
                    Category = "Core",
                    Tags = new List<string> { "core", "essential" }
                }
            }
        };

        var options = Options.Create(servicesConfig);
        _service = new ApiAggregatorService(_httpClient, options, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllServicesAsync_ShouldReturnAllServices()
    {
        // Act
        var services = await _service.GetAllServicesAsync();

        // Assert
        services.Should().HaveCount(2);
        services.Should().Contain(s => s.Name == "TestService");
        services.Should().Contain(s => s.Name == "AnotherService");
    }

    [Fact]
    public async Task GetServiceByNameAsync_WithValidName_ShouldReturnService()
    {
        // Act
        var service = await _service.GetServiceByNameAsync("TestService");

        // Assert
        service.Should().NotBeNull();
        service!.Name.Should().Be("TestService");
        service.DisplayName.Should().Be("Test Service");
    }

    [Fact]
    public async Task GetServiceByNameAsync_WithInvalidName_ShouldReturnNull()
    {
        // Act
        var service = await _service.GetServiceByNameAsync("NonExistentService");

        // Assert
        service.Should().BeNull();
    }

    [Fact]
    public async Task GetServicesByCategoryAsync_ShouldReturnFilteredServices()
    {
        // Act
        var services = await _service.GetServicesByCategoryAsync("Testing");

        // Assert
        services.Should().HaveCount(1);
        services[0].Name.Should().Be("TestService");
    }

    [Fact]
    public async Task CheckServiceHealthAsync_WhenServiceHealthy_ShouldReturnAvailable()
    {
        // Arrange
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var status = await _service.CheckServiceHealthAsync("TestService");

        // Assert
        status.IsAvailable.Should().BeTrue();
        status.ServiceName.Should().Be("TestService");
        status.HealthStatus.Should().Be("Healthy");
    }

    [Fact]
    public async Task CheckServiceHealthAsync_WhenServiceUnhealthy_ShouldReturnUnavailable()
    {
        // Arrange
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        // Act
        var status = await _service.CheckServiceHealthAsync("TestService");

        // Assert
        status.IsAvailable.Should().BeFalse();
        status.HealthStatus.Should().Be("Unhealthy");
    }

    [Fact]
    public async Task CheckServiceHealthAsync_WhenServiceNotFound_ShouldReturnError()
    {
        // Act
        var status = await _service.CheckServiceHealthAsync("NonExistentService");

        // Assert
        status.IsAvailable.Should().BeFalse();
        status.ErrorMessage.Should().Be("Service not registered");
    }

    [Fact]
    public async Task GetOpenApiSpecAsync_WhenSuccessful_ShouldReturnSpec()
    {
        // Arrange
        var expectedSpec = @"{""openapi"":""3.0.1"",""info"":{""title"":""Test API""}}";
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedSpec)
            });

        // Act
        var spec = await _service.GetOpenApiSpecAsync("TestService");

        // Assert
        spec.Should().Be(expectedSpec);
    }

    [Fact]
    public async Task GetOpenApiSpecAsync_WhenServiceNotFound_ShouldReturnNull()
    {
        // Act
        var spec = await _service.GetOpenApiSpecAsync("NonExistentService");

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnCorrectStats()
    {
        // Arrange
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var dashboard = await _service.GetDashboardAsync();

        // Assert
        dashboard.TotalServices.Should().Be(2);
        dashboard.HealthyServices.Should().BeGreaterOrEqualTo(0);
        dashboard.UnhealthyServices.Should().BeGreaterOrEqualTo(0);
        dashboard.ByCategory.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CheckAllServicesHealthAsync_ShouldReturnStatusForAllServices()
    {
        // Arrange
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var statuses = await _service.CheckAllServicesHealthAsync();

        // Assert
        statuses.Should().HaveCount(2);
        statuses.Should().OnlyContain(s => s.ServiceName != null);
    }
}
