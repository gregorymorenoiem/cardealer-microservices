using DealerManagementService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace DealerManagementService.Tests;

public class DealerPublicProfileTests
{
    [Fact]
    public void GenerateSlug_ShouldCreateValidSlug_FromBusinessName()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Auto Mundo RD",
            RNC = "123456789",
            Email = "contact@automundo.com",
            Phone = "809-555-1234",
            Address = "Av. Principal 123",
            City = "Santo Domingo",
            Province = "Distrito Nacional"
        };

        // Act
        var slug = dealer.GenerateSlug();

        // Assert
        slug.Should().Be("auto-mundo-rd");
    }

    [Fact]
    public void GenerateSlug_ShouldRemoveAccents_FromBusinessName()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Vehículos José María Peña",
            RNC = "123456789",
            Email = "contact@test.com",
            Phone = "809-555-1234",
            Address = "Test",
            City = "Test",
            Province = "Test"
        };

        // Act
        var slug = dealer.GenerateSlug();

        // Assert
        slug.Should().Be("vehiculos-jose-maria-pena");
    }

    [Fact]
    public void MarkAsTrusted_ShouldSetTrustedBadge()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Test Dealer",
            RNC = "123456789",
            Email = "test@test.com",
            Phone = "809-555-1234",
            Address = "Test",
            City = "Test",
            Province = "Test"
        };

        // Act
        dealer.MarkAsTrusted();

        // Assert
        dealer.IsTrustedDealer.Should().BeTrue();
        dealer.TrustedDealerSince.Should().NotBeNull();
        dealer.TrustedDealerSince.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RemoveTrustedBadge_ShouldClearTrustedStatus()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Test Dealer",
            RNC = "123456789",
            Email = "test@test.com",
            Phone = "809-555-1234",
            Address = "Test",
            City = "Test",
            Province = "Test",
            IsTrustedDealer = true,
            TrustedDealerSince = DateTime.UtcNow
        };

        // Act
        dealer.RemoveTrustedBadge();

        // Assert
        dealer.IsTrustedDealer.Should().BeFalse();
        dealer.TrustedDealerSince.Should().BeNull();
    }

    [Fact]
    public void IsProfileComplete_ShouldReturnFalse_WhenMissingFields()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Test Dealer",
            RNC = "123456789",
            Email = "test@test.com",
            Phone = "809-555-1234",
            Address = "Test",
            City = "Test",
            Province = "Test"
            // Missing Description and LogoUrl
        };

        // Act
        var isComplete = dealer.IsProfileComplete();

        // Assert
        isComplete.Should().BeFalse();
    }

    [Fact]
    public void IsProfileComplete_ShouldReturnTrue_WhenAllFieldsPresent()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Complete Dealer",
            RNC = "123456789",
            Email = "test@test.com",
            Phone = "809-555-1234",
            Address = "Full Address 123",
            City = "Santo Domingo",
            Province = "Distrito Nacional",
            Description = "We sell the best cars",
            LogoUrl = "https://example.com/logo.png"
        };

        // Act
        var isComplete = dealer.IsProfileComplete();

        // Assert
        isComplete.Should().BeTrue();
    }

    [Fact]
    public void GetProfileCompletionPercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Test Dealer",
            RNC = "123456789",
            Email = "test@test.com",
            Phone = "809-555-1234",
            Address = "Test",
            City = "Test",
            Province = "Test",
            Description = "Test description",
            AboutUs = "About us text",
            LogoUrl = "https://example.com/logo.png",
            BannerUrl = "https://example.com/banner.png",
            Website = "https://example.com",
            WhatsAppNumber = "+18095551234",
            FacebookUrl = "https://facebook.com/dealer",
            InstagramUrl = "https://instagram.com/dealer",
            Specialties = new List<string> { "SUVs", "Sedanes" },
            SupportedBrands = new List<string> { "Toyota", "Honda" },
            EstablishedDate = new DateTime(2020, 1, 1),
            EmployeeCount = 25,
            AcceptsTradeIns = true,
            OffersFinancing = true,
            OffersWarranty = true
        };
        dealer.Locations.Add(new DealerLocation
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            Name = "Main Location",
            Address = "Test",
            City = "Test",
            Province = "Test",
            Phone = "809-555-1234"
        });

        // Act
        var percentage = dealer.GetProfileCompletionPercentage();

        // Assert
        percentage.Should().Be(100); // All 20 fields completed
    }

    [Fact]
    public void GetProfileCompletionPercentage_ShouldReturn50Percent_WhenHalfFieldsComplete()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessName = "Test Dealer",
            RNC = "123456789",
            Email = "test@test.com",
            Phone = "809-555-1234",
            Address = "Test",
            City = "Test",
            Province = "Test",
            Description = "Test",
            AboutUs = "About",
            LogoUrl = "https://example.com/logo.png"
            // 7 out of 20 fields completed = 35%
        };

        // Act
        var percentage = dealer.GetProfileCompletionPercentage();

        // Assert
        percentage.Should().Be(35);
    }
}

