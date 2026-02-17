using LoggingService.Domain;
using FluentAssertions;

namespace LoggingService.Tests.Domain;

public class LogEntryTests
{
    [Fact]
    public void IsCritical_WhenLevelIsCritical_ReturnsTrue()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            Level = LogLevel.Critical,
            Message = "Critical error"
        };

        // Act
        var result = logEntry.IsCritical();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsError_WhenLevelIsError_ReturnsTrue()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            Level = LogLevel.Error,
            Message = "Error occurred"
        };

        // Act
        var result = logEntry.IsError();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsError_WhenLevelIsCritical_ReturnsTrue()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            Level = LogLevel.Critical,
            Message = "Critical error"
        };

        // Act
        var result = logEntry.IsError();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasException_WhenExceptionIsNotNull_ReturnsTrue()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            Exception = "System.Exception: Test exception"
        };

        // Act
        var result = logEntry.HasException();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasCorrelationId_WhenRequestIdExists_ReturnsTrue()
    {
        // Arrange
        var logEntry = new LogEntry
        {
            RequestId = "req-123"
        };

        // Act
        var result = logEntry.HasCorrelationId();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetAge_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var timestamp = DateTime.UtcNow.AddHours(-2);
        var logEntry = new LogEntry
        {
            Timestamp = timestamp
        };

        // Act
        var age = logEntry.GetAge();

        // Assert
        age.TotalHours.Should().BeApproximately(2, 0.1);
    }
}
