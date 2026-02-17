using MediatR;
using Microsoft.Extensions.Logging;
using Vehicle360ProcessingService.Application.DTOs;
using Vehicle360ProcessingService.Application.Features.Commands;
using Vehicle360ProcessingService.Domain.Entities;
using Vehicle360ProcessingService.Domain.Interfaces;

namespace Vehicle360ProcessingService.Application.Features.Handlers;

/// <summary>
/// Handler para iniciar el procesamiento 360
/// </summary>
public class StartVehicle360ProcessingHandler : IRequestHandler<StartVehicle360ProcessingCommand, StartProcessingResponse>
{
    private readonly IVehicle360JobRepository _repository;
    private readonly IMediaServiceClient _mediaClient;
    private readonly ILogger<StartVehicle360ProcessingHandler> _logger;

    public StartVehicle360ProcessingHandler(
        IVehicle360JobRepository repository,
        IMediaServiceClient mediaClient,
        ILogger<StartVehicle360ProcessingHandler> logger)
    {
        _repository = repository;
        _mediaClient = mediaClient;
        _logger = logger;
    }

    public async Task<StartProcessingResponse> Handle(
        StartVehicle360ProcessingCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting 360 processing for vehicle {VehicleId}", request.VehicleId);

        // Crear el job
        var job = new Vehicle360Job
        {
            VehicleId = request.VehicleId,
            UserId = request.UserId,
            TenantId = request.TenantId,
            FrameCount = request.FrameCount,
            Options = request.Options,
            OriginalFileName = request.VideoFileName,
            FileSizeBytes = request.VideoSize,
            VideoContentType = request.VideoContentType,
            ClientIpAddress = request.ClientIpAddress,
            UserAgent = request.UserAgent,
            CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString(),
            Status = Vehicle360JobStatus.Queued
        };

        // Si el video viene como stream, subirlo a S3 primero
        if (request.VideoStream != null && !string.IsNullOrEmpty(request.VideoFileName))
        {
            job.Status = Vehicle360JobStatus.UploadingVideo;
            
            var uploadResult = await _mediaClient.UploadVideoAsync(
                request.VideoStream,
                request.VideoFileName,
                request.VideoContentType ?? "video/mp4",
                $"videos/360/{request.VehicleId}",
                cancellationToken);

            if (!uploadResult.Success)
            {
                job.Status = Vehicle360JobStatus.Failed;
                job.ErrorMessage = uploadResult.ErrorMessage ?? "Failed to upload video";
                job.ErrorCode = uploadResult.ErrorCode;
                await _repository.CreateAsync(job, cancellationToken);

                return new StartProcessingResponse
                {
                    JobId = job.Id,
                    Status = job.Status.ToString(),
                    Message = job.ErrorMessage
                };
            }

            job.OriginalVideoUrl = uploadResult.Url;
            job.MediaUploadId = uploadResult.PublicId;
            job.Status = Vehicle360JobStatus.VideoUploaded;
        }
        else if (!string.IsNullOrEmpty(request.VideoUrl))
        {
            job.OriginalVideoUrl = request.VideoUrl;
            job.Status = Vehicle360JobStatus.VideoUploaded;
        }
        else
        {
            return new StartProcessingResponse
            {
                JobId = Guid.Empty,
                Status = "Failed",
                Message = "Either VideoStream or VideoUrl must be provided"
            };
        }

        // Guardar el job en DB
        job.Status = Vehicle360JobStatus.Queued;
        await _repository.CreateAsync(job, cancellationToken);

        // Obtener posición en cola
        var queuePosition = await _repository.GetQueuePositionAsync(job.Id, cancellationToken);

        _logger.LogInformation(
            "Job {JobId} created for vehicle {VehicleId}, queue position: {QueuePosition}", 
            job.Id, request.VehicleId, queuePosition);

        return new StartProcessingResponse
        {
            JobId = job.Id,
            Status = job.Status.ToString(),
            Message = "Video uploaded successfully. Processing queued.",
            QueuePosition = queuePosition,
            EstimatedWaitSeconds = queuePosition * 120 // ~2 min per job estimate
        };
    }
}

/// <summary>
/// Handler para procesar un job (orquestación principal)
/// </summary>
public class ProcessVehicle360JobHandler : IRequestHandler<ProcessVehicle360JobCommand, Vehicle360JobResponse>
{
    private readonly IVehicle360JobRepository _repository;
    private readonly IVideo360ServiceClient _video360Client;
    private readonly IBackgroundRemovalClient _bgRemovalClient;
    private readonly IMediaServiceClient _mediaClient;
    private readonly ILogger<ProcessVehicle360JobHandler> _logger;

