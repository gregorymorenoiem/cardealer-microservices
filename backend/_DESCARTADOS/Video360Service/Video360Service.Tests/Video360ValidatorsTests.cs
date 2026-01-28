using FluentAssertions;
using Video360Service.Application.DTOs;
using Video360Service.Application.Features.Commands;
using Video360Service.Application.Validators;
using Video360Service.Domain.Entities;
using Xunit;

namespace Video360Service.Tests;

public class Video360ValidatorsTests
{
    private readonly CreateVideo360JobRequestValidator _requestValidator;
    private readonly CreateVideo360JobCommandValidator _commandValidator;

    public Video360ValidatorsTests()
    {
        _requestValidator = new CreateVideo360JobRequestValidator();
        _commandValidator = new CreateVideo360JobCommandValidator();
    }

    [Fact]
    public void CreateVideo360JobRequest_ValidRequest_ShouldPass()
    {
        // Arrange
        var request = new CreateVideo360JobRequest
        {
            VehicleId = Guid.NewGuid(),
            FrameCount = 6
        };

        // Act
        var result = _requestValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void CreateVideo360JobRequest_EmptyVehicleId_ShouldFail()
    {
        // Arrange
        var request = new CreateVideo360JobRequest
        {
            VehicleId = Guid.Empty,
            FrameCount = 6
        };

        // Act
        var result = _requestValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VehicleId");
    }

    [Theory]
    [InlineData(3)]  // Too few
    [InlineData(13)] // Too many
    public void CreateVideo360JobRequest_InvalidFrameCount_ShouldFail(int frameCount)
    {
        // Arrange
        var request = new CreateVideo360JobRequest
        {
            VehicleId = Guid.NewGuid(),
            FrameCount = frameCount
        };

        // Act
        var result = _requestValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FrameCount");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(12)]
    public void CreateVideo360JobRequest_ValidFrameCount_ShouldPass(int frameCount)
    {
        // Arrange
        var request = new CreateVideo360JobRequest
        {
            VehicleId = Guid.NewGuid(),
            FrameCount = frameCount
        };

        // Act
        var result = _requestValidator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "FrameCount");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("bmp")]
    [InlineData("tiff")]
    public void CreateVideo360JobRequest_InvalidOutputFormat_ShouldFail(string format)
    {
        // Arrange
        var request = new CreateVideo360JobRequest
        {
            VehicleId = Guid.NewGuid(),
            FrameCount = 6,
            OutputFormat = format
        };

        // Act
        var result = _requestValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OutputFormat");
    }

    [Theory]
    [InlineData("jpg")]
    [InlineData("png")]
    [InlineData("webp")]
    public void CreateVideo360JobRequest_ValidOutputFormat_ShouldPass(string format)
    {
        // Arrange
        var request = new CreateVideo360JobRequest
        {
            VehicleId = Guid.NewGuid(),
            FrameCount = 6,
            OutputFormat = format
        };

        // Act
        var result = _requestValidator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "OutputFormat");
    }

    [Fact]
    public void CreateVideo360JobCommand_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateVideo360JobCommand
        {
            UserId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            VideoUrl = "https://example.com/video.mp4",
            OriginalFileName = "test_video.mp4",
            FileSizeBytes = 10_000_000,
            Options = new ProcessingOptions()
        };

        // Act
        var result = _commandValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void CreateVideo360JobCommand_EmptyUserId_ShouldFail()
    {
        // Arrange
        var command = new CreateVideo360JobCommand
        {
            UserId = Guid.Empty,
            VehicleId = Guid.NewGuid(),
            VideoUrl = "https://example.com/video.mp4",
            OriginalFileName = "test_video.mp4",
            FileSizeBytes = 10_000_000
        };

        // Act
        var result = _commandValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Theory]
    [InlineData("video.txt")]
    [InlineData("video.pdf")]
    [InlineData("video")]
    public void CreateVideo360JobCommand_InvalidFileExtension_ShouldFail(string fileName)
    {
        // Arrange
        var command = new CreateVideo360JobCommand
        {
            UserId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            VideoUrl = "https://example.com/video.mp4",
            OriginalFileName = fileName,
            FileSizeBytes = 10_000_000
        };

        // Act
        var result = _commandValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OriginalFileName");
    }

    [Theory]
    [InlineData("video.mp4")]
    [InlineData("test.mov")]
    [InlineData("clip.avi")]
    [InlineData("video.webm")]
    [InlineData("movie.mkv")]
    public void CreateVideo360JobCommand_ValidFileExtension_ShouldPass(string fileName)
    {
        // Arrange
        var command = new CreateVideo360JobCommand
        {
            UserId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            VideoUrl = "https://example.com/video.mp4",
            OriginalFileName = fileName,
            FileSizeBytes = 10_000_000,
            Options = new ProcessingOptions()
        };

        // Act
        var result = _commandValidator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "OriginalFileName");
    }

    [Fact]
    public void CreateVideo360JobCommand_FileTooLarge_ShouldFail()
    {
        // Arrange
        var command = new CreateVideo360JobCommand
        {
            UserId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            VideoUrl = "https://example.com/video.mp4",
            OriginalFileName = "test_video.mp4",
            FileSizeBytes = 600_000_000, // 600 MB (limit is 500 MB)
            Options = new ProcessingOptions()
        };

        // Act
        var result = _commandValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FileSizeBytes");
    }
}
