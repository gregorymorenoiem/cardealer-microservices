using PaymentService.Domain.Enums;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Services;

/// <summary>
/// Implementación de PaymentGatewayRegistry
/// Gestiona el registro centralizado de proveedores de pago
/// </summary>
public class PaymentGatewayRegistry : IPaymentGatewayRegistry
{
    private readonly Dictionary<PaymentGateway, IPaymentGatewayProvider> _providers;
    private readonly ILogger<PaymentGatewayRegistry> _logger;

    public PaymentGatewayRegistry(ILogger<PaymentGatewayRegistry> logger)
    {
        _logger = logger;
        _providers = new Dictionary<PaymentGateway, IPaymentGatewayProvider>();
    }

    public void Register(IPaymentGatewayProvider provider)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        if (_providers.ContainsKey(provider.Gateway))
        {
            _logger.LogWarning("Proveedor {Gateway} ya registrado, reemplazando", provider.Gateway);
        }

        _providers[provider.Gateway] = provider;
        _logger.LogInformation("Proveedor {Gateway} ({Name}) registrado exitosamente", 
            provider.Gateway, provider.Name);
    }

    public bool Unregister(PaymentGateway gateway)
    {
        if (_providers.Remove(gateway))
        {
            _logger.LogInformation("Proveedor {Gateway} removido del registro", gateway);
            return true;
        }

        _logger.LogWarning("Proveedor {Gateway} no encontrado en registro", gateway);
        return false;
    }

    public IPaymentGatewayProvider? Get(PaymentGateway gateway)
    {
        if (_providers.TryGetValue(gateway, out var provider))
        {
            return provider;
        }

        _logger.LogWarning("Proveedor {Gateway} no registrado", gateway);
        return null;
    }

    public IReadOnlyCollection<IPaymentGatewayProvider> GetAll()
    {
        return _providers.Values.ToList().AsReadOnly();
    }

    public bool Contains(PaymentGateway gateway)
    {
        return _providers.ContainsKey(gateway);
    }

    public int Count()
    {
        return _providers.Count;
    }

    public void Clear()
    {
        _providers.Clear();
        _logger.LogInformation("Registro de proveedores limpiado");
    }
}
