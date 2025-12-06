using MediaService.Application.Features.Media.Queries.GetMedia;
using MediaService.Domain.Entities;
using MediaService.Domain.Enums;
using MediaService.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaService.Tests.Unit.Queries;

public class GetMediaQueryHandlerTests
{
    private static readonly Guid TestDealerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Mock<IMediaRepository> _mediaRepositoryMock;
    private readonly Mock<ILogger<GetMediaQueryHandler>> _loggerMock;
    private readonly GetMediaQueryHandler _handler;

    public GetMediaQueryHandlerTests()
    {
        _mediaRepositoryMock = new Mock<IMediaRepository>();
        _loggerMock = new Mock<ILogger<GetMediaQueryHandler>>();
        _handler = new GetMediaQueryHandler(
            _mediaRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMedia_ReturnsSuccessWithMediaData()
    {
        // Arrange
        var mediaId = Guid.NewGuid().ToString();
        var mediaAsset = new ImageMedia(
            dealerId: TestDealerId,
            ownerId: "user-123",
            context: "profile",
            originalFileName: "avatar.jpg",
            contentType: "image/jpeg",
            sizeBytes: 1024 * 100,
            storageKey: "users/user-123/avatar.jpg",
            width: 800,
            height: 600);

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediaAsset);

        var query = new GetMediaQuery(mediaId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Media.OriginalFileName.Should().Be("avatar.jpg");
        result.Data.Media.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task Handle_NonExistingMedia_ReturnsFailure()
    {
        // Arrange
        var mediaId = Guid.NewGuid().ToString();

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MediaAsset?)null);

        var query = new GetMediaQuery(mediaId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var mediaId = Guid.NewGuid().ToString();

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetMediaQuery(mediaId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Error");
    }

    [Fact]
    public async Task Handle_VideoMedia_ReturnsCorrectMediaType()
    {
        // Arrange
        var mediaId = Guid.NewGuid().ToString();
        var videoMedia = new VideoMedia(
            dealerId: TestDealerId,
            ownerId: "user-456",
            context: "vehicle",
            originalFileName: "car-tour.mp4",
            contentType: "video/mp4",
            sizeBytes: 1024 * 1024 * 50,
            storageKey: "vehicles/user-456/car-tour.mp4");

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoMedia);

        var query = new GetMediaQuery(mediaId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Media.Type.Should().Be(MediaType.Video.ToString());
    }

    [Fact]
    public async Task Handle_DocumentMedia_ReturnsCorrectMediaType()
    {
        // Arrange
        var mediaId = Guid.NewGuid().ToString();
        var docMedia = new DocumentMedia(
            dealerId: TestDealerId,
            ownerId: "user-789",
            context: "contracts",
            originalFileName: "agreement.pdf",
            contentType: "application/pdf",
            sizeBytes: 1024 * 200,
            storageKey: "contracts/user-789/agreement.pdf");

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(docMedia);

        var query = new GetMediaQuery(mediaId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Media.Type.Should().Be(MediaType.Document.ToString());
    }
}
