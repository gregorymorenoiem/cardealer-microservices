using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Video360Service.Application.DTOs;
using Video360Service.Application.Features.Commands;
using Video360Service.Application.Interfaces;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Application.Features.Handlers;

/// <summary>
/// Handler para crear un nuevo job de extracción 360
/// </summary>
public class CreateVideo360JobCommandHandler : IRequestHandler<CreateVideo360JobCommand, Video360JobResponse>
{
    private readonly IVideo360JobRepository _jobRepository;
    private readonly IVideo360Orchestrator _orchestrator;
    private readonly IVideoStorageService _storageService;
    private readonly ILogger<CreateVideo360JobCommandHandler> _logger;

    public CreateVideo360JobCommandHandler(
        IVideo360JobRepository jobRepository,
        IVideo360Orchestrator orchestrator,
        IVideoStorageService storageService,
        ILogger<CreateVideo360JobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _orchestrator = orchestrator;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<Video360JobResponse> Handle(CreateVideo360JobCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        _logger.LogInformation("Creating Video360 job for user {UserId}, vehicle {VehicleId}", 
            command.UserId, request.VehicleId);
        
        // Crear el job
        var job = new Video360Job
        {
            UserId = command.UserId,
            VehicleId = request.VehicleId,
            TenantId = command.TenantId,
            OriginalFileName = request.FileName ?? "video.mp4",
            FrameCount = request.FrameCount,
            OutputFormat = request.OutputFormat,
            OutputQuality = request.OutputQuality,
            CallbackUrl = request.CallbackUrl,
            CorrelationId = request.CorrelationId,
            Priority = request.Priority,
            ProcessedSync = request.ProcessSync,
            MetadataJson = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null
        };
        
        // Determinar proveedor
        if (request.PreferredProvider.HasValue)
        {
            job.Provider = request.PreferredProvider.Value;
        }
        
        // Obtener bytes del video
        byte[] videoBytes;
        if (command.VideoBytes != null)
        {
            videoBytes = command.VideoBytes;
        }
        else if (!string.IsNullOrEmpty(request.VideoBase64))
        {
            var base64Data = request.VideoBase64;
            if (base64Data.Contains(','))
            {
                base64Data = base64Data.Split(',')[1];
            }
            videoBytes = Convert.FromBase64String(base64Data);
        }
        else if (!string.IsNullOrEmpty(request.VideoUrl))
        {
            // Descargar video
            job.SourceVideoUrl = request.VideoUrl;
            job.Status = ProcessingStatus.Uploading;
            job = await _jobRepository.CreateAsync(job, cancellationToken);
            
            try
            {
                videoBytes = await _storageService.DownloadAsync(request.VideoUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download video from URL: {Url}", request.VideoUrl);
                job.MarkAsFailed($"Error descargando video: {ex.Message}", "DOWNLOAD_ERROR");
                await _jobRepository.UpdateAsync(job, cancellationToken);
                return MapToResponse(job);
            }
        }
        else
        {
            throw new ArgumentException("Debe proporcionar VideoUrl, VideoBase64, o VideoBytes");
        }
        
        job.OriginalFileSizeBytes = videoBytes.Length;
        
        // Si el job no fue creado aún (caso Base64/bytes), crearlo
        if (job.Id == Guid.Empty || job.Status == ProcessingStatus.Pending)
        {
            job = await _jobRepository.CreateAsync(job, cancellationToken);
        }
        
        // Procesar de forma síncrona si se solicita
        if (request.ProcessSync)
        {
            try
            {
                job = await _orchestrator.ProcessVideoAsync(job, videoBytes, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing video for job {JobId}", job.Id);
                job.MarkAsFailed($"Error procesando video: {ex.Message}", "PROCESSING_ERROR");
            }
            
            await _jobRepository.UpdateAsync(job, cancellationToken);
        }
        else
        {
            // TODO: Encolar para procesamiento asíncrono (RabbitMQ/Background Service)
            _logger.LogInformation("Job {JobId} queued for async processing", job.Id);
        }
        
        return MapToResponse(job);
    }
    
    private static Video360JobResponse MapToResponse(Video360Job job)
    {
        return new Video360JobResponse
        {
            JobId = job.Id,
            VehicleId = job.VehicleId,
            Status = job.Status,
            Provider = job.Provider,
            SourceVideoUrl = job.SourceVideoUrl,
            VideoDurationSeconds = job.VideoDurationSeconds,
            VideoResolution = job.VideoResolution,
            Frames = job.ExtractedFrames.Select(f => new ExtractedFrameResponse
            {
                FrameId = f.Id,
                Index = f.FrameIndex,
                AngleDegrees = f.AngleDegrees,
                AngleLabel = f.AngleLabel ?? ExtractedFrame.GetAngleLabelByIndex(f.FrameIndex),
                TimestampSeconds = f.TimestampSeconds,
                ImageUrl = f.ImageUrl,
                ThumbnailUrl = f.ThumbnailUrl,
                FileSizeBytes = f.FileSizeBytes,
                ContentType = f.ContentType,
                Width = f.Width,
                Height = f.Height
            }).OrderBy(f => f.Index).ToList(),
            ProcessingTimeMs = job.ProcessingTimeMs,
            CostUsd = job.CostUsd,
            ErrorMessage = job.ErrorMessage,
            ErrorCode = job.ErrorCode,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
            Metadata = job.MetadataJson != null 
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(job.MetadataJson) 
                : null
        };
    }
}

/// <summary>
/// Handler para cancelar un job
/// </summary>
public class CancelVideo360JobCommandHandler : IRequestHandler<CancelVideo360JobCommand, bool>
{
    private readonly IVideo360JobRepository _jobRepository;
    private readonly ILogger<CancelVideo360JobCommandHandler> _logger;

    public CancelVideo360JobCommandHandler(
        IVideo360JobRepository jobRepository,
        ILogger<CancelVideo360JobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelVideo360JobCommand command, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(command.JobId, cancellationToken);
        
        if (job == null)
        {
            _logger.LogWarning("Job {JobId} not found for cancellation", command.JobId);
            return false;
        }
        
        // Verificar ownership si hay UserId
        if (command.UserId.HasValue && job.UserId != command.UserId)
        {
            _logger.LogWarning("User {UserId} not authorized to cancel job {JobId}", command.UserId, command.JobId);
            return false;
        }
        
        // Solo se pueden cancelar jobs pendientes o en proceso
        if (job.Status != ProcessingStatus.Pending && !job.IsInProgress)
        {
            _logger.LogWarning("Job {JobId} cannot be cancelled, status: {Status}", command.JobId, job.Status);
            return false;
        }
        
        job.Status = ProcessingStatus.Cancelled;
        job.UpdatedAt = DateTime.UtcNow;
        
        await _jobRepository.UpdateAsync(job, cancellationToken);
        
        _logger.LogInformation("Job {JobId} cancelled", command.JobId);
        return true;
    }
}

/// <summary>
/// Handler para reintentar un job fallido
/// </summary>
public class RetryVideo360JobCommandHandler : IRequestHandler<RetryVideo360JobCommand, Video360JobResponse>
{
    private readonly IVideo360Orchestrator _orchestrator;
    private readonly ILogger<RetryVideo360JobCommandHandler> _logger;

    public RetryVideo360JobCommandHandler(
        IVideo360Orchestrator orchestrator,
        ILogger<RetryVideo360JobCommandHandler> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task<Video360JobResponse> Handle(RetryVideo360JobCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrying job {JobId}", command.JobId);
        
        var job = await _orchestrator.RetryJobAsync(command.JobId, cancellationToken);
        
        return new Video360JobResponse
        {
            JobId = job.Id,
            VehicleId = job.VehicleId,
            Status = job.Status,
            Provider = job.Provider,
            SourceVideoUrl = job.SourceVideoUrl,
            VideoDurationSeconds = job.VideoDurationSeconds,
            VideoResolution = job.VideoResolution,
            Frames = job.ExtractedFrames.Select(f => new ExtractedFrameResponse
            {
                FrameId = f.Id,
                Index = f.FrameIndex,
                AngleDegrees = f.AngleDegrees,
                AngleLabel = f.AngleLabel ?? ExtractedFrame.GetAngleLabelByIndex(f.FrameIndex),
                TimestampSeconds = f.TimestampSeconds,
                ImageUrl = f.ImageUrl,
                ThumbnailUrl = f.ThumbnailUrl,
                FileSizeBytes = f.FileSizeBytes,
                ContentType = f.ContentType,
                Width = f.Width,
                Height = f.Height
            }).OrderBy(f => f.Index).ToList(),
            ProcessingTimeMs = job.ProcessingTimeMs,
            CostUsd = job.CostUsd,
            ErrorMessage = job.ErrorMessage,
            ErrorCode = job.ErrorCode,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt
        };
    }
}
