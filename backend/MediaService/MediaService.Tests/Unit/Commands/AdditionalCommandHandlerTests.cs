using MediaService.Application.Features.Media.Commands.FinalizeUpload;
using MediaService.Application.Features.Media.Commands.GetPresignedUrlsBatch;
using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Domain.Interfaces.Services;
using MediaService.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace MediaService.Tests.Unit.Commands;

// ============================================================================
// FINALIZE UPLOAD COMMAND HANDLER
// ============================================================================

public class FinalizeUploadCommandHandlerTests
{
    private readonly Mock<IMediaRepository> _mediaRepo = new();
    private readonly Mock<IMediaStorageService> _storageService = new();
    private readonly Mock<ILogger<FinalizeUploadCommandHandler>> _logger = new();
    private readonly FinalizeUploadCommandHandler _sut;
    private static readonly CancellationToken Ct = CancellationToken.None;

    public FinalizeUploadCommandHandlerTests()
    {
        _sut = new FinalizeUploadCommandHandler(
            _mediaRepo.Object,
            _storageService.Object,
            _logger.Object);
    }

    private ImageMedia CreateTestMedia(string id = "img_test123")
    {
        var media = new ImageMedia(
            Guid.NewGuid(), "owner1", "vehicles", "photo.jpg",
            "image/jpeg", 1024, "vehicles/pending/photo.jpg", 800, 600);
        return media;
    }

    [Fact]
    public async Task Handle_ValidMedia_FinalizeSuccessfully()
    {
        // Arrange
        var media = CreateTestMedia();
        var cdnUrl = "https://cdn.okla.do/vehicles/photo.jpg";

        _mediaRepo.Setup(r => r.GetByIdAsync(media.Id, Ct)).ReturnsAsync(media);
        _storageService.Setup(s => s.FileExistsAsync(media.StorageKey)).ReturnsAsync(true);
        _storageService.Setup(s => s.GetFileUrlAsync(media.StorageKey)).ReturnsAsync(cdnUrl);

        var command = new FinalizeUploadCommand(media.Id);

        // Act
        var result = await _sut.Handle(command, Ct);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.MediaId.Should().Be(media.Id);
        result.Data.CdnUrl.Should().Be(cdnUrl);
        result.Data.Status.Should().Be("Processed");
        _mediaRepo.Verify(r => r.UpdateAsync(media, Ct), Times.Once);
    }

    [Fact]
    public async Task Handle_MediaNotFound_ReturnsFail()
    {
        _mediaRepo.Setup(r => r.GetByIdAsync("nonexistent", Ct)).ReturnsAsync((MediaAsset?)null);

        var result = await _sut.Handle(new FinalizeUploadCommand("nonexistent"), Ct);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_FileNotInStorage_MarksAsFailedAndReturnsFail()
    {
        var media = CreateTestMedia();
        _mediaRepo.Setup(r => r.GetByIdAsync(media.Id, Ct)).ReturnsAsync(media);
        _storageService.Setup(s => s.FileExistsAsync(media.StorageKey)).ReturnsAsync(false);

        var result = await _sut.Handle(new FinalizeUploadCommand(media.Id), Ct);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _mediaRepo.Verify(r => r.UpdateAsync(It.Is<MediaAsset>(m =>
            m.Status == Domain.Enums.MediaStatus.Failed), Ct), Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ReturnsFail()
    {
        _mediaRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>(), Ct))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _sut.Handle(new FinalizeUploadCommand("test"), Ct);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Error");
    }
}

// ============================================================================
// GET PRESIGNED URLS BATCH COMMAND HANDLER
// ============================================================================

public class GetPresignedUrlsBatchCommandHandlerTests
{
    private readonly Mock<IMediaRepository> _mediaRepo = new();
    private readonly Mock<IMediaStorageService> _storageService = new();
    private readonly Mock<ILogger<GetPresignedUrlsBatchCommandHandler>> _logger = new();
    private readonly GetPresignedUrlsBatchCommandHandler _sut;
    private static readonly CancellationToken Ct = CancellationToken.None;

    public GetPresignedUrlsBatchCommandHandlerTests()
    {
        _sut = new GetPresignedUrlsBatchCommandHandler(
            _mediaRepo.Object,
            _storageService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_SingleFile_GeneratesPresignedUrl()
    {
        // Arrange
        var uploadResponse = new UploadUrlResponse
        {
            UploadUrl = "https://storage.example.com/presigned",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Headers = new Dictionary<string, string> { ["Content-Type"] = "image/jpeg" }
        };

        _storageService.Setup(s => s.GenerateUploadUrlAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(uploadResponse);

        var command = new GetPresignedUrlsBatchCommand
        {
            OwnerId = "user123",
            DealerId = Guid.NewGuid(),
            Category = "vehicles",
            VehicleId = Guid.NewGuid(),
            Files = new List<FileUploadInfo>
            {
                new() { FileName = "car.jpg", ContentType = "image/jpeg", Size = 2048 }
            }
        };

        // Act
        var result = await _sut.Handle(command, Ct);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items[0].PresignedUrl.Should().Be("https://storage.example.com/presigned");
        _mediaRepo.Verify(r => r.AddAsync(It.IsAny<ImageMedia>(), Ct), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleBatchFiles_GeneratesMultipleUrls()
    {
        var uploadResponse = new UploadUrlResponse
        {
            UploadUrl = "https://storage.example.com/presigned",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Headers = new()
        };

        _storageService.Setup(s => s.GenerateUploadUrlAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(uploadResponse);

        var command = new GetPresignedUrlsBatchCommand
        {
            OwnerId = "user123",
            DealerId = Guid.NewGuid(),
            Category = "vehicles",
            Files = new List<FileUploadInfo>
            {
                new() { FileName = "front.jpg", ContentType = "image/jpeg", Size = 1024 },
                new() { FileName = "back.png", ContentType = "image/png", Size = 2048 },
                new() { FileName = "side.webp", ContentType = "image/webp", Size = 3072 }
            }
        };

        var result = await _sut.Handle(command, Ct);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(3);
        _mediaRepo.Verify(r => r.AddAsync(It.IsAny<ImageMedia>(), Ct), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_StorageException_ReturnsFail()
    {
        _storageService.Setup(s => s.GenerateUploadUrlAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
            .ThrowsAsync(new Exception("Storage unavailable"));

        var command = new GetPresignedUrlsBatchCommand
        {
            OwnerId = "user123",
            DealerId = Guid.NewGuid(),
            Files = new List<FileUploadInfo>
            {
                new() { FileName = "car.jpg", ContentType = "image/jpeg", Size = 1024 }
            }
        };

        var result = await _sut.Handle(command, Ct);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NoVehicleId_UsesPendingInStorageKey()
    {
        var uploadResponse = new UploadUrlResponse
        {
            UploadUrl = "https://storage.example.com/presigned",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Headers = new()
        };

        _storageService.Setup(s => s.GenerateUploadUrlAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(uploadResponse);

        var command = new GetPresignedUrlsBatchCommand
        {
            OwnerId = "user123",
            DealerId = Guid.NewGuid(),
            VehicleId = null, // no vehicle ID
            Category = "vehicles",
            Files = new List<FileUploadInfo>
            {
                new() { FileName = "car.jpg", ContentType = "image/jpeg", Size = 1024 }
            }
        };

        var result = await _sut.Handle(command, Ct);

        result.Success.Should().BeTrue();
        result.Data!.Items[0].StorageKey.Should().Contain("pending");
    }
}
