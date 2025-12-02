using System.Security.Cryptography;
using System.Text;
using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileStorageService.Core.Services;

/// <summary>
/// Presigned URL service implementation
/// </summary>
public class PresignedUrlService : IPresignedUrlService
{
    private readonly StorageProviderConfig _config;
    private readonly IStorageProvider _storageProvider;
    private readonly ILogger<PresignedUrlService> _logger;
    private readonly byte[] _secretKey;

    public PresignedUrlService(
        IOptions<StorageProviderConfig> config,
        IStorageProvider storageProvider,
        ILogger<PresignedUrlService> logger)
    {
        _config = config.Value;
        _storageProvider = storageProvider;
        _logger = logger;

        // Use connection string as secret key base, or generate a random one
        var keyBase = !string.IsNullOrEmpty(_config.ConnectionString)
            ? _config.ConnectionString
            : Guid.NewGuid().ToString();

        using var sha256 = SHA256.Create();
        _secretKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyBase));
    }

    public async Task<UploadInitResponse> GenerateUploadUrlAsync(
        PresignedUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        var fileId = Guid.NewGuid().ToString();
        var storageKey = GenerateStorageKey(request.OwnerId, request.Context, request.FileName);
        var expiry = TimeSpan.FromMinutes(request.ExpirationMinutes);

        var presignedUrl = await _storageProvider.GenerateUploadUrlAsync(
            storageKey, request.ContentType, expiry);

        // Add token for validation
        presignedUrl.Token = GenerateAccessToken(storageKey, PresignedUrlType.Upload, request.ExpirationMinutes);
        presignedUrl.OwnerId = request.OwnerId;
        presignedUrl.Context = request.Context;
        presignedUrl.MaxSizeBytes = _config.MaxFileSizeBytes;
        presignedUrl.Metadata = request.Metadata;
        presignedUrl.CallbackUrl = request.CallbackUrl;

        var response = new UploadInitResponse
        {
            FileId = fileId,
            UploadUrl = presignedUrl,
            StorageKey = storageKey,
            Instructions = new UploadInstructions
            {
                Method = presignedUrl.HttpMethod,
                RequiredHeaders = presignedUrl.Headers,
                MaxSizeBytes = _config.MaxFileSizeBytes,
                AllowedContentTypes = _config.AllowedContentTypes,
                FinalizeEndpoint = $"/api/files/{fileId}/finalize",
                Notes = "After uploading to the presigned URL, call the finalize endpoint to complete processing"
            }
        };

        _logger.LogDebug("Generated upload URL for {FileName}, expires in {Minutes} minutes",
            request.FileName, request.ExpirationMinutes);

        return response;
    }

    public async Task<PresignedUrl> GenerateDownloadUrlAsync(
        string storageKey,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        var expiry = TimeSpan.FromMinutes(expirationMinutes);
        var presignedUrl = await _storageProvider.GenerateDownloadUrlAsync(storageKey, expiry);

        // Add token for validation
        presignedUrl.Token = GenerateAccessToken(storageKey, PresignedUrlType.Download, expirationMinutes);

        _logger.LogDebug("Generated download URL for {StorageKey}, expires in {Minutes} minutes",
            storageKey, expirationMinutes);

        return presignedUrl;
    }

    public string GenerateAccessToken(string storageKey, PresignedUrlType operation, int expirationMinutes = 60)
    {
        var expiration = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes).ToUnixTimeSeconds();
        var data = $"{storageKey}|{operation}|{expiration}";

        using var hmac = new HMACSHA256(_secretKey);
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        var signatureBase64 = Convert.ToBase64String(signature)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return $"{expiration}_{signatureBase64}";
    }

    public bool ValidateAccessToken(string token, string storageKey, PresignedUrlType operation)
    {
        try
        {
            var parts = token.Split('_', 2);
            if (parts.Length != 2)
                return false;

            if (!long.TryParse(parts[0], out var expiration))
                return false;

            // Check expiration
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiration)
            {
                _logger.LogDebug("Token expired for {StorageKey}", storageKey);
                return false;
            }

            // Verify signature
            var data = $"{storageKey}|{operation}|{expiration}";
            using var hmac = new HMACSHA256(_secretKey);
            var expectedSignature = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            var expectedBase64 = Convert.ToBase64String(expectedSignature)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            return parts[1] == expectedBase64;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed for {StorageKey}", storageKey);
            return false;
        }
    }

    public string GenerateStorageKey(string ownerId, string? context, string fileName)
    {
        // Generate a structured storage key
        // Format: {ownerId}/{context?}/{year}/{month}/{day}/{guid}_{filename}

        var now = DateTime.UtcNow;
        var guid = Guid.NewGuid().ToString("N")[..8]; // Short GUID prefix
        var sanitizedFileName = SanitizeFileName(fileName);

        var pathParts = new List<string> { ownerId };

        if (!string.IsNullOrEmpty(context))
        {
            pathParts.Add(context);
        }

        pathParts.Add(now.Year.ToString());
        pathParts.Add(now.Month.ToString("D2"));
        pathParts.Add(now.Day.ToString("D2"));
        pathParts.Add($"{guid}_{sanitizedFileName}");

        return string.Join("/", pathParts);
    }

    public async Task<string> GetPublicUrlAsync(string storageKey)
    {
        return await _storageProvider.GetPublicUrlAsync(storageKey);
    }

    public Task<bool> RevokeUrlAsync(string urlId)
    {
        // For stateless token-based URLs, we can't truly revoke them
        // In production, you might maintain a blacklist in Redis/cache

        _logger.LogInformation("URL revocation requested for {UrlId} (not implemented for stateless tokens)", urlId);
        return Task.FromResult(true);
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove dangerous characters and limit length
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new StringBuilder();

        foreach (var c in fileName)
        {
            if (!invalidChars.Contains(c) && c != ' ')
            {
                sanitized.Append(c);
            }
            else if (c == ' ')
            {
                sanitized.Append('_');
            }
        }

        var result = sanitized.ToString();

        // Limit to reasonable length
        if (result.Length > 100)
        {
            var extension = Path.GetExtension(result);
            var name = Path.GetFileNameWithoutExtension(result);
            result = name[..(100 - extension.Length)] + extension;
        }

        return result;
    }
}
