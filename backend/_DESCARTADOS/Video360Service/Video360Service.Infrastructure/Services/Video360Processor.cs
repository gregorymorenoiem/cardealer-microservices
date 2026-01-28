using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Infrastructure.Services;

/// <summary>
/// Configuración del procesador de video 360
/// </summary>
public class Video360ProcessorSettings
{
    public const string SectionName = "Video360Processor";
    
    /// <summary>
    /// Ruta al script Python de procesamiento
    /// </summary>
    public string PythonScriptPath { get; set; } = "/app/workers/video360_processor.py";
    
    /// <summary>
    /// Ruta al ejecutable de Python
    /// </summary>
    public string PythonExecutable { get; set; } = "python3";
    
    /// <summary>
    /// Timeout en segundos para el procesamiento
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;
    
    /// <summary>
    /// Directorio temporal para archivos
    /// </summary>
    public string TempDirectory { get; set; } = "/tmp/video360";
    
    /// <summary>
    /// URL del servicio Python si se usa HTTP
    /// </summary>
    public string? PythonServiceUrl { get; set; }
    
    /// <summary>
    /// Usar servicio HTTP en lugar de subprocess
    /// </summary>
    public bool UseHttpService { get; set; } = true;
}

/// <summary>
/// Implementación del procesador de video 360 que llama al worker Python
/// </summary>
public class Video360Processor : IVideo360Processor
{
    private readonly Video360ProcessorSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<Video360Processor> _logger;

