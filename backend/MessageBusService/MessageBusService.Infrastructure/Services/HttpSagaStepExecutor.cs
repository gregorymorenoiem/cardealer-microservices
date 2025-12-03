using System.Text.Json;
using Microsoft.Extensions.Logging;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Infrastructure.Services;

/// <summary>
/// Saga step executor for HTTP API calls
/// </summary>
public class HttpSagaStepExecutor : ISagaStepExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpSagaStepExecutor> _logger;

    public HttpSagaStepExecutor(
        IHttpClientFactory httpClientFactory,
        ILogger<HttpSagaStepExecutor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool CanHandle(string actionType)
    {
        return actionType.StartsWith("http.", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string?> ExecuteAsync(SagaStep step, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing HTTP action {ActionType} for step {StepId}",
            step.ActionType, step.Id);

        try
        {
            // Parse action: http.{method}.{serviceName}
            var parts = step.ActionType.Split('.');
            if (parts.Length < 3 || parts[0] != "http")
            {
                throw new InvalidOperationException($"Invalid HTTP action type: {step.ActionType}");
            }

            var method = parts[1].ToUpperInvariant();
            var serviceName = parts[2];

            // Parse payload to get URL and body
            var payload = JsonSerializer.Deserialize<HttpActionPayload>(step.ActionPayload);
            if (payload == null || string.IsNullOrEmpty(payload.Url))
            {
                throw new InvalidOperationException("Invalid HTTP action payload");
            }

            var httpClient = _httpClientFactory.CreateClient(serviceName);

            // Add saga headers
            httpClient.DefaultRequestHeaders.Add("X-Saga-Id", step.SagaId.ToString());
            httpClient.DefaultRequestHeaders.Add("X-Saga-Step-Id", step.Id.ToString());
            httpClient.DefaultRequestHeaders.Add("X-Saga-Step-Name", step.Name);

            if (step.Saga != null)
            {
                httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", step.Saga.CorrelationId);
            }

            HttpResponseMessage response;

            switch (method)
            {
                case "GET":
                    response = await httpClient.GetAsync(payload.Url, cancellationToken);
                    break;
                case "POST":
                    var postContent = new StringContent(
                        payload.Body ?? string.Empty,
                        System.Text.Encoding.UTF8,
                        "application/json");
                    response = await httpClient.PostAsync(payload.Url, postContent, cancellationToken);
                    break;
                case "PUT":
                    var putContent = new StringContent(
                        payload.Body ?? string.Empty,
                        System.Text.Encoding.UTF8,
                        "application/json");
                    response = await httpClient.PutAsync(payload.Url, putContent, cancellationToken);
                    break;
                case "DELETE":
                    response = await httpClient.DeleteAsync(payload.Url, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported HTTP method: {method}");
            }

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("HTTP step {StepId} completed with status {StatusCode}",
                step.Id, response.StatusCode);

            return responseBody;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing HTTP step {StepId}", step.Id);
            throw;
        }
    }

    public async Task CompensateAsync(SagaStep step, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Compensating HTTP action {CompensationType} for step {StepId}",
            step.CompensationActionType, step.Id);

        try
        {
            if (string.IsNullOrEmpty(step.CompensationActionType) ||
                string.IsNullOrEmpty(step.CompensationPayload))
            {
                _logger.LogWarning("No compensation action or payload defined for step {StepId}", step.Id);
                return;
            }

            // Parse compensation action: http.{method}.{serviceName}
            var parts = step.CompensationActionType.Split('.');
            if (parts.Length < 3 || parts[0] != "http")
            {
                throw new InvalidOperationException($"Invalid HTTP compensation type: {step.CompensationActionType}");
            }

            var method = parts[1].ToUpperInvariant();
            var serviceName = parts[2];

            // Parse compensation payload
            var payload = JsonSerializer.Deserialize<HttpActionPayload>(step.CompensationPayload);
            if (payload == null || string.IsNullOrEmpty(payload.Url))
            {
                throw new InvalidOperationException("Invalid HTTP compensation payload");
            }

            var httpClient = _httpClientFactory.CreateClient(serviceName);

            // Add saga compensation headers
            httpClient.DefaultRequestHeaders.Add("X-Saga-Id", step.SagaId.ToString());
            httpClient.DefaultRequestHeaders.Add("X-Saga-Step-Id", step.Id.ToString());
            httpClient.DefaultRequestHeaders.Add("X-Saga-Compensation", "true");

            if (step.Saga != null)
            {
                httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", step.Saga.CorrelationId);
            }

            HttpResponseMessage response;

            switch (method)
            {
                case "POST":
                case "PUT":
                    var content = new StringContent(
                        payload.Body ?? string.Empty,
                        System.Text.Encoding.UTF8,
                        "application/json");
                    response = method == "POST"
                        ? await httpClient.PostAsync(payload.Url, content, cancellationToken)
                        : await httpClient.PutAsync(payload.Url, content, cancellationToken);
                    break;
                case "DELETE":
                    response = await httpClient.DeleteAsync(payload.Url, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported HTTP compensation method: {method}");
            }

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("HTTP compensation for step {StepId} completed with status {StatusCode}",
                step.Id, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compensating HTTP step {StepId}", step.Id);
            throw;
        }
    }
}

public class HttpActionPayload
{
    public string Url { get; set; } = string.Empty;
    public string? Body { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}
