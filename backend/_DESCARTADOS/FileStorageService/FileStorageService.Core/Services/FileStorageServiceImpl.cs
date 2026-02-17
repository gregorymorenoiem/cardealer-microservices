using System.Diagnostics;
using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileStorageService.Core.Services;

/// <summary>
/// Main file storage service implementation
/// </summary>
public class FileStorageServiceImpl : IFileStorageService
{
    private readonly StorageProviderConfig _config;
    private readonly IStorageProvider _storageProvider;
    private readonly IPresignedUrlService _presignedUrlService;
    private readonly IVirusScanService _virusScanService;
    private readonly IMetadataExtractorService _metadataExtractor;
    private readonly IImageProcessingService _imageProcessor;
    private readonly ILogger<FileStorageServiceImpl> _logger;

    // In-memory file tracking (in production, use database/Redis)
    private readonly Dictionary<string, StoredFile> _fileStore = new();
    private readonly object _lock = new();

    public FileStorageServiceImpl(
        IOptions<StorageProviderConfig> config,
        IStorageProvider storageProvider,
        IPresignedUrlService presignedUrlService,
        IVirusScanService virusScanService,
        IMetadataExtractorService metadataExtractor,
        IImageProcessingService imageProcessor,
        ILogger<FileStorageServiceImpl> logger)
    {
        _config = config.Value;
        _storageProvider = storageProvider;
        _presignedUrlService = presignedUrlService;
        _virusScanService = virusScanService;
        _metadataExtractor = metadataExtractor;
        _imageProcessor = imageProcessor;
        _logger = logger;
    }

    public async Task<UploadInitResponse> InitializeUploadAsync(
        PresignedUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate content type
        if (!IsContentTypeAllowed(request.ContentType))
        {
            throw new InvalidOperationException($"Content type '{request.ContentType}' is not allowed");
        }

        // Validate file size if provided
        if (request.FileSizeBytes.HasValue && request.FileSizeBytes > _config.MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed ({_config.MaxFileSizeBytes} bytes)");
        }

        var response = await _presignedUrlService.GenerateUploadUrlAsync(request, cancellationToken);

        // Create pending file record
        var storedFile = new StoredFile
        {
            Id = response.FileId,
            OriginalFileName = request.FileName,
            StorageKey = response.StorageKey,
            ContentType = request.ContentType,
            SizeBytes = request.FileSizeBytes ?? 0,
            OwnerId = request.OwnerId,
            Context = request.Context,
            Status = FileStatus.Pending,
            ProviderType = _storageProvider.ProviderType,
            Tags = request.Metadata
        };

        storedFile.DetermineFileType();

        lock (_lock)
        {
            _fileStore[response.FileId] = storedFile;
        }

        _logger.LogInformation("Initialized upload for {FileId}: {FileName}", response.FileId, request.FileName);

        return response;
    }

    public async Task<UploadResult> FinalizeUploadAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new UploadResult();

        StoredFile? storedFile;
        lock (_lock)
        {
            _fileStore.TryGetValue(fileId, out storedFile);
        }

        if (storedFile == null)
        {
            return UploadResult.Failed("File not found", "FILE_NOT_FOUND");
        }

        try
        {
            storedFile.Status = FileStatus.Processing;

            // Verify file exists in storage
            if (!await _storageProvider.ExistsAsync(storedFile.StorageKey, cancellationToken))
            {
                storedFile.Status = FileStatus.Failed;
                storedFile.ErrorMessage = "File not found in storage";
                return UploadResult.Failed("File not found in storage", "UPLOAD_INCOMPLETE");
            }

            // Get actual file size
            storedFile.SizeBytes = await _storageProvider.GetFileSizeAsync(storedFile.StorageKey, cancellationToken);

            // Download file for processing
            await using var fileStream = await _storageProvider.DownloadAsync(storedFile.StorageKey, cancellationToken);
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancellationToken);

