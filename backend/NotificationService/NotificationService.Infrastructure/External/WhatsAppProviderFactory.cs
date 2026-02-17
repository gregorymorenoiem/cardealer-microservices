using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces.External;

namespace NotificationService.Infrastructure.External;

/// <summary>
/// Factory that resolves the correct WhatsApp provider based on ConfigurationService settings.
/// Supports: twilio (Twilio WhatsApp API), meta (Meta WhatsApp Business API), mock (logging only).
/// </summary>
public interface IWhatsAppProviderFactory
{
    Task<IWhatsAppProvider> GetProviderAsync(CancellationToken ct = default);
}

public class WhatsAppProviderFactory : IWhatsAppProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigurationServiceClient _configClient;
    private readonly ILogger<WhatsAppProviderFactory> _logger;

    public WhatsAppProviderFactory(
        IServiceProvider serviceProvider,
        IConfigurationServiceClient configClient,
        ILogger<WhatsAppProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _configClient = configClient;
        _logger = logger;
    }

    public async Task<IWhatsAppProvider> GetProviderAsync(CancellationToken ct = default)
    {
        var provider = await _configClient.GetValueAsync("whatsapp.provider", ct);

        return provider?.ToLowerInvariant() switch
        {
            "meta" => _serviceProvider.GetRequiredService<MetaWhatsAppService>(),
            "twilio" => _serviceProvider.GetRequiredService<TwilioWhatsAppService>(),
            "mock" => _serviceProvider.GetRequiredService<MockWhatsAppService>(),
            _ => _serviceProvider.GetRequiredService<TwilioWhatsAppService>() // default
        };
    }
}

/// <summary>
/// Mock WhatsApp provider that only logs messages (for development/testing).
/// </summary>
public class MockWhatsAppService : IWhatsAppProvider
{
    private readonly ILogger<MockWhatsAppService> _logger;

    public MockWhatsAppService(ILogger<MockWhatsAppService> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "MockWhatsApp";

    public Task<(bool success, string? messageId, string? error)> SendMessageAsync(
        string to, string message, Dictionary<string, object>? metadata = null)
    {
        _logger.LogInformation(
            "[MOCK WhatsApp] Message to {To}: {Message}", to, message);
        return Task.FromResult<(bool, string?, string?)>(
            (true, $"mock-wa-{Guid.NewGuid()}", null));
    }

    public Task<(bool success, string? messageId, string? error)> SendTemplateAsync(
        string to, string templateName, Dictionary<string, string>? parameters = null,
        string? languageCode = "es", Dictionary<string, object>? metadata = null)
    {
        _logger.LogInformation(
            "[MOCK WhatsApp] Template '{Template}' to {To} with params: {Params}",
            templateName, to, parameters != null ? string.Join(", ", parameters) : "none");
        return Task.FromResult<(bool, string?, string?)>(
            (true, $"mock-wa-template-{Guid.NewGuid()}", null));
    }
}
