using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FileStorageService.Core.Services;

/// <summary>
/// Audio processing service using FFmpeg
/// </summary>
public class AudioProcessingService : IAudioProcessingService
{
    private readonly ILogger<AudioProcessingService> _logger;
    private readonly FFmpegOptions _options;

    public AudioProcessingService(
        ILogger<AudioProcessingService> logger,
        FFmpegOptions? options = null)
    {
        _logger = logger;
        _options = options ?? new FFmpegOptions();
    }

    public async Task<AudioMetadata> ExtractAudioMetadataAsync(
        Stream audioStream,
        CancellationToken cancellationToken = default)
    {
        var inputFile = await SaveStreamToTempFile(audioStream, "input.mp3");

        try
        {
            var arguments = $"-v quiet -print_format json -show_format -show_streams \"{inputFile}\"";
            var output = await ExecuteFFprobeAsync(arguments, cancellationToken);

            if (string.IsNullOrEmpty(output))
            {
                throw new InvalidOperationException("Failed to extract audio metadata");
            }

            var metadata = ParseFFprobeAudioOutput(output);

            _logger.LogDebug("Extracted audio metadata: {Duration}s, {Bitrate}kbps",
                metadata.DurationSeconds, metadata.BitrateKbps);

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract audio metadata");
            throw;
        }
        finally
        {
            if (File.Exists(inputFile))
                File.Delete(inputFile);
        }
    }

    public async Task<Stream> ConvertAudioAsync(
        Stream audioStream,
        string outputFormat = "mp3",
        int? bitrate = 192,
        int? sampleRate = null,
        CancellationToken cancellationToken = default)
    {
        var inputFile = await SaveStreamToTempFile(audioStream, "input.mp3");
        var outputFile = Path.Combine(_options.WorkingDirectory, $"output_{Guid.NewGuid()}.{outputFormat}");

        try
        {
            var codecMap = new Dictionary<string, string>
            {
                ["mp3"] = "libmp3lame",
                ["aac"] = "aac",
                ["opus"] = "libopus",
                ["flac"] = "flac",
                ["wav"] = "pcm_s16le"
            };

            var codec = codecMap.ContainsKey(outputFormat.ToLower())
                ? codecMap[outputFormat.ToLower()]
                : "libmp3lame";

            var arguments = $"-i \"{inputFile}\" -acodec {codec}";

            if (bitrate.HasValue && !outputFormat.Equals("flac", StringComparison.OrdinalIgnoreCase))
            {
                arguments += $" -b:a {bitrate}k";
            }

            if (sampleRate.HasValue)
            {
                arguments += $" -ar {sampleRate}";
            }

            arguments += $" -y \"{outputFile}\"";

            var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

            if (!success || !File.Exists(outputFile))
            {
                throw new InvalidOperationException("Audio conversion failed");
            }

            var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));
            File.Delete(outputFile);

            _logger.LogInformation("Converted audio to {Format}", outputFormat);

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

    public async Task<Stream> NormalizeAudioAsync(
        Stream audioStream,
        double targetLevel = -16.0,
        CancellationToken cancellationToken = default)
    {
        var inputFile = await SaveStreamToTempFile(audioStream, "input.mp3");
        var outputFile = Path.Combine(_options.WorkingDirectory, $"normalized_{Guid.NewGuid()}.mp3");

        try
        {
            // Two-pass normalization using loudnorm filter
            var arguments = $"-i \"{inputFile}\" -af loudnorm=I={targetLevel}:TP=-1.5:LRA=11 -y \"{outputFile}\"";

            var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

            if (!success || !File.Exists(outputFile))
            {
                throw new InvalidOperationException("Audio normalization failed");
            }

            var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));
            File.Delete(outputFile);

            _logger.LogInformation("Normalized audio to {Level} LUFS", targetLevel);

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

