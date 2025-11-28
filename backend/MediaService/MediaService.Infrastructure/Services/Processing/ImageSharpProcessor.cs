using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using MediaService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using DomainImageInfo = MediaService.Domain.Interfaces.Services.ImageInfo;

namespace MediaService.Infrastructure.Services.Processing;

public class ImageSharpProcessor : IImageProcessor
{
    private readonly ILogger<ImageSharpProcessor> _logger;

    public ImageSharpProcessor(ILogger<ImageSharpProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<ImageProcessingResult> ProcessImageAsync(Stream imageStream, string originalFileName, IEnumerable<ImageVariantConfig> variants)
    {
        var result = new ImageProcessingResult();

        try
        {
            imageStream.Position = 0;
            using var image = await Image.LoadAsync(imageStream);
            result.OriginalImageInfo = new DomainImageInfo
            {
                Width = image.Width,
                Height = image.Height,
                Format = image.Metadata.DecodedImageFormat?.Name ?? "Unknown",
                FileSize = imageStream.Length
            };

            foreach (var variantConfig in variants)
            {
                try
                {
                    imageStream.Position = 0;
                    using var variantImage = await Image.LoadAsync(imageStream);
                    var processedVariant = await ProcessVariantAsync(variantImage, variantConfig);
                    result.Variants.Add(processedVariant);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process variant {VariantName}", variantConfig.Name);
                }
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Image processing failed");
        }

        return result;
    }

    public async Task<DomainImageInfo> GetImageInfoAsync(Stream imageStream)
    {
        try
        {
            imageStream.Position = 0;
            using var image = await Image.LoadAsync(imageStream);
            return new DomainImageInfo
            {
                Width = image.Width,
                Height = image.Height,
                Format = image.Metadata.DecodedImageFormat?.Name ?? "Unknown",
                FileSize = imageStream.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get image info");
            throw;
        }
    }

    public async Task<bool> ValidateImageAsync(Stream imageStream, string contentType)
    {
        try
        {
            imageStream.Position = 0;
            using var image = await Image.LoadAsync(imageStream);
            return image.Width > 0 && image.Height > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Stream> OptimizeImageAsync(Stream imageStream, string contentType, int quality)
    {
        try
        {
            imageStream.Position = 0;
            using var image = await Image.LoadAsync(imageStream);

            var encoder = GetEncoder(contentType, quality);
            var outputStream = new MemoryStream();

            await image.SaveAsync(outputStream, encoder);
            outputStream.Position = 0;

            return outputStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image optimization failed");
            throw;
        }
    }

    public async Task<Stream> CreateThumbnailAsync(Stream imageStream, int width, int height, string resizeMode = "Max")
    {
        try
        {
            imageStream.Position = 0;
            using var image = await Image.LoadAsync(imageStream);

            var resizeOptions = new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = GetResizeMode(resizeMode)
            };

            image.Mutate(x => x.Resize(resizeOptions));

            var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new JpegEncoder { Quality = 80 });
            outputStream.Position = 0;

            return outputStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Thumbnail creation failed");
            throw;
        }
    }

    private async Task<ProcessedVariant> ProcessVariantAsync(Image image, ImageVariantConfig config)
    {
        var resizeOptions = new ResizeOptions
        {
            Size = new Size(config.MaxWidth, config.MaxHeight),
            Mode = GetResizeMode(config.ResizeMode)
        };

        image.Mutate(x => x.Resize(resizeOptions));

        var encoder = GetEncoder(config.Format, config.Quality);
        var outputStream = new MemoryStream();

        await image.SaveAsync(outputStream, encoder);
        outputStream.Position = 0;

        return new ProcessedVariant
        {
            Name = config.Name,
            ImageStream = outputStream,
            ContentType = GetContentType(config.Format),
            SizeBytes = outputStream.Length,
            Width = image.Width,
            Height = image.Height,
            StorageKey = $"{config.Name}_{image.Width}x{image.Height}"
        };
    }

    private static ResizeMode GetResizeMode(string mode)
    {
        return mode.ToLower() switch
        {
            "crop" => ResizeMode.Crop,
            "pad" => ResizeMode.Pad,
            "boxpad" => ResizeMode.BoxPad,
            "max" => ResizeMode.Max,
            "min" => ResizeMode.Min,
            "stretch" => ResizeMode.Stretch,
            _ => ResizeMode.Max
        };
    }

    private static IImageEncoder GetEncoder(string format, int quality)
    {
        return format.ToLower() switch
        {
            "png" => new PngEncoder(),
            "webp" => new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = quality },
            _ => new JpegEncoder { Quality = quality }
        };
    }

    private static string GetContentType(string format)
    {
        return format.ToLower() switch
        {
            "png" => "image/png",
            "webp" => "image/webp",
            _ => "image/jpeg"
        };
    }
}