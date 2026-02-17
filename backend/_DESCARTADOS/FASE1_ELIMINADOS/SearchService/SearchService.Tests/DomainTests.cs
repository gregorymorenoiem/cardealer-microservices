using SearchService.Domain.Entities;
using SearchService.Domain.Enums;
using SearchService.Domain.ValueObjects;
using Xunit;

namespace SearchService.Tests.Domain;

public class SearchQueryTests
{
    [Fact]
    public void IsValid_WithValidQuery_ReturnsTrue()
    {
        // Arrange
        var query = new SearchQuery
        {
            QueryText = "test",
            IndexName = "vehicles",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = query.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithEmptyQueryText_ReturnsFalse()
    {
        // Arrange
        var query = new SearchQuery
        {
            QueryText = "",
            IndexName = "vehicles"
        };

        // Act
        var result = query.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithInvalidPageSize_ReturnsFalse()
    {
        // Arrange
        var query = new SearchQuery
        {
            QueryText = "test",
            IndexName = "vehicles",
            PageSize = 0
        };

        // Act
        var result = query.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetOffset_CalculatesCorrectly()
    {
        // Arrange
        var query = new SearchQuery
        {
            Page = 3,
            PageSize = 10
        };

        // Act
        var offset = query.GetOffset();

        // Assert
        Assert.Equal(20, offset);
    }
}

public class SearchResultTests
{
    [Fact]
    public void GetTotalPages_CalculatesCorrectly()
    {
        // Arrange
        var result = new SearchResult
        {
            TotalCount = 45,
            PageSize = 10
        };

        // Act
        var totalPages = result.GetTotalPages();

        // Assert
        Assert.Equal(5, totalPages);
    }

    [Fact]
    public void HasNextPage_WhenOnMiddlePage_ReturnsTrue()
    {
        // Arrange
        var result = new SearchResult
        {
            TotalCount = 100,
            PageSize = 10,
            CurrentPage = 5
        };

        // Act
        var hasNext = result.HasNextPage();

        // Assert
        Assert.True(hasNext);
    }

    [Fact]
    public void HasNextPage_WhenOnLastPage_ReturnsFalse()
    {
        // Arrange
        var result = new SearchResult
        {
            TotalCount = 100,
            PageSize = 10,
            CurrentPage = 10
        };

        // Act
        var hasNext = result.HasNextPage();

        // Assert
        Assert.False(hasNext);
    }

    [Fact]
    public void GetSummary_ReturnsFormattedString()
    {
        // Arrange
        var result = new SearchResult
        {
            TotalCount = 42,
            ExecutionTimeMs = 150,
            CurrentPage = 2,
            PageSize = 10
        };

        // Act
        var summary = result.GetSummary();

        // Assert
        Assert.Contains("42", summary);
        Assert.Contains("150ms", summary);
    }
}

public class IndexMetadataTests
{
    [Fact]
    public void GetFormattedSize_FormatsBytes()
    {
        // Arrange
        var metadata = new IndexMetadata { SizeInBytes = 512 };

        // Act
        var formatted = metadata.GetFormattedSize();

        // Assert
        Assert.Equal("512 B", formatted);
    }

    [Fact]
    public void GetFormattedSize_FormatsKilobytes()
    {
        // Arrange
        var metadata = new IndexMetadata { SizeInBytes = 2048 };

        // Act
        var formatted = metadata.GetFormattedSize();

        // Assert
        Assert.Equal("2.00 KB", formatted);
    }

    [Fact]
    public void GetFormattedSize_FormatsMegabytes()
    {
        // Arrange
        var metadata = new IndexMetadata { SizeInBytes = 5242880 }; // 5 MB

        // Act
        var formatted = metadata.GetFormattedSize();

        // Assert
        Assert.Equal("5.00 MB", formatted);
    }

    [Fact]
    public void IsHealthy_WithActiveStatus_ReturnsTrue()
    {
        // Arrange
        var metadata = new IndexMetadata { Status = IndexStatus.Active };

        // Act
        var isHealthy = metadata.IsHealthy();

        // Assert
        Assert.True(isHealthy);
    }

    [Fact]
    public void NeedsReindexing_WithReindexingStatus_ReturnsTrue()
    {
        // Arrange
        var metadata = new IndexMetadata { Status = IndexStatus.Reindexing };

        // Act
        var needsReindexing = metadata.NeedsReindexing();

        // Assert
        Assert.True(needsReindexing);
    }
}

public class PropertySearchDocumentTests
{
    [Fact]
    public void PropertySearchDocument_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var doc = new SearchService.Application.DTOs.PropertySearchDocument();

        // Assert
        Assert.Equal(string.Empty, doc.Id);
        Assert.Equal(string.Empty, doc.Title);
        Assert.Equal(string.Empty, doc.Description);
        Assert.Equal("active", doc.Status);
        Assert.Equal("MXN", doc.Currency);
        Assert.Equal("sqm", doc.AreaUnit);
        Assert.False(doc.IsFeatured);
        Assert.NotNull(doc.Amenities);
        Assert.Empty(doc.Amenities);
        Assert.NotNull(doc.Location);
        Assert.NotNull(doc.Seller);
    }

    [Fact]
    public void PropertySearchDocument_CanSetAllProperties()
    {
        // Arrange
        var doc = new SearchService.Application.DTOs.PropertySearchDocument
        {
            Id = "prop-123",
            DealerId = Guid.NewGuid(),
            Title = "Casa en Polanco",
            Description = "Hermosa casa con jardín",
            PropertyType = "house",
            ListingType = "sale",
            Status = "active",
            Price = 5000000,
            Currency = "MXN",
            PricePerSqMeter = 25000,
            TotalArea = 200,
            Bedrooms = 4,
            Bathrooms = 3,
            ParkingSpaces = 2,
            HasPool = true,
            HasGarden = true,
            HasSecurity = true,
            IsFeatured = true,
            Amenities = new List<string> { "pool", "garden", "gym" },
            Location = new SearchService.Application.DTOs.PropertyLocationDocument
            {
                City = "Ciudad de México",
                State = "CDMX",
                Neighborhood = "Polanco",
                Coordinates = new SearchService.Application.DTOs.GeoPoint { Lat = 19.4326, Lon = -99.1332 }
            }
        };

        // Assert
        Assert.Equal("prop-123", doc.Id);
        Assert.Equal("Casa en Polanco", doc.Title);
        Assert.Equal("house", doc.PropertyType);
        Assert.Equal(5000000, doc.Price);
        Assert.Equal(4, doc.Bedrooms);
        Assert.True(doc.HasPool);
        Assert.True(doc.IsFeatured);
        Assert.Contains("pool", doc.Amenities);
        Assert.Equal("Polanco", doc.Location.Neighborhood);
        Assert.Equal(19.4326, doc.Location.Coordinates!.Lat);
    }

    [Fact]
    public void PropertyLocationDocument_DefaultCountry_IsMexico()
    {
        // Arrange & Act
        var location = new SearchService.Application.DTOs.PropertyLocationDocument();

        // Assert
        Assert.Equal("México", location.Country);
    }

    [Fact]
    public void GeoPoint_StoresCoordinates()
    {
        // Arrange
        var point = new SearchService.Application.DTOs.GeoPoint
        {
            Lat = 19.4326,
            Lon = -99.1332
        };

        // Assert
        Assert.Equal(19.4326, point.Lat);
        Assert.Equal(-99.1332, point.Lon);
    }

    [Fact]
    public void PropertySellerDocument_DefaultValues()
    {
        // Arrange & Act
        var seller = new SearchService.Application.DTOs.PropertySellerDocument();

        // Assert
        Assert.Equal(string.Empty, seller.Id);
        Assert.Equal(string.Empty, seller.Name);
        Assert.False(seller.IsVerified);
        Assert.False(seller.IsDealership);
    }
}
