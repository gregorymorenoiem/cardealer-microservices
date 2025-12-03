using FileStorageService.Core.Models;

namespace FileStorageService.Core.Interfaces;

/// <summary>
/// Interface for audio processing operations using FFmpeg
/// </summary>
public interface IAudioProcessingService
{
    /// <summary>
    /// Extracts detailed audio metadata using FFprobe
    /// </summary>
    /// <param name="audioStream">Audio stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete audio metadata</returns>
    Task<AudioMetadata> ExtractAudioMetadataAsync(
        Stream audioStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts audio to different format
    /// </summary>
    /// <param name="audioStream">Source audio stream</param>
    /// <param name="outputFormat">Output format (mp3, aac, opus, flac, wav)</param>
    /// <param name="bitrate">Bitrate in kbps (for lossy formats)</param>
    /// <param name="sampleRate">Sample rate in Hz (44100, 48000, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Converted audio stream</returns>
    Task<Stream> ConvertAudioAsync(
        Stream audioStream,
        string outputFormat = "mp3",
        int? bitrate = 192,
        int? sampleRate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Normalizes audio volume
    /// </summary>
    /// <param name="audioStream">Source audio stream</param>
    /// <param name="targetLevel">Target loudness level in LUFS (default: -16)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Normalized audio stream</returns>
    Task<Stream> NormalizeAudioAsync(
        Stream audioStream,
        double targetLevel = -16.0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Trims audio to specific duration
    /// </summary>
    /// <param name="audioStream">Source audio stream</param>
    /// <param name="startTime">Start time in seconds</param>
    /// <param name="duration">Duration in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trimmed audio stream</returns>
    Task<Stream> TrimAudioAsync(
        Stream audioStream,
        double startTime,
        double duration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies fade in/out effects
    /// </summary>
    /// <param name="audioStream">Source audio stream</param>
    /// <param name="fadeInDuration">Fade in duration in seconds</param>
    /// <param name="fadeOutDuration">Fade out duration in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audio with fade effects</returns>
    Task<Stream> ApplyFadeAsync(
        Stream audioStream,
        double fadeInDuration = 0,
        double fadeOutDuration = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Concatenates multiple audio files
    /// </summary>
    /// <param name="audioStreams">List of audio streams to concatenate</param>
    /// <param name="outputFormat">Output format</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Concatenated audio stream</returns>
    Task<Stream> ConcatenateAudioAsync(
        IEnumerable<Stream> audioStreams,
        string outputFormat = "mp3",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts audio segment
    /// </summary>
    /// <param name="audioStream">Source audio stream</param>
    /// <param name="startTime">Start time in seconds</param>
    /// <param name="endTime">End time in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted audio segment</returns>
    Task<Stream> ExtractSegmentAsync(
        Stream audioStream,
        double startTime,
        double endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes audio speed without changing pitch
    /// </summary>
    /// <param name="audioStream">Source audio stream</param>
    /// <param name="speed">Speed multiplier (0.5 = half speed, 2.0 = double speed)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Speed-adjusted audio stream</returns>
    Task<Stream> ChangeSpeedAsync(
        Stream audioStream,
        double speed = 1.0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates waveform image from audio
    /// </summary>
    /// <param name="audioStream">Source audio stream</param>
    /// <param name="width">Image width</param>
    /// <param name="height">Image height</param>
    /// <param name="foregroundColor">Waveform color (hex)</param>
    /// <param name="backgroundColor">Background color (hex)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Waveform image stream (PNG)</returns>
    Task<Stream> GenerateWaveformAsync(
        Stream audioStream,
        int width = 1200,
        int height = 200,
        string foregroundColor = "0066CC",
        string backgroundColor = "FFFFFF",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if stream is a valid audio file
    /// </summary>
    /// <param name="audioStream">Audio stream</param>
    /// <returns>True if valid</returns>
    Task<bool> ValidateAudioAsync(Stream audioStream);

    /// <summary>
    /// Gets audio duration in seconds
    /// </summary>
    /// <param name="audioStream">Audio stream</param>
    /// <returns>Duration in seconds</returns>
    Task<double> GetAudioDurationAsync(Stream audioStream);
}
