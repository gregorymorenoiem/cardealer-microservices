using EventTrackingService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EventTrackingService.Tests.Domain;

public class FilterEventTests
{
    [Fact]
    public void FilterEvent_ShouldBeCreated_WithFilterDetails()
    {
        // Arrange & Act
        var evt = new FilterEvent
        {
            Id = Guid.NewGuid(),
            EventType = "Filter",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/search",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            FilterType = "Make",
            FilterValue = "Toyota",
            FilterOperator = "equals",
            ResultsAfterFilter = 45,
            PageContext = "/search"
        };

        // Assert
        evt.FilterType.Should().Be("Make");
        evt.FilterValue.Should().Be("Toyota");
        evt.ResultsAfterFilter.Should().Be(45);
    }

    [Fact]
    public void FilterEvent_IsZeroResults_ShouldReturnTrue_WhenNoResults()
    {
        // Arrange
        var evt = new FilterEvent
        {
            Id = Guid.NewGuid(),
            EventType = "Filter",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/search",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            FilterType = "Year",
            FilterValue = "1950",
            FilterOperator = "equals",
            ResultsAfterFilter = 0,
            PageContext = "/search"
        };

        // Act
        var result = evt.IsZeroResults();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FilterEvent_IsNarrowingFilter_ShouldReturnTrue_WhenReducesResultsMoreThan90Percent()
    {
        // Arrange
        var evt = new FilterEvent
        {
            Id = Guid.NewGuid(),
            EventType = "Filter",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/search",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            FilterType = "PriceRange",
            FilterValue = "3000000-5000000",
            FilterOperator = "between",
            ResultsAfterFilter = 5,
            PageContext = "/search"
        };

        // Act
        var result = evt.IsNarrowingFilter(100);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FilterEvent_IsNarrowingFilter_ShouldReturnFalse_WhenReducesResultsLessThan90Percent()
    {
        // Arrange
        var evt = new FilterEvent
        {
            Id = Guid.NewGuid(),
            EventType = "Filter",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/search",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            FilterType = "Color",
            FilterValue = "Black",
            FilterOperator = "equals",
            ResultsAfterFilter = 50,
            PageContext = "/search"
        };

        // Act
        var result = evt.IsNarrowingFilter(100);

        // Assert
        result.Should().BeFalse();
    }
}
