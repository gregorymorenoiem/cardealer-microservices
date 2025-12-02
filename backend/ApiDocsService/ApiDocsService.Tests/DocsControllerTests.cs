using ApiDocsService.Api.Controllers;
using ApiDocsService.Core.Interfaces;
using ApiDocsService.Core.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiDocsService.Tests;

public class DocsControllerTests
{
    private readonly Mock<IApiAggregatorService> _aggregatorMock;
    private readonly Mock<ILogger<DocsController>> _loggerMock;
    private readonly DocsController _controller;

    public DocsControllerTests()
    {
        _aggregatorMock = new Mock<IApiAggregatorService>();
        _loggerMock = new Mock<ILogger<DocsController>>();
        _controller = new DocsController(_aggregatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnDashboard()
    {
        // Arrange
        var dashboard = new ApiDocsDashboard
        {
            TotalServices = 5,
            HealthyServices = 4,
            UnhealthyServices = 1
        };
        _aggregatorMock.Setup(a => a.GetDashboardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dashboard);

        // Act
        var result = await _controller.GetDashboard(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDashboard = okResult.Value.Should().BeOfType<ApiDocsDashboard>().Subject;
        returnedDashboard.TotalServices.Should().Be(5);
    }

    [Fact]
    public async Task GetAllServices_ShouldReturnServicesList()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new() { Name = "Service1", DisplayName = "Service 1" },
            new() { Name = "Service2", DisplayName = "Service 2" }
        };
        _aggregatorMock.Setup(a => a.GetAllServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _controller.GetAllServices(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedServices = okResult.Value.Should().BeAssignableTo<List<ServiceInfo>>().Subject;
        returnedServices.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetService_WithValidName_ShouldReturnService()
    {
        // Arrange
        var service = new ServiceInfo { Name = "TestService", DisplayName = "Test Service" };
        _aggregatorMock.Setup(a => a.GetServiceByNameAsync("TestService", It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _controller.GetService("TestService", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedService = okResult.Value.Should().BeOfType<ServiceInfo>().Subject;
        returnedService.Name.Should().Be("TestService");
    }

    [Fact]
    public async Task GetService_WithInvalidName_ShouldReturnNotFound()
    {
        // Arrange
        _aggregatorMock.Setup(a => a.GetServiceByNameAsync("NonExistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceInfo?)null);

        // Act
        var result = await _controller.GetService("NonExistent", CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CheckServiceHealth_ShouldReturnStatus()
    {
        // Arrange
        var status = new ServiceStatus
        {
            ServiceName = "TestService",
            IsAvailable = true,
            HealthStatus = "Healthy"
        };
        _aggregatorMock.Setup(a => a.CheckServiceHealthAsync("TestService", It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

        // Act
        var result = await _controller.CheckServiceHealth("TestService", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStatus = okResult.Value.Should().BeOfType<ServiceStatus>().Subject;
        returnedStatus.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task SearchEndpoints_WithValidQuery_ShouldReturnResults()
    {
        // Arrange
        var endpoints = new List<ApiEndpointInfo>
        {
            new() { Path = "/api/users", Method = "GET", Summary = "Get all users" }
        };
        _aggregatorMock.Setup(a => a.SearchEndpointsAsync("users", It.IsAny<CancellationToken>()))
            .ReturnsAsync(endpoints);

        // Act
        var result = await _controller.SearchEndpoints("users", CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedEndpoints = okResult.Value.Should().BeAssignableTo<List<ApiEndpointInfo>>().Subject;
        returnedEndpoints.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchEndpoints_WithEmptyQuery_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.SearchEndpoints("", CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetCategories_ShouldReturnDistinctCategories()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new() { Name = "Service1", Category = "Core" },
            new() { Name = "Service2", Category = "Security" },
            new() { Name = "Service3", Category = "Core" }
        };
        _aggregatorMock.Setup(a => a.GetAllServicesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _controller.GetCategories(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var categories = okResult.Value.Should().BeAssignableTo<List<string>>().Subject;
        categories.Should().HaveCount(2);
        categories.Should().Contain("Core");
        categories.Should().Contain("Security");
    }
}
