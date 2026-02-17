using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace FileStorageService.Tests.Unit.Services;

public class ImageProcessingServiceTests : IDisposable
{
    private readonly ImageProcessingService _sut;
    private readonly Mock<ILogger<ImageProcessingService>> _loggerMock;
    private readonly byte[] _testImageBytes;

    public ImageProcessingServiceTests()
    {
        _loggerMock = new Mock<ILogger<ImageProcessingService>>();
        _sut = new ImageProcessingService(_loggerMock.Object);

        // Create a simple 100x100 red PNG for testing
        _testImageBytes = CreateTestImage();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    private static byte[] CreateTestImage()
    {
        // Minimal 100x100 red PNG
        using var image = new Image<Rgba32>(100, 100);
        image.Mutate(ctx => ctx.BackgroundColor(Color.Red));

        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    [Fact]
    public async Task CreateThumbnailAsync_ShouldResizeImage()
    {
        // Arrange
        using var inputStream = new MemoryStream(_testImageBytes);

        // Act
        var result = await _sut.CreateThumbnailAsync(inputStream, 50, 50);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);

        // Verify dimensions
        result.Position = 0;
        var (width, height) = await _sut.GetDimensionsAsync(result);
        width.Should().BeLessOrEqualTo(50);
        height.Should().BeLessOrEqualTo(50);
    }

    [Fact]
    public async Task OptimizeAsync_ShouldReturnOptimizedImage()
    {
        // Arrange
        using var inputStream = new MemoryStream(_testImageBytes);

        // Act
        var result = await _sut.OptimizeAsync(inputStream, "jpeg", 75);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDimensionsAsync_ShouldReturnCorrectSize()
    {
        // Arrange
        using var inputStream = new MemoryStream(_testImageBytes);

        // Act
        var (width, height) = await _sut.GetDimensionsAsync(inputStream);

        // Assert
        width.Should().Be(100);
        height.Should().Be(100);
    }

    [Fact]
    public async Task ValidateImageAsync_ShouldReturnTrue_ForValidImage()
    {
        // Arrange
        using var inputStream = new MemoryStream(_testImageBytes);

        // Act
        var result = await _sut.ValidateImageAsync(inputStream);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateImageAsync_ShouldReturnFalse_ForInvalidImage()
    {
        // Arrange
        using var inputStream = new MemoryStream("not an image"u8.ToArray());

        // Act
        var result = await _sut.ValidateImageAsync(inputStream);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ConvertFormatAsync_ShouldConvertToWebP()
    {
        // Arrange
        using var inputStream = new MemoryStream(_testImageBytes);

        // Act
        var result = await _sut.ConvertFormatAsync(inputStream, "webp");

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateVariantsAsync_ShouldCreateMultipleVariants()
    {
        // Arrange
        using var inputStream = new MemoryStream(_testImageBytes);
        var variants = new List<VariantConfig>
        {
            new() { Name = "small", MaxWidth = 50, MaxHeight = 50, Format = "jpeg", Quality = 80 },
            new() { Name = "medium", MaxWidth = 75, MaxHeight = 75, Format = "jpeg", Quality = 85 }
        };

        // Act
        var result = (await _sut.GenerateVariantsAsync(inputStream, variants)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Config.Name.Should().Be("small");
        result[1].Config.Name.Should().Be("medium");

        // Cleanup streams
        foreach (var (_, stream) in result)
        {
            await stream.DisposeAsync();
        }
    }

    [Fact]
    public async Task StripExifAsync_ShouldRemoveMetadata()
    {
        // Arrange
        using var inputStream = new MemoryStream(_testImageBytes);

        // Act
        var result = await _sut.StripExifAsync(inputStream);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }
}
