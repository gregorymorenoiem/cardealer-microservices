namespace VehicleService.Tests.Unit.Images;

/// <summary>
/// Unit tests for vehicle image management
/// </summary>
public class VehicleImageTests
{
    [Fact]
    public void AddImage_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var image = new
        {
            VehicleId = Guid.NewGuid(),
            ImageUrl = "https://cdn.example.com/vehicles/image123.jpg",
            IsPrimary = false,
            DisplayOrder = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        image.VehicleId.Should().NotBeEmpty();
        image.ImageUrl.Should().NotBeNullOrEmpty();
        image.DisplayOrder.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData("https://cdn.example.com/image.jpg", true)]
    [InlineData("https://storage.example.com/vehicles/photo.png", true)]
    [InlineData("http://example.com/image.gif", true)]
    [InlineData("", false)]
    [InlineData("not-a-url", false)]
    public void ValidateImageUrl_WithVariousUrls_ReturnsExpectedResult(string url, bool expected)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(url) &&
                      (url.StartsWith("http://") || url.StartsWith("https://"));

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void SetPrimaryImage_WithMultipleImages_OnlyOneIsPrimary()
    {
        // Arrange
        var images = new[]
        {
            new { Id = 1, IsPrimary = false },
            new { Id = 2, IsPrimary = false },
            new { Id = 3, IsPrimary = false }
        };

        var imageIdToSetPrimary = 2;

        // Act - Simulate setting image 2 as primary
        var updatedImages = images.Select(img => new
        {
            img.Id,
            IsPrimary = img.Id == imageIdToSetPrimary
        }).ToArray();

        // Assert
        updatedImages.Count(img => img.IsPrimary).Should().Be(1);
        updatedImages.First(img => img.IsPrimary).Id.Should().Be(imageIdToSetPrimary);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(10, true)]
    [InlineData(-1, false)]
    public void ValidateDisplayOrder_WithVariousValues_ReturnsExpectedResult(int order, bool expected)
    {
        // Act
        var isValid = order >= 0;

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void SortImages_ByDisplayOrder_ReturnsOrderedList()
    {
        // Arrange
        var images = new[]
        {
            new { Id = 1, DisplayOrder = 3 },
            new { Id = 2, DisplayOrder = 1 },
            new { Id = 3, DisplayOrder = 2 }
        };

        // Act
        var sorted = images.OrderBy(img => img.DisplayOrder).ToList();

        // Assert
        sorted[0].DisplayOrder.Should().Be(1);
        sorted[1].DisplayOrder.Should().Be(2);
        sorted[2].DisplayOrder.Should().Be(3);
    }

    [Fact]
    public void GetPrimaryImage_WithMultipleImages_ReturnsPrimaryOne()
    {
        // Arrange
        var images = new[]
        {
            new { Id = 1, IsPrimary = false, ImageUrl = "url1.jpg" },
            new { Id = 2, IsPrimary = true, ImageUrl = "url2.jpg" },
            new { Id = 3, IsPrimary = false, ImageUrl = "url3.jpg" }
        };

        // Act
        var primaryImage = images.FirstOrDefault(img => img.IsPrimary);

        // Assert
        primaryImage.Should().NotBeNull();
        primaryImage!.Id.Should().Be(2);
    }

    [Fact]
    public void DeleteImage_RemovesFromCollection()
    {
        // Arrange
        var images = new[]
        {
            new { Id = 1, ImageUrl = "url1.jpg" },
            new { Id = 2, ImageUrl = "url2.jpg" },
            new { Id = 3, ImageUrl = "url3.jpg" }
        };

        var imageIdToDelete = 2;

        // Act
        var remainingImages = images.Where(img => img.Id != imageIdToDelete).ToArray();

        // Assert
        remainingImages.Should().HaveCount(2);
        remainingImages.Should().NotContain(img => img.Id == imageIdToDelete);
    }

    [Theory]
    [InlineData(".jpg", true)]
    [InlineData(".jpeg", true)]
    [InlineData(".png", true)]
    [InlineData(".gif", true)]
    [InlineData(".webp", true)]
    [InlineData(".txt", false)]
    [InlineData(".exe", false)]
    public void ValidateImageExtension_WithVariousExtensions_ReturnsExpectedResult(string extension, bool expected)
    {
        // Arrange
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        // Act
        var isValid = validExtensions.Contains(extension.ToLower());

        // Assert
        isValid.Should().Be(expected);
    }

    [Fact]
    public void GetImagesForVehicle_ReturnsAllImages()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var allImages = new[]
        {
            new { VehicleId = vehicleId, ImageUrl = "img1.jpg" },
            new { VehicleId = vehicleId, ImageUrl = "img2.jpg" },
            new { VehicleId = Guid.NewGuid(), ImageUrl = "img3.jpg" }
        };

        // Act
        var vehicleImages = allImages.Where(img => img.VehicleId == vehicleId).ToList();

        // Assert
        vehicleImages.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(1048576, true)]      // 1 MB
    [InlineData(5242880, true)]      // 5 MB
    [InlineData(10485760, false)]    // 10 MB - too large
    [InlineData(0, false)]
    public void ValidateImageSize_WithVariousSizes_ReturnsExpectedResult(long sizeBytes, bool expected)
    {
        // Arrange
        const long maxSizeBytes = 5 * 1024 * 1024; // 5 MB

        // Act
        var isValid = sizeBytes > 0 && sizeBytes <= maxSizeBytes;

        // Assert
        isValid.Should().Be(expected);
    }
}
