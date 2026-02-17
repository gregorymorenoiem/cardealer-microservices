using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vehicle360ProcessingService.Domain.Interfaces;

namespace Vehicle360ProcessingService.Infrastructure.HttpClients;

/// <summary>
/// Cliente HTTP resiliente para Video360Service
/// </summary>
public class Video360ServiceHttpClient : IVideo360ServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Video360ServiceHttpClient> _logger;
    private readonly Video360ServiceOptions _options;

    public Video360ServiceHttpClient(
        HttpClient httpClient,
        ILogger<Video360ServiceHttpClient> logger,
        IOptions<Video360ServiceOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<Video360ProcessingResult> ProcessVideoAsync(
        string videoUrl,
        Guid vehicleId,
        int frameCount,
        Video360Options? options,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Sending video to Video360Service for extraction. VehicleId: {VehicleId}, Frames: {FrameCount}",
                vehicleId, frameCount);

            var request = new
            {
                videoUrl,
                vehicleId,
                frameCount,
                outputWidth = options?.OutputWidth ?? 1920,
                outputHeight = options?.OutputHeight ?? 1080,
                outputFormat = options?.OutputFormat ?? "jpg",
                jpegQuality = options?.JpegQuality ?? 90,
                smartFrameSelection = options?.SmartFrameSelection ?? true,
                autoCorrectExposure = options?.AutoCorrectExposure ?? true,
                generateThumbnails = options?.GenerateThumbnails ?? true
            };

            var response = await _httpClient.PostAsJsonAsync("/api/video360/process", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Video360ApiResponse>(cancellationToken: cancellationToken);
                
                if (result == null)
                {
                    return new Video360ProcessingResult
                    {
                        Success = false,
                        ErrorMessage = "Empty response from Video360Service"
                    };
                }

                // Si el procesamiento es síncrono y ya tenemos los frames
                if (result.Frames?.Count > 0)
                {
                    return new Video360ProcessingResult
                    {
                        Success = true,
                        JobId = result.JobId,
                        Frames = result.Frames.Select(f => new Video360Frame
                        {
                            SequenceNumber = f.SequenceNumber,
                            ViewName = f.ViewName,
                            AngleDegrees = f.AngleDegrees,
                            ImageUrl = f.ImageUrl,
                            ThumbnailUrl = f.ThumbnailUrl,
                            Width = f.Width,
                            Height = f.Height
                        }).ToList()
                    };
                }

                // Si es asíncrono, poll hasta completar
                if (result.JobId.HasValue)
                {
                    return await WaitForJobCompletionAsync(result.JobId.Value, cancellationToken);
                }

                return new Video360ProcessingResult
                {
                    Success = false,
                    ErrorMessage = "No job ID or frames returned"
                };
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Video360Service failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
            
            return new Video360ProcessingResult
            {
                Success = false,
                ErrorMessage = $"Video processing failed: {response.StatusCode}",
                ErrorCode = response.StatusCode.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Video360Service");
            return new Video360ProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorCode = "VIDEO360_ERROR"
            };
        }
    }

    private async Task<Video360ProcessingResult> WaitForJobCompletionAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var maxWaitTime = TimeSpan.FromMinutes(_options.MaxProcessingMinutes);
        var pollInterval = TimeSpan.FromSeconds(_options.PollIntervalSeconds);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var status = await GetJobStatusAsync(jobId, cancellationToken);
            
            if (status.IsComplete)
            {
                return await GetJobResultAsync(jobId, cancellationToken);
            }

            if (status.IsFailed)
            {
                return new Video360ProcessingResult
                {
                    Success = false,
                    JobId = jobId,
                    ErrorMessage = status.ErrorMessage ?? "Video processing failed"
                };
            }

            await Task.Delay(pollInterval, cancellationToken);
        }

        return new Video360ProcessingResult
        {
            Success = false,
            JobId = jobId,
            ErrorMessage = "Video processing timed out"
        };
    }

    public async Task<Video360JobStatus> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/video360/jobs/{jobId}/status", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Video360StatusResponse>(cancellationToken: cancellationToken);
                return new Video360JobStatus
                {
                    JobId = jobId,
                    Status = result?.Status ?? "Unknown",
                    Progress = result?.Progress ?? 0,
                    IsComplete = result?.IsComplete ?? false,
                    IsFailed = result?.IsFailed ?? false,
                    ErrorMessage = result?.ErrorMessage
                };
            }

            return new Video360JobStatus
            {
                JobId = jobId,
                Status = "Unknown",
                IsFailed = true,
                ErrorMessage = $"Failed to get status: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Video360 job status");
            return new Video360JobStatus
            {
                JobId = jobId,
                Status = "Error",
                IsFailed = true,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<Video360ProcessingResult> GetJobResultAsync(Guid jobId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/video360/jobs/{jobId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Video360ApiResponse>(cancellationToken: cancellationToken);
                
                return new Video360ProcessingResult
                {
                    Success = true,
                    JobId = jobId,
                    Frames = result?.Frames?.Select(f => new Video360Frame
                    {
                        SequenceNumber = f.SequenceNumber,
                        ViewName = f.ViewName,
                        AngleDegrees = f.AngleDegrees,
                        ImageUrl = f.ImageUrl,
                        ThumbnailUrl = f.ThumbnailUrl,
                        Width = f.Width,
                        Height = f.Height
                    }).ToList() ?? new List<Video360Frame>()
                };
            }

            return new Video360ProcessingResult
            {
                Success = false,
                JobId = jobId,
                ErrorMessage = $"Failed to get job result: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Video360 job result");
            return new Video360ProcessingResult
            {
                Success = false,
                JobId = jobId,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Internal DTOs
    private class Video360ApiResponse
    {
        public Guid? JobId { get; set; }
        public string? Status { get; set; }
        public List<Video360FrameResponse>? Frames { get; set; }
    }

    private class Video360FrameResponse
    {
        public int SequenceNumber { get; set; }
        public string ViewName { get; set; } = string.Empty;
        public int AngleDegrees { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    private class Video360StatusResponse
    {
        public string? Status { get; set; }
        public int Progress { get; set; }
        public bool IsComplete { get; set; }
        public bool IsFailed { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

public class Video360ServiceOptions
{
    public string BaseUrl { get; set; } = "http://video360service:8080";
    public int TimeoutSeconds { get; set; } = 300;
    public int MaxProcessingMinutes { get; set; } = 10;
    public int PollIntervalSeconds { get; set; } = 5;
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 60;
}
