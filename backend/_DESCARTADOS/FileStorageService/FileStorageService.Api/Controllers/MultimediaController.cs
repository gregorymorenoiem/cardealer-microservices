using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MultimediaController : ControllerBase
{
    private readonly ILogger<MultimediaController> _logger;
    private readonly IVideoProcessingService _videoProcessing;
    private readonly IAudioProcessingService _audioProcessing;

    public MultimediaController(
        ILogger<MultimediaController> logger,
        IVideoProcessingService videoProcessing,
        IAudioProcessingService audioProcessing)
    {
        _logger = logger;
        _videoProcessing = videoProcessing;
        _audioProcessing = audioProcessing;
    }

    // VIDEO ENDPOINTS

    [HttpPost("video/thumbnails")]
    public async Task<IActionResult> GenerateVideoThumbnails(
        IFormFile video,
        [FromForm] string timestamps,
        [FromForm] int width = 320,
        [FromForm] int height = 180)
    {
        try
        {
            using var stream = video.OpenReadStream();
            var timestampList = timestamps.Split(',').Select(double.Parse);

            var thumbnails = await _videoProcessing.GenerateVideoThumbnailsAsync(
                stream, timestampList, width, height);

            // Return first thumbnail
            var firstThumb = thumbnails.FirstOrDefault();
            if (firstThumb.Value != null)
            {
                firstThumb.Value.Position = 0;
                return File(firstThumb.Value, "image/jpeg", $"thumbnail_{firstThumb.Key}s.jpg");
            }

            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate video thumbnails");
            return StatusCode(500, new { error = "Failed to generate thumbnails" });
        }
    }

    [HttpPost("video/metadata")]
    public async Task<IActionResult> ExtractVideoMetadata(IFormFile video)
    {
        try
        {
            using var stream = video.OpenReadStream();
            var metadata = await _videoProcessing.ExtractVideoMetadataAsync(stream);
            return Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract video metadata");
            return StatusCode(500, new { error = "Failed to extract metadata" });
        }
    }

    [HttpPost("video/transcode")]
    public async Task<IActionResult> TranscodeVideo(
        IFormFile video,
        [FromForm] string outputFormat = "mp4",
        [FromForm] string? videoCodec = null,
        [FromForm] string? audioCodec = null,
        [FromForm] string? preset = null,
        [FromForm] int? crf = null)
    {
        try
        {
            using var stream = video.OpenReadStream();
            var transcodedStream = await _videoProcessing.TranscodeVideoAsync(
                stream,
                outputFormat,
                videoCodec ?? "h264",
                audioCodec ?? "aac",
                preset ?? "medium",
                crf ?? 23);

            return File(transcodedStream, $"video/{outputFormat}", $"transcoded.{outputFormat}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transcode video");
            return StatusCode(500, new { error = "Failed to transcode video" });
        }
    }

    [HttpPost("video/extract-audio")]
    public async Task<IActionResult> ExtractAudioFromVideo(
        IFormFile video,
        [FromForm] string format = "mp3",
        [FromForm] int bitrate = 192)
    {
        try
        {
            using var stream = video.OpenReadStream();
            var audioStream = await _videoProcessing.ExtractAudioFromVideoAsync(stream, format, bitrate);

            return File(audioStream, $"audio/{format}", $"audio.{format}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract audio");
            return StatusCode(500, new { error = "Failed to extract audio" });
        }
    }

    [HttpPost("video/watermark")]
    public async Task<IActionResult> ApplyWatermark(
        IFormFile video,
        IFormFile watermark,
        [FromForm] string position = "bottomright",
        [FromForm] int opacity = 50)
    {
        try
        {
            using var videoStream = video.OpenReadStream();
            using var watermarkStream = watermark.OpenReadStream();

            var watermarkedStream = await _videoProcessing.ApplyWatermarkAsync(
                videoStream, watermarkStream, position, opacity);

            return File(watermarkedStream, "video/mp4", "watermarked.mp4");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply watermark");
            return StatusCode(500, new { error = "Failed to apply watermark" });
        }
    }

    [HttpPost("video/validate")]
    public async Task<IActionResult> ValidateVideo(IFormFile video)
    {
        try
        {
            using var stream = video.OpenReadStream();
            var isValid = await _videoProcessing.ValidateVideoAsync(stream);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate video");
            return StatusCode(500, new { error = "Failed to validate video" });
        }
    }

    // AUDIO ENDPOINTS

    [HttpPost("audio/metadata")]
    public async Task<IActionResult> ExtractAudioMetadata(IFormFile audio)
    {
        try
        {
            using var stream = audio.OpenReadStream();
            var metadata = await _audioProcessing.ExtractAudioMetadataAsync(stream);
            return Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract audio metadata");
            return StatusCode(500, new { error = "Failed to extract metadata" });
        }
    }

    [HttpPost("audio/convert")]
    public async Task<IActionResult> ConvertAudio(
        IFormFile audio,
        [FromForm] string outputFormat = "mp3",
        [FromForm] int? bitrate = 192,
        [FromForm] int? sampleRate = null)
    {
        try
        {
            using var stream = audio.OpenReadStream();
            var convertedStream = await _audioProcessing.ConvertAudioAsync(
                stream, outputFormat, bitrate, sampleRate);

            return File(convertedStream, $"audio/{outputFormat}", $"converted.{outputFormat}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert audio");
            return StatusCode(500, new { error = "Failed to convert audio" });
        }
    }

    [HttpPost("audio/normalize")]
    public async Task<IActionResult> NormalizeAudio(
        IFormFile audio,
        [FromForm] double targetLevel = -16.0)
    {
        try
        {
            using var stream = audio.OpenReadStream();
            var normalizedStream = await _audioProcessing.NormalizeAudioAsync(stream, targetLevel);

            return File(normalizedStream, "audio/mp3", "normalized.mp3");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to normalize audio");
            return StatusCode(500, new { error = "Failed to normalize audio" });
        }
    }

    [HttpPost("audio/trim")]
    public async Task<IActionResult> TrimAudio(
        IFormFile audio,
        [FromForm] double startTime,
        [FromForm] double duration)
    {
        try
        {
            using var stream = audio.OpenReadStream();
            var trimmedStream = await _audioProcessing.TrimAudioAsync(stream, startTime, duration);

            return File(trimmedStream, "audio/mp3", "trimmed.mp3");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trim audio");
            return StatusCode(500, new { error = "Failed to trim audio" });
        }
    }

    [HttpPost("audio/fade")]
    public async Task<IActionResult> ApplyFade(
        IFormFile audio,
        [FromForm] double fadeInDuration = 0,
        [FromForm] double fadeOutDuration = 0)
    {
        try
        {
            using var stream = audio.OpenReadStream();
            var fadedStream = await _audioProcessing.ApplyFadeAsync(
                stream, fadeInDuration, fadeOutDuration);

            return File(fadedStream, "audio/mp3", "faded.mp3");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply fade");
            return StatusCode(500, new { error = "Failed to apply fade" });
        }
    }

    [HttpPost("audio/speed")]
    public async Task<IActionResult> ChangeSpeed(
        IFormFile audio,
        [FromForm] double speed = 1.0)
    {
        try
        {
            using var stream = audio.OpenReadStream();
            var speedChangedStream = await _audioProcessing.ChangeSpeedAsync(stream, speed);

            return File(speedChangedStream, "audio/mp3", $"speed_{speed}x.mp3");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change speed");
            return StatusCode(500, new { error = "Failed to change speed" });
        }
    }

    [HttpPost("audio/waveform")]
    public async Task<IActionResult> GenerateWaveform(
        IFormFile audio,
        [FromForm] int width = 1200,
        [FromForm] int height = 200,
        [FromForm] string foregroundColor = "0066CC",
        [FromForm] string backgroundColor = "FFFFFF")
    {
        try
        {
            using var stream = audio.OpenReadStream();
            var waveformStream = await _audioProcessing.GenerateWaveformAsync(
                stream, width, height, foregroundColor, backgroundColor);

            return File(waveformStream, "image/png", "waveform.png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate waveform");
            return StatusCode(500, new { error = "Failed to generate waveform" });
        }
    }

    [HttpPost("audio/validate")]
    public async Task<IActionResult> ValidateAudio(IFormFile audio)
    {
        try
        {
            using var stream = audio.OpenReadStream();
            var isValid = await _audioProcessing.ValidateAudioAsync(stream);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate audio");
            return StatusCode(500, new { error = "Failed to validate audio" });
        }
    }

    // SYSTEM

    [HttpGet("ffmpeg/available")]
    public async Task<IActionResult> CheckFFmpegAvailability()
    {
        try
        {
            var isAvailable = await _videoProcessing.IsFFmpegAvailableAsync();
            return Ok(new { isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check FFmpeg availability");
            return StatusCode(500, new { error = "Failed to check FFmpeg" });
        }
    }
}
