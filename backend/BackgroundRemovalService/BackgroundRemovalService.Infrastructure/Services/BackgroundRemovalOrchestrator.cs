using System.Diagnostics;
using System.Text.Json;
using BackgroundRemovalService.Application.DTOs;
using BackgroundRemovalService.Application.Interfaces;
using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;
using BackgroundRemovalService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackgroundRemovalService.Infrastructure.Services;

/// <summary>
/// Servicio orquestador principal para remoción de fondo.
/// Coordina entre proveedores, maneja reintentos, fallbacks y persistencia.
/// </summary>
public class BackgroundRemovalOrchestrator : IBackgroundRemovalOrchestrator
{
    private readonly IBackgroundRemovalProviderFactory _providerFactory;
    private readonly IBackgroundRemovalJobRepository _jobRepository;
    private readonly IProviderConfigurationRepository _providerConfigRepository;
    private readonly IUsageRecordRepository _usageRepository;
    private readonly IImageStorageService _storageService;
    private readonly ILogger<BackgroundRemovalOrchestrator> _logger;

    public BackgroundRemovalOrchestrator(
        IBackgroundRemovalProviderFactory providerFactory,
        IBackgroundRemovalJobRepository jobRepository,
        IProviderConfigurationRepository providerConfigRepository,
        IUsageRecordRepository usageRepository,
        IImageStorageService storageService,
        ILogger<BackgroundRemovalOrchestrator> logger)
    {
        _providerFactory = providerFactory;
        _jobRepository = jobRepository;
        _providerConfigRepository = providerConfigRepository;
        _usageRepository = usageRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<RemovalJobResponse> ProcessRemovalAsync(
        CreateRemovalJobRequest request,
        Guid? userId = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        // Crear el job
        var job = new BackgroundRemovalJob
        {
            UserId = userId,
            TenantId = tenantId,
            OriginalFileName = request.FileName ?? "unknown",
            OutputFormat = request.OutputFormat,
            OutputSize = request.OutputSize,
            CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString(),
            CallbackUrl = request.CallbackUrl,
            Priority = request.Priority,
            Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null,
            ExpiresAt = DateTime.UtcNow.AddDays(7) // Expirar en 7 días
        };

        // Obtener los bytes de la imagen
        byte[] imageBytes;
        if (!string.IsNullOrEmpty(request.ImageBase64))
        {
            var base64Data = request.ImageBase64;
            if (base64Data.Contains(','))
            {
                base64Data = base64Data.Split(',')[1];
            }
            imageBytes = Convert.FromBase64String(base64Data);
            job.SourceImageUrl = "base64-upload";
        }
        else if (!string.IsNullOrEmpty(request.ImageUrl))
        {
            imageBytes = await _storageService.DownloadImageAsync(request.ImageUrl, cancellationToken) 
                ?? throw new ArgumentException("Could not download image from URL");
            job.SourceImageUrl = request.ImageUrl;
        }
        else
        {
            throw new ArgumentException("Either ImageUrl or ImageBase64 must be provided");
        }

        job.OriginalFileSizeBytes = imageBytes.Length;
        job.OriginalContentType = DetectContentType(imageBytes);

        // Determinar el proveedor a usar
        IBackgroundRemovalProvider? provider = null;
        if (request.PreferredProvider.HasValue)
        {
            provider = _providerFactory.GetProvider(request.PreferredProvider.Value);
        }
        
        provider ??= await _providerFactory.GetBestAvailableProviderAsync(cancellationToken);
        
        if (provider == null)
        {
            job.MarkAsFailed("No available provider found", "NO_PROVIDER");
            await _jobRepository.CreateAsync(job, cancellationToken);
            
            return MapToResponse(job);
        }

        job.Provider = provider.ProviderType;
        await _jobRepository.CreateAsync(job, cancellationToken);

        // Procesar si es síncrono
        if (request.ProcessSync)
        {
            return await ProcessJobInternalAsync(job, imageBytes, provider, cancellationToken);
        }
        
        // Si es async, solo retornar el job creado (será procesado por un worker)
        return MapToResponse(job);
    }

    public async Task<RemovalJobResponse> ProcessJobAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null)
        {
            throw new ArgumentException($"Job {jobId} not found");
        }

        var provider = _providerFactory.GetProvider(job.Provider);
        if (provider == null)
        {
            job.MarkAsFailed("Provider not available", "PROVIDER_UNAVAILABLE");
            await _jobRepository.UpdateAsync(job, cancellationToken);
            return MapToResponse(job);
        }

        // Descargar la imagen original
        byte[]? imageBytes = null;
        if (job.SourceImageUrl != "base64-upload")
        {
            imageBytes = await _storageService.DownloadImageAsync(job.SourceImageUrl, cancellationToken);
        }

        if (imageBytes == null)
        {
            job.MarkAsFailed("Could not retrieve source image", "SOURCE_NOT_FOUND");
            await _jobRepository.UpdateAsync(job, cancellationToken);
            return MapToResponse(job);
        }

