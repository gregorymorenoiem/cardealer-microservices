using FluentAssertions;
using RealEstateService.Domain.Entities;
using Xunit;

namespace RealEstateService.Tests;

public class PropertyTests
{
    [Fact]
    public void Property_ShouldCreateWithDefaultValues()
    {
        // Arrange & Act
        var property = new Property();

        // Assert
        property.Id.Should().Be(Guid.Empty);
        property.Status.Should().Be(PropertyStatus.Draft);
        property.Currency.Should().Be("USD");
        property.Country.Should().Be("México");
        property.IsDeleted.Should().BeFalse();
        property.Images.Should().BeEmpty();
        property.Amenities.Should().BeEmpty();
    }

    [Fact]
    public void Property_ShouldSetAllRequiredFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();

        // Act
        var property = new Property
        {
            Id = id,
            DealerId = dealerId,
            Title = "Beautiful House in Polanco",
            Description = "Spacious 3-bedroom house with garden",
            Type = PropertyType.House,
            ListingType = ListingType.Sale,
            Status = PropertyStatus.Active,
            Price = 5000000,
            Currency = "MXN",
            Address = "Av. Presidente Masaryk 123",
            City = "Ciudad de México",
            State = "CDMX",
            ZipCode = "11560",
            Neighborhood = "Polanco",
            TotalArea = 250,
            BuiltArea = 200,
            Bedrooms = 3,
            Bathrooms = 2,
            ParkingSpaces = 2,
            SellerId = sellerId,
            SellerName = "John Doe"
        };

        // Assert
        property.Id.Should().Be(id);
        property.DealerId.Should().Be(dealerId);
        property.Title.Should().Be("Beautiful House in Polanco");
        property.Type.Should().Be(PropertyType.House);
        property.ListingType.Should().Be(ListingType.Sale);
        property.Status.Should().Be(PropertyStatus.Active);
        property.Price.Should().Be(5000000);
        property.Bedrooms.Should().Be(3);
        property.Bathrooms.Should().Be(2);
    }

    [Theory]
    [InlineData(PropertyType.House, "House")]
    [InlineData(PropertyType.Apartment, "Apartment")]
    [InlineData(PropertyType.Condo, "Condo")]
    [InlineData(PropertyType.Land, "Land")]
    [InlineData(PropertyType.Commercial, "Commercial")]
    public void PropertyType_ShouldHaveCorrectStringRepresentation(PropertyType type, string expected)
    {
        type.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData(ListingType.Sale)]
    [InlineData(ListingType.Rent)]
    [InlineData(ListingType.SaleOrRent)]
    public void ListingType_ShouldBeValid(ListingType type)
    {
        var property = new Property { ListingType = type };
        property.ListingType.Should().Be(type);
    }

    [Theory]
    [InlineData(PropertyStatus.Draft)]
    [InlineData(PropertyStatus.Active)]
    [InlineData(PropertyStatus.Pending)]
    [InlineData(PropertyStatus.Sold)]
    [InlineData(PropertyStatus.Rented)]
    [InlineData(PropertyStatus.Reserved)]
    [InlineData(PropertyStatus.Archived)]
    public void PropertyStatus_ShouldBeValid(PropertyStatus status)
    {
        var property = new Property { Status = status };
        property.Status.Should().Be(status);
    }

    [Fact]
    public void Property_ShouldCalculatePricePerSquareMeter()
    {
        // Arrange
        var property = new Property
        {
            Price = 1000000,
            TotalArea = 100
        };

        // Act
        var pricePerSqMeter = property.TotalArea > 0
            ? Math.Round(property.Price / property.TotalArea, 2)
            : 0;

        // Assert
        pricePerSqMeter.Should().Be(10000);
    }

    [Fact]
    public void Property_ShouldHandleZeroArea()
    {
        // Arrange
        var property = new Property
        {
            Price = 1000000,
            TotalArea = 0
        };

        // Act
        decimal? pricePerSqMeter = property.TotalArea > 0
            ? Math.Round(property.Price / property.TotalArea, 2)
            : null;

        // Assert
        pricePerSqMeter.Should().BeNull();
    }

    [Fact]
    public void Property_ShouldSupportAmenities()
    {
        // Arrange
        var property = new Property
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid()
        };

        // Act
        property.HasPool = true;
        property.HasGarden = true;
        property.HasGym = true;
        property.HasSecurity = true;
        property.HasElevator = false;
        property.IsFurnished = false;
        property.AllowsPets = true;

        // Assert
        property.HasPool.Should().BeTrue();
        property.HasGarden.Should().BeTrue();
        property.HasGym.Should().BeTrue();
        property.HasSecurity.Should().BeTrue();
        property.HasElevator.Should().BeFalse();
        property.IsFurnished.Should().BeFalse();
        property.AllowsPets.Should().BeTrue();
    }

    [Fact]
    public void Property_ShouldSupportLocation()
    {
        // Arrange & Act
        var property = new Property
        {
            Address = "Calle Principal 100",
            City = "Guadalajara",
            State = "Jalisco",
            ZipCode = "44100",
            Country = "México",
            Neighborhood = "Chapultepec",
            Latitude = 20.6597,
            Longitude = -103.3496
        };

        // Assert
        property.City.Should().Be("Guadalajara");
        property.State.Should().Be("Jalisco");
        property.Latitude.Should().BeApproximately(20.6597, 0.0001);
        property.Longitude.Should().BeApproximately(-103.3496, 0.0001);
    }

    [Fact]
    public void Property_ShouldTrackMetrics()
    {
        // Arrange
        var property = new Property
        {
            ViewCount = 100,
            FavoriteCount = 25,
            InquiryCount = 10
        };

        // Act
        property.ViewCount++;
        property.FavoriteCount++;

        // Assert
        property.ViewCount.Should().Be(101);
        property.FavoriteCount.Should().Be(26);
        property.InquiryCount.Should().Be(10);
    }
}

