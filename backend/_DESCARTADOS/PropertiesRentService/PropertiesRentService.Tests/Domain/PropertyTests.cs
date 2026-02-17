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
        property.PropertyType.Should().Be(PropertyType.House);
        property.PropertySubType.Should().Be(PropertySubType.SingleFamily);
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
            AgentId = Guid.NewGuid(),
            AgentName = "John Smith",
            Title = "Beautiful 3BR Home for Rent",
            StreetAddress = "123 Main Street",
            City = "Miami",
            State = "FL",
            ZipCode = "33101",
            Price = 2500.00m,
            Bedrooms = 3,
            Bathrooms = 2,
            SquareFeet = 1800,
            PropertyType = PropertyType.House,
            Status = PropertyStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        property.Id.Should().NotBe(Guid.Empty);
        property.DealerId.Should().NotBe(Guid.Empty);
        property.AgentId.Should().NotBe(Guid.Empty);
        property.Title.Should().NotBeNullOrEmpty();
        property.Price.Should().Be(2500.00m);
        property.Bedrooms.Should().Be(3);
        property.Bathrooms.Should().Be(2);
        property.SquareFeet.Should().Be(1800);
        property.AgentName.Should().Be("John Smith");
    }

    [Theory]
    [InlineData(PropertyStatus.Draft)]
    [InlineData(PropertyStatus.PendingReview)]
    [InlineData(PropertyStatus.Active)]
    [InlineData(PropertyStatus.UnderContract)]
    [InlineData(PropertyStatus.Pending)]
    [InlineData(PropertyStatus.Sold)]
    [InlineData(PropertyStatus.Closed)]
    [InlineData(PropertyStatus.Withdrawn)]
    [InlineData(PropertyStatus.Expired)]
    [InlineData(PropertyStatus.Archived)]
    public void Property_AllStatuses_ShouldBeValid(PropertyStatus status)
    {
        // Arrange
        var property = new Property { Status = status };

        // Assert
        property.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(PropertyType.House)]
    [InlineData(PropertyType.Condo)]
    [InlineData(PropertyType.Townhouse)]
    [InlineData(PropertyType.MultiFamily)]
    [InlineData(PropertyType.Apartment)]
    [InlineData(PropertyType.Land)]
    [InlineData(PropertyType.Commercial)]
    [InlineData(PropertyType.Industrial)]
    [InlineData(PropertyType.Farm)]
    [InlineData(PropertyType.MobileHome)]
    public void Property_AllTypes_ShouldBeValid(PropertyType type)
    {
        // Arrange
        var property = new Property { PropertyType = type };

        // Assert
        property.PropertyType.Should().Be(type);
    }

    [Theory]
    [InlineData(PropertySubType.SingleFamily)]
    [InlineData(PropertySubType.Duplex)]
    [InlineData(PropertySubType.Triplex)]
    [InlineData(PropertySubType.Fourplex)]
    [InlineData(PropertySubType.Condo)]
    [InlineData(PropertySubType.Loft)]
    [InlineData(PropertySubType.Penthouse)]
    [InlineData(PropertySubType.Studio)]
    public void Property_AllSubTypes_ShouldBeValid(PropertySubType subType)
    {
        // Arrange
        var property = new Property { PropertySubType = subType };

        // Assert
        property.PropertySubType.Should().Be(subType);
    }

    [Fact]
    public void Property_FinancialInfo_ShouldStoreValues()
    {
        // Arrange & Act
        var property = new Property
        {
            Price = 2500.00m,
            TaxesYearly = 8500.00m,
            HOAFeesMonthly = 350.00m,
            HOAName = "Sunset Community HOA",
            AssessedValue = 275000.00m
        };

        // Assert
        property.Price.Should().Be(2500.00m);
        property.TaxesYearly.Should().Be(8500.00m);
        property.HOAFeesMonthly.Should().Be(350.00m);
        property.HOAName.Should().Be("Sunset Community HOA");
        property.AssessedValue.Should().Be(275000.00m);
    }

    [Fact]
    public void Property_Images_CanBeAdded()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        var property = new Property
        {
            Id = propertyId,
            DealerId = dealerId,
            Title = "Test Rental Property"
        };

        var image = new PropertyImage
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            PropertyId = propertyId,
            Url = "https://example.com/image.jpg",
            ThumbnailUrl = "https://example.com/image_thumb.jpg",
            Title = "Front View",
            IsPrimary = true,
            ImageType = PropertyImageType.Exterior,
            SortOrder = 0
        };

        // Act
        property.Images.Add(image);

        // Assert
        property.Images.Should().HaveCount(1);
        property.Images.First().IsPrimary.Should().BeTrue();
        property.Images.First().Title.Should().Be("Front View");
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

        // Assert
        property.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Property_Location_ShouldStoreCoordinates()
    {
        // Arrange & Act
        var property = new Property
        {
            StreetAddress = "456 Ocean Drive",
            City = "Miami Beach",
            State = "FL",
            ZipCode = "33139",
            County = "Miami-Dade",
            Latitude = 25.7907,
            Longitude = -80.1300,
            Neighborhood = "South Beach"
        };

        // Assert
        property.City.Should().Be("Miami Beach");
        property.State.Should().Be("FL");
        property.Latitude.Should().BeApproximately(25.7907, 0.0001);
        property.Longitude.Should().BeApproximately(-80.1300, 0.0001);
        property.County.Should().Be("Miami-Dade");
        property.Neighborhood.Should().Be("South Beach");
    }

    [Fact]
    public void Property_Amenities_ShouldStorePoolAndBasement()
    {
        // Arrange & Act
        var property = new Property
        {
            HasPool = true,
            PoolType = PoolType.InGround,
            HasSpa = true,
            HasFireplace = true,
            FireplaceCount = 2,
            HasBasement = true,
            BasementType = BasementType.Finished
        };

        // Assert
        property.HasPool.Should().BeTrue();
        property.PoolType.Should().Be(PoolType.InGround);
        property.HasSpa.Should().BeTrue();
        property.HasFireplace.Should().BeTrue();
        property.FireplaceCount.Should().Be(2);
        property.HasBasement.Should().BeTrue();
        property.BasementType.Should().Be(BasementType.Finished);
    }

    [Fact]
    public void Property_Parking_ShouldStoreDetails()
    {
        // Arrange & Act
        var property = new Property
        {
            GarageSpaces = 2,
            GarageType = GarageType.Attached,
            ParkingSpaces = 4,
            ParkingType = ParkingType.Driveway
        };

        // Assert
        property.GarageSpaces.Should().Be(2);
        property.GarageType.Should().Be(GarageType.Attached);
        property.ParkingSpaces.Should().Be(4);
        property.ParkingType.Should().Be(ParkingType.Driveway);
    }

    [Fact]
    public void Property_SizeAndRooms_ShouldStoreDetails()
    {
        // Arrange & Act
        var property = new Property
        {
            SquareFeet = 2500,
            LotSizeAcres = 0.25m,
            LotSizeSquareFeet = 10890,
            Stories = 2,
            YearBuilt = 2020,
            YearRenovated = 2023,
            Bedrooms = 4,
            Bathrooms = 3,
            HalfBathrooms = 1,
            RoomsTotal = 10
        };

        // Assert
        property.SquareFeet.Should().Be(2500);
        property.LotSizeAcres.Should().Be(0.25m);
        property.Stories.Should().Be(2);
        property.YearBuilt.Should().Be(2020);
        property.Bedrooms.Should().Be(4);
        property.Bathrooms.Should().Be(3);
        property.HalfBathrooms.Should().Be(1);
    }

    [Fact]
    public void Property_Systems_ShouldStoreDetails()
    {
        // Arrange & Act
        var property = new Property
        {
            HeatingType = HeatingType.Forced,
            CoolingType = CoolingType.Central,
            HeatingFuel = "Natural Gas",
            WaterSource = "Municipal",
            SewerType = "Municipal"
        };

        // Assert
        property.HeatingType.Should().Be(HeatingType.Forced);
        property.CoolingType.Should().Be(CoolingType.Central);
        property.HeatingFuel.Should().Be("Natural Gas");
        property.WaterSource.Should().Be("Municipal");
        property.SewerType.Should().Be("Municipal");
    }

    [Fact]
    public void Property_Schools_ShouldStoreDistrict()
    {
        // Arrange & Act
        var property = new Property
        {
            SchoolDistrict = "Miami-Dade County Public Schools",
            ElementarySchool = "Coral Gables Elementary",
            MiddleSchool = "Ponce de Leon Middle",
            HighSchool = "Coral Gables Senior High"
        };

        // Assert
        property.SchoolDistrict.Should().Be("Miami-Dade County Public Schools");
        property.ElementarySchool.Should().Be("Coral Gables Elementary");
        property.MiddleSchool.Should().Be("Ponce de Leon Middle");
        property.HighSchool.Should().Be("Coral Gables Senior High");
    }

    [Fact]
    public void Property_EngagementMetrics_ShouldStartAtZero()
    {
        // Arrange
        var property = new Property();

        // Assert
        property.ViewCount.Should().Be(0);
        property.SavedCount.Should().Be(0);
        property.InquiryCount.Should().Be(0);
        property.TourRequestCount.Should().Be(0);
    }
}
