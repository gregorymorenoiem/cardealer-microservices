using FluentAssertions;
using RateLimitingService.Core.Models;

namespace RateLimitingService.Tests.Models;

public class RateLimitStatisticsTests
{
    [Fact]
    public void RateLimitStatistics_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var stats = new RateLimitStatistics();

        // Assert
        stats.TotalRequests.Should().Be(0);
        stats.AllowedRequests.Should().Be(0);
        stats.BlockedRequests.Should().Be(0);
        stats.ClientStats.Should().NotBeNull().And.BeEmpty();
        stats.EndpointStats.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(1000, 50, 5)]
    [InlineData(0, 0, 0)]
    [InlineData(100, 0, 0)]
    [InlineData(100, 100, 100)]
    public void BlockRate_ShouldCalculateCorrectly(long total, long blocked, double expectedRate)
    {
        // Arrange
        var stats = new RateLimitStatistics
        {
            TotalRequests = total,
            BlockedRequests = blocked
        };

        // Act & Assert
        stats.BlockRate.Should().Be(expectedRate);
    }

    [Fact]
    public void ClientStatistics_UsagePercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var clientStats = new ClientStatistics
        {
            ClientId = "test-client",
            CurrentUsage = 50,
            MaxAllowed = 100
        };

        // Assert
        clientStats.UsagePercentage.Should().Be(50);
    }

    [Fact]
    public void ClientStatistics_UsagePercentage_WhenMaxIsZero_ShouldReturnZero()
    {
        // Arrange
        var clientStats = new ClientStatistics
        {
            ClientId = "test-client",
            CurrentUsage = 50,
            MaxAllowed = 0
        };

        // Assert
        clientStats.UsagePercentage.Should().Be(0);
    }

    [Fact]
    public void EndpointStatistics_ShouldSetValuesCorrectly()
    {
        // Arrange
        var endpointStats = new EndpointStatistics
        {
            Endpoint = "/api/test",
            Method = "GET",
            TotalRequests = 1000,
            BlockedRequests = 50,
            AvgRequestsPerMinute = 16.67
        };

        // Assert
        endpointStats.Endpoint.Should().Be("/api/test");
        endpointStats.Method.Should().Be("GET");
        endpointStats.TotalRequests.Should().Be(1000);
        endpointStats.BlockedRequests.Should().Be(50);
        endpointStats.AvgRequestsPerMinute.Should().BeApproximately(16.67, 0.01);
    }
}
