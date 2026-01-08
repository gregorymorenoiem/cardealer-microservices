using DealerAnalyticsService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace DealerAnalyticsService.Tests;

public class AnalyticsEntitiesTests
{
    // ============================================
    // ProfileView Tests
    // ============================================

    [Fact]
    public void ProfileView_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var view = new ProfileView();

        // Assert
        view.Id.Should().NotBeEmpty();
        view.ViewedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ProfileView_IsDuplicateView_ShouldReturnTrue_WhenSameIPWithin30Minutes()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var view = new ProfileView
        {
            ViewerIpAddress = ipAddress,
            ViewedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        // Act
        var isDuplicate = view.IsDuplicateView(ipAddress, 30);

        // Assert
        isDuplicate.Should().BeTrue();
    }

    [Fact]
    public void ProfileView_IsDuplicateView_ShouldReturnFalse_WhenDifferentIP()
    {
        // Arrange
        var view = new ProfileView
        {
            ViewerIpAddress = "192.168.1.1",
            ViewedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        // Act
        var isDuplicate = view.IsDuplicateView("192.168.1.2", 30);

        // Assert
        isDuplicate.Should().BeFalse();
    }

    [Fact]
    public void ProfileView_IsBounce_ShouldReturnTrue_WhenDurationLessThan10Seconds()
    {
        // Arrange
        var view = new ProfileView
        {
            DurationSeconds = 5
        };

        // Act
        var isBounce = view.IsBounce();

        // Assert
        isBounce.Should().BeTrue();
    }

    [Fact]
    public void ProfileView_IsBounce_ShouldReturnFalse_WhenDurationMoreThan10Seconds()
    {
        // Arrange
        var view = new ProfileView
        {
            DurationSeconds = 45
        };

        // Act
        var isBounce = view.IsBounce();

        // Assert
        isBounce.Should().BeFalse();
    }

    [Fact]
    public void ProfileView_IsEngagedVisit_ShouldReturnTrue_WhenDurationMoreThan2Minutes()
    {
        // Arrange
        var view = new ProfileView
        {
            DurationSeconds = 150 // 2.5 minutes
        };

        // Act
        var isEngaged = view.IsEngagedVisit();

        // Assert
        isEngaged.Should().BeTrue();
    }

    [Fact]
    public void ProfileView_IsEngagedVisit_ShouldReturnFalse_WhenDurationLessThan2Minutes()
    {
        // Arrange
        var view = new ProfileView
        {
            DurationSeconds = 60 // 1 minute
        };

        // Act
        var isEngaged = view.IsEngagedVisit();

        // Assert
        isEngaged.Should().BeFalse();
    }

    // ============================================
    // ContactEvent Tests
    // ============================================

    [Fact]
    public void ContactEvent_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var contactEvent = new ContactEvent();

        // Assert
        contactEvent.Id.Should().NotBeEmpty();
        contactEvent.ClickedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        contactEvent.ConvertedToInquiry.Should().BeFalse();
    }

    [Fact]
    public void ContactEvent_MarkAsConverted_ShouldSetConversionFields()
    {
        // Arrange
        var contactEvent = new ContactEvent();

        // Act
        contactEvent.MarkAsConverted();

        // Assert
        contactEvent.ConvertedToInquiry.Should().BeTrue();
        contactEvent.ConversionDate.Should().NotBeNull();
        contactEvent.ConversionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ContactEvent_GetTimeToConversion_ShouldReturnNull_WhenNotConverted()
    {
        // Arrange
        var contactEvent = new ContactEvent();

        // Act
        var timeToConversion = contactEvent.GetTimeToConversion();

        // Assert
        timeToConversion.Should().BeNull();
    }

    [Fact]
    public void ContactEvent_GetTimeToConversion_ShouldReturnTimeSpan_WhenConverted()
    {
        // Arrange
        var contactEvent = new ContactEvent
        {
            ClickedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        contactEvent.MarkAsConverted();

        // Act
        var timeToConversion = contactEvent.GetTimeToConversion();

        // Assert
        timeToConversion.Should().NotBeNull();
        timeToConversion.Value.TotalMinutes.Should().BeApproximately(10, 0.1);
    }

    [Fact]
    public void ContactEvent_IsQuickConversion_ShouldReturnTrue_WhenConvertedWithin30Minutes()
    {
        // Arrange
        var contactEvent = new ContactEvent
        {
            ClickedAt = DateTime.UtcNow.AddMinutes(-15)
        };
        contactEvent.MarkAsConverted();

        // Act
        var isQuick = contactEvent.IsQuickConversion(30);

        // Assert
        isQuick.Should().BeTrue();
    }

    [Fact]
    public void ContactEvent_IsQuickConversion_ShouldReturnFalse_WhenConvertedAfter30Minutes()
    {
        // Arrange
        var contactEvent = new ContactEvent
        {
            ClickedAt = DateTime.UtcNow.AddMinutes(-45)
        };
        contactEvent.MarkAsConverted();

        // Act
        var isQuick = contactEvent.IsQuickConversion(30);

        // Assert
        isQuick.Should().BeFalse();
    }

    // ============================================
    // DailyAnalyticsSummary Tests
    // ============================================

    [Fact]
    public void DailyAnalyticsSummary_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var summary = new DailyAnalyticsSummary();

        // Assert
        summary.Id.Should().NotBeEmpty();
        summary.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        summary.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void DailyAnalyticsSummary_GetBounceRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new DailyAnalyticsSummary
        {
            TotalViews = 100,
            BounceCount = 25
        };

        // Act
        var bounceRate = summary.GetBounceRate();

        // Assert
        bounceRate.Should().Be(25.0);
    }

    [Fact]
    public void DailyAnalyticsSummary_GetBounceRate_ShouldReturn0_WhenNoViews()
    {
        // Arrange
        var summary = new DailyAnalyticsSummary
        {
            TotalViews = 0,
            BounceCount = 0
        };

        // Act
        var bounceRate = summary.GetBounceRate();

        // Assert
        bounceRate.Should().Be(0);
    }

    [Fact]
    public void DailyAnalyticsSummary_GetEngagementRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new DailyAnalyticsSummary
        {
            TotalViews = 100,
            EngagedVisits = 40
        };

        // Act
        var engagementRate = summary.GetEngagementRate();

        // Assert
        engagementRate.Should().Be(40.0);
    }

