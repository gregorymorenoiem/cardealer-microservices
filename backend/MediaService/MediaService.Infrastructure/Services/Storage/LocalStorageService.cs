using MediaService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaService.Infrastructure.Services.Storage;

public class LocalStorageService : IMediaStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(string basePath, string baseUrl, ILogger<LocalStorageService> logger)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        _logger = logger;

        // Asegurarse de que el directorio base exista
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public Task<UploadUrlResponse> GenerateUploadUrlAsync(string storageKey, string contentType, TimeSpan? expiry = null)
    {
        var uploadUrl = $"{_baseUrl.TrimEnd('/')}/{storageKey.TrimStart('/')}";
        var expiresAt = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromHours(1));

        _logger.LogInformation("URL de subida generada: {Url}", uploadUrl);

        var response = new UploadUrlResponse
        {
            UploadUrl = uploadUrl,
            ExpiresAt = expiresAt,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = contentType
            },
            StorageKey = storageKey
        };

        return Task.FromResult(response);
    }

    public Task<bool> ValidateFileAsync(string contentType, long fileSize)
    {
        // Lógica de validación básica
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "video/mp4", "application/pdf" };
        var maxSize = 100 * 1024 * 1024; // 100MB

        return Task.FromResult(allowedTypes.Contains(contentType) && fileSize <= maxSize);
    }

    public Task<string> GenerateStorageKeyAsync(string ownerId, string? context, string fileName)
    {
        var safeFileName = Path.GetFileNameWithoutExtension(fileName)
            .Replace(" ", "_")
            .ToLowerInvariant();
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var random = Path.GetRandomFileName().Replace(".", "").Substring(0, 8);

        var key = $"{ownerId}/{context ?? "default"}/{timestamp}_{random}_{safeFileName}{extension}";
        return Task.FromResult(key);
    }

    public Task<bool> FileExistsAsync(string storageKey)
    {
        var fullPath = GetFullPath(storageKey);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<Stream> DownloadFileAsync(string storageKey)
    {
        var fullPath = GetFullPath(storageKey);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Archivo no encontrado: {storageKey}");
        }

        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public async Task UploadFileAsync(string storageKey, Stream fileStream, string contentType)
    {
        var fullPath = GetFullPath(storageKey);
        var directory = Path.GetDirectoryName(fullPath) ?? _basePath;

        // Crear directorio si no existe
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Subir archivo
        using (var file = File.Create(fullPath))
        {
            await fileStream.CopyToAsync(file);
        }

        _logger.LogInformation("Archivo subido exitosamente: {StorageKey}", storageKey);
    }

    public Task DeleteFileAsync(string storageKey)
    {
        var fullPath = GetFullPath(storageKey);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Archivo eliminado: {StorageKey}", storageKey);
        }

        return Task.CompletedTask;
    }

    public Task<string> GetFileUrlAsync(string storageKey)
    {
        return Task.FromResult($"{_baseUrl.TrimEnd('/')}/{storageKey.TrimStart('/')}");
    }

    public Task CopyFileAsync(string sourceKey, string destinationKey)
    {
        var sourceFullPath = GetFullPath(sourceKey);
        var destFullPath = GetFullPath(destinationKey);
        var destDirectory = Path.GetDirectoryName(destFullPath) ?? _basePath;

        // Crear directorio de destino si no existe
        if (!Directory.Exists(destDirectory))
        {
            Directory.CreateDirectory(destDirectory);
        }

        File.Copy(sourceFullPath, destFullPath, overwrite: true);
        _logger.LogInformation("Archivo copiado de {Source} a {Destination}", sourceKey, destinationKey);

        return Task.CompletedTask;
    }

    public Task MoveFileAsync(string sourceKey, string destinationKey)
    {
        var sourceFullPath = GetFullPath(sourceKey);
        var destFullPath = GetFullPath(destinationKey);
        var destDirectory = Path.GetDirectoryName(destFullPath) ?? _basePath;

        // Crear directorio de destino si no existe
        if (!Directory.Exists(destDirectory))
        {
            Directory.CreateDirectory(destDirectory);
        }

        File.Move(sourceFullPath, destFullPath);
        _logger.LogInformation("Archivo movido de {Source} a {Destination}", sourceKey, destinationKey);

        return Task.CompletedTask;
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            // Verificar que el directorio base sea accesible
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            // Intentar crear y eliminar un archivo de prueba
            var testFilePath = Path.Combine(_basePath, $"healthcheck_{Guid.NewGuid()}.tmp");

            await File.WriteAllTextAsync(testFilePath, "healthcheck");
            File.Delete(testFilePath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check falló para LocalStorageService");
            return false;
        }
    }

    public Task<long> GetFileSizeAsync(string storageKey)
    {
        var fullPath = GetFullPath(storageKey);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Archivo no encontrado: {storageKey}");
        }

        var fileInfo = new FileInfo(fullPath);
        return Task.FromResult(fileInfo.Length);
    }

    private string GetFullPath(string storageKey)
    {
        // Prevenir path traversal attacks
        var fullPath = Path.GetFullPath(Path.Combine(_basePath, storageKey));

        if (!fullPath.StartsWith(_basePath))
        {
            throw new UnauthorizedAccessException("Acceso no autorizado a la ruta del archivo");
        }

        return fullPath;
    }
}