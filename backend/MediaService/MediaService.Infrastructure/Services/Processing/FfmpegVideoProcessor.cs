using MediaService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace MediaService.Infrastructure.Services.Processing;

public class FfmpegVideoProcessor : IVideoProcessor
{
    private readonly ILogger<FfmpegVideoProcessor> _logger;
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;

    public FfmpegVideoProcessor(ILogger<FfmpegVideoProcessor> logger)
    {
        _logger = logger;

        // Rutas de FFmpeg (configurables desde settings)
        _ffmpegPath = Environment.GetEnvironmentVariable("FFMPEG_PATH") ?? "ffmpeg";
        _ffprobePath = Environment.GetEnvironmentVariable("FFPROBE_PATH") ?? "ffprobe";
    }

    public async Task<VideoProcessingResult> ProcessVideoAsync(Stream videoStream, string originalFileName, VideoProcessingConfig config)
    {
        var startTime = DateTime.UtcNow;
        var result = new VideoProcessingResult
        {
            Success = false,
            Variants = new List<ProcessedVideoVariant>(),
            Thumbnails = new List<ProcessedThumbnail>()
        };

        string? tempInputPath = null;
        string? tempOutputDir = null;

        try
        {
            _logger.LogInformation("Processing video: {FileName}", originalFileName);

            // Guardar stream temporal
            tempInputPath = Path.Combine(Path.GetTempPath(), $"input_{Guid.NewGuid()}{Path.GetExtension(originalFileName)}");
            tempOutputDir = Path.Combine(Path.GetTempPath(), $"output_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempOutputDir);

            using (var fileStream = File.Create(tempInputPath))
            {
                videoStream.Position = 0;
                await videoStream.CopyToAsync(fileStream);
            }

            // Obtener información del video
            result.OriginalVideoInfo = await GetVideoInfoAsync(tempInputPath);

            // Generar thumbnails
            if (config.GenerateThumbnails)
            {
                result.Thumbnails = await GenerateThumbnailsInternalAsync(
                    tempInputPath,
                    tempOutputDir,
                    config.ThumbnailCount,
                    result.OriginalVideoInfo.Duration
                );
            }

            // Extraer audio
            if (config.ExtractAudio)
            {
                result.AudioTrack = await ExtractAudioInternalAsync(tempInputPath, tempOutputDir, config.AudioFormat);
            }

            // Generar variantes según bitrate ladder
            if (config.BitrateLadder?.Any() == true)
            {
                foreach (var profile in config.BitrateLadder)
                {
                    try
                    {
                        var variant = await TranscodeVideoAsync(
                            tempInputPath,
                            tempOutputDir,
                            profile,
                            config.OutputFormat
                        );
                        result.Variants.Add(variant);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process variant {ProfileName}", profile.Name);
                    }
                }
            }

            // Generar HLS si está configurado
            if (config.GenerateHls && result.Variants.Any())
            {
                await GenerateHlsPlaylistAsync(tempOutputDir, result.Variants, config.HlsSegmentSeconds);
            }

            result.Success = true;
            result.ProcessingDuration = DateTime.UtcNow - startTime;

            _logger.LogInformation("Video processing completed in {Duration}ms", result.ProcessingDuration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Video processing failed for {FileName}", originalFileName);
        }
        finally
        {
            // Cleanup
            try
            {
                if (tempInputPath != null && File.Exists(tempInputPath))
                    File.Delete(tempInputPath);
                if (tempOutputDir != null && Directory.Exists(tempOutputDir))
                    Directory.Delete(tempOutputDir, true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup temp files");
            }
        }

        return result;
    }

    public async Task<VideoInfo> GetVideoInfoAsync(Stream videoStream)
    {
        string? tempPath = null;
        try
        {
            // Guardar stream temporal
            tempPath = Path.Combine(Path.GetTempPath(), $"probe_{Guid.NewGuid()}.mp4");
            using (var fileStream = File.Create(tempPath))
            {
                videoStream.Position = 0;
                await videoStream.CopyToAsync(fileStream);
            }

            return await GetVideoInfoAsync(tempPath);
        }
        finally
        {
            if (tempPath != null && File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private async Task<VideoInfo> GetVideoInfoAsync(string filePath)
    {
        try
        {
            var arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"";
            var output = await ExecuteProcessAsync(_ffprobePath, arguments);

            var probeData = JsonDocument.Parse(output);
            var videoStream = probeData.RootElement
                .GetProperty("streams")
                .EnumerateArray()
                .FirstOrDefault(s => s.GetProperty("codec_type").GetString() == "video");

            var format = probeData.RootElement.GetProperty("format");

            var info = new VideoInfo
            {
                Width = videoStream.TryGetProperty("width", out var width) ? width.GetInt32() : 0,
                Height = videoStream.TryGetProperty("height", out var height) ? height.GetInt32() : 0,
                VideoCodec = videoStream.TryGetProperty("codec_name", out var codec) ? codec.GetString() ?? "" : "",
                Duration = format.TryGetProperty("duration", out var duration)
                    ? TimeSpan.FromSeconds(double.Parse(duration.GetString() ?? "0"))
                    : TimeSpan.Zero,
                Bitrate = format.TryGetProperty("bit_rate", out var bitrate)
                    ? int.Parse(bitrate.GetString() ?? "0")
                    : 0,
                FrameRate = videoStream.TryGetProperty("r_frame_rate", out var fps)
                    ? ParseFrameRate(fps.GetString() ?? "0")
                    : 0,
                FileSize = new FileInfo(filePath).Length,
                Format = format.TryGetProperty("format_name", out var formatName) ? formatName.GetString() ?? "" : ""
            };

            var audioStream = probeData.RootElement
                .GetProperty("streams")
                .EnumerateArray()
                .FirstOrDefault(s => s.GetProperty("codec_type").GetString() == "audio");

            if (audioStream.ValueKind != JsonValueKind.Undefined)
            {
                info.AudioCodec = audioStream.TryGetProperty("codec_name", out var audioCodec)
                    ? audioCodec.GetString() ?? ""
                    : "";
            }

            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to probe video info");
            throw;
        }
    }

    public async Task<bool> ValidateVideoAsync(Stream videoStream, string contentType)
    {
        try
        {
            var info = await GetVideoInfoAsync(videoStream);
            return info.Width > 0 && info.Height > 0 && info.Duration > TimeSpan.Zero;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Stream>> GenerateThumbnailsAsync(Stream videoStream, int count, int? width = null, int? height = null)
    {
        string? tempInputPath = null;
        string? tempOutputDir = null;

        try
        {
            // Guardar stream temporal
            tempInputPath = Path.Combine(Path.GetTempPath(), $"thumb_input_{Guid.NewGuid()}.mp4");
            tempOutputDir = Path.Combine(Path.GetTempPath(), $"thumb_output_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempOutputDir);

            using (var fileStream = File.Create(tempInputPath))
            {
                videoStream.Position = 0;
                await videoStream.CopyToAsync(fileStream);
            }

            var videoInfo = await GetVideoInfoAsync(tempInputPath);
            var thumbnails = await GenerateThumbnailsInternalAsync(tempInputPath, tempOutputDir, count, videoInfo.Duration);

            // Convertir ProcessedThumbnail a Stream
            var streams = new List<Stream>();
            foreach (var thumb in thumbnails)
            {
                var ms = new MemoryStream();
                thumb.ImageStream.Position = 0;
                await thumb.ImageStream.CopyToAsync(ms);
                ms.Position = 0;
                streams.Add(ms);
            }

            return streams;
        }
        finally
        {
            if (tempInputPath != null && File.Exists(tempInputPath))
                File.Delete(tempInputPath);
            if (tempOutputDir != null && Directory.Exists(tempOutputDir))
                Directory.Delete(tempOutputDir, true);
        }
    }

    public async Task<Stream> ExtractAudioAsync(Stream videoStream, string audioFormat)
    {
        string? tempInputPath = null;
        string? tempOutputDir = null;

        try
        {
            tempInputPath = Path.Combine(Path.GetTempPath(), $"audio_input_{Guid.NewGuid()}.mp4");
            tempOutputDir = Path.Combine(Path.GetTempPath(), $"audio_output_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempOutputDir);

            using (var fileStream = File.Create(tempInputPath))
            {
                videoStream.Position = 0;
                await videoStream.CopyToAsync(fileStream);
            }

            return await ExtractAudioInternalAsync(tempInputPath, tempOutputDir, audioFormat);
        }
        finally
        {
            if (tempInputPath != null && File.Exists(tempInputPath))
                File.Delete(tempInputPath);
            if (tempOutputDir != null && Directory.Exists(tempOutputDir))
                Directory.Delete(tempOutputDir, true);
        }
    }

    #region Private Helper Methods

    private async Task<List<ProcessedThumbnail>> GenerateThumbnailsInternalAsync(
        string inputPath,
        string outputDir,
        int count,
        TimeSpan duration)
    {
        var thumbnails = new List<ProcessedThumbnail>();
        var interval = duration.TotalSeconds / (count + 1);

        for (int i = 1; i <= count; i++)
        {
            var timestamp = TimeSpan.FromSeconds(interval * i);
            var outputPath = Path.Combine(outputDir, $"thumb_{i:D3}.jpg");

            var arguments = $"-ss {timestamp.TotalSeconds:F3} -i \"{inputPath}\" -vframes 1 -q:v 2 \"{outputPath}\"";

            try
            {
                await ExecuteProcessAsync(_ffmpegPath, arguments);

                if (File.Exists(outputPath))
                {
                    var thumbStream = new MemoryStream();
                    using (var fileStream = File.OpenRead(outputPath))
                    {
                        await fileStream.CopyToAsync(thumbStream);
                    }
                    thumbStream.Position = 0;

                    // Obtener dimensiones del thumbnail
                    var thumbInfo = await GetImageDimensionsAsync(outputPath);

                    thumbnails.Add(new ProcessedThumbnail
                    {
                        Index = i - 1,
                        Timestamp = timestamp,
                        ImageStream = thumbStream,
                        ContentType = "image/jpeg",
                        SizeBytes = thumbStream.Length,
                        Width = thumbInfo.width,
                        Height = thumbInfo.height,
                        StorageKey = $"thumb_{i:D3}.jpg"
                    });

                    File.Delete(outputPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate thumbnail at {Timestamp}", timestamp);
            }
        }

        return thumbnails;
    }

    private async Task<Stream> ExtractAudioInternalAsync(string inputPath, string outputDir, string audioFormat)
    {
        var outputPath = Path.Combine(outputDir, $"audio.{audioFormat}");

        var codec = audioFormat.ToLower() switch
        {
            "mp3" => "libmp3lame",
            "aac" => "aac",
            "opus" => "libopus",
            "wav" => "pcm_s16le",
            _ => "aac"
        };

        var arguments = $"-i \"{inputPath}\" -vn -acodec {codec} -ab 192k \"{outputPath}\"";

        await ExecuteProcessAsync(_ffmpegPath, arguments);

        if (File.Exists(outputPath))
        {
            var audioStream = new MemoryStream();
            using (var fileStream = File.OpenRead(outputPath))
            {
                await fileStream.CopyToAsync(audioStream);
            }
            audioStream.Position = 0;
            File.Delete(outputPath);
            return audioStream;
        }

        return Stream.Null;
    }

    private async Task<ProcessedVideoVariant> TranscodeVideoAsync(
        string inputPath,
        string outputDir,
        VideoBitrateProfile profile,
        string outputFormat)
    {
        var outputFileName = $"{profile.Name}.{outputFormat}";
        var outputPath = Path.Combine(outputDir, outputFileName);

        // Construir argumentos de FFmpeg
        var arguments = new StringBuilder();
        arguments.Append($"-i \"{inputPath}\" ");
        arguments.Append($"-vf scale=-2:{profile.Height} ");
        arguments.Append($"-c:v {profile.VideoCodec} ");
        arguments.Append($"-b:v {profile.VideoBitrate} ");
        arguments.Append($"-c:a {profile.AudioCodec} ");
        arguments.Append($"-b:a {profile.AudioBitrate} ");
        arguments.Append($"-profile:v {profile.Profile} ");
        arguments.Append($"-level {profile.Level} ");
        arguments.Append($"-movflags +faststart ");
        arguments.Append($"\"{outputPath}\"");

        await ExecuteProcessAsync(_ffmpegPath, arguments.ToString());

        if (File.Exists(outputPath))
        {
            var variantStream = new MemoryStream();
            using (var fileStream = File.OpenRead(outputPath))
            {
                await fileStream.CopyToAsync(variantStream);
            }
            variantStream.Position = 0;

            var variantInfo = await GetVideoInfoAsync(outputPath);
            File.Delete(outputPath);

            return new ProcessedVideoVariant
            {
                Name = profile.Name,
                VideoStream = variantStream,
                ContentType = $"video/{outputFormat}",
                SizeBytes = variantStream.Length,
                Width = variantInfo.Width,
                Height = variantInfo.Height,
                VideoCodec = variantInfo.VideoCodec,
                AudioCodec = variantInfo.AudioCodec,
                Duration = variantInfo.Duration,
                Bitrate = variantInfo.Bitrate,
                StorageKey = outputFileName
            };
        }

        throw new InvalidOperationException($"Failed to transcode video variant {profile.Name}");
    }

    private async Task GenerateHlsPlaylistAsync(string outputDir, List<ProcessedVideoVariant> variants, int segmentSeconds)
    {
        // Generar playlist master m3u8
        var masterPlaylist = new StringBuilder();
        masterPlaylist.AppendLine("#EXTM3U");
        masterPlaylist.AppendLine("#EXT-X-VERSION:3");

        foreach (var variant in variants.OrderByDescending(v => v.Height))
        {
            var bandwidth = variant.Bitrate;
            masterPlaylist.AppendLine($"#EXT-X-STREAM-INF:BANDWIDTH={bandwidth},RESOLUTION={variant.Width}x{variant.Height}");
            masterPlaylist.AppendLine($"{variant.Name}/playlist.m3u8");
        }

        var masterPlaylistPath = Path.Combine(outputDir, "master.m3u8");
        await File.WriteAllTextAsync(masterPlaylistPath, masterPlaylist.ToString());

        _logger.LogInformation("Generated HLS master playlist with {Count} variants", variants.Count);
    }

    private async Task<(int width, int height)> GetImageDimensionsAsync(string imagePath)
    {
        try
        {
            var arguments = $"-v quiet -print_format json -show_streams \"{imagePath}\"";
            var output = await ExecuteProcessAsync(_ffprobePath, arguments);

            var probeData = JsonDocument.Parse(output);
            var stream = probeData.RootElement.GetProperty("streams").EnumerateArray().First();

            var width = stream.GetProperty("width").GetInt32();
            var height = stream.GetProperty("height").GetInt32();

            return (width, height);
        }
        catch
        {
            return (0, 0);
        }
    }

    private double ParseFrameRate(string frameRateStr)
    {
        try
        {
            if (frameRateStr.Contains('/'))
            {
                var parts = frameRateStr.Split('/');
                var numerator = double.Parse(parts[0]);
                var denominator = double.Parse(parts[1]);
                return numerator / denominator;
            }
            return double.Parse(frameRateStr);
        }
        catch
        {
            return 0;
        }
    }

    private async Task<string> ExecuteProcessAsync(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                output.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                error.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            _logger.LogError("FFmpeg process failed: {Error}", error.ToString());
            throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}: {error}");
        }

        return output.ToString();
    }

    #endregion
}