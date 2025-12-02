using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RateLimitingService.Api.Controllers;
using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;

namespace RateLimitingService.Tests.Controllers;

public class RateLimitControllerTests
{
    private readonly Mock<IRateLimitingService> _mockService;
    private readonly Mock<ILogger<RateLimitController>> _mockLogger;
    private readonly RateLimitController _controller;

    public RateLimitControllerTests()
    {
        _mockService = new Mock<IRateLimitingService>();
        _mockLogger = new Mock<ILogger<RateLimitController>>();
        _controller = new RateLimitController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CheckRateLimit_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var result = RateLimitResult.Allowed("client1", "policy1", 99, DateTime.UtcNow.AddMinutes(1));
        _mockService.Setup(x => x.CheckRateLimitAsync("client1", "/api/test", null, default))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.CheckRateLimit("client1", "/api/test");

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)response.Result!;
        okResult.Value.Should().Be(result);
    }

    [Fact]
    public async Task CheckRateLimit_WithMissingClientId_ReturnsBadRequest()
    {
        // Act
        var response = await _controller.CheckRateLimit("", "/api/test");

        // Assert
        response.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CheckRateLimit_WithMissingEndpoint_ReturnsBadRequest()
    {
        // Act
        var response = await _controller.CheckRateLimit("client1", "");

        // Assert
        response.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetPolicies_ReturnsPolicies()
    {
        // Arrange
        var policies = new List<RateLimitPolicy>
        {
            new() { Id = "1", Name = "Policy 1" },
            new() { Id = "2", Name = "Policy 2" }
        };
        _mockService.Setup(x => x.GetAllPoliciesAsync(default))
            .ReturnsAsync(policies);

        // Act
        var response = await _controller.GetPolicies();

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)response.Result!;
        var returnedPolicies = okResult.Value as IEnumerable<RateLimitPolicy>;
        returnedPolicies.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPolicy_WhenExists_ReturnsPolicy()
    {
        // Arrange
        var policy = new RateLimitPolicy { Id = "1", Name = "Test Policy" };
        _mockService.Setup(x => x.GetPolicyAsync("1", default))
            .ReturnsAsync(policy);

        // Act
        var response = await _controller.GetPolicy("1");

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPolicy_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(x => x.GetPolicyAsync("999", default))
            .ReturnsAsync((RateLimitPolicy?)null);

        // Act
        var response = await _controller.GetPolicy("999");

        // Assert
        response.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreatePolicy_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreatePolicyRequest
        {
            Name = "New Policy",
            Description = "Test",
            MaxRequests = 100,
            WindowSeconds = 60
        };

        var createdPolicy = new RateLimitPolicy
        {
            Id = "new-id",
            Name = "New Policy"
        };

        _mockService.Setup(x => x.CreatePolicyAsync(It.IsAny<RateLimitPolicy>(), default))
            .ReturnsAsync(createdPolicy);

        // Act
        var response = await _controller.CreatePolicy(request);

        // Assert
        response.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)response.Result!;
        createdResult.ActionName.Should().Be(nameof(RateLimitController.GetPolicy));
    }

    [Fact]
    public async Task CreatePolicy_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var response = await _controller.CreatePolicy(null!);

        // Assert
        response.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdatePolicy_WhenExists_ReturnsOk()
    {
        // Arrange
        var existingPolicy = new RateLimitPolicy { Id = "1", Name = "Old Name" };
        var request = new UpdatePolicyRequest { Name = "New Name" };

        _mockService.Setup(x => x.GetPolicyAsync("1", default))
            .ReturnsAsync(existingPolicy);
        _mockService.Setup(x => x.UpdatePolicyAsync(It.IsAny<RateLimitPolicy>(), default))
            .ReturnsAsync(existingPolicy);

        // Act
        var response = await _controller.UpdatePolicy("1", request);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdatePolicy_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(x => x.GetPolicyAsync("999", default))
            .ReturnsAsync((RateLimitPolicy?)null);

        // Act
        var response = await _controller.UpdatePolicy("999", new UpdatePolicyRequest());

        // Assert
        response.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeletePolicy_WhenExists_ReturnsNoContent()
    {
        // Arrange
        _mockService.Setup(x => x.DeletePolicyAsync("1", default))
            .ReturnsAsync(true);

        // Act
        var response = await _controller.DeletePolicy("1");

        // Assert
        response.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeletePolicy_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(x => x.DeletePolicyAsync("999", default))
            .ReturnsAsync(false);

        // Act
        var response = await _controller.DeletePolicy("999");

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetStatistics_ReturnsStatistics()
    {
        // Arrange
        var stats = new RateLimitStatistics
        {
            TotalRequests = 1000,
            AllowedRequests = 950,
            BlockedRequests = 50
        };
        _mockService.Setup(x => x.GetStatisticsAsync(null, null, default))
            .ReturnsAsync(stats);

        // Act
        var response = await _controller.GetStatistics();

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)response.Result!;
        var returnedStats = okResult.Value as RateLimitStatistics;
        returnedStats!.TotalRequests.Should().Be(1000);
    }

    [Fact]
    public async Task GetClientUsage_WhenExists_ReturnsClientStats()
    {
        // Arrange
        var clientStats = new ClientStatistics
        {
            ClientId = "client1",
            CurrentUsage = 50,
            MaxAllowed = 100
        };
        _mockService.Setup(x => x.GetClientUsageAsync("client1", default))
            .ReturnsAsync(clientStats);

        // Act
        var response = await _controller.GetClientUsage("client1");

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetClientUsage_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(x => x.GetClientUsageAsync("unknown", default))
            .ReturnsAsync((ClientStatistics?)null);

        // Act
        var response = await _controller.GetClientUsage("unknown");

        // Assert
        response.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ResetClientLimit_WhenExists_ReturnsNoContent()
    {
        // Arrange
        _mockService.Setup(x => x.ResetClientLimitAsync("client1", default))
            .ReturnsAsync(true);

        // Act
        var response = await _controller.ResetClientLimit("client1");

        // Assert
        response.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ResetClientLimit_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(x => x.ResetClientLimitAsync("unknown", default))
            .ReturnsAsync(false);

        // Act
        var response = await _controller.ResetClientLimit("unknown");

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void GetTiers_ReturnsAllTiers()
    {
        // Act
        var response = _controller.GetTiers();

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)response.Result!;
        var tiers = okResult.Value as Dictionary<string, int>;
        tiers.Should().NotBeNull();
        tiers!.Should().ContainKey("free");
        tiers.Should().ContainKey("premium");
        tiers.Should().ContainKey("enterprise");
    }
}
