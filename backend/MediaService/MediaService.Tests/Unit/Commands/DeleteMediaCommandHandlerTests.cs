using MediaService.Application.Features.Media.Commands.DeleteMedia;
using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Domain.Interfaces.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaService.Tests.Unit.Commands;

public class DeleteMediaCommandHandlerTests
{
    private static readonly Guid TestDealerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Mock<IMediaRepository> _mediaRepositoryMock;
    private readonly Mock<IMediaVariantRepository> _variantRepositoryMock;
    private readonly Mock<IMediaStorageService> _storageServiceMock;
    private readonly Mock<ILogger<DeleteMediaCommandHandler>> _loggerMock;
    private readonly DeleteMediaCommandHandler _handler;

    public DeleteMediaCommandHandlerTests()
    {
        _mediaRepositoryMock = new Mock<IMediaRepository>();
        _variantRepositoryMock = new Mock<IMediaVariantRepository>();
        _storageServiceMock = new Mock<IMediaStorageService>();
        _loggerMock = new Mock<ILogger<DeleteMediaCommandHandler>>();
        _handler = new DeleteMediaCommandHandler(
            _mediaRepositoryMock.Object,
            _variantRepositoryMock.Object,
            _storageServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMedia_DeletesSuccessfully()
    {
        // Arrange
        var mediaId = Guid.NewGuid().ToString();
        var storageKey = "users/user-123/avatar.jpg";
        var mediaAsset = new ImageMedia(
            dealerId: TestDealerId,
            ownerId: "user-123",
            context: "profile",
            originalFileName: "avatar.jpg",
            contentType: "image/jpeg",
            sizeBytes: 1024 * 100,
            storageKey: storageKey,
            width: 800,
            height: 600);

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediaAsset);

        _storageServiceMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _variantRepositoryMock
            .Setup(x => x.DeleteByMediaIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mediaRepositoryMock
            .Setup(x => x.DeleteAsync(mediaAsset, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteMediaCommand(mediaId, "admin-user", "cleanup");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.MediaId.Should().Be(mediaId);
        result.Data.Success.Should().BeTrue();
        _mediaRepositoryMock.Verify(x => x.DeleteAsync(mediaAsset, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingMedia_ReturnsFailure()
    {
        // Arrange
        var mediaId = Guid.NewGuid().ToString();

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MediaAsset?)null);

        var command = new DeleteMediaCommand(mediaId, "admin-user");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _storageServiceMock.Verify(x => x.DeleteFileAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_StorageDeleteFails_StillDeletesFromDatabase()
    {
        // Arrange
        var mediaId = Guid.NewGuid().ToString();
        var storageKey = "users/user-123/avatar.jpg";
        var mediaAsset = new ImageMedia(
            dealerId: TestDealerId,
            ownerId: "user-123",
            context: "profile",
            originalFileName: "avatar.jpg",
            contentType: "image/jpeg",
            sizeBytes: 1024 * 100,
            storageKey: storageKey,
            width: 800,
            height: 600);

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediaAsset);

        _storageServiceMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Storage unavailable"));

        _variantRepositoryMock
            .Setup(x => x.DeleteByMediaIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mediaRepositoryMock
            .Setup(x => x.DeleteAsync(mediaAsset, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteMediaCommand(mediaId, "admin-user");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.DeletedFiles.Should().Be(0); // Storage delete failed
        _mediaRepositoryMock.Verify(x => x.DeleteAsync(mediaAsset, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var mediaId = Guid.NewGuid().ToString();

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var command = new DeleteMediaCommand(mediaId, "admin-user");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Error");
    }

    [Fact]
    public async Task Handle_DeletesVariantsBeforeMainMedia()
    {
        // Arrange
        var mediaId = Guid.NewGuid().ToString();
        var storageKey = "users/user-123/avatar.jpg";
        var mediaAsset = new ImageMedia(
            dealerId: TestDealerId,
            ownerId: "user-123",
            context: "profile",
            originalFileName: "avatar.jpg",
            contentType: "image/jpeg",
            sizeBytes: 1024 * 100,
            storageKey: storageKey,
            width: 800,
            height: 600);

        var callOrder = new List<string>();

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediaAsset);

        _storageServiceMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _variantRepositoryMock
            .Setup(x => x.DeleteByMediaIdAsync(mediaId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("variants"))
            .Returns(Task.CompletedTask);

        _mediaRepositoryMock
            .Setup(x => x.DeleteAsync(mediaAsset, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("media"))
            .Returns(Task.CompletedTask);

        var command = new DeleteMediaCommand(mediaId, "admin-user");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().ContainInOrder("variants", "media");
    }
}
