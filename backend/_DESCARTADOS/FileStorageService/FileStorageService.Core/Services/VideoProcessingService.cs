using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FileStorageService.Core.Services;

/// <summary>
/// Video processing service using FFmpeg
/// </summary>
public class VideoProcessingService : IVideoProcessingService
{
    private readonly ILogger<VideoProcessingService> _logger;
    private readonly FFmpegOptions _options;

    public VideoProcessingService(
        ILogger<VideoProcessingService> logger,
        FFmpegOptions? options = null)
    {
        _logger = logger;
        _options = options ?? new FFmpegOptions();
    }

    public async Task<Dictionary<double, Stream>> GenerateVideoThumbnailsAsync(
        Stream videoStream,
        IEnumerable<double> timestamps,
        int width = 320,
        int height = 180,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<double, Stream>();
        var inputFile = await SaveStreamToTempFile(videoStream, "input.mp4");

        try
        {
            foreach (var timestamp in timestamps)
            {
                var outputFile = Path.Combine(_options.WorkingDirectory, $"thumb_{Guid.NewGuid()}.jpg");

                var arguments = $"-ss {timestamp} -i \"{inputFile}\" -vframes 1 -vf scale={width}:{height} -y \"{outputFile}\"";

                var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

                if (success && File.Exists(outputFile))
                {
                    var thumbnailStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));
                    results[timestamp] = thumbnailStream;
                    File.Delete(outputFile);
                }
            }

            _logger.LogInformation("Generated {Count} video thumbnails", results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate video thumbnails");
            throw;
        }
        finally
        {
            if (File.Exists(inputFile))
                File.Delete(inputFile);
        }

