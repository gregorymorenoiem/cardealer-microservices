using System.Security.Cryptography;
using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackupDRService.Core.Services;

/// <summary>
/// Local file system storage provider
/// </summary>
public class LocalStorageProvider : IStorageProvider
{
    private readonly ILogger<LocalStorageProvider> _logger;
    private readonly BackupOptions _options;

    public LocalStorageProvider(
        ILogger<LocalStorageProvider> logger,
        IOptions<BackupOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public StorageType StorageType => StorageType.Local;

    public async Task<StorageUploadResult> UploadAsync(string localFilePath, string destinationPath)
    {
        try
        {
            if (!File.Exists(localFilePath))
            {
                return StorageUploadResult.Failed($"Source file not found: {localFilePath}");
            }

            var fullDestPath = GetFullPath(destinationPath);
            var directory = Path.GetDirectoryName(fullDestPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Copy file to destination
            await Task.Run(() => File.Copy(localFilePath, fullDestPath, overwrite: true));

            var fileInfo = new FileInfo(fullDestPath);
            var checksum = await CalculateChecksumAsync(fullDestPath);

            _logger.LogInformation("File uploaded to local storage: {Path}, Size: {Size}",
                destinationPath, fileInfo.Length);

            return StorageUploadResult.Succeeded(destinationPath, fileInfo.Length, checksum);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to local storage: {Path}", destinationPath);
            return StorageUploadResult.Failed(ex.Message);
        }
    }

    public async Task<StorageDownloadResult> DownloadAsync(string storagePath, string localDestinationPath)
    {
        try
        {
            var fullSourcePath = GetFullPath(storagePath);
            if (!File.Exists(fullSourcePath))
            {
                return StorageDownloadResult.Failed($"File not found in storage: {storagePath}");
            }

            var directory = Path.GetDirectoryName(localDestinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await Task.Run(() => File.Copy(fullSourcePath, localDestinationPath, overwrite: true));

            var fileInfo = new FileInfo(localDestinationPath);

            _logger.LogInformation("File downloaded from local storage: {Path}, Size: {Size}",
                storagePath, fileInfo.Length);

            return StorageDownloadResult.Succeeded(localDestinationPath, fileInfo.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file from local storage: {Path}", storagePath);
            return StorageDownloadResult.Failed(ex.Message);
        }
    }

    public Task<bool> DeleteAsync(string storagePath)
    {
        try
        {
            var fullPath = GetFullPath(storagePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted from local storage: {Path}", storagePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from local storage: {Path}", storagePath);
            return Task.FromResult(false);
        }
    }

    public Task<bool> ExistsAsync(string storagePath)
    {
        var fullPath = GetFullPath(storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<StorageFileInfo?> GetFileInfoAsync(string storagePath)
    {
        try
        {
            var fullPath = GetFullPath(storagePath);
            if (!File.Exists(fullPath))
            {
                return Task.FromResult<StorageFileInfo?>(null);
            }

            var fileInfo = new FileInfo(fullPath);
            return Task.FromResult<StorageFileInfo?>(new StorageFileInfo
            {
                Name = fileInfo.Name,
                Path = storagePath,
                SizeBytes = fileInfo.Length,
                CreatedAt = fileInfo.CreationTimeUtc,
                ModifiedAt = fileInfo.LastWriteTimeUtc,
                StorageType = StorageType.Local
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file info from local storage: {Path}", storagePath);
            return Task.FromResult<StorageFileInfo?>(null);
        }
    }

    public Task<IEnumerable<StorageFileInfo>> ListFilesAsync(string directoryPath, string? pattern = null)
    {
        try
        {
            var fullPath = GetFullPath(directoryPath);
            if (!Directory.Exists(fullPath))
            {
                return Task.FromResult<IEnumerable<StorageFileInfo>>(Array.Empty<StorageFileInfo>());
            }

            var searchPattern = pattern ?? "*";
            var files = Directory.GetFiles(fullPath, searchPattern, SearchOption.TopDirectoryOnly);

            var fileInfos = files.Select(f =>
            {
                var info = new FileInfo(f);
                return new StorageFileInfo
                {
                    Name = info.Name,
                    Path = GetRelativePath(f),
                    SizeBytes = info.Length,
                    CreatedAt = info.CreationTimeUtc,
                    ModifiedAt = info.LastWriteTimeUtc,
                    StorageType = StorageType.Local
                };
            });

            return Task.FromResult<IEnumerable<StorageFileInfo>>(fileInfos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list files from local storage: {Path}", directoryPath);
            return Task.FromResult<IEnumerable<StorageFileInfo>>(Array.Empty<StorageFileInfo>());
        }
    }

    public Task<long> GetTotalStorageUsedAsync()
    {
        try
        {
            var basePath = _options.LocalStoragePath;
            if (!Directory.Exists(basePath))
            {
                return Task.FromResult(0L);
            }

            var totalSize = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length);

            return Task.FromResult(totalSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate total storage used");
            return Task.FromResult(0L);
        }
    }

    public async Task<bool> VerifyIntegrityAsync(string storagePath, string expectedChecksum)
    {
        try
        {
            var actualChecksum = await CalculateChecksumAsync(storagePath);
            return string.Equals(actualChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify file integrity: {Path}", storagePath);
            return false;
        }
    }

    public async Task<string> CalculateChecksumAsync(string storagePath)
    {
        var fullPath = GetFullPath(storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {storagePath}");
        }

        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(fullPath);
        var hash = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private string GetFullPath(string relativePath)
    {
        // If already an absolute path, return as-is
        if (Path.IsPathRooted(relativePath))
        {
            return relativePath;
        }

        return Path.Combine(_options.LocalStoragePath, relativePath);
    }

    private string GetRelativePath(string fullPath)
    {
        if (fullPath.StartsWith(_options.LocalStoragePath))
        {
            return fullPath.Substring(_options.LocalStoragePath.Length).TrimStart(Path.DirectorySeparatorChar);
        }
        return fullPath;
    }
}