    public ProcessVehicle360JobHandler(
        IVehicle360JobRepository repository,
        IVideo360ServiceClient video360Client,
        IBackgroundRemovalClient bgRemovalClient,
        IMediaServiceClient mediaClient,
        ILogger<ProcessVehicle360JobHandler> logger)
    {
        _repository = repository;
        _video360Client = video360Client;
        _bgRemovalClient = bgRemovalClient;
        _mediaClient = mediaClient;
        _logger = logger;
    }

    public async Task<Vehicle360JobResponse> Handle(
        ProcessVehicle360JobCommand request, 
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
        {
            throw new ArgumentException($"Job {request.JobId} not found");
        }

        _logger.LogInformation("Processing job {JobId} for vehicle {VehicleId}", job.Id, job.VehicleId);

        try
        {
            job.StartProcessing();
            await _repository.UpdateAsync(job, cancellationToken);

            // PASO 1: Extraer frames con Video360Service
            _logger.LogInformation("Step 1: Extracting frames from video for job {JobId}", job.Id);
            job.UpdateProgress(10, Vehicle360JobStatus.ExtractingFrames);
            await _repository.UpdateAsync(job, cancellationToken);

            var video360Result = await _video360Client.ProcessVideoAsync(
                job.OriginalVideoUrl!,
                job.VehicleId,
                job.FrameCount,
                new Video360Options
                {
                    OutputWidth = job.Options.OutputWidth,
                    OutputHeight = job.Options.OutputHeight,
                    OutputFormat = "jpg", // JPG para frames intermedios
                    JpegQuality = job.Options.JpegQuality,
                    SmartFrameSelection = job.Options.SmartFrameSelection,
                    AutoCorrectExposure = job.Options.AutoCorrectExposure,
                    GenerateThumbnails = job.Options.GenerateThumbnails
                },
                cancellationToken);

            if (!video360Result.Success || video360Result.Frames.Count == 0)
            {
                throw new InvalidOperationException(
                    video360Result.ErrorMessage ?? "Video360Service failed to extract frames");
            }

            // Actualizar job con frames extraídos
            var extractedFrames = video360Result.Frames.Select(f => new ExtractedFrameInfo
            {
                SequenceNumber = f.SequenceNumber,
                ViewName = f.ViewName,
                AngleDegrees = f.AngleDegrees,
                ImageUrl = f.ImageUrl,
                ThumbnailUrl = f.ThumbnailUrl
            }).ToList();

            job.SetFramesExtracted(video360Result.JobId!.Value, extractedFrames);
            await _repository.UpdateAsync(job, cancellationToken);

            _logger.LogInformation(
                "Extracted {FrameCount} frames for job {JobId}", 
                extractedFrames.Count, job.Id);

            // PASO 2: Remover fondos con BackgroundRemovalService
            _logger.LogInformation("Step 2: Removing backgrounds for job {JobId}", job.Id);
            job.UpdateProgress(45, Vehicle360JobStatus.RemovingBackgrounds);
            await _repository.UpdateAsync(job, cancellationToken);

            var imageUrls = job.ProcessedFrames.Select(f => f.OriginalImageUrl).ToList();
            var bgResults = await _bgRemovalClient.RemoveBackgroundBatchAsync(
                imageUrls,
                new BackgroundRemovalOptions
                {
                    OutputFormat = "png",
                    BackgroundColor = job.Options.BackgroundColor,
                    OutputWidth = job.Options.OutputWidth,
                    OutputHeight = job.Options.OutputHeight
                },
                maxConcurrency: 3,
                cancellationToken);

            // Actualizar cada frame con su imagen procesada
            var removalJobIds = new List<Guid>();
            for (int i = 0; i < bgResults.Count; i++)
            {
                var bgResult = bgResults[i];
                var frame = job.ProcessedFrames[i];

                if (bgResult.Success && !string.IsNullOrEmpty(bgResult.ProcessedImageUrl))
                {
                    job.UpdateFrameProcessed(
                        frame.SequenceNumber, 
                        bgResult.ProcessedImageUrl, 
                        bgResult.JobId);
                    
                    if (bgResult.JobId.HasValue)
                        removalJobIds.Add(bgResult.JobId.Value);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to remove background for frame {FrameNumber}: {Error}",
                        frame.SequenceNumber, bgResult.ErrorMessage);
                    
                    // Usar imagen original si falla la remoción
                    frame.ProcessedImageUrl = frame.OriginalImageUrl;
                    frame.Status = FrameProcessingStatus.Failed;
                    frame.ErrorMessage = bgResult.ErrorMessage;
                }
            }

            job.BackgroundRemovalJobIds = removalJobIds;
            await _repository.UpdateAsync(job, cancellationToken);

            // PASO 3: Subir imágenes finales a S3 (si las URLs son temporales)
            _logger.LogInformation("Step 3: Uploading final images for job {JobId}", job.Id);
            job.UpdateProgress(85, Vehicle360JobStatus.UploadingResults);
            await _repository.UpdateAsync(job, cancellationToken);

            foreach (var frame in job.ProcessedFrames)
            {
                if (!string.IsNullOrEmpty(frame.ProcessedImageUrl) && 
                    frame.Status == FrameProcessingStatus.Completed)
                {
                    // Re-subir a nuestro S3 para URLs permanentes
                    var finalFileName = $"frame_{frame.SequenceNumber:D2}_{frame.ViewName.ToLower().Replace(" ", "_")}.png";
                    var uploadResult = await _mediaClient.UploadImageFromUrlAsync(
                        frame.ProcessedImageUrl,
                        finalFileName,
                        $"vehicles/{job.VehicleId}/360",
                        cancellationToken);

                    if (uploadResult.Success)
                    {
                        frame.ProcessedImageUrl = uploadResult.Url;
                    }
                }
            }

            await _repository.UpdateAsync(job, cancellationToken);

            // Completar el job
            job.Complete();
            await _repository.UpdateAsync(job, cancellationToken);

            _logger.LogInformation(
                "Job {JobId} completed successfully in {DurationMs}ms",
                job.Id, job.ProcessingDurationMs);

            return Vehicle360JobResponse.FromEntity(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed: {Error}", job.Id, ex.Message);
            
            job.Fail(ex.Message, ex.GetType().Name);
            await _repository.UpdateAsync(job, cancellationToken);

            return Vehicle360JobResponse.FromEntity(job);
        }
    }
}

