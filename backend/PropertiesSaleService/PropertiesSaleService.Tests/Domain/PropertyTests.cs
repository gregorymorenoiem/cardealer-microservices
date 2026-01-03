using FluentAssertions;
using PropertiesSaleService.Domain.Entities;
using Xunit;

namespace PropertiesSaleService.Tests.Domain;

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
            Title = "Beautiful 3BR Home in Miami",
            StreetAddress = "123 Main Street",
            City = "Miami",
            State = "FL",
            ZipCode = "33101",
            Price = 450000.00m,
            Bedrooms = 3,
            Bathrooms = 2.5m,
            SquareFeet = 2200,
            PropertyType = PropertyType.SingleFamily,
            Status = PropertyStatus.Active
        };

        // Assert
        property.Id.Should().NotBe(Guid.Empty);
        property.DealerId.Should().NotBe(Guid.Empty);
        property.Title.Should().NotBeNullOrEmpty();
        property.Price.Should().Be(450000.00m);
        property.Bedrooms.Should().Be(3);
        property.Bathrooms.Should().Be(2.5m);
        property.SquareFeet.Should().Be(2200);
    }

    [Theory]
    [InlineData(PropertyStatus.Draft)]
    [InlineData(PropertyStatus.Active)]
    [InlineData(PropertyStatus.Pending)]
    [InlineData(PropertyStatus.Sold)]
    [InlineData(PropertyStatus.Inactive)]
    [InlineData(PropertyStatus.UnderContract)]
    public void Property_AllStatuses_ShouldBeValid(PropertyStatus status)
    {
        // Arrange
        var property = new Property { Status = status };

        // Assert
        property.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(PropertyType.SingleFamily)]
    [InlineData(PropertyType.Condo)]
    [InlineData(PropertyType.Townhouse)]
    [InlineData(PropertyType.MultiFamily)]
    [InlineData(PropertyType.Land)]
    [InlineData(PropertyType.Commercial)]
    public void Property_AllTypes_ShouldBeValid(PropertyType type)
    {
        // Arrange
        var property = new Property { PropertyType = type };

        // Assert
        property.PropertyType.Should().Be(type);
    }

    [Fact]
    public void Property_Images_CanBeAdded()
    {
        // Arrange
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Test Property"
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
    public void Property_Category_CanBeAssigned()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Residential",
            Slug = "residential"
        };

        var property = new Property
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Category = category
        };

        // Assert
        property.CategoryId.Should().Be(category.Id);
        property.Category.Should().NotBeNull();
        property.Category!.Name.Should().Be("Residential");
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
            City = "Miami",
            State = "FL",
            ZipCode = "33101",
            Latitude = 25.7617m,
            Longitude = -80.1918m
        };

        // Assert
        property.City.Should().Be("Miami");
        property.State.Should().Be("FL");
        property.Latitude.Should().BeApproximately(25.7617m, 0.0001m);
        property.Longitude.Should().BeApproximately(-80.1918m, 0.0001m);
    }

    [Fact]
    public void Property_Features_ShouldStoreAmenities()
    {
        // Arrange & Act
        var property = new Property
        {
            HasPool = true,
            HasGarage = true,
            GarageSpaces = 2,
            HasBasement = false,
            HasFireplace = true,
            LotSize = 0.25m,
            YearBuilt = 2020
        };

        // Assert
        property.HasPool.Should().BeTrue();
        property.HasGarage.Should().BeTrue();
        property.GarageSpaces.Should().Be(2);
        property.HasBasement.Should().BeFalse();
        property.HasFireplace.Should().BeTrue();
        property.LotSize.Should().Be(0.25m);
        property.YearBuilt.Should().Be(2020);
    }

    [Fact]
    public void Property_MLSNumber_ShouldBeStorable()
    {
        // Arrange & Act
        var property = new Property
        {
            MLSNumber = "MLS-12345678"
        };

        // Assert
        property.MLSNumber.Should().Be("MLS-12345678");
    }

    [Fact]
    public void Property_HOA_ShouldStoreDetails()
    {
        // Arrange & Act
        var property = new Property
        {
            HasHOA = true,
            HOAFee = 350.00m,
            HOAFrequency = "Monthly"
        };

        // Assert
        property.HasHOA.Should().BeTrue();
        property.HOAFee.Should().Be(350.00m);
        property.HOAFrequency.Should().Be("Monthly");
    }
}