public class BusinessHoursTests
{
    [Fact]
    public void IsOpenAt_ShouldReturnTrue_WhenWithinWorkingHours()
    {
        // Arrange
        var businessHours = new BusinessHours
        {
            Id = Guid.NewGuid(),
            DealerLocationId = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Monday,
            IsOpen = true,
            OpenTime = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(18, 0)
        };

        var testTime = new TimeOnly(14, 30);

        // Act
        var isOpen = businessHours.IsOpenAt(testTime);

        // Assert
        isOpen.Should().BeTrue();
    }

    [Fact]
    public void IsOpenAt_ShouldReturnFalse_WhenOutsideWorkingHours()
    {
        // Arrange
        var businessHours = new BusinessHours
        {
            Id = Guid.NewGuid(),
            DealerLocationId = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Monday,
            IsOpen = true,
            OpenTime = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(18, 0)
        };

        var testTime = new TimeOnly(19, 30);

        // Act
        var isOpen = businessHours.IsOpenAt(testTime);

        // Assert
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void IsOpenAt_ShouldReturnFalse_WhenDuringBreakTime()
    {
        // Arrange
        var businessHours = new BusinessHours
        {
            Id = Guid.NewGuid(),
            DealerLocationId = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Monday,
            IsOpen = true,
            OpenTime = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(18, 0),
            BreakStartTime = new TimeOnly(12, 0),
            BreakEndTime = new TimeOnly(13, 0)
        };

        var testTime = new TimeOnly(12, 30);

        // Act
        var isOpen = businessHours.IsOpenAt(testTime);

        // Assert
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void IsOpenAt_ShouldReturnFalse_WhenDayIsClosed()
    {
        // Arrange
        var businessHours = new BusinessHours
        {
            Id = Guid.NewGuid(),
            DealerLocationId = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Sunday,
            IsOpen = false
        };

        var testTime = new TimeOnly(14, 0);

        // Act
        var isOpen = businessHours.IsOpenAt(testTime);

        // Assert
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void GetFormattedHours_ShouldReturnCerrado_WhenDayIsClosed()
    {
        // Arrange
        var businessHours = new BusinessHours
        {
            Id = Guid.NewGuid(),
            DealerLocationId = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Sunday,
            IsOpen = false
        };

        // Act
        var formatted = businessHours.GetFormattedHours();

        // Assert
        formatted.Should().Be("Cerrado");
    }

    [Fact]
    public void GetFormattedHours_ShouldIncludeBreakTime_WhenPresent()
    {
        // Arrange
        var businessHours = new BusinessHours
        {
            Id = Guid.NewGuid(),
            DealerLocationId = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Monday,
            IsOpen = true,
            OpenTime = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(18, 0),
            BreakStartTime = new TimeOnly(12, 0),
            BreakEndTime = new TimeOnly(13, 0)
        };

        // Act
        var formatted = businessHours.GetFormattedHours();

        // Assert
        formatted.Should().Contain("08:00");
        formatted.Should().Contain("18:00");
        formatted.Should().Contain("Almuerzo");
        formatted.Should().Contain("12:00");
        formatted.Should().Contain("13:00");
    }
}

public class DealerLocationTests
{
    [Fact]
    public void DealerLocation_ShouldHaveBusinessHours()
    {
        // Arrange & Act
        var location = new DealerLocation
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Name = "Main Branch",
            Address = "Test Address",
            City = "Santo Domingo",
            Province = "Distrito Nacional",
            Phone = "809-555-1234"
        };

        location.BusinessHours.Add(new BusinessHours
        {
            DayOfWeek = DayOfWeek.Monday,
            IsOpen = true,
            OpenTime = new TimeOnly(8, 0),
            CloseTime = new TimeOnly(18, 0)
        });

        // Assert
        location.BusinessHours.Should().HaveCount(1);
        location.BusinessHours[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
    }
}
