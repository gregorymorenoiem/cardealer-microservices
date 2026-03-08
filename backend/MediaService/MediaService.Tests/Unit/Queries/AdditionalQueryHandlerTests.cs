using MediaService.Application.Features.Media.Queries.GetMediaByOwner;
using MediaService.Application.Features.Media.Queries.GetMediaVariants;
using MediaService.Application.Features.Media.Queries.ListMedia;
using MediaService.Application.DTOs;
using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace MediaService.Tests.Unit.Queries;

// ============================================================================
// GET MEDIA BY OWNER QUERY HANDLER
// ============================================================================

public class GetMediaByOwnerQueryHandlerTests
{
    private readonly Mock<IMediaRepository> _mediaRepo = new();
    private readonly Mock<ILogger<GetMediaByOwnerQueryHandler>> _logger = new();
    private readonly GetMediaByOwnerQueryHandler _sut;
    private static readonly CancellationToken Ct = CancellationToken.None;

    public GetMediaByOwnerQueryHandlerTests()
    {
        _sut = new GetMediaByOwnerQueryHandler(_mediaRepo.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_ReturnsPagedMediaForOwner()
    {
        // Arrange
        var ownerId = "owner123";
        var media1 = new ImageMedia(Guid.NewGuid(), ownerId, "vehicles", "car1.jpg", "image/jpeg", 1024, "key1", 800, 600);
        var media2 = new ImageMedia(Guid.NewGuid(), ownerId, "vehicles", "car2.jpg", "image/jpeg", 2048, "key2", 1920, 1080);

        _mediaRepo.Setup(r => r.GetPaginatedAsync(
                ownerId, "vehicles", null, null, null, null, 1, 50, null, true))
            .ReturnsAsync((new List<MediaAsset> { media1, media2 }.AsEnumerable(), 2));

        var query = new GetMediaByOwnerQuery(ownerId, "vehicles");

        // Act
        var result = await _sut.Handle(query, Ct);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.OwnerId.Should().Be(ownerId);
        result.Data.Media.Items.Should().HaveCount(2);
        result.Data.Media.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        var ownerId = "nobody";
        _mediaRepo.Setup(r => r.GetPaginatedAsync(
                ownerId, null, null, null, null, null, 1, 50, null, true))
            .ReturnsAsync((Enumerable.Empty<MediaAsset>(), 0));

        var query = new GetMediaByOwnerQuery(ownerId);

        var result = await _sut.Handle(query, Ct);

        result.Success.Should().BeTrue();
        result.Data!.Media.Items.Should().BeEmpty();
        result.Data.Media.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithMediaTypeFilter_ParsesEnumCorrectly()
    {
        _mediaRepo.Setup(r => r.GetPaginatedAsync(
                "owner1", null, Domain.Enums.MediaType.Image, null, null, null, 1, 50, null, true))
            .ReturnsAsync((Enumerable.Empty<MediaAsset>(), 0));

        var query = new GetMediaByOwnerQuery("owner1") { MediaType = "Image" };

        var result = await _sut.Handle(query, Ct);

        result.Success.Should().BeTrue();
        _mediaRepo.Verify(r => r.GetPaginatedAsync(
            "owner1", null, Domain.Enums.MediaType.Image, null, null, null, 1, 50, null, true), Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ReturnsFail()
    {
        _mediaRepo.Setup(r => r.GetPaginatedAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Domain.Enums.MediaType?>(),
                It.IsAny<Domain.Enums.MediaStatus?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("DB failure"));

        var query = new GetMediaByOwnerQuery("owner1");

        var result = await _sut.Handle(query, Ct);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Error");
    }
}

// ============================================================================
// GET MEDIA VARIANTS QUERY HANDLER
// ============================================================================

public class GetMediaVariantsQueryHandlerTests
{
    private readonly Mock<IMediaVariantRepository> _variantRepo = new();
    private readonly Mock<IMediaRepository> _mediaRepo = new();
    private readonly Mock<ILogger<GetMediaVariantsQueryHandler>> _logger = new();
    private readonly GetMediaVariantsQueryHandler _sut;
    private static readonly CancellationToken Ct = CancellationToken.None;

    public GetMediaVariantsQueryHandlerTests()
    {
        _sut = new GetMediaVariantsQueryHandler(
            _variantRepo.Object,
            _mediaRepo.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_MediaExists_ReturnsAllVariants()
    {
        // Arrange
        var mediaId = "img_test123";
        _mediaRepo.Setup(r => r.ExistsAsync(mediaId, Ct)).ReturnsAsync(true);

        var variants = new List<MediaVariant>
        {
            new(mediaId, "thumb", "key_thumb", 200, 200, 5000, ".jpg", 80),
            new(mediaId, "small", "key_small", 400, 400, 15000, ".jpg", 80),
            new(mediaId, "medium", "key_medium", 800, 800, 50000, ".jpg", 85)
        };

        _variantRepo.Setup(r => r.GetByMediaIdAsync(mediaId, Ct))
            .ReturnsAsync(variants.AsEnumerable());

        var query = new GetMediaVariantsQuery(mediaId);

        // Act
        var result = await _sut.Handle(query, Ct);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.MediaId.Should().Be(mediaId);
        result.Data.Variants.Should().HaveCount(3);
        result.Data.Variants[0].Name.Should().Be("thumb");
        result.Data.Variants[1].Name.Should().Be("small");
        result.Data.Variants[2].Name.Should().Be("medium");
    }

    [Fact]
    public async Task Handle_WithVariantNameFilter_ReturnsSpecificVariant()
    {
        var mediaId = "img_test123";
        _mediaRepo.Setup(r => r.ExistsAsync(mediaId, Ct)).ReturnsAsync(true);

        var variant = new MediaVariant(mediaId, "thumb", "key_thumb", 200, 200, 5000, ".jpg", 80);
        _variantRepo.Setup(r => r.GetByMediaIdAndNameAsync(mediaId, "thumb", Ct))
            .ReturnsAsync(variant);

        var query = new GetMediaVariantsQuery(mediaId, "thumb");

        var result = await _sut.Handle(query, Ct);

        result.Success.Should().BeTrue();
        result.Data!.Variants.Should().HaveCount(1);
        result.Data.Variants[0].Name.Should().Be("thumb");
    }

    [Fact]
    public async Task Handle_MediaNotFound_ReturnsFail()
    {
        _mediaRepo.Setup(r => r.ExistsAsync("nonexistent", Ct)).ReturnsAsync(false);

        var result = await _sut.Handle(new GetMediaVariantsQuery("nonexistent"), Ct);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_VariantNameNotFound_ReturnsEmptyList()
    {
        var mediaId = "img_test123";
        _mediaRepo.Setup(r => r.ExistsAsync(mediaId, Ct)).ReturnsAsync(true);
        _variantRepo.Setup(r => r.GetByMediaIdAndNameAsync(mediaId, "xlarge", Ct))
            .ReturnsAsync((MediaVariant?)null);

        var query = new GetMediaVariantsQuery(mediaId, "xlarge");

        var result = await _sut.Handle(query, Ct);

        result.Success.Should().BeTrue();
        result.Data!.Variants.Should().BeEmpty();
    }
}

// ============================================================================
// LIST MEDIA QUERY HANDLER
// ============================================================================

public class ListMediaQueryHandlerTests
{
    private readonly Mock<IMediaRepository> _mediaRepo = new();
    private readonly Mock<ILogger<ListMediaQueryHandler>> _logger = new();
    private readonly ListMediaQueryHandler _sut;
    private static readonly CancellationToken Ct = CancellationToken.None;

    public ListMediaQueryHandlerTests()
    {
        _sut = new ListMediaQueryHandler(_mediaRepo.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_ReturnsPagedMedia()
    {
        var media = new ImageMedia(Guid.NewGuid(), "owner1", "vehicles", "car.jpg", "image/jpeg", 1024, "key1", 800, 600);

        _mediaRepo.Setup(r => r.GetPaginatedAsync(
                null, null, null, null, null, null, 1, 50, null, true))
            .ReturnsAsync((new List<MediaAsset> { media }.AsEnumerable(), 1));

        var query = new ListMediaQuery();

        var result = await _sut.Handle(query, Ct);

        result.Success.Should().BeTrue();
        result.Data!.Media.Items.Should().HaveCount(1);
        result.Data.Media.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithMediaTypeFilter_ParsesEnum()
    {
        _mediaRepo.Setup(r => r.GetPaginatedAsync(
                null, null, Domain.Enums.MediaType.Video, null, null, null, 1, 50, null, true))
            .ReturnsAsync((Enumerable.Empty<MediaAsset>(), 0));

        var query = new ListMediaQuery { MediaType = "Video" };

        var result = await _sut.Handle(query, Ct);

        result.Success.Should().BeTrue();
        _mediaRepo.Verify(r => r.GetPaginatedAsync(
            null, null, Domain.Enums.MediaType.Video, null, null, null, 1, 50, null, true), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidMediaType_PassesNullForType()
    {
        _mediaRepo.Setup(r => r.GetPaginatedAsync(
                null, null, null, null, null, null, 1, 50, null, true))
            .ReturnsAsync((Enumerable.Empty<MediaAsset>(), 0));

        var query = new ListMediaQuery { MediaType = "InvalidType" };

        var result = await _sut.Handle(query, Ct);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ReturnsFail()
    {
        _mediaRepo.Setup(r => r.GetPaginatedAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Domain.Enums.MediaType?>(),
                It.IsAny<Domain.Enums.MediaStatus?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Connection failed"));

        var result = await _sut.Handle(new ListMediaQuery(), Ct);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Error");
    }
}
