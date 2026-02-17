using FileStorageService.Core.Models;

namespace FileStorageService.Core.Interfaces;

/// <summary>
/// Interface for image processing operations
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// Creates a thumbnail
    /// </summary>
    /// <param name="imageStream">Source image stream</param>
    /// <param name="width">Target width</param>
    /// <param name="height">Target height</param>
    /// <param name="resizeMode">Resize mode (max, crop, pad)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Thumbnail stream</returns>
    Task<Stream> CreateThumbnailAsync(Stream imageStream, int width, int height, string resizeMode = "max", CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimizes an image
    /// </summary>
    /// <param name="imageStream">Source image stream</param>
    /// <param name="format">Output format</param>
    /// <param name="quality">Quality (1-100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Optimized image stream</returns>
    Task<Stream> OptimizeAsync(Stream imageStream, string format = "jpeg", int quality = 85, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates multiple variants of an image
    /// </summary>
    /// <param name="imageStream">Source image stream</param>
    /// <param name="variants">Variant configurations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated variants with streams</returns>
    Task<IEnumerable<(VariantConfig Config, Stream Stream)>> GenerateVariantsAsync(Stream imageStream, IEnumerable<VariantConfig> variants, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets image dimensions
    /// </summary>
    /// <param name="imageStream">Image stream</param>
    /// <returns>Width and height</returns>
    Task<(int Width, int Height)> GetDimensionsAsync(Stream imageStream);

    /// <summary>
    /// Validates if stream is a valid image
    /// </summary>
    /// <param name="imageStream">Image stream</param>
    /// <returns>True if valid</returns>
    Task<bool> ValidateImageAsync(Stream imageStream);

    /// <summary>
    /// Converts image to different format
    /// </summary>
    /// <param name="imageStream">Source stream</param>
    /// <param name="targetFormat">Target format</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Converted stream</returns>
    Task<Stream> ConvertFormatAsync(Stream imageStream, string targetFormat, CancellationToken cancellationToken = default);

    /// <summary>
    /// Strips EXIF metadata from image
    /// </summary>
    /// <param name="imageStream">Image stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Image without EXIF</returns>
    Task<Stream> StripExifAsync(Stream imageStream, CancellationToken cancellationToken = default);
}
