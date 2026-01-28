using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Vehicle360ProcessingService.Application.Features.Commands;
using Vehicle360ProcessingService.Application.Features.Handlers;
using Vehicle360ProcessingService.Domain.Entities;
using Vehicle360ProcessingService.Domain.Interfaces;

namespace Vehicle360ProcessingService.Tests;

public class Vehicle360ProcessingTests
{
    private readonly Mock<IVehicle360JobRepository> _repositoryMock;
    private readonly Mock<IMediaServiceClient> _mediaClientMock;
    private readonly Mock<IVideo360ServiceClient> _video360ClientMock;
    private readonly Mock<IBackgroundRemovalClient> _bgRemovalClientMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<StartVehicle360ProcessingHandler>> _startLoggerMock;
    private readonly Mock<ILogger<ProcessVehicle360JobHandler>> _processLoggerMock;

    public Vehicle360ProcessingTests()
    {
        _repositoryMock = new Mock<IVehicle360JobRepository>();
        _mediaClientMock = new Mock<IMediaServiceClient>();
        _video360ClientMock = new Mock<IVideo360ServiceClient>();
        _bgRemovalClientMock = new Mock<IBackgroundRemovalClient>();
        _mediatorMock = new Mock<IMediator>();
        _startLoggerMock = new Mock<ILogger<StartVehicle360ProcessingHandler>>();
        _processLoggerMock = new Mock<ILogger<ProcessVehicle360JobHandler>>();
    }

    #region Entity Tests

