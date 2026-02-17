using FileStorageService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace FileStorageService.Core.Services;

/// <summary>
/// Image processing service implementation using ImageSharp
/// </summary>
public class ImageProcessingService : IImageProcessingService
{
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(ILogger<ImageProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<Stream> CreateThumbnailAsync(
        Stream imageStream,
        int width,
        int height,
        string resizeMode = "max",
        CancellationToken cancellationToken = default)
    {
        imageStream.Position = 0;

        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        var resizeOptions = new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ParseResizeMode(resizeMode)
        };

        image.Mutate(x => x.Resize(resizeOptions));

        var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 85 }, cancellationToken);
        outputStream.Position = 0;

        _logger.LogDebug("Created thumbnail {Width}x{Height} with mode {Mode}", width, height, resizeMode);

        return outputStream;
    }

    public async Task<Stream> OptimizeAsync(
        Stream imageStream,
        string format = "jpeg",
        int quality = 85,
        CancellationToken cancellationToken = default)
    {
        imageStream.Position = 0;

        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        var outputStream = new MemoryStream();
        var encoder = GetEncoder(format, quality);

        await image.SaveAsync(outputStream, encoder, cancellationToken);
        outputStream.Position = 0;

        _logger.LogDebug("Optimized image to {Format} with quality {Quality}", format, quality);

        return outputStream;
    }

    public async Task<IEnumerable<(VariantConfig Config, Stream Stream)>> GenerateVariantsAsync(
        Stream imageStream,
        IEnumerable<VariantConfig> variants,
        CancellationToken cancellationToken = default)
    {
        var results = new List<(VariantConfig Config, Stream Stream)>();

        // Read original image once
        imageStream.Position = 0;
        using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);

        foreach (var variant in variants)
        {
            try
            {
                using var clonedImage = originalImage.Clone(ctx =>
                {
                    var resizeOptions = new ResizeOptions
                    {
                        Size = new Size(variant.MaxWidth, variant.MaxHeight),
                        Mode = ParseResizeMode(variant.ResizeMode)
                    };
                    ctx.Resize(resizeOptions);
                });

                var outputStream = new MemoryStream();
                var encoder = GetEncoder(variant.Format, variant.Quality);

                await clonedImage.SaveAsync(outputStream, encoder, cancellationToken);
                outputStream.Position = 0;

                results.Add((variant, outputStream));

                _logger.LogDebug("Generated variant {Name} at {Width}x{Height}",
                    variant.Name, variant.MaxWidth, variant.MaxHeight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate variant {Name}", variant.Name);
            }
        }

        return results;
    }

    public async Task<(int Width, int Height)> GetDimensionsAsync(Stream imageStream)
    {
        imageStream.Position = 0;

        var imageInfo = await Image.IdentifyAsync(imageStream);

        if (imageInfo == null)
        {
            throw new InvalidOperationException("Unable to identify image");
        }

        return (imageInfo.Width, imageInfo.Height);
    }

    public async Task<bool> ValidateImageAsync(Stream imageStream)
    {
        try
        {
            imageStream.Position = 0;
            var imageInfo = await Image.IdentifyAsync(imageStream);
            return imageInfo != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Stream> ConvertFormatAsync(
        Stream imageStream,
        string targetFormat,
        CancellationToken cancellationToken = default)
    {
        imageStream.Position = 0;

        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        var outputStream = new MemoryStream();
        var encoder = GetEncoder(targetFormat, 90);

        await image.SaveAsync(outputStream, encoder, cancellationToken);
        outputStream.Position = 0;

        _logger.LogDebug("Converted image to {Format}", targetFormat);

        return outputStream;
    }

    public async Task<Stream> StripExifAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        imageStream.Position = 0;

        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        // Remove EXIF metadata
        image.Metadata.ExifProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.XmpProfile = null;

        var outputStream = new MemoryStream();

        // Save in original format or default to JPEG
        var format = image.Metadata.DecodedImageFormat;
        if (format != null)
        {
            var encoder = GetEncoderForFormat(format);
            await image.SaveAsync(outputStream, encoder, cancellationToken);
        }
        else
        {
            await image.SaveAsJpegAsync(outputStream, cancellationToken);
        }

        outputStream.Position = 0;

        _logger.LogDebug("Stripped EXIF data from image");

        return outputStream;
    }

    private static IImageEncoder GetEncoderForFormat(IImageFormat format)
    {
        return format.Name.ToLowerInvariant() switch
        {
            "jpeg" or "jpg" => new JpegEncoder { Quality = 90 },
            "png" => new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression },
            "webp" => new WebpEncoder { Quality = 90 },
            "gif" => new SixLabors.ImageSharp.Formats.Gif.GifEncoder(),
            _ => new JpegEncoder { Quality = 90 }
        };
    }

    private static ResizeMode ParseResizeMode(string mode)
    {
        return mode.ToLowerInvariant() switch
        {
            "max" => ResizeMode.Max,
            "crop" => ResizeMode.Crop,
            "pad" => ResizeMode.Pad,
            "stretch" => ResizeMode.Stretch,
            "boxpad" => ResizeMode.BoxPad,
            "min" => ResizeMode.Min,
            _ => ResizeMode.Max
        };
    }

    private static IImageEncoder GetEncoder(string format, int quality)
    {
        return format.ToLowerInvariant() switch
        {
            "jpeg" or "jpg" => new JpegEncoder { Quality = quality },
            "png" => new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression },
            "webp" => new WebpEncoder { Quality = quality },
            _ => new JpegEncoder { Quality = quality }
        };
    }
}
