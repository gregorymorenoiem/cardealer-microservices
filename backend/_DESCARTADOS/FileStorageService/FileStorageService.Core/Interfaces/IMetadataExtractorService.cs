using FileStorageService.Core.Models;

namespace FileStorageService.Core.Interfaces;

/// <summary>
/// Interface for extracting metadata from files
/// </summary>
public interface IMetadataExtractorService
{
    /// <summary>
    /// Extracts metadata from a file stream
    /// </summary>
    /// <param name="fileStream">File content stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">Content type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted metadata</returns>
    Task<FileMetadata> ExtractAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts image-specific metadata including EXIF
    /// </summary>
    /// <param name="imageStream">Image stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Image metadata with EXIF</returns>
    Task<(ImageMetadata? Image, ExifData? Exif)> ExtractImageMetadataAsync(Stream imageStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts video metadata
    /// </summary>
    /// <param name="videoStream">Video stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Video metadata</returns>
    Task<VideoMetadata?> ExtractVideoMetadataAsync(Stream videoStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts audio metadata
    /// </summary>
    /// <param name="audioStream">Audio stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audio metadata</returns>
    Task<AudioMetadata?> ExtractAudioMetadataAsync(Stream audioStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts document metadata
    /// </summary>
    /// <param name="documentStream">Document stream</param>
    /// <param name="contentType">Content type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document metadata</returns>
    Task<DocumentMetadata?> ExtractDocumentMetadataAsync(Stream documentStream, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates content hash
    /// </summary>
    /// <param name="fileStream">File stream</param>
    /// <param name="algorithm">Hash algorithm (default SHA-256)</param>
    /// <returns>Hash string</returns>
    Task<string> CalculateHashAsync(Stream fileStream, string algorithm = "SHA256");

    /// <summary>
    /// Checks if extractor supports a content type
    /// </summary>
    /// <param name="contentType">MIME type</param>
    /// <returns>True if supported</returns>
    bool SupportsContentType(string contentType);
}