public class PropertyImageTests
{
    [Fact]
    public void PropertyImage_ShouldCreateWithDefaults()
    {
        // Arrange & Act
        var image = new PropertyImage();

        // Assert
        image.Category.Should().Be(ImageCategory.General);
        image.IsPrimary.Should().BeFalse();
        image.SortOrder.Should().Be(0);
    }

    [Fact]
    public void PropertyImage_ShouldSetAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();

        // Act
        var image = new PropertyImage
        {
            Id = id,
            DealerId = dealerId,
            PropertyId = propertyId,
            Url = "https://example.com/image.jpg",
            ThumbnailUrl = "https://example.com/image-thumb.jpg",
            Caption = "Living Room",
            Category = ImageCategory.LivingRoom,
            SortOrder = 1,
            IsPrimary = true
        };

        // Assert
        image.Id.Should().Be(id);
        image.DealerId.Should().Be(dealerId);
        image.PropertyId.Should().Be(propertyId);
        image.Url.Should().Be("https://example.com/image.jpg");
        image.Category.Should().Be(ImageCategory.LivingRoom);
        image.IsPrimary.Should().BeTrue();
    }

    [Theory]
    [InlineData(ImageCategory.Exterior)]
    [InlineData(ImageCategory.Interior)]
    [InlineData(ImageCategory.Kitchen)]
    [InlineData(ImageCategory.Bedroom)]
    [InlineData(ImageCategory.Bathroom)]
    [InlineData(ImageCategory.Garden)]
    [InlineData(ImageCategory.Pool)]
    [InlineData(ImageCategory.FloorPlan)]
    public void ImageCategory_ShouldBeValid(ImageCategory category)
    {
        var image = new PropertyImage { Category = category };
        image.Category.Should().Be(category);
    }
}

public class PropertyAmenityTests
{
    [Fact]
    public void PropertyAmenity_ShouldSetAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();

        // Act
        var amenity = new PropertyAmenity
        {
            Id = id,
            DealerId = dealerId,
            PropertyId = propertyId,
            Name = "Swimming Pool",
            Category = AmenityCategory.Recreation,
            Icon = "pool"
        };

        // Assert
        amenity.Id.Should().Be(id);
        amenity.Name.Should().Be("Swimming Pool");
        amenity.Category.Should().Be(AmenityCategory.Recreation);
        amenity.Icon.Should().Be("pool");
    }

    [Theory]
    [InlineData(AmenityCategory.Security)]
    [InlineData(AmenityCategory.Recreation)]
    [InlineData(AmenityCategory.Comfort)]
    [InlineData(AmenityCategory.Services)]
    [InlineData(AmenityCategory.Outdoor)]
    [InlineData(AmenityCategory.Technology)]
    public void AmenityCategory_ShouldBeValid(AmenityCategory category)
    {
        var amenity = new PropertyAmenity { Category = category };
        amenity.Category.Should().Be(category);
    }
}