    public async Task<Stream> TrimAudioAsync(
        Stream audioStream,
        double startTime,
        double duration,
        CancellationToken cancellationToken = default)
    {
        var inputFile = await SaveStreamToTempFile(audioStream, "input.mp3");
        var outputFile = Path.Combine(_options.WorkingDirectory, $"trimmed_{Guid.NewGuid()}.mp3");

        try
        {
            var arguments = $"-ss {startTime} -i \"{inputFile}\" -t {duration} -c copy -y \"{outputFile}\"";

            var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

            if (!success || !File.Exists(outputFile))
            {
                throw new InvalidOperationException("Audio trimming failed");
            }

            var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));
            File.Delete(outputFile);

            _logger.LogInformation("Trimmed audio from {Start}s for {Duration}s", startTime, duration);

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

    public async Task<Stream> ApplyFadeAsync(
        Stream audioStream,
        double fadeInDuration = 0,
        double fadeOutDuration = 0,
        CancellationToken cancellationToken = default)
    {
        var inputFile = await SaveStreamToTempFile(audioStream, "input.mp3");
        var outputFile = Path.Combine(_options.WorkingDirectory, $"faded_{Guid.NewGuid()}.mp3");

        try
        {
            var metadata = await ExtractAudioMetadataAsync(audioStream, cancellationToken);
            var duration = metadata.DurationSeconds;

            var filters = new List<string>();

            if (fadeInDuration > 0)
            {
                filters.Add($"afade=t=in:st=0:d={fadeInDuration}");
            }

            if (fadeOutDuration > 0 && duration > 0)
            {
                var fadeOutStart = duration - fadeOutDuration;
                filters.Add($"afade=t=out:st={fadeOutStart}:d={fadeOutDuration}");
            }

            if (filters.Count == 0)
            {
                throw new ArgumentException("No fade effects specified");
            }

            var filterChain = string.Join(",", filters);
            var arguments = $"-i \"{inputFile}\" -af \"{filterChain}\" -y \"{outputFile}\"";

            var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

            if (!success || !File.Exists(outputFile))
            {
                throw new InvalidOperationException("Fade application failed");
            }

            var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));
            File.Delete(outputFile);

            _logger.LogInformation("Applied fade effects: in={FadeIn}s, out={FadeOut}s", fadeInDuration, fadeOutDuration);

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

    public async Task<Stream> ConcatenateAudioAsync(
        IEnumerable<Stream> audioStreams,
        string outputFormat = "mp3",
        CancellationToken cancellationToken = default)
    {
        var inputFiles = new List<string>();
        var concatFile = Path.Combine(_options.WorkingDirectory, $"concat_{Guid.NewGuid()}.txt");
        var outputFile = Path.Combine(_options.WorkingDirectory, $"concatenated_{Guid.NewGuid()}.{outputFormat}");

        try
        {
            // Save all input streams
            int index = 0;
            foreach (var stream in audioStreams)
            {
                var inputFile = await SaveStreamToTempFile(stream, $"input_{index}.mp3");
                inputFiles.Add(inputFile);
                index++;
            }

            // Create concat file
            var concatContent = string.Join("\n", inputFiles.Select(f => $"file '{f}'"));
            await File.WriteAllTextAsync(concatFile, concatContent, cancellationToken);

            // Concatenate
            var arguments = $"-f concat -safe 0 -i \"{concatFile}\" -c copy -y \"{outputFile}\"";
            var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

            if (!success || !File.Exists(outputFile))
            {
                throw new InvalidOperationException("Audio concatenation failed");
            }

            var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));

            _logger.LogInformation("Concatenated {Count} audio files", inputFiles.Count);

