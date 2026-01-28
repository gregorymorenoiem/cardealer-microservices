using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vehicle360ProcessingService.Domain.Interfaces;

namespace Vehicle360ProcessingService.Infrastructure.HttpClients;

/// <summary>
/// Cliente HTTP resiliente para BackgroundRemovalService
/// </summary>
public class BackgroundRemovalHttpClient : IBackgroundRemovalClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BackgroundRemovalHttpClient> _logger;
    private readonly BackgroundRemovalServiceOptions _options;

    public BackgroundRemovalHttpClient(
        HttpClient httpClient,
        ILogger<BackgroundRemovalHttpClient> logger,
        IOptions<BackgroundRemovalServiceOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<BackgroundRemovalResult> RemoveBackgroundAsync(
        string imageUrl,
        BackgroundRemovalOptions? options,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Sending image to BackgroundRemovalService");

            var request = new
            {
                imageUrl,
                outputFormat = options?.OutputFormat ?? "png",
                backgroundColor = options?.BackgroundColor ?? "transparent",
                outputWidth = options?.OutputWidth,
                outputHeight = options?.OutputHeight,
                maintainAspectRatio = options?.MaintainAspectRatio ?? true
            };

            var response = await _httpClient.PostAsJsonAsync("/api/backgroundremoval/remove", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BgRemovalApiResponse>(cancellationToken: cancellationToken);
                
                // Si es síncrono y ya tenemos el resultado
                if (!string.IsNullOrEmpty(result?.ProcessedImageUrl))
                {
                    return new BackgroundRemovalResult
                    {
                        Success = true,
                        JobId = result.JobId,
                        ProcessedImageUrl = result.ProcessedImageUrl
                    };
                }

                // Si es asíncrono, esperar
                if (result?.JobId != null)
                {
                    return await WaitForJobCompletionAsync(result.JobId.Value, cancellationToken);
                }

                return new BackgroundRemovalResult
                {
                    Success = false,
                    ErrorMessage = "No job ID or result returned"
                };
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("BackgroundRemovalService failed: {StatusCode} - {Error}", 
                response.StatusCode, errorContent);
            
            return new BackgroundRemovalResult
            {
                Success = false,
                ErrorMessage = $"Background removal failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling BackgroundRemovalService");
            return new BackgroundRemovalResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<BackgroundRemovalResult>> RemoveBackgroundBatchAsync(
        List<string> imageUrls,
        BackgroundRemovalOptions? options,
        int maxConcurrency,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing batch of {Count} images with concurrency {Concurrency}",
            imageUrls.Count, maxConcurrency);

        var results = new List<BackgroundRemovalResult>();
        var semaphore = new SemaphoreSlim(maxConcurrency);
        var tasks = new List<Task<BackgroundRemovalResult>>();

        foreach (var imageUrl in imageUrls)
        {
            await semaphore.WaitAsync(cancellationToken);
            
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    return await RemoveBackgroundAsync(imageUrl, options, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        var completedResults = await Task.WhenAll(tasks);
        results.AddRange(completedResults);

        var successCount = results.Count(r => r.Success);
        _logger.LogInformation(
            "Batch processing completed. Success: {Success}/{Total}",
            successCount, results.Count);

        return results;
    }

    private async Task<BackgroundRemovalResult> WaitForJobCompletionAsync(
        Guid jobId, 
        CancellationToken cancellationToken)
    {
        var maxWaitTime = TimeSpan.FromMinutes(_options.MaxProcessingMinutes);
        var pollInterval = TimeSpan.FromSeconds(_options.PollIntervalSeconds);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var status = await GetJobStatusAsync(jobId, cancellationToken);
            
            if (status.IsComplete && !string.IsNullOrEmpty(status.ProcessedImageUrl))
            {
                return new BackgroundRemovalResult
                {
                    Success = true,
                    JobId = jobId,
                    ProcessedImageUrl = status.ProcessedImageUrl
                };
            }

            if (status.IsFailed)
            {
                return new BackgroundRemovalResult
                {
                    Success = false,
                    JobId = jobId,
                    ErrorMessage = status.ErrorMessage ?? "Background removal failed"
                };
            }

            await Task.Delay(pollInterval, cancellationToken);
        }

        return new BackgroundRemovalResult
        {
            Success = false,
            JobId = jobId,
            ErrorMessage = "Background removal timed out"
        };
    }

    public async Task<BgJobStatus> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/backgroundremoval/jobs/{jobId}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BgJobApiResponse>(cancellationToken: cancellationToken);
                return new BgJobStatus
                {
                    JobId = jobId,
                    Status = result?.Status ?? "Unknown",
                    Progress = result?.Progress ?? 0,
                    IsComplete = result?.Status == "Completed",
                    IsFailed = result?.Status == "Failed",
                    ProcessedImageUrl = result?.ProcessedImageUrl,
                    ErrorMessage = result?.ErrorMessage
                };
            }

            return new BgJobStatus
            {
                JobId = jobId,
                Status = "Error",
                IsFailed = true,
                ErrorMessage = $"Failed to get status: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting background removal job status");
            return new BgJobStatus
            {
                JobId = jobId,
                Status = "Error",
                IsFailed = true,
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
    private class BgRemovalApiResponse
    {
        public Guid? JobId { get; set; }
        public string? Status { get; set; }
        public string? ProcessedImageUrl { get; set; }
    }

    private class BgJobApiResponse
    {
        public string? Status { get; set; }
        public int Progress { get; set; }
        public string? ProcessedImageUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

public class BackgroundRemovalServiceOptions
{
    public string BaseUrl { get; set; } = "http://backgroundremovalservice:8080";
    public int TimeoutSeconds { get; set; } = 180;
    public int MaxProcessingMinutes { get; set; } = 5;
    public int PollIntervalSeconds { get; set; } = 2;
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}
