using FileStorageService.Core.Models;

namespace FileStorageService.Core.Interfaces;

/// <summary>
/// Interface for presigned URL operations
/// </summary>
public interface IPresignedUrlService
{
    /// <summary>
    /// Generates a presigned URL for file upload
    /// </summary>
    /// <param name="request">Upload request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload initialization response</returns>
    Task<UploadInitResponse> GenerateUploadUrlAsync(PresignedUrlRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a presigned URL for file download
    /// </summary>
    /// <param name="storageKey">Storage key of the file</param>
    /// <param name="expirationMinutes">URL expiration in minutes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Presigned URL for download</returns>
    Task<PresignedUrl> GenerateDownloadUrlAsync(string storageKey, int expirationMinutes = 60, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a short-lived access token for URL validation
    /// </summary>
    /// <param name="storageKey">Storage key</param>
    /// <param name="operation">Operation type (upload/download)</param>
    /// <param name="expirationMinutes">Token expiration</param>
    /// <returns>Access token</returns>
    string GenerateAccessToken(string storageKey, PresignedUrlType operation, int expirationMinutes = 60);

    /// <summary>
    /// Validates an access token
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <param name="storageKey">Expected storage key</param>
    /// <param name="operation">Expected operation</param>
    /// <returns>True if valid</returns>
    bool ValidateAccessToken(string token, string storageKey, PresignedUrlType operation);

    /// <summary>
    /// Generates a unique storage key for a file
    /// </summary>
    /// <param name="ownerId">Owner identifier</param>
    /// <param name="context">Optional context/category</param>
    /// <param name="fileName">Original file name</param>
    /// <returns>Storage key</returns>
    string GenerateStorageKey(string ownerId, string? context, string fileName);

    /// <summary>
    /// Gets the public URL for a file (via CDN if configured)
    /// </summary>
    /// <param name="storageKey">Storage key</param>
    /// <returns>Public URL</returns>
    Task<string> GetPublicUrlAsync(string storageKey);

    /// <summary>
    /// Revokes/invalidates a presigned URL
    /// </summary>
    /// <param name="urlId">URL identifier</param>
    /// <returns>True if revoked</returns>
    Task<bool> RevokeUrlAsync(string urlId);
}
