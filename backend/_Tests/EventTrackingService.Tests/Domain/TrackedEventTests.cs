using EventTrackingService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EventTrackingService.Tests.Domain;

public class TrackedEventTests
{
    [Fact]
    public void TrackedEvent_ShouldBeCreated_WithValidData()
    {
        // Arrange & Act
        var evt = new TrackedEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PageView",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do/vehicles",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows"
        };

        // Assert
        evt.Should().NotBeNull();
        evt.EventType.Should().Be("PageView");
        evt.SessionId.Should().Be("session-123");
    }

    [Fact]
    public void TrackedEvent_IsAuthenticated_ShouldReturnTrue_WhenUserIdExists()
    {
        // Arrange
        var evt = new TrackedEvent
        {
            Id = Guid.NewGuid(),
            EventType = "VehicleView",
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows"
        };

        // Act
        var result = evt.IsAuthenticated();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TrackedEvent_IsAuthenticated_ShouldReturnFalse_WhenUserIdIsNull()
    {
        // Arrange
        var evt = new TrackedEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PageView",
            UserId = null,
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows"
        };

        // Act
        var result = evt.IsAuthenticated();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TrackedEvent_IsMobile_ShouldReturnTrue_WhenDeviceTypeIsMobileOrTablet()
    {
        // Arrange
        var evt = new TrackedEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PageView",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do",
            DeviceType = "Mobile",
            Browser = "Safari",
            OperatingSystem = "iOS"
        };

        // Act
        var result = evt.IsMobile();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TrackedEvent_IsFromCampaign_ShouldReturnTrue_WhenCampaignExists()
    {
        // Arrange
        var evt = new TrackedEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PageView",
            Timestamp = DateTime.UtcNow,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows",
            Campaign = "summer_sale_2026",
            Source = "google",
            Medium = "cpc"
        };

        // Act
        var result = evt.IsFromCampaign();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TrackedEvent_GetAge_ShouldReturnCorrectTimeSpan()
    {
        // Arrange
        var pastTime = DateTime.UtcNow.AddMinutes(-30);
        var evt = new TrackedEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PageView",
            Timestamp = pastTime,
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows"
        };

        // Act
        var age = evt.GetAge();

        // Assert
        age.TotalMinutes.Should().BeGreaterThan(29);
        age.TotalMinutes.Should().BeLessThan(31);
    }

    [Fact]
    public void TrackedEvent_IsRecent_ShouldReturnTrue_WhenWithinLastHour()
    {
        // Arrange
        var evt = new TrackedEvent
        {
            Id = Guid.NewGuid(),
            EventType = "Search",
            Timestamp = DateTime.UtcNow.AddMinutes(-30),
            SessionId = "session-123",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            CurrentUrl = "https://okla.com.do",
            DeviceType = "Desktop",
            Browser = "Chrome",
            OperatingSystem = "Windows"
        };

        // Act
        var result = evt.IsRecent();

        // Assert
        result.Should().BeTrue();
    }
}