            // Virus scan
            if (_config.EnableVirusScan && _virusScanService.ShouldScan(storedFile.ContentType))
            {
                var scanStep = Stopwatch.StartNew();
                memoryStream.Position = 0;

                var scanResult = await _virusScanService.ScanAsync(memoryStream, storedFile.OriginalFileName, cancellationToken);
                storedFile.ScanResult = scanResult;

                result.ProcessingSteps.Add(ProcessingStep.Completed("VirusScan", scanStep.ElapsedMilliseconds));

                if (!scanResult.IsClean)
                {
                    storedFile.Status = FileStatus.Quarantined;

                    // Move to quarantine
                    var quarantineKey = $"quarantine/{storedFile.StorageKey}";
                    await _storageProvider.MoveAsync(storedFile.StorageKey, quarantineKey, cancellationToken);
                    storedFile.StorageKey = quarantineKey;

                    _logger.LogWarning("File {FileId} quarantined due to threat: {ThreatName}",
                        fileId, scanResult.ThreatName);

                    return UploadResult.VirusDetected(scanResult.ThreatName ?? "Unknown threat", scanResult);
                }
            }
            else
            {
                result.ProcessingSteps.Add(ProcessingStep.Skipped("VirusScan", "Disabled or not applicable"));
            }

            // Extract metadata
            if (_config.EnableMetadataExtraction)
            {
                var metadataStep = Stopwatch.StartNew();
                memoryStream.Position = 0;

                storedFile.Metadata = await _metadataExtractor.ExtractAsync(
                    memoryStream, storedFile.OriginalFileName, storedFile.ContentType, cancellationToken);
                storedFile.ContentHash = storedFile.Metadata.ContentHash;

                result.ProcessingSteps.Add(ProcessingStep.Completed("MetadataExtraction", metadataStep.ElapsedMilliseconds));
                result.Metadata = storedFile.Metadata;
            }
            else
            {
                result.ProcessingSteps.Add(ProcessingStep.Skipped("MetadataExtraction", "Disabled"));
            }

            // Generate image variants if applicable
            if (_config.EnableImageOptimization && storedFile.IsImage)
            {
                var variantStep = Stopwatch.StartNew();

                try
                {
                    var defaultVariants = new List<VariantConfig>
                    {
                        new() { Name = "thumbnail", MaxWidth = 150, MaxHeight = 150, Format = "jpeg", Quality = 80, ResizeMode = "crop" },
                        new() { Name = "small", MaxWidth = 320, MaxHeight = 320, Format = "jpeg", Quality = 85 },
                        new() { Name = "medium", MaxWidth = 640, MaxHeight = 640, Format = "jpeg", Quality = 85 }
                    };

                    memoryStream.Position = 0;
                    var variants = await _imageProcessor.GenerateVariantsAsync(memoryStream, defaultVariants, cancellationToken);

                    foreach (var (config, stream) in variants)
                    {
                        var variantKey = GetVariantStorageKey(storedFile.StorageKey, config.Name, config.Format);

                        await _storageProvider.UploadAsync(variantKey, stream, $"image/{config.Format}", cancellationToken: cancellationToken);

                        var variant = new FileVariant
                        {
                            Name = config.Name,
                            StorageKey = variantKey,
                            ContentType = $"image/{config.Format}",
                            SizeBytes = stream.Length,
                            Width = config.MaxWidth,
                            Height = config.MaxHeight,
                            PublicUrl = await _presignedUrlService.GetPublicUrlAsync(variantKey)
                        };

                        storedFile.Variants.Add(variant);
                        await stream.DisposeAsync();
                    }

                    result.ProcessingSteps.Add(ProcessingStep.Completed("ImageVariants", variantStep.ElapsedMilliseconds));
                    result.Variants = storedFile.Variants;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate image variants for {FileId}", fileId);
                    result.ProcessingSteps.Add(ProcessingStep.Failed("ImageVariants", ex.Message));
                }
            }
            else if (storedFile.IsImage)
            {
                result.ProcessingSteps.Add(ProcessingStep.Skipped("ImageVariants", "Disabled"));
            }

