using LoggingService.Domain;
using FluentAssertions;

namespace LoggingService.Tests.Domain;

public class LogFilterTests
{
    [Fact]
    public void IsValid_WhenEndDateBeforeStartDate_ReturnsFalse()
    {
        // Arrange
        var filter = new LogFilter
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(-1)
        };

        // Act
        var result = filter.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenPageSizeExceedsMax_ReturnsFalse()
    {
        // Arrange
        var filter = new LogFilter
        {
            PageSize = 1001
        };

        // Act
        var result = filter.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenPageNumberIsZero_ReturnsFalse()
    {
        // Arrange
        var filter = new LogFilter
        {
            PageNumber = 0
        };

        // Act
        var result = filter.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenAllParametersAreValid_ReturnsTrue()
    {
        // Arrange
        var filter = new LogFilter
        {
            StartDate = DateTime.UtcNow.AddHours(-1),
            EndDate = DateTime.UtcNow,
            PageSize = 100,
            PageNumber = 1
        };

        // Act
        var result = filter.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetSkip_ReturnsCorrectOffset()
    {
        // Arrange
        var filter = new LogFilter
        {
            PageNumber = 3,
            PageSize = 50
        };

        // Act
        var skip = filter.GetSkip();

        // Assert
        skip.Should().Be(100); // (3 - 1) * 50 = 100
    }
}