    public Video360Processor(
        IOptions<Video360ProcessorSettings> settings,
        HttpClient httpClient,
        ILogger<Video360Processor> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
        
        if (!string.IsNullOrEmpty(_settings.PythonServiceUrl))
        {
            _httpClient.BaseAddress = new Uri(_settings.PythonServiceUrl);
        }
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public async Task<Video360ProcessingResult> ProcessVideoAsync(
        string videoPath,
        ProcessingOptions options,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            if (_settings.UseHttpService && !string.IsNullOrEmpty(_settings.PythonServiceUrl))
            {
                return await ProcessViaHttpAsync(videoPath, options, progress, cancellationToken);
            }
            else
            {
                return await ProcessViaSubprocessAsync(videoPath, options, progress, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando video {VideoPath}", videoPath);
            return new Video360ProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private async Task<Video360ProcessingResult> ProcessViaHttpAsync(
        string videoPath,
        ProcessingOptions options,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Procesando video via HTTP service: {VideoPath}", videoPath);

        using var content = new MultipartFormDataContent();
        
        // Agregar el video
        await using var videoStream = File.OpenRead(videoPath);
        var videoContent = new StreamContent(videoStream);
        content.Add(videoContent, "video", Path.GetFileName(videoPath));
        
        // Agregar opciones
        content.Add(new StringContent(options.FrameCount.ToString()), "frame_count");
        content.Add(new StringContent(options.OutputWidth.ToString()), "output_width");
        content.Add(new StringContent(options.OutputHeight.ToString()), "output_height");
        content.Add(new StringContent(options.JpegQuality.ToString()), "jpeg_quality");
        content.Add(new StringContent(options.OutputFormat), "output_format");
        content.Add(new StringContent(options.SmartFrameSelection.ToString().ToLower()), "smart_selection");
        content.Add(new StringContent(options.AutoCorrectExposure.ToString().ToLower()), "auto_exposure");
        content.Add(new StringContent(options.GenerateThumbnails.ToString().ToLower()), "generate_thumbnails");
        content.Add(new StringContent(options.ThumbnailWidth.ToString()), "thumbnail_width");

        var response = await _httpClient.PostAsync("/api/process", content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Error del servicio Python: {StatusCode} - {Body}", response.StatusCode, errorBody);
            
            return new Video360ProcessingResult
            {
                Success = false,
                ErrorMessage = $"Error del servicio: {response.StatusCode} - {errorBody}",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        var resultJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<PythonProcessingResult>(resultJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        stopwatch.Stop();

        if (result == null)
        {
            return new Video360ProcessingResult
            {
                Success = false,
                ErrorMessage = "Respuesta vacía del servicio Python",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        return new Video360ProcessingResult
        {
            Success = result.Success,
            ErrorMessage = result.Error,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
            VideoInfo = result.VideoInfo != null ? new VideoInfo
            {
                DurationSeconds = result.VideoInfo.DurationSeconds,
                TotalFrames = result.VideoInfo.TotalFrames,
                Fps = result.VideoInfo.Fps,
                Width = result.VideoInfo.Width,
                Height = result.VideoInfo.Height,
                Codec = result.VideoInfo.Codec ?? string.Empty
            } : null,
            Frames = result.Frames?.Select(f => new ExtractedFrameResult
            {
                SequenceNumber = f.SequenceNumber,
                ViewName = f.ViewName,
                AngleDegrees = f.AngleDegrees,
                LocalFilePath = f.FilePath,
                ThumbnailPath = f.ThumbnailPath,
                Width = f.Width,
                Height = f.Height,
                FileSizeBytes = f.FileSizeBytes,
                SourceFrameNumber = f.SourceFrameNumber,
                TimestampSeconds = f.TimestampSeconds,
                QualityScore = f.QualityScore
            }).ToList() ?? new List<ExtractedFrameResult>()
        };
    }

    private async Task<Video360ProcessingResult> ProcessViaSubprocessAsync(
        string videoPath,
        ProcessingOptions options,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Procesando video via subprocess: {VideoPath}", videoPath);

        var outputDir = Path.Combine(_settings.TempDirectory, Guid.NewGuid().ToString());
        Directory.CreateDirectory(outputDir);

        var optionsJson = JsonSerializer.Serialize(new
        {
            frame_count = options.FrameCount,
            output_width = options.OutputWidth,
            output_height = options.OutputHeight,
            jpeg_quality = options.JpegQuality,
            output_format = options.OutputFormat,
            smart_selection = options.SmartFrameSelection,
            auto_exposure = options.AutoCorrectExposure,
            generate_thumbnails = options.GenerateThumbnails,
            thumbnail_width = options.ThumbnailWidth
        });

        var startInfo = new ProcessStartInfo
        {
            FileName = _settings.PythonExecutable,
            Arguments = $"\"{_settings.PythonScriptPath}\" \"{videoPath}\" \"{outputDir}\" '{optionsJson}'",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        
        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
                
                // Parsear progreso si viene en el output
                if (e.Data.StartsWith("PROGRESS:"))
                {
                    if (int.TryParse(e.Data.Replace("PROGRESS:", "").Trim(), out var p))
                    {
                        progress?.Report(p);
                    }
                }
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var completed = await Task.Run(() => 
            process.WaitForExit(_settings.TimeoutSeconds * 1000), cancellationToken);

        if (!completed)
        {
            process.Kill(true);
            return new Video360ProcessingResult
            {
                Success = false,
                ErrorMessage = "Timeout procesando video",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        stopwatch.Stop();

        if (process.ExitCode != 0)
        {
            return new Video360ProcessingResult
            {
                Success = false,
                ErrorMessage = errorBuilder.ToString(),
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        // Leer resultado del archivo JSON generado
        var resultFile = Path.Combine(outputDir, "result.json");
        if (!File.Exists(resultFile))
        {
            return new Video360ProcessingResult
            {
                Success = false,
                ErrorMessage = "No se generó archivo de resultado",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        var resultJson = await File.ReadAllTextAsync(resultFile, cancellationToken);
        var result = JsonSerializer.Deserialize<PythonProcessingResult>(resultJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
            return new Video360ProcessingResult
            {
                Success = false,
                ErrorMessage = "Error parseando resultado",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }

        return new Video360ProcessingResult
        {
            Success = result.Success,
            ErrorMessage = result.Error,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
            VideoInfo = result.VideoInfo != null ? new VideoInfo
            {
                DurationSeconds = result.VideoInfo.DurationSeconds,
                TotalFrames = result.VideoInfo.TotalFrames,
                Fps = result.VideoInfo.Fps,
                Width = result.VideoInfo.Width,
                Height = result.VideoInfo.Height,
                Codec = result.VideoInfo.Codec ?? string.Empty
            } : null,
            Frames = result.Frames?.Select(f => new ExtractedFrameResult
            {
                SequenceNumber = f.SequenceNumber,
                ViewName = f.ViewName,
                AngleDegrees = f.AngleDegrees,
                LocalFilePath = f.FilePath,
                ThumbnailPath = f.ThumbnailPath,
                Width = f.Width,
                Height = f.Height,
                FileSizeBytes = f.FileSizeBytes,
                SourceFrameNumber = f.SourceFrameNumber,
                TimestampSeconds = f.TimestampSeconds,
                QualityScore = f.QualityScore
            }).ToList() ?? new List<ExtractedFrameResult>()
        };
    }

    public async Task<VideoInfo> GetVideoInfoAsync(string videoPath, CancellationToken cancellationToken = default)
    {
        if (_settings.UseHttpService && !string.IsNullOrEmpty(_settings.PythonServiceUrl))
        {
            var response = await _httpClient.GetAsync($"/api/info?path={Uri.EscapeDataString(videoPath)}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var info = JsonSerializer.Deserialize<PythonVideoInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return new VideoInfo
                {
                    DurationSeconds = info?.DurationSeconds ?? 0,
                    TotalFrames = info?.TotalFrames ?? 0,
                    Fps = info?.Fps ?? 0,
                    Width = info?.Width ?? 0,
                    Height = info?.Height ?? 0,
                    Codec = info?.Codec ?? string.Empty
                };
            }
        }

        return new VideoInfo();
    }

    public async Task<VideoValidationResult> ValidateVideoAsync(string videoPath, CancellationToken cancellationToken = default)
    {
        var result = new VideoValidationResult { IsValid = true };
        
        try
        {
            if (!File.Exists(videoPath))
            {
                result.IsValid = false;
                result.Errors.Add("El archivo de video no existe");
                return result;
            }

            var fileInfo = new FileInfo(videoPath);
            if (fileInfo.Length == 0)
            {
                result.IsValid = false;
                result.Errors.Add("El archivo de video está vacío");
                return result;
            }

            if (fileInfo.Length > 500 * 1024 * 1024) // 500 MB
            {
                result.Warnings.Add("El archivo es muy grande (>500MB), el procesamiento puede tardar");
            }

            var videoInfo = await GetVideoInfoAsync(videoPath, cancellationToken);
            result.VideoInfo = videoInfo;

            if (videoInfo.DurationSeconds < 5)
            {
                result.IsValid = false;
                result.Errors.Add("El video debe durar al menos 5 segundos");
            }

            if (videoInfo.DurationSeconds > 120)
            {
                result.Warnings.Add("El video es muy largo (>2 minutos), considere recortarlo");
            }

            if (videoInfo.Width < 640 || videoInfo.Height < 480)
            {
                result.Warnings.Add("La resolución del video es baja, las imágenes pueden tener poca calidad");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando video {VideoPath}", videoPath);
            result.IsValid = false;
            result.Errors.Add($"Error validando video: {ex.Message}");
        }

        return result;
    }

    // DTOs internos para deserialización
    private class PythonProcessingResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public PythonVideoInfo? VideoInfo { get; set; }
        public List<PythonFrameResult>? Frames { get; set; }
    }

    private class PythonVideoInfo
    {
        public double DurationSeconds { get; set; }
        public int TotalFrames { get; set; }
        public double Fps { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? Codec { get; set; }
    }

    private class PythonFrameResult
    {
        public int SequenceNumber { get; set; }
        public string ViewName { get; set; } = string.Empty;
        public int AngleDegrees { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long FileSizeBytes { get; set; }
        public int SourceFrameNumber { get; set; }
        public double TimestampSeconds { get; set; }
        public int? QualityScore { get; set; }
    }
}
