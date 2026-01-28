using Google.Cloud.Dialogflow.V2;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Domain.Models;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Diagnostics;

namespace ChatbotService.Infrastructure.Services;

public class DialogflowSettings
{
    public string ProjectId { get; set; } = string.Empty;
    public string CredentialsPath { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "es";
    public int TimeoutSeconds { get; set; } = 30;
}

public class DialogflowService : IDialogflowService
{
    private readonly DialogflowSettings _settings;
    private readonly ILogger<DialogflowService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private SessionsClient? _sessionsClient;
    private AgentsClient? _agentsClient;
    private IntentsClient? _intentsClient;
    private EntityTypesClient? _entityTypesClient;
    private DateTime? _lastSuccessfulCall;
    private int _failedCallsLast24h;

    public DialogflowService(IOptions<DialogflowSettings> settings, ILogger<DialogflowService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _retryPolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) => {
                    _logger.LogWarning(exception, "Dialogflow retry {RetryCount} after {TimeSpan}s", retryCount, timeSpan.TotalSeconds);
                });

        _circuitBreakerPolicy = Policy.Handle<Exception>()
            .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1),
                (exception, duration) => _logger.LogError(exception, "Circuit breaker opened for {Duration}s", duration.TotalSeconds),
                () => _logger.LogInformation("Circuit breaker reset"));
    }

    private async Task<SessionsClient> GetSessionsClientAsync()
    {
        if (_sessionsClient == null)
        {
            var builder = new SessionsClientBuilder();
            if (!string.IsNullOrEmpty(_settings.CredentialsPath))
                builder.CredentialsPath = _settings.CredentialsPath;
            _sessionsClient = await builder.BuildAsync();
        }
        return _sessionsClient;
    }

    private async Task<AgentsClient> GetAgentsClientAsync()
    {
        if (_agentsClient == null)
        {
            var builder = new AgentsClientBuilder();
            if (!string.IsNullOrEmpty(_settings.CredentialsPath))
                builder.CredentialsPath = _settings.CredentialsPath;
            _agentsClient = await builder.BuildAsync();
        }
        return _agentsClient;
    }

    private async Task<IntentsClient> GetIntentsClientAsync()
    {
        if (_intentsClient == null)
        {
            var builder = new IntentsClientBuilder();
            if (!string.IsNullOrEmpty(_settings.CredentialsPath))
                builder.CredentialsPath = _settings.CredentialsPath;
            _intentsClient = await builder.BuildAsync();
        }
        return _intentsClient;
    }

    private async Task<EntityTypesClient> GetEntityTypesClientAsync()
    {
        if (_entityTypesClient == null)
        {
            var builder = new EntityTypesClientBuilder();
            if (!string.IsNullOrEmpty(_settings.CredentialsPath))
                builder.CredentialsPath = _settings.CredentialsPath;
            _entityTypesClient = await builder.BuildAsync();
        }
        return _entityTypesClient;
    }

    public async Task<DialogflowDetectionResult> DetectIntentAsync(string sessionId, string text, string? languageCode = null, CancellationToken ct = default)
    {
        var result = new DialogflowDetectionResult { QueryText = text, LanguageCode = languageCode ?? _settings.LanguageCode };
        try
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    var client = await GetSessionsClientAsync();
                    var session = new SessionName(_settings.ProjectId, sessionId);
                    var queryInput = new QueryInput
                    {
                        Text = new TextInput { Text = text, LanguageCode = languageCode ?? _settings.LanguageCode }
                    };

                    var response = await client.DetectIntentAsync(session, queryInput, ct);
                    var queryResult = response.QueryResult;

                    result.DetectedIntent = queryResult.Intent?.DisplayName;
                    result.ConfidenceScore = queryResult.IntentDetectionConfidence;
                    result.FulfillmentText = queryResult.FulfillmentText;
                    result.IsFallback = queryResult.Intent?.IsFallback ?? string.IsNullOrEmpty(queryResult.Intent?.DisplayName);
                    result.ResponseId = response.ResponseId;

                    foreach (var param in queryResult.Parameters.Fields)
                        result.Parameters[param.Key] = param.Value.StringValue;

                    result.OutputContexts = queryResult.OutputContexts.Select(c => c.Name).ToList();
                    
                    _lastSuccessfulCall = DateTime.UtcNow;
                    return result;
                }));
        }
        catch (Exception ex)
        {
            _failedCallsLast24h++;
            _logger.LogError(ex, "Failed to detect intent for session {SessionId}", sessionId);
            result.IsFallback = true;
            result.FulfillmentText = "Lo siento, no pude procesar tu mensaje. ¿Podrías intentar de nuevo?";
            return result;
        }
    }

    public async Task<DialogflowAgentInfo> GetAgentInfoAsync(CancellationToken ct = default)
    {
        try
        {
            var client = await GetAgentsClientAsync();
            var intentsClient = await GetIntentsClientAsync();
            var entityTypesClient = await GetEntityTypesClientAsync();
            
            var projectName = $"projects/{_settings.ProjectId}";
            var agentName = $"projects/{_settings.ProjectId}/agent";
            
            var agent = await client.GetAgentAsync(projectName, ct);
            var intents = intentsClient.ListIntentsAsync(agentName);
            var entityTypes = entityTypesClient.ListEntityTypesAsync(agentName);

            var intentCount = 0;
            await foreach (var _ in intents) intentCount++;
            
            var entityCount = 0;
            await foreach (var _ in entityTypes) entityCount++;

            return new DialogflowAgentInfo
            {
                DisplayName = agent.DisplayName,
                DefaultLanguageCode = agent.DefaultLanguageCode,
                SupportedLanguageCodes = agent.SupportedLanguageCodes.ToList(),
                TimeZone = agent.TimeZone,
                Description = agent.Description,
                IntentCount = intentCount,
                EntityTypeCount = entityCount,
                IsHealthy = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get agent info");
            return new DialogflowAgentInfo { IsHealthy = false, HealthMessage = ex.Message };
        }
    }

    public async Task<bool> TrainAgentAsync(CancellationToken ct = default)
    {
        try
        {
            var client = await GetAgentsClientAsync();
            var projectName = $"projects/{_settings.ProjectId}";
            var operation = await client.TrainAgentAsync(projectName, ct);
            await operation.PollUntilCompletedAsync();
            _logger.LogInformation("Agent training completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to train agent");
            return false;
        }
    }

    public async Task<bool> CreateIntentAsync(SuggestedIntent intent, CancellationToken ct = default)
    {
        try
        {
            var client = await GetIntentsClientAsync();
            var newIntent = new Intent
            {
                DisplayName = intent.IntentName,
                TrainingPhrases = { intent.TrainingPhrases.Select(p => new Intent.Types.TrainingPhrase
                {
                    Parts = { new Intent.Types.TrainingPhrase.Types.Part { Text = p } },
                    Type = Intent.Types.TrainingPhrase.Types.Type.Example
                }) },
                Messages = { new Intent.Types.Message
                {
                    Text = new Intent.Types.Message.Types.Text { Text_ = { intent.SuggestedResponses } }
                } }
            };

            await client.CreateIntentAsync(new AgentName(_settings.ProjectId), newIntent, ct);
            _logger.LogInformation("Created new intent: {IntentName}", intent.IntentName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create intent {IntentName}", intent.IntentName);
            return false;
        }
    }

    public async Task<bool> AddTrainingPhrasesAsync(string intentName, IEnumerable<string> phrases, CancellationToken ct = default)
    {
        try
        {
            var client = await GetIntentsClientAsync();
            var intents = client.ListIntentsAsync(new AgentName(_settings.ProjectId));
            
            Intent? targetIntent = null;
            await foreach (var i in intents)
            {
                if (i.DisplayName == intentName) { targetIntent = i; break; }
            }

            if (targetIntent == null)
            {
                _logger.LogWarning("Intent not found: {IntentName}", intentName);
                return false;
            }

            foreach (var phrase in phrases)
            {
                targetIntent.TrainingPhrases.Add(new Intent.Types.TrainingPhrase
                {
                    Parts = { new Intent.Types.TrainingPhrase.Types.Part { Text = phrase } },
                    Type = Intent.Types.TrainingPhrase.Types.Type.Example
                });
            }

            var updateRequest = new UpdateIntentRequest
            {
                Intent = targetIntent,
                LanguageCode = _settings.LanguageCode
            };
            await client.UpdateIntentAsync(updateRequest);
            _logger.LogInformation("Added {Count} training phrases to intent {IntentName}", phrases.Count(), intentName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add training phrases to {IntentName}", intentName);
            return false;
        }
    }

    public async Task<bool> TestConnectivityAsync(CancellationToken ct = default)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            await GetAgentInfoAsync(ct);
            sw.Stop();
            _logger.LogInformation("Dialogflow connectivity test passed in {Ms}ms", sw.ElapsedMilliseconds);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dialogflow connectivity test failed");
            return false;
        }
    }

    public Task<DialogflowHealthStatus> GetHealthStatusAsync(CancellationToken ct = default)
    {
        return Task.FromResult(new DialogflowHealthStatus
        {
            IsConnected = _lastSuccessfulCall.HasValue && _lastSuccessfulCall.Value > DateTime.UtcNow.AddMinutes(-5),
            LastSuccessfulCall = _lastSuccessfulCall,
            FailedCallsLast24h = _failedCallsLast24h
        });
    }
}
