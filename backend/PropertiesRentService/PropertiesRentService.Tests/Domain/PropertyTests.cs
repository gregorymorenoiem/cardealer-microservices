using FluentAssertions;
using PropertiesRentService.Domain.Entities;
using Xunit;

namespace PropertiesRentService.Tests.Domain;

public class PropertyTests
{
    [Fact]
    public void Property_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var property = new Property();

        // Assert
        property.Id.Should().Be(Guid.Empty);
        property.IsDeleted.Should().BeFalse();
        property.Status.Should().Be(PropertyStatus.Draft);
        property.Currency.Should().Be("USD");
        property.Images.Should().NotBeNull();
        property.Images.Should().BeEmpty();
    }

    [Fact]
    public void Property_WithRequiredFields_ShouldBeValid()
    {
        // Arrange & Act
        var property = new Property
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Title = "Spacious 2BR Apartment for Rent",
            StreetAddress = "456 Ocean Drive",
            City = "Miami Beach",
            State = "FL",
            ZipCode = "33139",
            PricePerMonth = 2500.00m,
            Bedrooms = 2,
            Bathrooms = 2,
            SquareFeet = 1200,
            PropertyType = PropertyType.Condo,
            Status = PropertyStatus.Active
        };

        // Assert
        property.Id.Should().NotBe(Guid.Empty);
        property.DealerId.Should().NotBe(Guid.Empty);
        property.Title.Should().NotBeNullOrEmpty();
        property.PricePerMonth.Should().Be(2500.00m);
        property.Bedrooms.Should().Be(2);
        property.Bathrooms.Should().Be(2);
        property.SquareFeet.Should().Be(1200);
    }

    [Theory]
    [InlineData(PropertyStatus.Draft)]
    [InlineData(PropertyStatus.Active)]
    [InlineData(PropertyStatus.Pending)]
    [InlineData(PropertyStatus.Rented)]
    [InlineData(PropertyStatus.Inactive)]
    public void Property_AllStatuses_ShouldBeValid(PropertyStatus status)
    {
        // Arrange
        var property = new Property { Status = status };

        // Assert
        property.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(PropertyType.Apartment)]
    [InlineData(PropertyType.Condo)]
    [InlineData(PropertyType.House)]
    [InlineData(PropertyType.Townhouse)]
    [InlineData(PropertyType.Studio)]
    [InlineData(PropertyType.Room)]
    public void Property_AllTypes_ShouldBeValid(PropertyType type)
    {
        // Arrange
        var property = new Property { PropertyType = type };

        // Assert
        property.PropertyType.Should().Be(type);
    }

    [Fact]
    public void Property_RentalPricing_ShouldSupportMultiplePeriods()
    {
        // Arrange & Act
        var property = new Property
        {
            PricePerMonth = 2500.00m,
            PricePerWeek = 750.00m,
            SecurityDeposit = 2500.00m,
            ApplicationFee = 50.00m
        };

        // Assert
        property.PricePerMonth.Should().Be(2500.00m);
        property.PricePerWeek.Should().Be(750.00m);
        property.SecurityDeposit.Should().Be(2500.00m);
        property.ApplicationFee.Should().Be(50.00m);
    }

    [Fact]
    public void Property_AvailabilityPeriod_ShouldBeSetable()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date;

        // Act
        var property = new Property
        {
            AvailableFrom = startDate,
            MinLeaseTerm = 12,
            MaxLeaseTerm = 24
        };

        // Assert
        property.AvailableFrom.Should().Be(startDate);
        property.MinLeaseTerm.Should().Be(12);
        property.MaxLeaseTerm.Should().Be(24);
    }

    [Fact]
    public void Property_Images_CanBeAdded()
    {
        // Arrange
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Test Rental Property"
        };

        var image = new PropertyImage
        {
            Id = Guid.NewGuid(),
            PropertyId = property.Id,
            Url = "https://example.com/image.jpg",
            IsPrimary = true
        };

        // Act
        property.Images.Add(image);

        // Assert
        property.Images.Should().HaveCount(1);
        property.Images.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void Property_SoftDelete_ShouldSetIsDeletedFlag()
    {
        // Arrange
        var property = new Property
        {
            Id = Guid.NewGuid(),
            IsDeleted = false
        };

        // Act
        property.IsDeleted = true;
        property.DeletedAt = DateTime.UtcNow;

        // Assert
        property.IsDeleted.Should().BeTrue();
        property.DeletedAt.Should().NotBeNull();
        property.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Property_Location_ShouldStoreCoordinates()
    {
        // Arrange & Act
        var property = new Property
        {
            City = "Miami Beach",
            State = "FL",
            ZipCode = "33139",
            Latitude = 25.7907m,
            Longitude = -80.1300m
        };

        // Assert
        property.City.Should().Be("Miami Beach");
        property.State.Should().Be("FL");
        property.Latitude.Should().BeApproximately(25.7907m, 0.0001m);
        property.Longitude.Should().BeApproximately(-80.1300m, 0.0001m);
    }

    [Fact]
    public void Property_RentalPolicies_ShouldStorePetAndSmokingInfo()
    {
        // Arrange & Act
        var property = new Property
        {
            AllowsPets = true,
            PetDeposit = 500.00m,
            AllowsSmoking = false,
            UtilitiesIncluded = true
        };

        // Assert
        property.AllowsPets.Should().BeTrue();
        property.PetDeposit.Should().Be(500.00m);
        property.AllowsSmoking.Should().BeFalse();
        property.UtilitiesIncluded.Should().BeTrue();
    }

    [Fact]
    public void Property_Amenities_ShouldStoreDetails()
    {
        // Arrange & Act
        var property = new Property
        {
            HasParking = true,
            ParkingSpaces = 1,
            HasLaundry = true,
            HasAirConditioning = true,
            HasHeating = true,
            IsFurnished = false
        };

        // Assert
        property.HasParking.Should().BeTrue();
        property.ParkingSpaces.Should().Be(1);
        property.HasLaundry.Should().BeTrue();
        property.HasAirConditioning.Should().BeTrue();
        property.HasHeating.Should().BeTrue();
        property.IsFurnished.Should().BeFalse();
    }
}
