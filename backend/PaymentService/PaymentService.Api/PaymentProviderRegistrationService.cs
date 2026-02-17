using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Services.Providers;

namespace PaymentService.Api;

/// <summary>
/// Background service that registers payment providers on application startup.
/// This runs after the DI container is fully built, avoiding resolution issues.
/// </summary>
public class PaymentProviderRegistrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentProviderRegistrationService> _logger;

    public PaymentProviderRegistrationService(
        IServiceProvider serviceProvider,
        ILogger<PaymentProviderRegistrationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Starting payment provider registration...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var registry = scope.ServiceProvider.GetRequiredService<IPaymentGatewayRegistry>();

            // Register each provider
            await RegisterProviderAsync<AzulPaymentProvider>(scope, registry, "AZUL", cancellationToken);
            await RegisterProviderAsync<CardNETPaymentProvider>(scope, registry, "CardNET", cancellationToken);
            await RegisterProviderAsync<PixelPayPaymentProvider>(scope, registry, "PixelPay", cancellationToken);
            await RegisterProviderAsync<FygaroPaymentProvider>(scope, registry, "Fygaro", cancellationToken);
            await RegisterProviderAsync<PayPalPaymentProvider>(scope, registry, "PayPal", cancellationToken);

            // Log summary
            var registeredProviders = registry.GetAll();
            _logger.LogInformation("📊 Total providers registered: {ProviderCount}", registeredProviders.Count);

            foreach (var provider in registeredProviders)
            {
                var configErrors = provider.ValidateConfiguration();
                var status = configErrors.Count == 0 ? "✅ CONFIGURED" : "⚠️ NOT CONFIGURED";
                _logger.LogInformation("   • {ProviderName}: {Status}", provider.Name, status);
            }

            _logger.LogInformation("✅ Payment provider registration completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during payment provider registration");
            // Don't throw - allow the app to continue running even if provider registration fails
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Payment provider registration service stopping");
        return Task.CompletedTask;
    }

    private async Task RegisterProviderAsync<TProvider>(
        IServiceScope scope, 
        IPaymentGatewayRegistry registry, 
        string providerName,
        CancellationToken cancellationToken) where TProvider : class, IPaymentGatewayProvider
    {
        try
        {
            var provider = scope.ServiceProvider.GetRequiredService<TProvider>();
            registry.Register(provider);
            _logger.LogInformation("✅ {ProviderName} payment provider registered successfully", providerName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Could not register {ProviderName} payment provider: {Message}", 
                providerName, ex.Message);
        }
        
        await Task.CompletedTask;
    }
}