        return results;
    }

    public async Task<VideoMetadata> ExtractVideoMetadataAsync(
        Stream videoStream,
        CancellationToken cancellationToken = default)
    {
        var inputFile = await SaveStreamToTempFile(videoStream, "input.mp4");

        try
        {
            var arguments = $"-v quiet -print_format json -show_format -show_streams \"{inputFile}\"";

            var output = await ExecuteFFprobeAsync(arguments, cancellationToken);

            if (string.IsNullOrEmpty(output))
            {
                throw new InvalidOperationException("Failed to extract video metadata");
            }

            var metadata = ParseFFprobeOutput(output);

            _logger.LogDebug("Extracted video metadata: {Duration}s, {Width}x{Height}",
                metadata.DurationSeconds, metadata.Width, metadata.Height);

            return metadata;
        }
        finally
        {
            if (File.Exists(inputFile))
                File.Delete(inputFile);
        }
    }

    public async Task<Stream> TranscodeVideoAsync(
        Stream videoStream,
        string outputFormat = "mp4",
        string videoCodec = "h264",
        string audioCodec = "aac",
        string preset = "medium",
        int crf = 23,
        CancellationToken cancellationToken = default)
    {
        var inputFile = await SaveStreamToTempFile(videoStream, "input.mp4");
        var outputFile = Path.Combine(_options.WorkingDirectory, $"output_{Guid.NewGuid()}.{outputFormat}");

        try
        {
            var codecMap = new Dictionary<string, string>
            {
                ["h264"] = "libx264",
                ["h265"] = "libx265",
                ["vp9"] = "libvpx-vp9",
                ["av1"] = "libaom-av1"
            };

            var vcodec = codecMap.ContainsKey(videoCodec) ? codecMap[videoCodec] : videoCodec;

            var arguments = $"-i \"{inputFile}\" -c:v {vcodec} -preset {preset} -crf {crf} -c:a {audioCodec} -y \"{outputFile}\"";

            if (_options.UseHardwareAcceleration && !string.IsNullOrEmpty(_options.HardwareAccelerationMethod))
            {
                arguments = $"-hwaccel {_options.HardwareAccelerationMethod} {arguments}";
            }

            var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

            if (!success || !File.Exists(outputFile))
            {
                throw new InvalidOperationException("Video transcoding failed");
            }

            var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));
            File.Delete(outputFile);

            _logger.LogInformation("Transcoded video to {Format} ({Codec})", outputFormat, videoCodec);

            return outputStream;
        }
        finally
        {
            if (File.Exists(inputFile))
                File.Delete(inputFile);
            if (File.Exists(outputFile))
                File.Delete(outputFile);
        }
    }

    public async Task<IEnumerable<(VideoVariantConfig Config, Stream Stream)>> GenerateVideoVariantsAsync(
        Stream videoStream,
        IEnumerable<VideoVariantConfig> variants,
        CancellationToken cancellationToken = default)
    {
        var results = new List<(VideoVariantConfig Config, Stream Stream)>();
        var inputFile = await SaveStreamToTempFile(videoStream, "input.mp4");

        try
        {
            foreach (var variant in variants)
            {
                var outputFile = Path.Combine(_options.WorkingDirectory, $"{variant.Name}_{Guid.NewGuid()}.{variant.Format}");

                var codecMap = new Dictionary<string, string>
                {
                    ["h264"] = "libx264",
                    ["h265"] = "libx265",
                    ["vp9"] = "libvpx-vp9"
                };

                var vcodec = codecMap.ContainsKey(variant.VideoCodec) ? codecMap[variant.VideoCodec] : variant.VideoCodec;

                var scaleFilter = $"scale={variant.Width}:{variant.Height}";
                var videoArgs = $"-c:v {vcodec} -preset {variant.Preset} -crf {variant.CRF} -b:v {variant.VideoBitrate}k";
                var audioArgs = variant.IncludeAudio ? $"-c:a {variant.AudioCodec} -b:a {variant.AudioBitrate}k" : "-an";

                if (variant.FrameRate.HasValue)
                {
                    videoArgs += $" -r {variant.FrameRate.Value}";
                }

                if (variant.KeyframeInterval.HasValue)
                {
                    videoArgs += $" -g {variant.KeyframeInterval.Value * (variant.FrameRate ?? 30)}";
                }

                var arguments = $"-i \"{inputFile}\" -vf \"{scaleFilter}\" {videoArgs} {audioArgs} -y \"{outputFile}\"";

                var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

                if (success && File.Exists(outputFile))
                {
                    var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));
                    results.Add((variant, outputStream));
                    File.Delete(outputFile);

                    _logger.LogInformation("Generated video variant: {Name} ({Width}x{Height})",
                        variant.Name, variant.Width, variant.Height);
                }
            }
        }
        finally
        {
            if (File.Exists(inputFile))
                File.Delete(inputFile);
        }

        return results;
    }

    public async Task<Stream> ExtractAudioFromVideoAsync(
        Stream videoStream,
        string outputFormat = "mp3",
        int bitrate = 192,
        CancellationToken cancellationToken = default)
    {
        var inputFile = await SaveStreamToTempFile(videoStream, "input.mp4");
        var outputFile = Path.Combine(_options.WorkingDirectory, $"audio_{Guid.NewGuid()}.{outputFormat}");

        try
        {
            var arguments = $"-i \"{inputFile}\" -vn -acodec libmp3lame -b:a {bitrate}k -y \"{outputFile}\"";

            var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

            if (!success || !File.Exists(outputFile))
            {
                throw new InvalidOperationException("Audio extraction failed");
            }

            var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));
            File.Delete(outputFile);

            _logger.LogInformation("Extracted audio from video to {Format}", outputFormat);

            return outputStream;
        }
        finally
        {
            if (File.Exists(inputFile))
                File.Delete(inputFile);
            if (File.Exists(outputFile))
                File.Delete(outputFile);
        }
    }

    public async Task<Stream> CreateVideoPreviewAsync(
        Stream videoStream,
        IEnumerable<(double Start, double Duration)> segments,
        CancellationToken cancellationToken = default)
    {
        var inputFile = await SaveStreamToTempFile(videoStream, "input.mp4");
        var concatFile = Path.Combine(_options.WorkingDirectory, $"concat_{Guid.NewGuid()}.txt");
        var outputFile = Path.Combine(_options.WorkingDirectory, $"preview_{Guid.NewGuid()}.mp4");
        var segmentFiles = new List<string>();

        try
        {
            // Extract each segment
            int index = 0;
            foreach (var (start, duration) in segments)
            {
                var segmentFile = Path.Combine(_options.WorkingDirectory, $"segment_{index}_{Guid.NewGuid()}.mp4");
                var arguments = $"-ss {start} -i \"{inputFile}\" -t {duration} -c copy -y \"{segmentFile}\"";

                var success = await ExecuteFFmpegAsync(arguments, cancellationToken);
                if (success && File.Exists(segmentFile))
                {
                    segmentFiles.Add(segmentFile);
                }
                index++;
            }

            if (segmentFiles.Count == 0)
            {
                throw new InvalidOperationException("No segments extracted");
            }

            // Create concat file
            var concatContent = string.Join("\n", segmentFiles.Select(f => $"file '{f}'"));
            await File.WriteAllTextAsync(concatFile, concatContent, cancellationToken);

            // Concatenate segments
            var concatArgs = $"-f concat -safe 0 -i \"{concatFile}\" -c copy -y \"{outputFile}\"";
            await ExecuteFFmpegAsync(concatArgs, cancellationToken);

            if (!File.Exists(outputFile))
            {
                throw new InvalidOperationException("Video preview creation failed");
            }

            var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));

            _logger.LogInformation("Created video preview with {Count} segments", segmentFiles.Count);

            return outputStream;
        }
        finally
        {
            if (File.Exists(inputFile)) File.Delete(inputFile);
            if (File.Exists(concatFile)) File.Delete(concatFile);
            if (File.Exists(outputFile)) File.Delete(outputFile);
            foreach (var segmentFile in segmentFiles)
            {
                if (File.Exists(segmentFile)) File.Delete(segmentFile);
            }
        }
    }

    public async Task<Stream> ApplyWatermarkAsync(
        Stream videoStream,
        Stream watermarkImageStream,
        string position = "bottom-right",
        double opacity = 0.7,
        CancellationToken cancellationToken = default)
    {
        var inputFile = await SaveStreamToTempFile(videoStream, "input.mp4");
        var watermarkFile = await SaveStreamToTempFile(watermarkImageStream, "watermark.png");
        var outputFile = Path.Combine(_options.WorkingDirectory, $"watermarked_{Guid.NewGuid()}.mp4");

        try
        {
            var overlayPos = position.ToLower() switch
            {
                "top-left" => "10:10",
                "top-right" => "W-w-10:10",
                "bottom-left" => "10:H-h-10",
                "bottom-right" => "W-w-10:H-h-10",
                "center" => "(W-w)/2:(H-h)/2",
                _ => "W-w-10:H-h-10"
            };

            var filterComplex = $"[1:v]format=rgba,colorchannelmixer=aa={opacity}[logo];[0:v][logo]overlay={overlayPos}";
            var arguments = $"-i \"{inputFile}\" -i \"{watermarkFile}\" -filter_complex \"{filterComplex}\" -codec:a copy -y \"{outputFile}\"";

            var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

            if (!success || !File.Exists(outputFile))
            {
                throw new InvalidOperationException("Watermark application failed");
            }

            var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));

            _logger.LogInformation("Applied watermark to video at position {Position}", position);

            return outputStream;
        }
        finally
        {
            if (File.Exists(inputFile)) File.Delete(inputFile);
            if (File.Exists(watermarkFile)) File.Delete(watermarkFile);
            if (File.Exists(outputFile)) File.Delete(outputFile);
        }
    }

    public async Task<bool> ValidateVideoAsync(Stream videoStream)
    {
        try
        {
            var metadata = await ExtractVideoMetadataAsync(videoStream);
            return metadata.DurationSeconds > 0 && metadata.Width > 0 && metadata.Height > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<double> GetVideoDurationAsync(Stream videoStream)
    {
        var metadata = await ExtractVideoMetadataAsync(videoStream);
        return metadata.DurationSeconds;
    }

    public async Task<bool> IsFFmpegAvailableAsync()
    {
        try
        {
            var output = await ExecuteFFmpegAsync("-version", CancellationToken.None);
            return output;
        }
        catch
        {
            return false;
        }
    }

    // Helper methods

    private async Task<string> SaveStreamToTempFile(Stream stream, string fileName)
    {
        var tempFile = Path.Combine(_options.WorkingDirectory, $"{Guid.NewGuid()}_{fileName}");
        stream.Position = 0;

        using (var fileStream = File.Create(tempFile))
        {
            await stream.CopyToAsync(fileStream);
        }

        stream.Position = 0;
        return tempFile;
    }

    private async Task<bool> ExecuteFFmpegAsync(string arguments, CancellationToken cancellationToken)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = _options.FFmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return false;
            }

            var errorOutput = new StringBuilder();
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorOutput.AppendLine(e.Data);
                }
            };

            process.BeginErrorReadLine();

            var timeout = _options.TimeoutSeconds > 0
                ? TimeSpan.FromSeconds(_options.TimeoutSeconds)
                : Timeout.InfiniteTimeSpan;

            await process.WaitForExitAsync(cancellationToken);

            var success = process.ExitCode == 0;

            if (!success)
            {
                _logger.LogWarning("FFmpeg failed: {Error}", errorOutput.ToString());
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FFmpeg execution failed");
            return false;
        }
    }

    private async Task<string> ExecuteFFprobeAsync(string arguments, CancellationToken cancellationToken)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = _options.FFprobePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return string.Empty;
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            return process.ExitCode == 0 ? output : string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FFprobe execution failed");
            return string.Empty;
        }
    }

    private VideoMetadata ParseFFprobeOutput(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var metadata = new VideoMetadata();

            if (root.TryGetProperty("format", out var format))
            {
                if (format.TryGetProperty("duration", out var duration))
                {
                    metadata.DurationSeconds = double.Parse(duration.GetString() ?? "0");
                }

                if (format.TryGetProperty("bit_rate", out var bitrate))
                {
                    metadata.VideoBitrateKbps = int.Parse(bitrate.GetString() ?? "0") / 1000;
                }
            }

            if (root.TryGetProperty("streams", out var streams))
            {
                foreach (var stream in streams.EnumerateArray())
                {
                    if (!stream.TryGetProperty("codec_type", out var codecType))
                        continue;

                    var type = codecType.GetString();

                    if (type == "video" && metadata.Width == 0)
                    {
                        if (stream.TryGetProperty("width", out var width))
                        {
                            metadata.Width = width.GetInt32();
                        }

                        if (stream.TryGetProperty("height", out var height))
                        {
                            metadata.Height = height.GetInt32();
                        }

                        if (stream.TryGetProperty("codec_name", out var codec))
                        {
                            metadata.VideoCodec = codec.GetString();
                        }

                        if (stream.TryGetProperty("r_frame_rate", out var framerate))
                        {
                            var fps = framerate.GetString();
                            if (!string.IsNullOrEmpty(fps) && fps.Contains('/'))
                            {
                                var parts = fps.Split('/');
                                if (parts.Length == 2 && int.TryParse(parts[0], out var num) && int.TryParse(parts[1], out var den) && den > 0)
                                {
                                    metadata.FrameRate = num / (double)den;
                                }
                            }
                        }
                    }
                    else if (type == "audio")
                    {
                        metadata.HasAudio = true;

                        if (stream.TryGetProperty("codec_name", out var audioCodec))
                        {
                            metadata.AudioCodec = audioCodec.GetString();
                        }

                        if (stream.TryGetProperty("bit_rate", out var audioBitrate))
                        {
                            metadata.AudioBitrateKbps = int.Parse(audioBitrate.GetString() ?? "0") / 1000;
                        }
                    }
                }
            }

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse FFprobe output");
            return new VideoMetadata();
        }
    }
}
