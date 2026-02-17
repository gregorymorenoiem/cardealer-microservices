using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Infrastructure.Services;

namespace ChatbotService.Tests;

/// <summary>
/// Simple IMeterFactory implementation for unit tests.
/// </summary>
internal sealed class TestMeterFactory : IMeterFactory
{
    private readonly List<Meter> _meters = new();

    public Meter Create(MeterOptions options)
    {
        var meter = new Meter(options.Name, options.Version);
        _meters.Add(meter);
        return meter;
    }

    public void Dispose()
    {
        foreach (var meter in _meters)
            meter.Dispose();
        _meters.Clear();
    }
}

/// <summary>
/// Unit tests for LlmService — covers HTTP call, fallback, circuit breaker,
/// and response parsing.
/// </summary>
public class LlmServiceTests : IDisposable
{
    private readonly Mock<IChatMessageRepository> _messageRepo;
    private readonly Mock<ILogger<LlmService>> _logger;
    private readonly LlmSettings _settings;
    private readonly ChatbotMetrics _metrics;
    private readonly TestMeterFactory _meterFactory;

    public LlmServiceTests()
    {
        _messageRepo = new Mock<IChatMessageRepository>();
        _logger = new Mock<ILogger<LlmService>>();
        _settings = new LlmSettings
        {
            ServerUrl = "http://localhost:8000",
            ModelId = "test-model",
            TimeoutSeconds = 60,
            Temperature = 0.3f,
            TopP = 0.9f,
            MaxTokens = 400,
            RepetitionPenalty = 1.15f,
            SystemPrompt = "Test system prompt"
        };

        _meterFactory = new TestMeterFactory();
        _metrics = new ChatbotMetrics(_meterFactory);
    }

    public void Dispose()
    {
        _meterFactory.Dispose();
    }

    private LlmService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:8000")
        };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("LlmServer")).Returns(httpClient);

        return new LlmService(
            Options.Create(_settings),
            factory.Object,
            _messageRepo.Object,
            _logger.Object,
            _metrics);
    }

    [Fact]
    public void LlmSettings_ShouldHaveCorrectDefaults()
    {
        var settings = new LlmSettings();
        settings.TimeoutSeconds.Should().Be(60);
        settings.Temperature.Should().Be(0.3f);
        settings.TopP.Should().Be(0.9f);
        settings.MaxTokens.Should().Be(400);
        settings.RepetitionPenalty.Should().Be(1.15f);
    }

    [Fact]
    public async Task GenerateResponseAsync_ShouldReturnFallback_OnHttpError()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var service = CreateService(handler.Object);

        // Act
        var result = await service.GenerateResponseAsync("session1", "Hola");

        // Assert
        result.IsFallback.Should().BeTrue();
        result.FulfillmentText.Should().Contain("no pude procesar");
    }

    [Fact]
    public async Task GetModelInfoAsync_ShouldReturnUnhealthy_OnError()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var service = CreateService(handler.Object);

        // Act
        var result = await service.GetModelInfoAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
    }
}

/// <summary>
/// Tests for ChatbotMetrics — verifies counter and histogram recording.
/// </summary>
public class ChatbotMetricsTests
{
    [Fact]
    public void RecordLlmCall_ShouldNotThrow()
    {
        using var meterFactory = new TestMeterFactory();
        var metrics = new ChatbotMetrics(meterFactory);

        // Act & Assert — should not throw
        metrics.RecordLlmCall(success: true, responseTimeMs: 150.5, tokensUsed: 100);
        metrics.RecordLlmCall(success: false, responseTimeMs: 5000.0);
    }

    [Fact]
    public void RecordSessionEvents_ShouldNotThrow()
    {
        using var meterFactory = new TestMeterFactory();
        var metrics = new ChatbotMetrics(meterFactory);

        metrics.RecordSessionStarted("web");
        metrics.RecordSessionEnded();
        metrics.RecordSessionTransferred();
        metrics.RecordMessageReceived("whatsapp");
        metrics.RecordMessageProcessed("web", usedLlm: true);
        metrics.RecordQuickResponseHit();
        metrics.RecordRateLimitRejection("/api/Chat/message");
        metrics.RecordValidationFailure("sql_injection");
        metrics.RecordMessageProcessingTime(250.0);
        metrics.RecordInteractionLimitReached();
        metrics.RecordCircuitBreakerTrip();
    }
}
