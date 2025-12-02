using System.Security.Cryptography;
using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileStorageService.Core.Services;

/// <summary>
/// Local file system storage provider implementation
/// </summary>
public class LocalStorageProvider : IStorageProvider
{
    private readonly StorageProviderConfig _config;
    private readonly ILogger<LocalStorageProvider> _logger;
    private readonly string _basePath;

    public LocalStorageProvider(
        IOptions<StorageProviderConfig> config,
        ILogger<LocalStorageProvider> logger)
    {
        _config = config.Value;
        _logger = logger;
        _basePath = Path.GetFullPath(_config.BasePath);

        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger.LogInformation("Created storage directory: {BasePath}", _basePath);
        }
    }

    public StorageProviderType ProviderType => StorageProviderType.Local;

    public async Task UploadAsync(
        string storageKey,
        Stream fileStream,
        string contentType,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var filePath = GetFullPath(storageKey);
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStreamOutput = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

        // Store metadata in a sidecar file
        if (metadata != null && metadata.Count > 0)
        {
            await SaveMetadataAsync(storageKey, metadata, cancellationToken);
        }

        _logger.LogDebug("Uploaded file to {StorageKey}", storageKey);
    }

    public async Task<Stream> DownloadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var filePath = GetFullPath(storageKey);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {storageKey}");
        }

        var memoryStream = new MemoryStream();
        await using var fileStream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var filePath = GetFullPath(storageKey);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogDebug("Deleted file: {StorageKey}", storageKey);
        }

        // Also delete metadata sidecar file
        var metadataPath = GetMetadataPath(storageKey);
        if (File.Exists(metadataPath))
        {
            File.Delete(metadataPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var filePath = GetFullPath(storageKey);
        return Task.FromResult(File.Exists(filePath));
    }

    public async Task CopyAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default)
    {
        var sourcePath = GetFullPath(sourceKey);
        var destPath = GetFullPath(destinationKey);

        var destDirectory = Path.GetDirectoryName(destPath);
        if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
        {
            Directory.CreateDirectory(destDirectory);
        }

        await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        await using var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        await sourceStream.CopyToAsync(destStream, cancellationToken);

        // Copy metadata if exists
        var sourceMetadata = await GetMetadataAsync(sourceKey, cancellationToken);
        if (sourceMetadata.Count > 0)
        {
            await SaveMetadataAsync(destinationKey, sourceMetadata, cancellationToken);
        }

        _logger.LogDebug("Copied file from {Source} to {Dest}", sourceKey, destinationKey);
    }

    public async Task MoveAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default)
    {
        await CopyAsync(sourceKey, destinationKey, cancellationToken);
        await DeleteAsync(sourceKey, cancellationToken);
        _logger.LogDebug("Moved file from {Source} to {Dest}", sourceKey, destinationKey);
    }

    public Task<string> GetPublicUrlAsync(string storageKey)
    {
        // For local storage, return CDN URL if configured, otherwise return relative path
        if (!string.IsNullOrEmpty(_config.CdnBaseUrl))
        {
            return Task.FromResult($"{_config.CdnBaseUrl.TrimEnd('/')}/{storageKey}");
        }

        // Return a relative URL for local files (should be served by static file middleware)
        return Task.FromResult($"/files/{storageKey}");
    }

    public Task<PresignedUrl> GenerateUploadUrlAsync(string storageKey, string contentType, TimeSpan expiry)
    {
        // For local storage, we generate a token-based URL
        var token = GenerateSecureToken(storageKey, "upload", expiry);

        var presignedUrl = new PresignedUrl
        {
            Url = $"/api/files/upload/{storageKey}?token={token}",
            Type = PresignedUrlType.Upload,
            StorageKey = storageKey,
            HttpMethod = "PUT",
            ContentType = contentType,
            ExpiresAt = DateTime.UtcNow.Add(expiry),
            Token = token,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = contentType
            }
        };

        return Task.FromResult(presignedUrl);
    }

    public Task<PresignedUrl> GenerateDownloadUrlAsync(string storageKey, TimeSpan expiry)
    {
        var token = GenerateSecureToken(storageKey, "download", expiry);

        var presignedUrl = new PresignedUrl
        {
            Url = $"/api/files/download/{storageKey}?token={token}",
            Type = PresignedUrlType.Download,
            StorageKey = storageKey,
            HttpMethod = "GET",
            ExpiresAt = DateTime.UtcNow.Add(expiry),
            Token = token
        };

        return Task.FromResult(presignedUrl);
    }

    public async Task<Dictionary<string, string>> GetMetadataAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var metadataPath = GetMetadataPath(storageKey);

        if (!File.Exists(metadataPath))
        {
            return new Dictionary<string, string>();
        }

        var json = await File.ReadAllTextAsync(metadataPath, cancellationToken);
        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json)
            ?? new Dictionary<string, string>();
    }

    public async Task SetMetadataAsync(string storageKey, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
    {
        await SaveMetadataAsync(storageKey, metadata, cancellationToken);
    }

    public Task<long> GetFileSizeAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var filePath = GetFullPath(storageKey);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {storageKey}");
        }

        var fileInfo = new FileInfo(filePath);
        return Task.FromResult(fileInfo.Length);
    }

    public Task<IEnumerable<string>> ListAsync(string prefix, int maxResults = 1000, CancellationToken cancellationToken = default)
    {
        var searchPath = GetFullPath(prefix);
        var directory = Path.GetDirectoryName(searchPath) ?? _basePath;
        var pattern = Path.GetFileName(searchPath);

        if (string.IsNullOrEmpty(pattern))
        {
            pattern = "*";
        }

        if (!Directory.Exists(directory))
        {
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        }

        var files = Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories)
            .Take(maxResults)
            .Select(f => GetRelativePath(f))
            .Where(f => !f.EndsWith(".metadata.json"))
            .ToList();

        return Task.FromResult<IEnumerable<string>>(files);
    }

    public Task<bool> IsHealthyAsync()
    {
        try
        {
            var testPath = Path.Combine(_basePath, ".health-check");
            File.WriteAllText(testPath, DateTime.UtcNow.ToString("O"));
            File.Delete(testPath);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storage health check failed");
            return Task.FromResult(false);
        }
    }

    private string GetFullPath(string storageKey)
    {
        // Sanitize the storage key to prevent directory traversal
        var sanitizedKey = storageKey.Replace("..", "").TrimStart('/').TrimStart('\\');
        return Path.Combine(_basePath, sanitizedKey);
    }

    private string GetRelativePath(string fullPath)
    {
        return Path.GetRelativePath(_basePath, fullPath).Replace('\\', '/');
    }

    private string GetMetadataPath(string storageKey)
    {
        return GetFullPath(storageKey) + ".metadata.json";
    }

    private async Task SaveMetadataAsync(string storageKey, Dictionary<string, string> metadata, CancellationToken cancellationToken)
    {
        var metadataPath = GetMetadataPath(storageKey);
        var json = System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(metadataPath, json, cancellationToken);
    }

    private static string GenerateSecureToken(string storageKey, string operation, TimeSpan expiry)
    {
        var expiration = DateTime.UtcNow.Add(expiry).Ticks;
        var data = $"{storageKey}|{operation}|{expiration}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return $"{expiration}_{Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=')}";
    }
}