            // Set public URL
            storedFile.PublicUrl = await _presignedUrlService.GetPublicUrlAsync(storedFile.StorageKey);

            // Mark as ready
            storedFile.Status = FileStatus.Ready;
            storedFile.UpdatedAt = DateTime.UtcNow;

            stopwatch.Stop();

            result.Success = true;
            result.File = storedFile;
            result.BytesUploaded = storedFile.SizeBytes;
            result.DurationMs = stopwatch.ElapsedMilliseconds;
            result.ScanResult = storedFile.ScanResult;

            _logger.LogInformation("Finalized upload for {FileId} in {Duration}ms", fileId, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            storedFile.Status = FileStatus.Failed;
            storedFile.ErrorMessage = ex.Message;

            _logger.LogError(ex, "Failed to finalize upload for {FileId}", fileId);

            return UploadResult.Failed(ex.Message, "PROCESSING_ERROR");
        }
    }

    public async Task<UploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string ownerId,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Validate
        if (!IsContentTypeAllowed(contentType))
        {
            return UploadResult.Failed($"Content type '{contentType}' is not allowed", "INVALID_CONTENT_TYPE");
        }

        if (fileStream.Length > _config.MaxFileSizeBytes)
        {
            return UploadResult.Failed($"File size exceeds maximum ({_config.MaxFileSizeBytes} bytes)", "FILE_TOO_LARGE");
        }

        var fileId = Guid.NewGuid().ToString();
        var storageKey = _presignedUrlService.GenerateStorageKey(ownerId, context, fileName);

        var storedFile = new StoredFile
        {
            Id = fileId,
            OriginalFileName = fileName,
            StorageKey = storageKey,
            ContentType = contentType,
            SizeBytes = fileStream.Length,
            OwnerId = ownerId,
            Context = context,
            Status = FileStatus.Uploading,
            ProviderType = _storageProvider.ProviderType
        };

        storedFile.DetermineFileType();

        try
        {
            // Copy to memory stream for multiple reads
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            // Upload to storage
            await _storageProvider.UploadAsync(storageKey, memoryStream, contentType, cancellationToken: cancellationToken);

            // Store file reference
            lock (_lock)
            {
                _fileStore[fileId] = storedFile;
            }

            // Finalize (scan, extract metadata, etc.)
            return await FinalizeUploadAsync(fileId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload failed for {FileName}", fileName);
            return UploadResult.Failed(ex.Message, "UPLOAD_ERROR");
        }
    }

    public async Task<DownloadResult> DownloadAsync(string fileId, CancellationToken cancellationToken = default)
    {
        StoredFile? storedFile;
        lock (_lock)
        {
            _fileStore.TryGetValue(fileId, out storedFile);
        }

        if (storedFile == null)
        {
            return DownloadResult.Failed("File not found");
        }

        if (storedFile.Status != FileStatus.Ready)
        {
            return DownloadResult.Failed($"File is not available (status: {storedFile.Status})");
        }

        try
        {
            var stream = await _storageProvider.DownloadAsync(storedFile.StorageKey, cancellationToken);

            return DownloadResult.Successful(
                stream,
                storedFile.ContentType,
                storedFile.OriginalFileName,
                storedFile.SizeBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download failed for {FileId}", fileId);
            return DownloadResult.Failed(ex.Message);
        }
    }

    public async Task<PresignedUrl> GetDownloadUrlAsync(
        string fileId,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        StoredFile? storedFile;
        lock (_lock)
        {
            _fileStore.TryGetValue(fileId, out storedFile);
        }

        if (storedFile == null)
        {
            throw new FileNotFoundException($"File {fileId} not found");
        }

        return await _presignedUrlService.GenerateDownloadUrlAsync(storedFile.StorageKey, expirationMinutes, cancellationToken);
    }

    public async Task<DeleteResult> DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        StoredFile? storedFile;
        lock (_lock)
        {
            _fileStore.TryGetValue(fileId, out storedFile);
        }

        if (storedFile == null)
        {
            return DeleteResult.Failed("File not found");
        }

        try
        {
            var deletedKeys = new List<string> { storedFile.StorageKey };

            // Delete main file
            await _storageProvider.DeleteAsync(storedFile.StorageKey, cancellationToken);

            // Delete variants
            foreach (var variant in storedFile.Variants)
            {
                await _storageProvider.DeleteAsync(variant.StorageKey, cancellationToken);
                deletedKeys.Add(variant.StorageKey);
            }

            // Update file status
            storedFile.Status = FileStatus.Deleted;
            storedFile.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Deleted file {FileId} and {VariantCount} variants", fileId, storedFile.Variants.Count);

            return DeleteResult.Successful(fileId, deletedKeys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete failed for {FileId}", fileId);
            return DeleteResult.Failed(ex.Message);
        }
    }

    public Task<StoredFile?> GetFileInfoAsync(string fileId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _fileStore.TryGetValue(fileId, out var storedFile);
            return Task.FromResult(storedFile);
        }
    }

    public Task<IEnumerable<StoredFile>> ListFilesAsync(
        string ownerId,
        string? context = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _fileStore.Values.Where(f => f.OwnerId == ownerId && f.Status != FileStatus.Deleted);

            if (!string.IsNullOrEmpty(context))
            {
                query = query.Where(f => f.Context == context);
            }

            var files = query
                .OrderByDescending(f => f.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToList();

            return Task.FromResult<IEnumerable<StoredFile>>(files);
        }
    }

    public async Task<StoredFile?> CopyFileAsync(
        string fileId,
        string? newOwnerId = null,
        string? newContext = null,
        CancellationToken cancellationToken = default)
    {
        StoredFile? sourceFile;
        lock (_lock)
        {
            _fileStore.TryGetValue(fileId, out sourceFile);
        }

        if (sourceFile == null)
        {
            return null;
        }

        var newFileId = Guid.NewGuid().ToString();
        var newStorageKey = _presignedUrlService.GenerateStorageKey(
            newOwnerId ?? sourceFile.OwnerId,
            newContext ?? sourceFile.Context,
            sourceFile.OriginalFileName);

        // Copy in storage
        await _storageProvider.CopyAsync(sourceFile.StorageKey, newStorageKey, cancellationToken);

        var newFile = new StoredFile
        {
            Id = newFileId,
            OriginalFileName = sourceFile.OriginalFileName,
            StorageKey = newStorageKey,
            ContentType = sourceFile.ContentType,
            SizeBytes = sourceFile.SizeBytes,
            FileType = sourceFile.FileType,
            Status = FileStatus.Ready,
            OwnerId = newOwnerId ?? sourceFile.OwnerId,
            Context = newContext ?? sourceFile.Context,
            ProviderType = sourceFile.ProviderType,
            ContentHash = sourceFile.ContentHash,
            Metadata = sourceFile.Metadata,
            Tags = new Dictionary<string, string>(sourceFile.Tags),
            PublicUrl = await _presignedUrlService.GetPublicUrlAsync(newStorageKey)
        };

        lock (_lock)
        {
            _fileStore[newFileId] = newFile;
        }

        _logger.LogInformation("Copied file {SourceId} to {NewId}", fileId, newFileId);

        return newFile;
    }

    public async Task<ScanResult> RescanFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        StoredFile? storedFile;
        lock (_lock)
        {
            _fileStore.TryGetValue(fileId, out storedFile);
        }

        if (storedFile == null)
        {
            return ScanResult.Error(fileId, "File not found");
        }

        var result = await _virusScanService.ScanByKeyAsync(storedFile.StorageKey, cancellationToken);
        storedFile.ScanResult = result;
        storedFile.UpdatedAt = DateTime.UtcNow;

        return result;
    }

    public async Task<FileMetadata?> RefreshMetadataAsync(string fileId, CancellationToken cancellationToken = default)
    {
        StoredFile? storedFile;
        lock (_lock)
        {
            _fileStore.TryGetValue(fileId, out storedFile);
        }

        if (storedFile == null)
        {
            return null;
        }

        await using var stream = await _storageProvider.DownloadAsync(storedFile.StorageKey, cancellationToken);

        storedFile.Metadata = await _metadataExtractor.ExtractAsync(
            stream, storedFile.OriginalFileName, storedFile.ContentType, cancellationToken);
        storedFile.ContentHash = storedFile.Metadata.ContentHash;
        storedFile.UpdatedAt = DateTime.UtcNow;

        return storedFile.Metadata;
    }

    public async Task<IEnumerable<FileVariant>> GenerateVariantsAsync(
        string fileId,
        IEnumerable<VariantConfig> variants,
        CancellationToken cancellationToken = default)
    {
        StoredFile? storedFile;
        lock (_lock)
        {
            _fileStore.TryGetValue(fileId, out storedFile);
        }

        if (storedFile == null || !storedFile.IsImage)
        {
            return Array.Empty<FileVariant>();
        }

        await using var stream = await _storageProvider.DownloadAsync(storedFile.StorageKey, cancellationToken);
        var generatedVariants = await _imageProcessor.GenerateVariantsAsync(stream, variants, cancellationToken);

        var result = new List<FileVariant>();

        foreach (var (config, variantStream) in generatedVariants)
        {
            var variantKey = GetVariantStorageKey(storedFile.StorageKey, config.Name, config.Format);

            await _storageProvider.UploadAsync(variantKey, variantStream, $"image/{config.Format}", cancellationToken: cancellationToken);

            var variant = new FileVariant
            {
                Name = config.Name,
                StorageKey = variantKey,
                ContentType = $"image/{config.Format}",
                SizeBytes = variantStream.Length,
                Width = config.MaxWidth,
                Height = config.MaxHeight,
                PublicUrl = await _presignedUrlService.GetPublicUrlAsync(variantKey)
            };

            storedFile.Variants.Add(variant);
            result.Add(variant);

            await variantStream.DisposeAsync();
        }

        storedFile.UpdatedAt = DateTime.UtcNow;

        return result;
    }

    public Task<StoredFile?> UpdateTagsAsync(
        string fileId,
        Dictionary<string, string> tags,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_fileStore.TryGetValue(fileId, out var storedFile))
            {
                storedFile.Tags = tags;
                storedFile.UpdatedAt = DateTime.UtcNow;
                return Task.FromResult<StoredFile?>(storedFile);
            }
        }

        return Task.FromResult<StoredFile?>(null);
    }

    public Task<StorageStatistics> GetStatisticsAsync(string? ownerId = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var query = _fileStore.Values.Where(f => f.Status != FileStatus.Deleted);

            if (!string.IsNullOrEmpty(ownerId))
            {
                query = query.Where(f => f.OwnerId == ownerId);
            }

            var files = query.ToList();

            var stats = new StorageStatistics
            {
                TotalFiles = files.Count,
                TotalSizeBytes = files.Sum(f => f.SizeBytes),
                FilesByType = files.GroupBy(f => f.FileType).ToDictionary(g => g.Key, g => (long)g.Count()),
                SizeByType = files.GroupBy(f => f.FileType).ToDictionary(g => g.Key, g => g.Sum(f => f.SizeBytes)),
                FilesByStatus = files.GroupBy(f => f.Status).ToDictionary(g => g.Key, g => (long)g.Count()),
                QuarantinedFiles = files.Count(f => f.Status == FileStatus.Quarantined),
                TotalVariants = files.Sum(f => f.Variants.Count)
            };

            return Task.FromResult(stats);
        }
    }

    private bool IsContentTypeAllowed(string contentType)
    {
        if (_config.AllowedContentTypes == null || _config.AllowedContentTypes.Length == 0)
            return true;

        return _config.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetVariantStorageKey(string originalKey, string variantName, string format)
    {
        var directory = Path.GetDirectoryName(originalKey) ?? "";
        var fileName = Path.GetFileNameWithoutExtension(originalKey);
        return $"{directory}/{fileName}_{variantName}.{format}".Replace('\\', '/').TrimStart('/');
    }
}