    [Fact]
    public void DailyAnalyticsSummary_GetContactConversionRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new DailyAnalyticsSummary
        {
            TotalViews = 200,
            TotalContacts = 30
        };

        // Act
        var conversionRate = summary.GetContactConversionRate();

        // Assert
        conversionRate.Should().Be(15.0);
    }

    [Fact]
    public void DailyAnalyticsSummary_GetInquiryConversionRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var summary = new DailyAnalyticsSummary
        {
            TotalContacts = 50,
            ConvertedInquiries = 10
        };

        // Act
        var inquiryRate = summary.GetInquiryConversionRate();

        // Assert
        inquiryRate.Should().Be(20.0);
    }

    [Fact]
    public void DailyAnalyticsSummary_GetTopContactMethod_ShouldReturnWhatsApp_WhenMostClicks()
    {
        // Arrange
        var summary = new DailyAnalyticsSummary
        {
            PhoneClicks = 10,
            EmailClicks = 5,
            WhatsAppClicks = 50,
            WebsiteClicks = 15
        };

        // Act
        var topMethod = summary.GetTopContactMethod();

        // Assert
        topMethod.Should().Be(ContactType.WhatsApp);
    }

    [Fact]
    public void DailyAnalyticsSummary_GetTopContactMethod_ShouldReturnPhone_WhenMostClicks()
    {
        // Arrange
        var summary = new DailyAnalyticsSummary
        {
            PhoneClicks = 60,
            EmailClicks = 5,
            WhatsAppClicks = 20,
            WebsiteClicks = 15
        };

        // Act
        var topMethod = summary.GetTopContactMethod();

        // Assert
        topMethod.Should().Be(ContactType.Phone);
    }

    [Fact]
    public void DailyAnalyticsSummary_IsToday_ShouldReturnTrue_WhenDateIsToday()
    {
        // Arrange
        var summary = new DailyAnalyticsSummary
        {
            Date = DateTime.UtcNow.Date
        };

        // Act
        var isToday = summary.IsToday();

        // Assert
        isToday.Should().BeTrue();
    }

    [Fact]
    public void DailyAnalyticsSummary_IsToday_ShouldReturnFalse_WhenDateIsNotToday()
    {
        // Arrange
        var summary = new DailyAnalyticsSummary
        {
            Date = DateTime.UtcNow.AddDays(-1).Date
        };

        // Act
        var isToday = summary.IsToday();

        // Assert
        isToday.Should().BeFalse();
    }

    [Fact]
    public void DailyAnalyticsSummary_Touch_ShouldUpdateTimestamp()
    {
        // Arrange
        var summary = new DailyAnalyticsSummary
        {
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
        var oldUpdatedAt = summary.UpdatedAt;

        // Act
        summary.Touch();

        // Assert
        summary.UpdatedAt.Should().BeAfter(oldUpdatedAt);
        summary.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    // ============================================
    // ContactType Enum Tests
    // ============================================

    [Fact]
    public void ContactType_ShouldHaveExpectedValues()
    {
        // Assert
        ((int)ContactType.Phone).Should().Be(1);
        ((int)ContactType.Email).Should().Be(2);
        ((int)ContactType.WhatsApp).Should().Be(3);
        ((int)ContactType.Website).Should().Be(4);
        ((int)ContactType.SocialMedia).Should().Be(5);
    }
}
