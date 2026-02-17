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
        property.Country.Should().Be("USA");
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
            AgentName = "John Doe",
            Title = "Beautiful 3BR Home in Miami",
            StreetAddress = "123 Main Street",
            City = "Miami",
            State = "FL",
            ZipCode = "33101",
            Price = 450000.00m,
            Bedrooms = 3,
            Bathrooms = 2,
            SquareFeet = 2200,
            PropertyType = PropertyType.House,
            PropertySubType = PropertySubType.SingleFamily,
            Status = PropertyStatus.Active
        };

        // Assert
        property.Id.Should().NotBe(Guid.Empty);
        property.DealerId.Should().NotBe(Guid.Empty);
        property.AgentId.Should().NotBe(Guid.Empty);
        property.Title.Should().NotBeNullOrEmpty();
        property.Price.Should().Be(450000.00m);
        property.Bedrooms.Should().Be(3);
        property.Bathrooms.Should().Be(2);
        property.SquareFeet.Should().Be(2200);
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
    [InlineData(PropertySubType.VacantLand)]
    [InlineData(PropertySubType.Office)]
    [InlineData(PropertySubType.Retail)]
    public void Property_AllSubTypes_ShouldBeValid(PropertySubType subType)
    {
        // Arrange
        var property = new Property { PropertySubType = subType };

        // Assert
        property.PropertySubType.Should().Be(subType);
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
            DealerId = Guid.NewGuid(),
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
            DealerId = Guid.Empty,
            Name = "Residential",
            Slug = "residential",
            IsSystem = true
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
        property.UpdatedAt = DateTime.UtcNow;

        // Assert
        property.IsDeleted.Should().BeTrue();
        property.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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
            Latitude = 25.7617,
            Longitude = -80.1918
        };

        // Assert
        property.City.Should().Be("Miami");
        property.State.Should().Be("FL");
        property.Latitude.Should().BeApproximately(25.7617, 0.0001);
        property.Longitude.Should().BeApproximately(-80.1918, 0.0001);
    }

    [Fact]
    public void Property_Features_ShouldStoreAmenities()
    {
        // Arrange & Act
        var property = new Property
        {
            HasPool = true,
            PoolType = PoolType.InGround,
            GarageSpaces = 2,
            GarageType = GarageType.Attached,
            HasBasement = false,
            BasementType = BasementType.None,
            HasFireplace = true,
            FireplaceCount = 2,
            LotSizeAcres = 0.25m,
            YearBuilt = 2020
        };

        // Assert
        property.HasPool.Should().BeTrue();
        property.PoolType.Should().Be(PoolType.InGround);
        property.GarageSpaces.Should().Be(2);
        property.GarageType.Should().Be(GarageType.Attached);
        property.HasBasement.Should().BeFalse();
        property.BasementType.Should().Be(BasementType.None);
        property.HasFireplace.Should().BeTrue();
        property.FireplaceCount.Should().Be(2);
        property.LotSizeAcres.Should().Be(0.25m);
        property.YearBuilt.Should().Be(2020);
    }

    [Fact]
    public void Property_MLSNumber_ShouldBeStorable()
    {
        // Arrange & Act
        var property = new Property
        {
            MLSNumber = "MLS-12345678",
            ParcelNumber = "123-456-789",
            PropertyId = "PROP-001"
        };

        // Assert
        property.MLSNumber.Should().Be("MLS-12345678");
        property.ParcelNumber.Should().Be("123-456-789");
        property.PropertyId.Should().Be("PROP-001");
    }

    [Fact]
    public void Property_HOA_ShouldStoreDetails()
    {
        // Arrange & Act
        var property = new Property
        {
            HOAFeesMonthly = 350.00m,
            HOAName = "Sunset Community HOA"
        };

        // Assert
        property.HOAFeesMonthly.Should().Be(350.00m);
        property.HOAName.Should().Be("Sunset Community HOA");
    }

    [Fact]
    public void Property_SaleInfo_ShouldStoreDates()
    {
        // Arrange
        var listingDate = DateTime.UtcNow.AddDays(-30);
        var contractDate = DateTime.UtcNow.AddDays(-5);

        var property = new Property
        {
            ListingDate = listingDate,
            ContractDate = contractDate,
            IsNewConstruction = true,
            IsForeclosure = false,
            IsShortSale = false,
            VirtualTourAvailable = true,
            VirtualTourUrl = "https://tour.example.com/123"
        };

        // Assert
        property.ListingDate.Should().Be(listingDate);
        property.ContractDate.Should().Be(contractDate);
        property.IsNewConstruction.Should().BeTrue();
        property.IsForeclosure.Should().BeFalse();
        property.VirtualTourAvailable.Should().BeTrue();
        property.VirtualTourUrl.Should().Be("https://tour.example.com/123");
    }

    [Fact]
    public void Property_Metrics_ShouldTrackEngagement()
    {
        // Arrange & Act
        var property = new Property
        {
            ViewCount = 150,
            SavedCount = 25,
            InquiryCount = 10,
            TourRequestCount = 5,
            DaysOnMarket = 30,
            PriceChanges = 2
        };

        // Assert
        property.ViewCount.Should().Be(150);
        property.SavedCount.Should().Be(25);
        property.InquiryCount.Should().Be(10);
        property.TourRequestCount.Should().Be(5);
        property.DaysOnMarket.Should().Be(30);
        property.PriceChanges.Should().Be(2);
    }

    [Fact]
    public void Property_Schools_ShouldStoreSchoolInfo()
    {
        // Arrange & Act
        var property = new Property
        {
            ElementarySchool = "Lincoln Elementary",
            MiddleSchool = "Jefferson Middle",
            HighSchool = "Washington High",
            SchoolDistrict = "Miami-Dade County Schools"
        };

        // Assert
        property.ElementarySchool.Should().Be("Lincoln Elementary");
        property.MiddleSchool.Should().Be("Jefferson Middle");
        property.HighSchool.Should().Be("Washington High");
        property.SchoolDistrict.Should().Be("Miami-Dade County Schools");
    }

    [Fact]
    public void Property_Systems_ShouldStoreUtilities()
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
    public void Property_Construction_ShouldStoreDetails()
    {
        // Arrange & Act
        var property = new Property
        {
            ConstructionType = "Wood Frame",
            RoofType = "Shingle",
            ExteriorType = "Brick",
            FoundationType = "Slab",
            ArchitecturalStyle = ArchitecturalStyle.Colonial,
            Stories = 2,
            YearRenovated = 2018
        };

        // Assert
        property.ConstructionType.Should().Be("Wood Frame");
        property.RoofType.Should().Be("Shingle");
        property.ExteriorType.Should().Be("Brick");
        property.FoundationType.Should().Be("Slab");
        property.ArchitecturalStyle.Should().Be(ArchitecturalStyle.Colonial);
        property.Stories.Should().Be(2);
        property.YearRenovated.Should().Be(2018);
    }
}
