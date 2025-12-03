using ApiDocsService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ApiDocsService.Api.Controllers;

/// <summary>
/// Controller for API testing capabilities
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TestingController : ControllerBase
{
    private readonly IApiAggregatorService _aggregatorService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TestingController> _logger;

    public TestingController(
        IApiAggregatorService aggregatorService,
        IHttpClientFactory httpClientFactory,
        ILogger<TestingController> logger)
    {
        _aggregatorService = aggregatorService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Execute a test request to an API endpoint
    /// </summary>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(TestExecutionResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<TestExecutionResult>> ExecuteTest(
        [FromBody] TestRequest request,
        CancellationToken cancellationToken)
    {
        var result = new TestExecutionResult
        {
            RequestId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow
        };

        try
        {
            var service = await _aggregatorService.GetServiceByNameAsync(request.ServiceName, cancellationToken);
            if (service == null)
            {
                result.Success = false;
                result.ErrorMessage = $"Service '{request.ServiceName}' not found";
                return Ok(result);
            }

            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{service.BaseUrl.TrimEnd('/')}{request.Path}";

            // Add query parameters
            if (request.QueryParameters?.Any() == true)
            {
                var query = string.Join("&", request.QueryParameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                url += $"?{query}";
            }

            var httpRequest = new HttpRequestMessage(new HttpMethod(request.Method), url);

            // Add headers
            if (request.Headers?.Any() == true)
            {
                foreach (var header in request.Headers)
                {
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Add authorization
            if (!string.IsNullOrEmpty(request.Authorization))
            {
                httpRequest.Headers.Authorization = AuthenticationHeaderValue.Parse(request.Authorization);
            }

            // Add body
            if (request.Body != null)
            {
                var json = JsonSerializer.Serialize(request.Body);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            stopwatch.Stop();

            result.Success = response.IsSuccessStatusCode;
            result.StatusCode = (int)response.StatusCode;
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.ResponseHeaders = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            result.ResponseBody = responseBody;

            _logger.LogInformation("Test executed: {Method} {Url} -> {StatusCode} ({ResponseTime}ms)",
                request.Method, url, result.StatusCode, result.ResponseTimeMs);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error executing test request");
        }

        return Ok(result);
    }

    /// <summary>
    /// Execute multiple test requests
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(BatchTestResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<BatchTestResult>> ExecuteBatchTest(
        [FromBody] BatchTestRequest batchRequest,
        CancellationToken cancellationToken)
    {
        var batchResult = new BatchTestResult
        {
            BatchId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            TotalTests = batchRequest.Tests.Count
        };

        foreach (var test in batchRequest.Tests)
        {
            var result = await ExecuteTest(test, cancellationToken);
            if (result.Value != null)
            {
                batchResult.Results.Add(result.Value);
                if (result.Value.Success)
                    batchResult.SuccessCount++;
                else
                    batchResult.FailureCount++;
            }
        }

        return Ok(batchResult);
    }

    /// <summary>
    /// Get collection of saved test requests
    /// </summary>
    [HttpGet("collections")]
    [ProducesResponseType(typeof(List<TestCollection>), StatusCodes.Status200OK)]
    public ActionResult<List<TestCollection>> GetTestCollections()
    {
        // In a real implementation, this would load from a database or file storage
        var collections = new List<TestCollection>
        {
            new()
            {
                Id = "1",
                Name = "Health Checks",
                Description = "Health check tests for all services",
                Tests = new List<TestRequest>()
            }
        };

        return Ok(collections);
    }
}

/// <summary>
/// Test request model
/// </summary>
public class TestRequest
{
    public string ServiceName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public Dictionary<string, string>? Headers { get; set; }
    public Dictionary<string, string>? QueryParameters { get; set; }
    public object? Body { get; set; }
    public string? Authorization { get; set; }
}

/// <summary>
/// Test execution result
/// </summary>
public class TestExecutionResult
{
    public string RequestId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? ResponseBody { get; set; }
    public Dictionary<string, string>? ResponseHeaders { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Batch test request
/// </summary>
public class BatchTestRequest
{
    public List<TestRequest> Tests { get; set; } = new();
    public bool StopOnFailure { get; set; }
}

/// <summary>
/// Batch test result
/// </summary>
public class BatchTestResult
{
    public string BatchId { get; set; } = string.Empty;
    public int TotalTests { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<TestExecutionResult> Results { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Test collection
/// </summary>
public class TestCollection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TestRequest> Tests { get; set; } = new();
}
