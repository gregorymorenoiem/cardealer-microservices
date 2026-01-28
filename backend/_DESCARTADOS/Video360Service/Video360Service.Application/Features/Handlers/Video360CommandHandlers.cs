using MediatR;
using Microsoft.Extensions.Logging;
using Video360Service.Application.DTOs;
using Video360Service.Application.Features.Commands;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Application.Features.Handlers;

/// <summary>
/// Handler para crear un trabajo de procesamiento de video 360
/// </summary>
public class CreateVideo360JobHandler : IRequestHandler<CreateVideo360JobCommand, Video360JobResponse>
{
    private readonly IVideo360JobRepository _jobRepository;
    private readonly ILogger<CreateVideo360JobHandler> _logger;

    public CreateVideo360JobHandler(
        IVideo360JobRepository jobRepository,
        ILogger<CreateVideo360JobHandler> logger)
    {
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async Task<Video360JobResponse> Handle(CreateVideo360JobCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creando trabajo de video 360 para vehículo {VehicleId}", request.VehicleId);

        var job = new Video360Job
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            VehicleId = request.VehicleId,
            VideoUrl = request.VideoUrl,
            OriginalFileName = request.OriginalFileName,
            FileSizeBytes = request.FileSizeBytes,
            FramesToExtract = request.Options.FrameCount,
            Options = request.Options,
            Status = Video360JobStatus.Queued
        };

        var createdJob = await _jobRepository.CreateAsync(job, cancellationToken);
        var queuePosition = await _jobRepository.GetQueuePositionAsync(createdJob.Id, cancellationToken);

        _logger.LogInformation("Trabajo {JobId} creado en posición {Position} de la cola", 
            createdJob.Id, queuePosition);

        return MapToResponse(createdJob, queuePosition);
    }

    private static Video360JobResponse MapToResponse(Video360Job job, int? queuePosition = null)
    {
        return new Video360JobResponse
        {
            Id = job.Id,
            VehicleId = job.VehicleId,
            UserId = job.UserId,
            VideoUrl = job.VideoUrl,
            OriginalFileName = job.OriginalFileName,
            FileSizeBytes = job.FileSizeBytes,
            DurationSeconds = job.DurationSeconds,
            FramesToExtract = job.FramesToExtract,
            Status = job.Status,
            Progress = job.Progress,
            ErrorMessage = job.ErrorMessage,
            QueuePosition = queuePosition,
            ProcessingStartedAt = job.ProcessingStartedAt,
            ProcessingCompletedAt = job.ProcessingCompletedAt,
            ProcessingDurationMs = job.ProcessingDurationMs,
            CreatedAt = job.CreatedAt,
            Frames = job.ExtractedFrames.Select(f => new ExtractedFrameResponse
            {
                Id = f.Id,
                SequenceNumber = f.SequenceNumber,
                AngleDegrees = f.AngleDegrees,
                ViewName = f.ViewName,
                ImageUrl = f.ImageUrl,
                ThumbnailUrl = f.ThumbnailUrl,
                Width = f.Width,
                Height = f.Height,
                FileSizeBytes = f.FileSizeBytes,
                Format = f.Format,
                QualityScore = f.QualityScore,
                IsPrimary = f.IsPrimary,
                TimestampSeconds = f.TimestampSeconds
            }).OrderBy(f => f.SequenceNumber).ToList()
        };
    }
}

/// <summary>
/// Handler para procesar un video 360
/// </summary>
public class ProcessVideo360Handler : IRequestHandler<ProcessVideo360Command, Video360JobResponse>
{
    private readonly IVideo360JobRepository _jobRepository;
    private readonly IExtractedFrameRepository _frameRepository;
    private readonly IVideo360Processor _processor;
    private readonly IStorageService _storageService;
    private readonly ILogger<ProcessVideo360Handler> _logger;