        return await ProcessJobInternalAsync(job, imageBytes, provider, cancellationToken);
    }

    public async Task<RemovalJobResponse> RetryJobAsync(
        Guid jobId,
        BackgroundRemovalProvider? alternateProvider = null,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null)
        {
            throw new ArgumentException($"Job {jobId} not found");
        }

        if (!job.CanRetry())
        {
            throw new InvalidOperationException($"Job {jobId} has exceeded maximum retries");
        }

        job.IncrementRetry();
        
        if (alternateProvider.HasValue)
        {
            job.FallbackProvider = job.Provider;
            job.Provider = alternateProvider.Value;
        }

        await _jobRepository.UpdateAsync(job, cancellationToken);
        
        return await ProcessJobAsync(jobId, cancellationToken);
    }

    public async Task<bool> CancelJobAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null)
        {
            return false;
        }

        if (job.Status == ProcessingStatus.Completed || job.Status == ProcessingStatus.Processing)
        {
            return false; // No se puede cancelar
        }

        job.Cancel();
        await _jobRepository.UpdateAsync(job, cancellationToken);
        return true;
    }

    public async Task<RemovalJobResponse?> GetJobStatusAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        return job == null ? null : MapToResponse(job);
    }

    public async Task<RemovalJobListResponse> GetUserJobsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _jobRepository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
        var items = jobs.Select(MapToResponse).ToList();
        
        return new RemovalJobListResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = items.Count // TODO: Implementar conteo real
        };
    }

    public async Task<UsageStatisticsResponse> GetUsageStatisticsAsync(
        Guid? userId = null,
        int? billingPeriod = null,
        CancellationToken cancellationToken = default)
    {
        var period = billingPeriod ?? int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        
        // TODO: Implementar agregaciones reales desde UsageRecordRepository
        return new UsageStatisticsResponse
        {
            UserId = userId,
            BillingPeriod = period,
            TotalRequests = 0,
            SuccessfulRequests = 0,
            FailedRequests = 0,
            TotalCreditsConsumed = 0,
            TotalCostUsd = 0,
            AverageProcessingTimeMs = 0,
            RequestsByProvider = [],
            CostByProvider = []
        };
    }

    public async Task<IEnumerable<ProviderHealthResponse>> CheckProvidersHealthAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<ProviderHealthResponse>();
        var providers = _providerFactory.GetAllProviders();
        
        foreach (var provider in providers)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var isAvailable = await provider.IsAvailableAsync(cancellationToken);
                var accountInfo = await provider.GetAccountInfoAsync(cancellationToken);
                stopwatch.Stop();
                
                results.Add(new ProviderHealthResponse
                {
                    Provider = provider.ProviderType,
                    IsHealthy = isAvailable && accountInfo.IsActive,
                    Status = isAvailable ? "Healthy" : "Unavailable",
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    AvailableCredits = accountInfo.AvailableCredits,
                    CheckedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                results.Add(new ProviderHealthResponse
                {
                    Provider = provider.ProviderType,
                    IsHealthy = false,
                    Status = "Error",
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    ErrorMessage = ex.Message,
                    CheckedAt = DateTime.UtcNow
                });
            }
        }
        
        return results;
    }

    public async Task<IEnumerable<ProviderInfoResponse>> GetProvidersInfoAsync(
        CancellationToken cancellationToken = default)
    {
        var configs = await _providerConfigRepository.GetAllAsync(cancellationToken);
        var providers = _providerFactory.GetAllProviders().ToDictionary(p => p.ProviderType);
        
        return configs.Select(config => new ProviderInfoResponse
        {
            Provider = config.Provider,
            Name = config.Name,
            IsEnabled = config.IsEnabled,
            IsAvailable = config.IsAvailable(),
            CostPerImageUsd = config.CostPerImageUsd,
            RateLimitPerMinute = config.RateLimitPerMinute,
            RateLimitPerDay = config.RateLimitPerDay,
            RequestsUsedToday = config.RequestsUsedToday,
            AvailableCredits = config.AvailableCredits,
            SuccessRate = config.SuccessRate,
            AverageResponseTimeMs = config.AverageResponseTimeMs,
            SupportedInputFormats = config.SupportedInputFormats.Split(','),
            SupportedOutputFormats = config.SupportedOutputFormats.Split(',')
        });
    }

    // === Private Methods ===

    private async Task<RemovalJobResponse> ProcessJobInternalAsync(
        BackgroundRemovalJob job,
        byte[] imageBytes,
        IBackgroundRemovalProvider provider,
        CancellationToken cancellationToken)
    {
        job.MarkAsProcessing();
        await _jobRepository.UpdateAsync(job, cancellationToken);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var options = new BackgroundRemovalOptions
            {
                OutputFormat = job.OutputFormat,
                OutputSize = job.OutputSize
                // TODO: Cargar más opciones desde job.Metadata
            };
            
            var result = await provider.RemoveBackgroundAsync(imageBytes, options, cancellationToken);
            stopwatch.Stop();
            
            if (!result.IsSuccess)
            {
                // Intentar con fallback si está configurado
                if (job.FallbackProvider.HasValue && job.CanRetry())
                {
                    var fallbackProvider = _providerFactory.GetProvider(job.FallbackProvider.Value);
                    if (fallbackProvider != null)
                    {
                        _logger.LogInformation("Trying fallback provider {Provider} for job {JobId}",
                            fallbackProvider.ProviderName, job.Id);
                        
                        job.IncrementRetry();
                        job.Provider = job.FallbackProvider.Value;
                        result = await fallbackProvider.RemoveBackgroundAsync(imageBytes, options, cancellationToken);
                    }
                }
                
                if (!result.IsSuccess)
                {
                    job.MarkAsFailed(result.ErrorMessage ?? "Unknown error", result.ErrorCode);
                    await _jobRepository.UpdateAsync(job, cancellationToken);
                    await RecordUsageAsync(job, false, 0, cancellationToken);
                    
                    return MapToResponse(job);
                }
            }
            
            // Guardar la imagen resultante
            var resultFileName = $"bg-removed/{job.Id}/{Path.GetFileNameWithoutExtension(job.OriginalFileName)}_nobg.png";
            var resultUrl = await _storageService.SaveImageAsync(
                result.ImageBytes!,
                resultFileName,
                result.ContentType ?? "image/png",
                null,
                cancellationToken);
            
            job.MarkAsCompleted(
                resultUrl,
                result.ImageBytes!.Length,
                result.ContentType ?? "image/png",
                stopwatch.ElapsedMilliseconds);
            
            job.CreditsConsumed = result.CreditsConsumed;
            
            // Actualizar configuración del proveedor
            var providerConfig = await _providerConfigRepository.GetByProviderAsync(provider.ProviderType, cancellationToken);
            if (providerConfig != null)
            {
                providerConfig.RecordSuccess(stopwatch.ElapsedMilliseconds);
                await _providerConfigRepository.UpdateAsync(providerConfig, cancellationToken);
            }
            
            await _jobRepository.UpdateAsync(job, cancellationToken);
            await RecordUsageAsync(job, true, stopwatch.ElapsedMilliseconds, cancellationToken);
            
            _logger.LogInformation(
                "Background removal completed for job {JobId} using {Provider} in {Time}ms",
                job.Id, provider.ProviderName, stopwatch.ElapsedMilliseconds);
            
            return MapToResponse(job);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing background removal job {JobId}", job.Id);
            
            job.MarkAsFailed(ex.Message, "EXCEPTION");
            
            var providerConfig = await _providerConfigRepository.GetByProviderAsync(provider.ProviderType, cancellationToken);
            if (providerConfig != null)
            {
                providerConfig.RecordFailure();
                await _providerConfigRepository.UpdateAsync(providerConfig, cancellationToken);
            }
            
            await _jobRepository.UpdateAsync(job, cancellationToken);
            await RecordUsageAsync(job, false, stopwatch.ElapsedMilliseconds, cancellationToken);
            
            return MapToResponse(job);
        }
    }

    private async Task RecordUsageAsync(
        BackgroundRemovalJob job,
        bool isSuccess,
        long processingTimeMs,
        CancellationToken cancellationToken)
    {
        var config = await _providerConfigRepository.GetByProviderAsync(job.Provider, cancellationToken);
        
        var record = new UsageRecord
        {
            JobId = job.Id,
            UserId = job.UserId,
            TenantId = job.TenantId,
            Provider = job.Provider,
            IsSuccess = isSuccess,
            InputSizeBytes = job.OriginalFileSizeBytes,
            OutputSizeBytes = job.ResultFileSizeBytes,
            ProcessingTimeMs = processingTimeMs,
            CreditsConsumed = job.CreditsConsumed ?? 0,
            CostUsd = config?.CostPerImageUsd ?? 0
        };
        
        await _usageRepository.CreateAsync(record, cancellationToken);
    }

    private static RemovalJobResponse MapToResponse(BackgroundRemovalJob job)
    {
        return new RemovalJobResponse
        {
            JobId = job.Id,
            Status = job.Status,
            Provider = job.Provider,
            SourceImageUrl = job.SourceImageUrl,
            ResultImageUrl = job.ResultImageUrl,
            ProcessingTimeMs = job.ProcessingTimeMs,
            CreditsConsumed = job.CreditsConsumed,
            ErrorMessage = job.ErrorMessage,
            ErrorCode = job.ErrorCode,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.ProcessingCompletedAt
        };
    }

    private static string DetectContentType(byte[] imageBytes)
    {
        if (imageBytes.Length < 4) return "application/octet-stream";
        
        // PNG: 89 50 4E 47
        if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
            return "image/png";
        
        // JPEG: FF D8 FF
        if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
            return "image/jpeg";
        
        // WebP: 52 49 46 46 ... 57 45 42 50
        if (imageBytes[0] == 0x52 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 && imageBytes[3] == 0x46)
            return "image/webp";
        
        // GIF: 47 49 46 38
        if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 && imageBytes[3] == 0x38)
            return "image/gif";
        
        return "image/png";
    }
}
