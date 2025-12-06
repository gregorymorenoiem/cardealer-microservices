using MediaService.Application.Features.Media.Commands.InitUpload;
using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Domain.Interfaces.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaService.Tests.Unit.Commands;

public class InitUploadCommandHandlerTests
{
    private static readonly Guid TestDealerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Mock<IMediaRepository> _mediaRepositoryMock;
    private readonly Mock<IMediaStorageService> _storageServiceMock;
    private readonly Mock<ILogger<InitUploadCommandHandler>> _loggerMock;
    private readonly InitUploadCommandHandler _handler;

    public InitUploadCommandHandlerTests()
    {
        _mediaRepositoryMock = new Mock<IMediaRepository>();
        _storageServiceMock = new Mock<IMediaStorageService>();
        _loggerMock = new Mock<ILogger<InitUploadCommandHandler>>();
        _handler = new InitUploadCommandHandler(
            _mediaRepositoryMock.Object,
            _storageServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidImageUpload_ReturnsSuccessWithUploadUrl()
    {
        // Arrange
        var command = new InitUploadCommand(
            dealerId: TestDealerId,
            ownerId: "user-123",
            context: "profile",
            fileName: "avatar.jpg",
            contentType: "image/jpeg",
            fileSize: 1024 * 100);

        var storageKey = "users/user-123/profile/avatar.jpg";
        var uploadUrl = "https://storage.example.com/upload?token=abc123";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _storageServiceMock
            .Setup(x => x.GenerateStorageKeyAsync(command.OwnerId, command.Context, command.FileName))
            .ReturnsAsync(storageKey);

        _storageServiceMock
            .Setup(x => x.GenerateUploadUrlAsync(storageKey, command.ContentType, It.IsAny<TimeSpan?>()))
            .ReturnsAsync(new UploadUrlResponse
            {
                UploadUrl = uploadUrl,
                ExpiresAt = expiresAt,
                Headers = new Dictionary<string, string>(),
                StorageKey = storageKey
            });

        _mediaRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<MediaAsset>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UploadUrl.Should().Be(uploadUrl);
        result.Data.StorageKey.Should().Be(storageKey);
        _mediaRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ImageMedia>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidVideoUpload_CreatesVideoMediaEntity()
    {
        // Arrange
        var command = new InitUploadCommand(
            dealerId: TestDealerId,
            ownerId: "user-456",
            context: "vehicle",
            fileName: "car-tour.mp4",
            contentType: "video/mp4",
            fileSize: 1024 * 1024 * 50);

        var storageKey = "vehicles/user-456/car-tour.mp4";

        _storageServiceMock
            .Setup(x => x.GenerateStorageKeyAsync(command.OwnerId, command.Context, command.FileName))
            .ReturnsAsync(storageKey);

        _storageServiceMock
            .Setup(x => x.GenerateUploadUrlAsync(storageKey, command.ContentType, It.IsAny<TimeSpan?>()))
            .ReturnsAsync(new UploadUrlResponse
            {
                UploadUrl = "https://upload.url",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Headers = new Dictionary<string, string>()
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _mediaRepositoryMock.Verify(x => x.AddAsync(It.IsAny<VideoMedia>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidDocumentUpload_CreatesDocumentMediaEntity()
    {
        // Arrange
        var command = new InitUploadCommand(
            dealerId: TestDealerId,
            ownerId: "user-789",
            context: "documents",
            fileName: "contract.pdf",
            contentType: "application/pdf",
            fileSize: 1024 * 500);

        var storageKey = "documents/user-789/contract.pdf";

        _storageServiceMock
            .Setup(x => x.GenerateStorageKeyAsync(command.OwnerId, command.Context, command.FileName))
            .ReturnsAsync(storageKey);

        _storageServiceMock
            .Setup(x => x.GenerateUploadUrlAsync(storageKey, command.ContentType, It.IsAny<TimeSpan?>()))
            .ReturnsAsync(new UploadUrlResponse
            {
                UploadUrl = "https://upload.url",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Headers = new Dictionary<string, string>()
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _mediaRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DocumentMedia>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StorageServiceThrowsException_ReturnsFailure()
    {
        // Arrange
        var command = new InitUploadCommand(
            dealerId: TestDealerId,
            ownerId: "user-123",
            context: "profile",
            fileName: "avatar.jpg",
            contentType: "image/jpeg",
            fileSize: 1024);

        _storageServiceMock
            .Setup(x => x.GenerateStorageKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Storage service unavailable"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Error");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/webp")]
    public async Task Handle_ImageContentTypes_CreatesImageMedia(string contentType)
    {
        // Arrange
        var command = new InitUploadCommand(TestDealerId, "user-123", "test", "file.img", contentType, 1024);

        _storageServiceMock
            .Setup(x => x.GenerateStorageKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("key");

        _storageServiceMock
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(new UploadUrlResponse
            {
                UploadUrl = "url",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Headers = new Dictionary<string, string>()
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _mediaRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ImageMedia>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("video/mp4")]
    [InlineData("video/webm")]
    [InlineData("video/quicktime")]
    public async Task Handle_VideoContentTypes_CreatesVideoMedia(string contentType)
    {
        // Arrange
        var command = new InitUploadCommand(TestDealerId, "user-123", "test", "file.vid", contentType, 1024);

        _storageServiceMock
            .Setup(x => x.GenerateStorageKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("key");

        _storageServiceMock
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(new UploadUrlResponse
            {
                UploadUrl = "url",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Headers = new Dictionary<string, string>()
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _mediaRepositoryMock.Verify(x => x.AddAsync(It.IsAny<VideoMedia>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