    public ProcessVideo360Handler(
        IVideo360JobRepository jobRepository,
        IExtractedFrameRepository frameRepository,
        IVideo360Processor processor,
        IStorageService storageService,
        ILogger<ProcessVideo360Handler> logger)
    {
        _jobRepository = jobRepository;
        _frameRepository = frameRepository;
        _processor = processor;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<Video360JobResponse> Handle(ProcessVideo360Command request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
        {
            throw new KeyNotFoundException($"Job {request.JobId} not found");
        }

        _logger.LogInformation("Iniciando procesamiento de video 360 para job {JobId}", job.Id);
        
        job.StartProcessing();
        await _jobRepository.UpdateAsync(job, cancellationToken);

        try
        {
            // Descargar video a ruta temporal
            var tempDir = Path.Combine(Path.GetTempPath(), "video360", job.Id.ToString());
            Directory.CreateDirectory(tempDir);
            var videoPath = Path.Combine(tempDir, "input.mp4");

            job.Status = Video360JobStatus.Processing;
            job.UpdateProgress(5);
            await _jobRepository.UpdateAsync(job, cancellationToken);

            // Descargar el video
            await _storageService.DownloadToPathAsync(job.VideoUrl, videoPath, cancellationToken);

            job.Status = Video360JobStatus.ExtractingFrames;
            job.UpdateProgress(10);
            await _jobRepository.UpdateAsync(job, cancellationToken);

            // Procesar el video con OpenCV
            var progress = new Progress<int>(p =>
            {
                job.UpdateProgress(10 + (int)(p * 0.6)); // 10-70%
                _jobRepository.UpdateAsync(job, cancellationToken).Wait();
            });

            var result = await _processor.ProcessVideoAsync(videoPath, job.Options, progress, cancellationToken);

            if (!result.Success)
            {
                job.Fail(result.ErrorMessage ?? "Error desconocido en procesamiento");
                await _jobRepository.UpdateAsync(job, cancellationToken);
                return MapToResponse(job);
            }

            // Actualizar duración del video
            if (result.VideoInfo != null)
            {
                job.DurationSeconds = result.VideoInfo.DurationSeconds;
            }

            job.Status = Video360JobStatus.UploadingImages;
            job.UpdateProgress(70);
            await _jobRepository.UpdateAsync(job, cancellationToken);

            // Subir frames al storage y crear registros
            var frames = new List<ExtractedFrame>();
            var frameCount = result.Frames.Count;
            
            for (int i = 0; i < frameCount; i++)
            {
                var frameResult = result.Frames[i];
                var frame = ExtractedFrame.CreateForPosition(frameResult.SequenceNumber, job.Id);
                
                // Subir imagen principal
                var uploadResult = await _storageService.UploadFromPathAsync(
                    frameResult.LocalFilePath,
                    $"vehicles/{job.VehicleId}/360",
                    cancellationToken);

                if (uploadResult.Success)
                {
                    frame.ImageUrl = uploadResult.Url;
                    frame.FileSizeBytes = uploadResult.FileSizeBytes;
                }

                // Subir thumbnail si existe
                if (!string.IsNullOrEmpty(frameResult.ThumbnailPath))
                {
                    var thumbResult = await _storageService.UploadFromPathAsync(
                        frameResult.ThumbnailPath,
                        $"vehicles/{job.VehicleId}/360/thumbnails",
                        cancellationToken);

                    if (thumbResult.Success)
                    {
                        frame.ThumbnailUrl = thumbResult.Url;
                    }
                }

                frame.Width = frameResult.Width;
                frame.Height = frameResult.Height;
                frame.SourceFrameNumber = frameResult.SourceFrameNumber;
                frame.TimestampSeconds = frameResult.TimestampSeconds;
                frame.QualityScore = frameResult.QualityScore;
                frame.Format = job.Options.OutputFormat;
                
                frames.Add(frame);

                // Actualizar progreso
                job.UpdateProgress(70 + (int)((i + 1) / (double)frameCount * 25)); // 70-95%
                await _jobRepository.UpdateAsync(job, cancellationToken);
            }

            // Guardar frames en base de datos
            await _frameRepository.CreateManyAsync(frames, cancellationToken);

            // Limpiar archivos temporales
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error limpiando directorio temporal {TempDir}", tempDir);
            }

            // Completar el trabajo
            job.Complete();
            await _jobRepository.UpdateAsync(job, cancellationToken);

            _logger.LogInformation("Procesamiento completado para job {JobId}: {FrameCount} frames extraídos", 
                job.Id, frames.Count);

            // Recargar con frames
            var completedJob = await _jobRepository.GetByIdWithFramesAsync(job.Id, cancellationToken);
            return MapToResponse(completedJob!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando video 360 para job {JobId}", job.Id);
            job.Fail(ex.Message);
            await _jobRepository.UpdateAsync(job, cancellationToken);
            return MapToResponse(job);
        }
    }

    private static Video360JobResponse MapToResponse(Video360Job job)
    {
        return new Video360JobResponse
        {
            Id = job.Id,
            VehicleId = job.VehicleId,
            UserId = job.UserId,
            VideoUrl = job.VideoUrl,
            OriginalFileName = job.OriginalFileName,
            FileSizeBytes = job.FileSizeBytes,
            DurationSeconds = job.DurationSeconds,
            FramesToExtract = job.FramesToExtract,
            Status = job.Status,
            Progress = job.Progress,
            ErrorMessage = job.ErrorMessage,
            ProcessingStartedAt = job.ProcessingStartedAt,
            ProcessingCompletedAt = job.ProcessingCompletedAt,
            ProcessingDurationMs = job.ProcessingDurationMs,
            CreatedAt = job.CreatedAt,
            Frames = job.ExtractedFrames.Select(f => new ExtractedFrameResponse
            {
                Id = f.Id,
                SequenceNumber = f.SequenceNumber,
                AngleDegrees = f.AngleDegrees,
                ViewName = f.ViewName,
                ImageUrl = f.ImageUrl,
                ThumbnailUrl = f.ThumbnailUrl,
                Width = f.Width,
                Height = f.Height,
                FileSizeBytes = f.FileSizeBytes,
                Format = f.Format,
                QualityScore = f.QualityScore,
                IsPrimary = f.IsPrimary,
                TimestampSeconds = f.TimestampSeconds
            }).OrderBy(f => f.SequenceNumber).ToList()
        };
    }
}
