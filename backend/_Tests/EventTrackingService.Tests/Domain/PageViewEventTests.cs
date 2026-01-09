using EventTrackingService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EventTrackingService.Tests.Domain;

public class PageViewEventTests
{
    [Fact]
    public void PageViewEvent_ShouldBeCreated_WithAllProperties()
    {
        // Arrange & Act
        var evt = new PageViewEvent
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
            OperatingSystem = "Windows",
            PageUrl = "https://okla.com.do/vehicles",
            PageTitle = "Vehículos en Venta - OKLA",
            PreviousUrl = "https://okla.com.do",
            ScrollDepth = 75,
            TimeOnPage = 120
        };

        // Assert
        evt.PageUrl.Should().Be("https://okla.com.do/vehicles");
        evt.PageTitle.Should().Be("Vehículos en Venta - OKLA");
        evt.ScrollDepth.Should().Be(75);
        evt.TimeOnPage.Should().Be(120);
    }

    [Fact]
    public void PageViewEvent_MarkAsBounce_ShouldSetBounceAndExitFlags()
    {
        // Arrange
        var evt = new PageViewEvent
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
            PageUrl = "https://okla.com.do",
            PageTitle = "Home"
        };

        // Act
        evt.MarkAsBounce();

        // Assert
        evt.IsBounce.Should().BeTrue();
        evt.IsExit.Should().BeTrue();
    }

    [Fact]
    public void PageViewEvent_MarkAsExit_ShouldSetExitFlag()
    {
        // Arrange
        var evt = new PageViewEvent
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
            OperatingSystem = "Windows",
            PageUrl = "https://okla.com.do/vehicles",
            PageTitle = "Vehículos"
        };

        // Act
        evt.MarkAsExit();

        // Assert
        evt.IsExit.Should().BeTrue();
        evt.IsBounce.Should().BeFalse();
    }

    [Fact]
    public void PageViewEvent_SetTimeOnPage_ShouldMarkAsBounce_WhenLessThan10Seconds()
    {
        // Arrange
        var evt = new PageViewEvent
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
            PageUrl = "https://okla.com.do",
            PageTitle = "Home"
        };

        // Act
        evt.SetTimeOnPage(5);

        // Assert
        evt.TimeOnPage.Should().Be(5);
        evt.IsBounce.Should().BeTrue();
    }

    [Fact]
    public void PageViewEvent_SetTimeOnPage_ShouldNotMarkAsBounce_When10SecondsOrMore()
    {
        // Arrange
        var evt = new PageViewEvent
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
            PageUrl = "https://okla.com.do",
            PageTitle = "Home"
        };

        // Act
        evt.SetTimeOnPage(60);

        // Assert
        evt.TimeOnPage.Should().Be(60);
        evt.IsBounce.Should().BeFalse();
    }

    [Fact]
    public void PageViewEvent_IsEngaged_ShouldReturnTrue_WhenScrollDepthAbove50()
    {
        // Arrange
        var evt = new PageViewEvent
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
            OperatingSystem = "Windows",
            PageUrl = "https://okla.com.do/vehicles",
            PageTitle = "Vehículos",
            ScrollDepth = 75
        };

        // Act
        var result = evt.IsEngaged();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PageViewEvent_IsEngaged_ShouldReturnFalse_WhenScrollDepth50OrLess()
    {
        // Arrange
        var evt = new PageViewEvent
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
            PageUrl = "https://okla.com.do",
            PageTitle = "Home",
            ScrollDepth = 30
        };

        // Act
        var result = evt.IsEngaged();

        // Assert
        result.Should().BeFalse();
    }
}
