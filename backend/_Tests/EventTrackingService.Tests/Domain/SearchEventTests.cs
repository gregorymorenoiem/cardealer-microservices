using EventTrackingService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EventTrackingService.Tests.Domain;

public class SearchEventTests
{
    [Fact]
    public void SearchEvent_ShouldBeCreated_WithSearchQuery()
    {
        // Arrange & Act
        var evt = new SearchEvent
        {
            Id = Guid.NewGuid(),
            EventType = "Search",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/search",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            SearchQuery = "Toyota Corolla 2020",
            ResultsCount = 15,
            SearchType = "General"
        };

        // Assert
        evt.SearchQuery.Should().Be("Toyota Corolla 2020");
        evt.ResultsCount.Should().Be(15);
    }

    [Fact]
    public void SearchEvent_RecordClick_ShouldSetPositionAndVehicleId()
    {
        // Arrange
        var evt = new SearchEvent
        {
            Id = Guid.NewGuid(),
            EventType = "Search",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/search",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            SearchQuery = "Honda Civic",
            ResultsCount = 10
        };
        var vehicleId = Guid.NewGuid();

        // Act
        evt.RecordClick(3, vehicleId);

        // Assert
        evt.ClickedPosition.Should().Be(3);
        evt.ClickedVehicleId.Should().Be(vehicleId);
    }

    [Fact]
    public void SearchEvent_IsSuccessful_ShouldReturnTrue_WhenHasResultsAndClicked()
    {
        // Arrange
        var evt = new SearchEvent
        {
            Id = Guid.NewGuid(),
            EventType = "Search",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/search",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            SearchQuery = "SUV",
            ResultsCount = 50,
            ClickedPosition = 1
        };

        // Act
        var result = evt.IsSuccessful();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SearchEvent_IsSuccessful_ShouldReturnFalse_WhenNoClicked()
    {
        // Arrange
        var evt = new SearchEvent
        {
            Id = Guid.NewGuid(),
            EventType = "Search",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/search",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            SearchQuery = "Rare Car",
            ResultsCount = 5,
            ClickedPosition = null
        };

        // Act
        var result = evt.IsSuccessful();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SearchEvent_IsZeroResults_ShouldReturnTrue_WhenResultsCountIsZero()
    {
        // Arrange
        var evt = new SearchEvent
        {
            Id = Guid.NewGuid(),
            EventType = "Search",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/search",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            SearchQuery = "NonExistent Car Model 9999",
            ResultsCount = 0
        };

        // Act
        var result = evt.IsZeroResults();

        // Assert
        result.Should().BeTrue();
    }
}
