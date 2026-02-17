using PaymentService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Services;

/// <summary>
/// Implementación de PaymentGatewayFactory
/// Factory pattern para crear instancias de proveedores de pago
/// Soporta inyección de dependencias dinámica
/// </summary>
public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IPaymentGatewayRegistry _registry;
    private readonly ILogger<PaymentGatewayFactory> _logger;
    private readonly IConfiguration _configuration;
    private PaymentGateway _defaultGateway = PaymentGateway.Azul; // Default

    public PaymentGatewayFactory(
        IPaymentGatewayRegistry registry,
        ILogger<PaymentGatewayFactory> logger,
        IConfiguration configuration)
    {
        _registry = registry;
        _logger = logger;
        _configuration = configuration;

        // Leer gateway por defecto desde configuración
        var defaultGateway = _configuration["PaymentGateway:Default"];
        if (!string.IsNullOrEmpty(defaultGateway) && 
            Enum.TryParse<PaymentGateway>(defaultGateway, out var gateway))
        {
            _defaultGateway = gateway;
        }
    }

    public IPaymentGatewayProvider GetProvider(PaymentGateway gateway)
    {
        var provider = _registry.Get(gateway);
        
        if (provider == null)
        {
            var message = $"Proveedor de pago '{gateway}' no está registrado";
            _logger.LogError(message);
            throw new KeyNotFoundException(message);
        }

        return provider;
    }

    public IPaymentGatewayProvider GetDefaultProvider()
    {
        return GetProvider(_defaultGateway);
    }

    public IEnumerable<IPaymentGatewayProvider> GetAllProviders()
    {
        return _registry.GetAll();
    }

    public bool IsProviderAvailable(PaymentGateway gateway)
    {
        try
        {
            var provider = GetProvider(gateway);
            var errors = provider.ValidateConfiguration();
            
            if (errors.Count > 0)
            {
                _logger.LogWarning(
                    "Proveedor {Gateway} tiene problemas de configuración: {Errors}",
                    gateway,
                    string.Join(", ", errors));
                return false;
            }

            return true;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    public Dictionary<PaymentGateway, GatewayStats> GetGatewayStats()
    {
        var stats = new Dictionary<PaymentGateway, GatewayStats>();

        foreach (var provider in GetAllProviders())
        {
            var configErrors = provider.ValidateConfiguration();
            var isConfigured = configErrors.Count == 0;

            stats[provider.Gateway] = new GatewayStats
            {
                Gateway = provider.Gateway,
                Name = provider.Name,
                Type = provider.Type,
                IsActive = true,
                IsConfigured = isConfigured,
                CommissionRange = GetCommissionRange(provider.Gateway),
                FixedCost = GetFixedCost(provider.Gateway),
                MonthlyCost = GetMonthlyCost(provider.Gateway),
                SupportsTokenization = GetSupportsTokenization(provider.Gateway),
                TokenizationMethod = GetTokenizationMethod(provider.Gateway),
                SupportedCurrencies = new List<string> { "DOP", "USD" },
                SupportedPaymentMethods = GetSupportedPaymentMethods(provider.Gateway)
            };
        }

        return stats;
    }

    #region Helper Methods

    private string GetCommissionRange(PaymentGateway gateway) => gateway switch
    {
        PaymentGateway.Azul => "2.9% - 4.5%",
        PaymentGateway.CardNET => "2.5% - 4.5%",
        PaymentGateway.PixelPay => "1.0% - 3.5%",
        PaymentGateway.Fygaro => "Varía según configuración",
        _ => "N/A"
    };

    private string GetFixedCost(PaymentGateway gateway) => gateway switch
    {
        PaymentGateway.Azul => "RD$5 - 10",
        PaymentGateway.CardNET => "RD$5 - 10",
        PaymentGateway.PixelPay => "US$0.15 - 0.25",
        PaymentGateway.Fygaro => "Varía",
        _ => "N/A"
    };

    private string GetMonthlyCost(PaymentGateway gateway) => gateway switch
    {
        PaymentGateway.Azul => "US$30 - 50",
        PaymentGateway.CardNET => "US$30 - 50",
        PaymentGateway.PixelPay => "Varía",
        PaymentGateway.Fygaro => "US$15+",
        _ => "N/A"
    };

    private bool GetSupportsTokenization(PaymentGateway gateway) => gateway switch
    {
        PaymentGateway.Azul => true,
        PaymentGateway.CardNET => true,
        PaymentGateway.PixelPay => true,
        PaymentGateway.Fygaro => true,
        _ => false
    };

    private string? GetTokenizationMethod(PaymentGateway gateway) => gateway switch
    {
        PaymentGateway.Azul => "Cybersource",
        PaymentGateway.CardNET => "Solicitar (Manual)",
        PaymentGateway.PixelPay => "Nativa (API)",
        PaymentGateway.Fygaro => "Módulo de Suscripciones",
        _ => null
    };

    private List<PaymentMethod> GetSupportedPaymentMethods(PaymentGateway gateway) => gateway switch
    {
        PaymentGateway.Azul => new List<PaymentMethod>
        {
            PaymentMethod.CreditCard,
            PaymentMethod.DebitCard,
            PaymentMethod.TokenizedCard,
            PaymentMethod.MobilePayment
        },
        PaymentGateway.CardNET => new List<PaymentMethod>
        {
            PaymentMethod.CreditCard,
            PaymentMethod.DebitCard,
            PaymentMethod.TokenizedCard,
            PaymentMethod.ACH
        },
        PaymentGateway.PixelPay => new List<PaymentMethod>
        {
            PaymentMethod.CreditCard,
            PaymentMethod.DebitCard,
            PaymentMethod.TokenizedCard,
            PaymentMethod.MobilePayment,
            PaymentMethod.EWallet
        },
        PaymentGateway.Fygaro => new List<PaymentMethod>
        {
            PaymentMethod.CreditCard,
            PaymentMethod.DebitCard,
            PaymentMethod.TokenizedCard,
            PaymentMethod.ACH,
            PaymentMethod.MobilePayment
        },
        _ => new List<PaymentMethod>()
    };

    #endregion
}
