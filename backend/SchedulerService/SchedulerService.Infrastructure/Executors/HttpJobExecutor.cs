using Microsoft.Extensions.Logging;
using SchedulerService.Domain.Entities;
using SchedulerService.Domain.Interfaces;
using SchedulerService.Domain.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SchedulerService.Infrastructure.Executors;

/// <summary>
/// Executor for HTTP-based jobs
/// </summary>
public class HttpJobExecutor : IJobExecutor
{
    private readonly ILogger<HttpJobExecutor> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public string ExecutorType => "HTTP";

    public HttpJobExecutor(
        ILogger<HttpJobExecutor> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public bool CanExecute(Job job)
    {
        // HTTP jobs are identified by JobType starting with "http" or "https"
        return job.JobType.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               job.JobType.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ExecutionResult> ExecuteAsync(Job job, JobExecution execution, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = job.JobType;
            var method = job.Parameters.GetValueOrDefault("HttpMethod", "POST").ToUpperInvariant();
            var contentType = job.Parameters.GetValueOrDefault("ContentType", "application/json");

            _logger.LogInformation("Executing HTTP job: {Method} {Url}", method, url);

            using var httpClient = _httpClientFactory.CreateClient();

            // Set timeout
            if (job.TimeoutSeconds > 0)
            {
                httpClient.Timeout = TimeSpan.FromSeconds(job.TimeoutSeconds);
            }

            // Build request
            var request = new HttpRequestMessage(new HttpMethod(method), url);

            // Add headers
            foreach (var param in job.Parameters.Where(p => p.Key.StartsWith("Header_")))
            {
                var headerName = param.Key.Substring(7); // Remove "Header_" prefix
                request.Headers.TryAddWithoutValidation(headerName, param.Value);
            }

            // Add body for POST/PUT/PATCH
            if (method is "POST" or "PUT" or "PATCH")
            {
                var bodyContent = job.Parameters.GetValueOrDefault("Body", string.Empty);
                if (!string.IsNullOrWhiteSpace(bodyContent))
                {
                    request.Content = new StringContent(bodyContent, Encoding.UTF8, contentType);
                }
                else
                {
                    // Use all non-special parameters as JSON body
                    var bodyParams = job.Parameters
                        .Where(p => !p.Key.StartsWith("Header_") &&
                                   p.Key != "HttpMethod" &&
                                   p.Key != "ContentType" &&
                                   p.Key != "Body")
                        .ToDictionary(p => p.Key, p => (object)p.Value);

                    if (bodyParams.Any())
                    {
                        var json = JsonSerializer.Serialize(bodyParams);
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    }
                }
            }

            // Execute request
            var response = await httpClient.SendAsync(request, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "HTTP job completed successfully. Status: {StatusCode}",
                    response.StatusCode);

                return ExecutionResult.SuccessResult(responseBody);
            }
            else
            {
                _logger.LogError(
                    "HTTP job failed. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseBody);

                return ExecutionResult.FailureResult(
                    $"HTTP request failed with status {response.StatusCode}",
                    responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing HTTP job {Url}", job.JobType);
            return ExecutionResult.FailureResult(ex.Message, ex.StackTrace);
        }
    }
}
