using System.Security.Cryptography;
using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace FileStorageService.Core.Services;

/// <summary>
/// Metadata extraction service implementation
/// </summary>
public class MetadataExtractorService : IMetadataExtractorService
{
    private readonly ILogger<MetadataExtractorService> _logger;

    private static readonly HashSet<string> SupportedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/tiff", "image/bmp"
    };

    private static readonly HashSet<string> SupportedVideoTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4", "video/webm", "video/quicktime", "video/x-msvideo"
    };

    private static readonly HashSet<string> SupportedAudioTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "audio/mpeg", "audio/wav", "audio/ogg", "audio/flac"
    };

    private static readonly HashSet<string> SupportedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    public MetadataExtractorService(ILogger<MetadataExtractorService> logger)
    {
        _logger = logger;
    }

    public async Task<FileMetadata> ExtractAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var metadata = new FileMetadata
        {
            ContentType = contentType,
            Extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant()
        };

        try
        {
            // Calculate file size and hash
            fileStream.Position = 0;
            metadata.SizeBytes = fileStream.Length;
            metadata.ContentHash = await CalculateHashAsync(fileStream);

            // Extract type-specific metadata
            if (SupportedImageTypes.Contains(contentType))
            {
                fileStream.Position = 0;
                var (imageMetadata, exifData) = await ExtractImageMetadataAsync(fileStream, cancellationToken);
                metadata.Image = imageMetadata;
                metadata.Exif = exifData;
            }
            else if (SupportedVideoTypes.Contains(contentType))
            {
                fileStream.Position = 0;
                metadata.Video = await ExtractVideoMetadataAsync(fileStream, cancellationToken);
            }
            else if (SupportedAudioTypes.Contains(contentType))
            {
                fileStream.Position = 0;
                metadata.Audio = await ExtractAudioMetadataAsync(fileStream, cancellationToken);
            }
            else if (SupportedDocumentTypes.Contains(contentType))
            {
                fileStream.Position = 0;
                metadata.Document = await ExtractDocumentMetadataAsync(fileStream, contentType, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting metadata for {FileName}", fileName);
            metadata.Errors.Add($"Metadata extraction error: {ex.Message}");
        }

        metadata.ExtractedAt = DateTime.UtcNow;
        metadata.ExtractionDurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

        return metadata;
    }

    public async Task<(ImageMetadata? Image, ExifData? Exif)> ExtractImageMetadataAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        imageStream.Position = 0;

        ImageMetadata? imageMetadata = null;
        ExifData? exifData = null;

        try
        {
            // Use ImageSharp to get basic image info
            var imageInfo = await Image.IdentifyAsync(imageStream, cancellationToken);

            if (imageInfo != null)
            {
                imageMetadata = new ImageMetadata
                {
                    Width = imageInfo.Width,
                    Height = imageInfo.Height,
                    Format = imageInfo.Metadata.DecodedImageFormat?.Name ?? "Unknown",
                    BitsPerPixel = imageInfo.PixelType?.BitsPerPixel ?? 0,
                    HorizontalResolution = imageInfo.Metadata.HorizontalResolution,
                    VerticalResolution = imageInfo.Metadata.VerticalResolution
                };
            }

            // Use MetadataExtractor for EXIF
            imageStream.Position = 0;
            var directories = ImageMetadataReader.ReadMetadata(imageStream);

            exifData = new ExifData();

            foreach (var directory in directories)
            {
                if (directory is ExifIfd0Directory ifd0)
                {
                    exifData.Make = ifd0.GetDescription(ExifDirectoryBase.TagMake);
                    exifData.Model = ifd0.GetDescription(ExifDirectoryBase.TagModel);
                    exifData.Software = ifd0.GetDescription(ExifDirectoryBase.TagSoftware);

                    if (ifd0.TryGetInt32(ExifDirectoryBase.TagOrientation, out var orientation))
                        exifData.Orientation = orientation;
                }
                else if (directory is ExifSubIfdDirectory subIfd)
                {
                    if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTaken))
                        exifData.DateTaken = dateTaken;

                    exifData.ExposureTime = subIfd.GetDescription(ExifDirectoryBase.TagExposureTime);

                    if (subIfd.TryGetDouble(ExifDirectoryBase.TagFNumber, out var fNumber))
                        exifData.FNumber = fNumber;

                    if (subIfd.TryGetInt32(ExifDirectoryBase.TagIsoEquivalent, out var iso))
                        exifData.IsoSpeed = iso;

                    if (subIfd.TryGetDouble(ExifDirectoryBase.TagFocalLength, out var focalLength))
                        exifData.FocalLength = focalLength;

                    exifData.Flash = subIfd.GetDescription(ExifDirectoryBase.TagFlash);
                    exifData.WhiteBalance = subIfd.GetDescription(ExifDirectoryBase.TagWhiteBalance);
                    exifData.MeteringMode = subIfd.GetDescription(ExifDirectoryBase.TagMeteringMode);
                    exifData.LensModel = subIfd.GetDescription(ExifDirectoryBase.TagLensModel);
                }
                else if (directory is GpsDirectory gps)
                {
                    var geoLocation = gps.GetGeoLocation();
                    if (geoLocation != null)
                    {
                        exifData.GpsLatitude = geoLocation.Latitude;
                        exifData.GpsLongitude = geoLocation.Longitude;
                    }

                    if (gps.TryGetDouble(GpsDirectory.TagAltitude, out var altitude))
                        exifData.GpsAltitude = altitude;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting image metadata");
        }

        return (imageMetadata, exifData);
    }

    public Task<VideoMetadata?> ExtractVideoMetadataAsync(
        Stream videoStream,
        CancellationToken cancellationToken = default)
    {
        // Basic video metadata extraction
        // For full video metadata, you would need FFprobe or similar
        var metadata = new VideoMetadata
        {
            ContainerFormat = "mp4" // Default assumption
        };

        _logger.LogDebug("Video metadata extraction is limited without FFprobe");

        return Task.FromResult<VideoMetadata?>(metadata);
    }

    public Task<AudioMetadata?> ExtractAudioMetadataAsync(
        Stream audioStream,
        CancellationToken cancellationToken = default)
    {
        // Basic audio metadata extraction
        // For full audio metadata, you would need TagLib or similar
        var metadata = new AudioMetadata();

        _logger.LogDebug("Audio metadata extraction is limited");

        return Task.FromResult<AudioMetadata?>(metadata);
    }

    public Task<DocumentMetadata?> ExtractDocumentMetadataAsync(
        Stream documentStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var metadata = new DocumentMetadata();

        // Basic document metadata - for PDF you would need iTextSharp or similar
        _logger.LogDebug("Document metadata extraction is limited for {ContentType}", contentType);

        return Task.FromResult<DocumentMetadata?>(metadata);
    }

    public async Task<string> CalculateHashAsync(Stream fileStream, string algorithm = "SHA256")
    {
        fileStream.Position = 0;

        using var hashAlgorithm = algorithm.ToUpperInvariant() switch
        {
            "MD5" => (HashAlgorithm)MD5.Create(),
            "SHA1" => SHA1.Create(),
            "SHA512" => SHA512.Create(),
            _ => SHA256.Create()
        };

        var hash = await hashAlgorithm.ComputeHashAsync(fileStream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public bool SupportsContentType(string contentType)
    {
        return SupportedImageTypes.Contains(contentType) ||
               SupportedVideoTypes.Contains(contentType) ||
               SupportedAudioTypes.Contains(contentType) ||
               SupportedDocumentTypes.Contains(contentType);
    }
}