/// <summary>
/// Handler para reintentar un job fallido
/// </summary>
public class RetryVehicle360JobHandler : IRequestHandler<RetryVehicle360JobCommand, Vehicle360JobResponse>
{
    private readonly IVehicle360JobRepository _repository;
    private readonly IMediator _mediator;
    private readonly ILogger<RetryVehicle360JobHandler> _logger;

    public RetryVehicle360JobHandler(
        IVehicle360JobRepository repository,
        IMediator mediator,
        ILogger<RetryVehicle360JobHandler> logger)
    {
        _repository = repository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Vehicle360JobResponse> Handle(
        RetryVehicle360JobCommand request, 
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
        {
            throw new ArgumentException($"Job {request.JobId} not found");
        }

        if (job.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You don't have permission to retry this job");
        }

        if (!job.CanRetry())
        {
            throw new InvalidOperationException(
                $"Job cannot be retried. Status: {job.Status}, RetryCount: {job.RetryCount}/{job.MaxRetries}");
        }

        _logger.LogInformation("Retrying job {JobId}, attempt {Attempt}", job.Id, job.RetryCount + 1);

        job.PrepareRetry();
        await _repository.UpdateAsync(job, cancellationToken);

        // Procesar el job nuevamente
        return await _mediator.Send(new ProcessVehicle360JobCommand { JobId = job.Id }, cancellationToken);
    }
}

/// <summary>
/// Handler para cancelar un job
/// </summary>
public class CancelVehicle360JobHandler : IRequestHandler<CancelVehicle360JobCommand, bool>
{
    private readonly IVehicle360JobRepository _repository;
    private readonly ILogger<CancelVehicle360JobHandler> _logger;

    public CancelVehicle360JobHandler(
        IVehicle360JobRepository repository,
        ILogger<CancelVehicle360JobHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelVehicle360JobCommand request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
        {
            return false;
        }

        if (job.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You don't have permission to cancel this job");
        }

        if (job.IsComplete || job.IsFailed)
        {
            return false;
        }

        _logger.LogInformation("Cancelling job {JobId}: {Reason}", job.Id, request.Reason);

        job.Status = Vehicle360JobStatus.Cancelled;
        job.ErrorMessage = request.Reason ?? "Cancelled by user";
        job.CompletedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(job, cancellationToken);

        return true;
    }
}
