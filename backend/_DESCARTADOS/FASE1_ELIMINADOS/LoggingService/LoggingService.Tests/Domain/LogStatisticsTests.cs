using LoggingService.Domain;
using FluentAssertions;

namespace LoggingService.Tests.Domain;

public class LogStatisticsTests
{
    [Fact]
    public void GetErrorRate_WhenNoLogs_ReturnsZero()
    {
        // Arrange
        var statistics = new LogStatistics
        {
            TotalLogs = 0,
            ErrorCount = 0,
            CriticalCount = 0
        };

        // Act
        var errorRate = statistics.GetErrorRate();

        // Assert
        errorRate.Should().Be(0);
    }

    [Fact]
    public void GetErrorRate_CalculatesCorrectPercentage()
    {
        // Arrange
        var statistics = new LogStatistics
        {
            TotalLogs = 100,
            ErrorCount = 10,
            CriticalCount = 5
        };

        // Act
        var errorRate = statistics.GetErrorRate();

        // Assert
        errorRate.Should().Be(15.0); // (10 + 5) / 100 * 100 = 15%
    }

    [Fact]
    public void GetMostActiveService_ReturnsServiceWithMostLogs()
    {
        // Arrange
        var statistics = new LogStatistics
        {
            LogsByService = new Dictionary<string, int>
            {
                { "AuthService", 100 },
                { "MediaService", 250 },
                { "ErrorService", 50 }
            }
        };

        // Act
        var mostActive = statistics.GetMostActiveService();

        // Assert
        mostActive.Should().Be("MediaService");
    }

    [Fact]
    public void GetMostActiveService_WhenNoLogs_ReturnsNone()
    {
        // Arrange
        var statistics = new LogStatistics
        {
            LogsByService = new Dictionary<string, int>()
        };

        // Act
        var mostActive = statistics.GetMostActiveService();

        // Assert
        mostActive.Should().Be("None");
    }

    [Fact]
    public void GetLogSpan_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var oldestLog = DateTime.UtcNow.AddHours(-5);
        var newestLog = DateTime.UtcNow;
        var statistics = new LogStatistics
        {
            OldestLog = oldestLog,
            NewestLog = newestLog
        };

        // Act
        var span = statistics.GetLogSpan();

        // Assert
        span?.TotalHours.Should().BeApproximately(5, 0.1);
    }

    [Fact]
    public void GetLogSpan_WhenNoLogs_ReturnsNull()
    {
        // Arrange
        var statistics = new LogStatistics
        {
            OldestLog = null,
            NewestLog = null
        };

        // Act
        var span = statistics.GetLogSpan();

        // Assert
        span.Should().BeNull();
    }
}