    [Fact]
    public void Vehicle360Job_ShouldBeCreatedWithCorrectDefaults()
    {
        // Arrange & Act
        var job = new Vehicle360Job
        {
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        // Assert
        job.Id.Should().NotBeEmpty();
        job.Status.Should().Be(Vehicle360JobStatus.Pending);
        job.Progress.Should().Be(0);
        job.FrameCount.Should().Be(6);
        job.RetryCount.Should().Be(0);
        job.MaxRetries.Should().Be(3);
        job.ProcessedFrames.Should().BeEmpty();
        job.Options.Should().NotBeNull();
    }

    [Fact]
    public void Vehicle360Job_StartProcessing_ShouldUpdateStatus()
    {
        // Arrange
        var job = new Vehicle360Job
        {
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = Vehicle360JobStatus.Queued
        };

        // Act
        job.StartProcessing();

        // Assert
        job.Status.Should().Be(Vehicle360JobStatus.Processing);
        job.StartedAt.Should().NotBeNull();
        job.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Vehicle360Job_Complete_ShouldSetCompletedStatus()
    {
        // Arrange
        var job = new Vehicle360Job
        {
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = Vehicle360JobStatus.Processing
        };
        job.StartProcessing();

        // Act
        job.Complete();

        // Assert
        job.Status.Should().Be(Vehicle360JobStatus.Completed);
        job.IsComplete.Should().BeTrue();
        job.IsFailed.Should().BeFalse();
        job.CompletedAt.Should().NotBeNull();
        job.Progress.Should().Be(100);
    }

    [Fact]
    public void Vehicle360Job_Fail_ShouldSetFailedStatus()
    {
        // Arrange
        var job = new Vehicle360Job
        {
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = Vehicle360JobStatus.Processing
        };
        var errorMessage = "Test error message";
        var errorCode = "TEST_ERROR";

        // Act
        job.Fail(errorMessage, errorCode);

        // Assert
        job.Status.Should().Be(Vehicle360JobStatus.Failed);
        job.IsFailed.Should().BeTrue();
        job.IsComplete.Should().BeFalse();
        job.ErrorMessage.Should().Be(errorMessage);
        job.ErrorCode.Should().Be(errorCode);
    }

    [Fact]
    public void Vehicle360Job_CanRetry_ShouldReturnTrueWhenFailedAndRetryCountBelowMax()
    {
        // Arrange
        var job = new Vehicle360Job
        {
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = Vehicle360JobStatus.Failed,
            RetryCount = 1,
            MaxRetries = 3
        };

        // Act
        var canRetry = job.CanRetry();

        // Assert
        canRetry.Should().BeTrue();
    }

    [Fact]
    public void Vehicle360Job_CanRetry_ShouldReturnFalseWhenRetryCountAtMax()
    {
        // Arrange
        var job = new Vehicle360Job
        {
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = Vehicle360JobStatus.Failed,
            RetryCount = 3,
            MaxRetries = 3
        };

        // Act
        var canRetry = job.CanRetry();

        // Assert
        canRetry.Should().BeFalse();
    }

    [Fact]
    public void Vehicle360Job_SetFramesExtracted_ShouldPopulateProcessedFrames()
    {
        // Arrange
        var job = new Vehicle360Job
        {
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FrameCount = 6
        };
        var video360JobId = Guid.NewGuid();
        var frames = new List<ExtractedFrameInfo>
        {
            new() { SequenceNumber = 1, ViewName = "Front", AngleDegrees = 0, ImageUrl = "url1" },
            new() { SequenceNumber = 2, ViewName = "Front-Right", AngleDegrees = 60, ImageUrl = "url2" },
            new() { SequenceNumber = 3, ViewName = "Right", AngleDegrees = 120, ImageUrl = "url3" }
        };

        // Act
        job.SetFramesExtracted(video360JobId, frames);

        // Assert
        job.Status.Should().Be(Vehicle360JobStatus.FramesExtracted);
        job.Video360JobId.Should().Be(video360JobId);
        job.ProcessedFrames.Should().HaveCount(3);
        job.ProcessedFrames[0].ViewName.Should().Be("Front");
        job.ProcessedFrames[0].Status.Should().Be(FrameProcessingStatus.Pending);
    }

    [Fact]
    public void Vehicle360Job_UpdateFrameProcessed_ShouldUpdateCorrectFrame()
    {
        // Arrange
        var job = new Vehicle360Job
        {
            VehicleId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };
        var frames = new List<ExtractedFrameInfo>
        {
            new() { SequenceNumber = 1, ViewName = "Front", ImageUrl = "original1" },
            new() { SequenceNumber = 2, ViewName = "Right", ImageUrl = "original2" }
        };
        job.SetFramesExtracted(Guid.NewGuid(), frames);
        
        var bgJobId = Guid.NewGuid();

        // Act
        job.UpdateFrameProcessed(1, "processed1.png", bgJobId);

        // Assert
        var frame = job.ProcessedFrames.First(f => f.SequenceNumber == 1);
        frame.ProcessedImageUrl.Should().Be("processed1.png");
        frame.BackgroundRemovalJobId.Should().Be(bgJobId);
        frame.Status.Should().Be(FrameProcessingStatus.Completed);
    }

    #endregion

    #region Handler Tests

    [Fact]
    public async Task StartProcessingHandler_WithVideoUrl_ShouldCreateJobSuccessfully()
    {
        // Arrange
        var handler = new StartVehicle360ProcessingHandler(
            _repositoryMock.Object,
            _mediaClientMock.Object,
            _startLoggerMock.Object);

        var command = new StartVehicle360ProcessingCommand
        {
            UserId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            VideoUrl = "https://example.com/video.mp4",
            FrameCount = 6
        };

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Vehicle360Job>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle360Job job, CancellationToken _) => job);
        _repositoryMock.Setup(r => r.GetQueuePositionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().NotBeEmpty();
        result.Status.Should().Be("Queued");
        result.QueuePosition.Should().Be(1);
        
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Vehicle360Job>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartProcessingHandler_WithVideoStream_ShouldUploadAndCreateJob()
    {
        // Arrange
        var handler = new StartVehicle360ProcessingHandler(
            _repositoryMock.Object,
            _mediaClientMock.Object,
            _startLoggerMock.Object);

        var videoStream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        var command = new StartVehicle360ProcessingCommand
        {
            UserId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            VideoStream = videoStream,
            VideoFileName = "test.mp4",
            VideoContentType = "video/mp4",
            FrameCount = 6
        };

        _mediaClientMock.Setup(m => m.UploadVideoAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MediaUploadResult 
            { 
                Success = true, 
                Url = "https://s3.example.com/video.mp4",
                PublicId = "video123"
            });

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Vehicle360Job>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle360Job job, CancellationToken _) => job);
        _repositoryMock.Setup(r => r.GetQueuePositionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Queued");
        
        _mediaClientMock.Verify(m => m.UploadVideoAsync(
            It.IsAny<Stream>(),
            "test.mp4",
            "video/mp4",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartProcessingHandler_WithoutVideoOrUrl_ShouldReturnError()
    {
        // Arrange
        var handler = new StartVehicle360ProcessingHandler(
            _repositoryMock.Object,
            _mediaClientMock.Object,
            _startLoggerMock.Object);

        var command = new StartVehicle360ProcessingCommand
        {
            UserId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            FrameCount = 6
            // No VideoStream or VideoUrl
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.JobId.Should().BeEmpty();
        result.Status.Should().Be("Failed");
        result.Message.Should().Contain("VideoStream or VideoUrl must be provided");
    }

    #endregion

    #region Enum Tests

    [Theory]
    [InlineData(Vehicle360JobStatus.Pending)]
    [InlineData(Vehicle360JobStatus.Queued)]
    [InlineData(Vehicle360JobStatus.Processing)]
    [InlineData(Vehicle360JobStatus.ExtractingFrames)]
    [InlineData(Vehicle360JobStatus.RemovingBackgrounds)]
    [InlineData(Vehicle360JobStatus.Completed)]
    [InlineData(Vehicle360JobStatus.Failed)]
    public void Vehicle360JobStatus_ShouldHaveExpectedValues(Vehicle360JobStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(Vehicle360JobStatus), status).Should().BeTrue();
    }

    [Theory]
    [InlineData(FrameProcessingStatus.Pending)]
    [InlineData(FrameProcessingStatus.Processing)]
    [InlineData(FrameProcessingStatus.Completed)]
    [InlineData(FrameProcessingStatus.Failed)]
    public void FrameProcessingStatus_ShouldHaveExpectedValues(FrameProcessingStatus status)
    {
        // Assert
        Enum.IsDefined(typeof(FrameProcessingStatus), status).Should().BeTrue();
    }

    #endregion

    #region ProcessingOptions Tests

    [Fact]
    public void ProcessingOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var options = new ProcessingOptions();

        // Assert
        options.OutputWidth.Should().Be(1920);
        options.OutputHeight.Should().Be(1080);
        options.OutputFormat.Should().Be("png");
        options.JpegQuality.Should().Be(90);
        options.SmartFrameSelection.Should().BeTrue();
        options.AutoCorrectExposure.Should().BeTrue();
        options.GenerateThumbnails.Should().BeTrue();
        options.BackgroundColor.Should().Be("transparent");
    }

    #endregion
}
