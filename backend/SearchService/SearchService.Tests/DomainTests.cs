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