            return outputStream;
        }
        finally
        {
            foreach (var file in inputFiles)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            if (File.Exists(concatFile))
                File.Delete(concatFile);
            if (File.Exists(outputFile))
                File.Delete(outputFile);
        }
    }

    public async Task<Stream> ExtractSegmentAsync(
        Stream audioStream,
        double startTime,
        double endTime,
        CancellationToken cancellationToken = default)
    {
        var duration = endTime - startTime;
        if (duration <= 0)
        {
            throw new ArgumentException("End time must be greater than start time");
        }

        return await TrimAudioAsync(audioStream, startTime, duration, cancellationToken);
    }

    public async Task<Stream> ChangeSpeedAsync(
        Stream audioStream,
        double speed = 1.0,
        CancellationToken cancellationToken = default)
    {
        if (speed <= 0)
        {
            throw new ArgumentException("Speed must be greater than 0");
        }

        var inputFile = await SaveStreamToTempFile(audioStream, "input.mp3");
        var outputFile = Path.Combine(_options.WorkingDirectory, $"speed_{Guid.NewGuid()}.mp3");

        try
        {
            // Use atempo filter (supports 0.5 to 2.0)
            var atempoFilter = speed.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            var arguments = $"-i \"{inputFile}\" -filter:a \"atempo={atempoFilter}\" -y \"{outputFile}\"";

            var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

            if (!success || !File.Exists(outputFile))
            {
                throw new InvalidOperationException("Speed change failed");
            }

            var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));
            File.Delete(outputFile);

            _logger.LogInformation("Changed audio speed to {Speed}x", speed);

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

    public async Task<Stream> GenerateWaveformAsync(
        Stream audioStream,
        int width = 1200,
        int height = 200,
        string foregroundColor = "0066CC",
        string backgroundColor = "FFFFFF",
        CancellationToken cancellationToken = default)
    {
        var inputFile = await SaveStreamToTempFile(audioStream, "input.mp3");
        var outputFile = Path.Combine(_options.WorkingDirectory, $"waveform_{Guid.NewGuid()}.png");

        try
        {
            var arguments = $"-i \"{inputFile}\" " +
                           $"-filter_complex \"showwavespic=s={width}x{height}:colors=#{foregroundColor}\" " +
                           $"-frames:v 1 -y \"{outputFile}\"";

            var success = await ExecuteFFmpegAsync(arguments, cancellationToken);

            if (!success || !File.Exists(outputFile))
            {
                throw new InvalidOperationException("Waveform generation failed");
            }

            var outputStream = new MemoryStream(await File.ReadAllBytesAsync(outputFile, cancellationToken));
            File.Delete(outputFile);

            _logger.LogInformation("Generated waveform image {Width}x{Height}", width, height);

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

    public async Task<bool> ValidateAudioAsync(Stream audioStream)
    {
        try
        {
            var metadata = await ExtractAudioMetadataAsync(audioStream);
            return metadata.DurationSeconds > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<double> GetAudioDurationAsync(Stream audioStream)
    {
        var metadata = await ExtractAudioMetadataAsync(audioStream);
        return metadata.DurationSeconds;
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

    private AudioMetadata ParseFFprobeAudioOutput(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var metadata = new AudioMetadata();

            if (root.TryGetProperty("format", out var format))
            {
                if (format.TryGetProperty("duration", out var duration))
                {
                    metadata.DurationSeconds = double.Parse(duration.GetString() ?? "0");
                }

                if (format.TryGetProperty("bit_rate", out var bitrate))
                {
                    metadata.BitrateKbps = int.Parse(bitrate.GetString() ?? "0") / 1000;
                }
            }

            if (root.TryGetProperty("streams", out var streams))
            {
                foreach (var stream in streams.EnumerateArray())
                {
                    if (!stream.TryGetProperty("codec_type", out var codecType) ||
                        codecType.GetString() != "audio")
                        continue;

                    if (stream.TryGetProperty("codec_name", out var codec))
                    {
                        metadata.Codec = codec.GetString();
                    }

                    if (stream.TryGetProperty("sample_rate", out var sampleRate))
                    {
                        if (int.TryParse(sampleRate.GetString(), out var sr))
                        {
                            metadata.SampleRateHz = sr;
                        }
                    }

                    if (stream.TryGetProperty("channels", out var channels))
                    {
                        metadata.Channels = channels.GetInt32();
                    }

                    break; // Take first audio stream
                }
            }

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse FFprobe output");
            return new AudioMetadata();
        }
    }
}
