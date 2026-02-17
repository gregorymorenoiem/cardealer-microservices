using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.Features.Charge.Commands;

/// <summary>
/// Handler para procesar un cobro usando el sistema multi-proveedor
/// </summary>
public class ChargeCommandHandler : IRequestHandler<ChargeCommand, ChargeResponseDto>
{
    private readonly IAzulTransactionRepository _transactionRepository;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChargeCommandHandler> _logger;

    public ChargeCommandHandler(
        IAzulTransactionRepository transactionRepository,
        IPaymentGatewayFactory gatewayFactory,
        IConfiguration configuration,
        ILogger<ChargeCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _gatewayFactory = gatewayFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Procesa el cobro usando el proveedor seleccionado o el default.
    /// 
    /// BUSINESS RULE: Charges are ALWAYS processed regardless of the gateway's
    /// enabled/disabled status. Disabled gateways only prevent NEW users from
    /// selecting them. Existing users with saved payment methods continue to
    /// be charged normally (recurring, subscriptions, etc.).
    /// </summary>
    public async Task<ChargeResponseDto> Handle(ChargeCommand request, CancellationToken cancellationToken)
    {
        var chargeRequest = request.ChargeRequest;
        
        // Determinar el proveedor a usar
        var gateway = chargeRequest.Gateway ?? GetDefaultGateway();
        
        _logger.LogInformation(
            "🔵 Procesando cobro para usuario {UserId} | Proveedor: {Gateway} | Monto: {Amount} {Currency}",
            chargeRequest.UserId, gateway, chargeRequest.Amount, chargeRequest.Currency);

        try
        {
            // Obtener el proveedor desde el Factory
            var provider = _gatewayFactory.GetProvider(gateway);
            
            _logger.LogInformation("✅ Usando proveedor: {ProviderName} ({ProviderType})", 
                provider.Name, provider.Type);

            // Crear el request para el proveedor
            var providerRequest = new ChargeRequest
            {
                UserId = chargeRequest.UserId,
                Amount = chargeRequest.Amount,
                Currency = chargeRequest.Currency,
                Description = chargeRequest.Description,
                PaymentMethod = Enum.Parse<PaymentMethod>(chargeRequest.PaymentMethod),
                CustomerEmail = chargeRequest.CustomerEmail,
                CustomerPhone = chargeRequest.CustomerPhone,
                CustomerIpAddress = chargeRequest.CustomerIpAddress,
                InvoiceReference = chargeRequest.InvoiceReference,
                IsRecurring = chargeRequest.IsRecurring,
                SubscriptionId = chargeRequest.SubscriptionId,
                CardData = !string.IsNullOrEmpty(chargeRequest.CardNumber) ? new CardData
                {
                    CardNumber = chargeRequest.CardNumber!,
                    ExpiryMonth = chargeRequest.CardExpiryMonth!,
                    ExpiryYear = chargeRequest.CardExpiryYear!,
                    CVV = chargeRequest.CardCVV!,
                    CardholderName = chargeRequest.CardholderName!,
                    Email = chargeRequest.CustomerEmail,
                    Phone = chargeRequest.CustomerPhone
                } : null,
                CardToken = chargeRequest.CardToken
            };

            // Procesar con el proveedor seleccionado
            var paymentResult = await provider.ChargeAsync(providerRequest, cancellationToken);

            // Crear entidad de transacción para persistir
            var transaction = new AzulTransaction
            {
                Id = paymentResult.TransactionId ?? Guid.NewGuid(),
                UserId = chargeRequest.UserId,
                Amount = chargeRequest.Amount,
                Currency = chargeRequest.Currency,
                Status = paymentResult.Status,
                PaymentMethod = Enum.Parse<PaymentMethod>(chargeRequest.PaymentMethod),
                TransactionType = chargeRequest.TransactionType,
                CustomerEmail = chargeRequest.CustomerEmail,
                CustomerPhone = chargeRequest.CustomerPhone,
                CustomerIpAddress = chargeRequest.CustomerIpAddress,
                InvoiceReference = chargeRequest.InvoiceReference,
                IsRecurring = chargeRequest.IsRecurring,
                SubscriptionId = chargeRequest.SubscriptionId,
                CreatedAt = DateTime.UtcNow,
                // Datos del proveedor
                AzulTransactionId = paymentResult.ExternalTransactionId,
                AuthorizationCode = paymentResult.AuthorizationCode,
                ResponseCode = paymentResult.ResponseCode,
                ResponseMessage = paymentResult.ResponseMessage,
                CardToken = paymentResult.CardToken,
                CardLastFour = paymentResult.CardLastFour,
                CompletedAt = paymentResult.Success ? DateTime.UtcNow : null
            };

            // Guardar transacción
            await _transactionRepository.CreateAsync(transaction, cancellationToken);

            _logger.LogInformation(
                "✅ Cobro procesado exitosamente | ID: {TransactionId} | Proveedor: {Gateway} | Comisión: {Commission}",
                transaction.Id, gateway, paymentResult.Commission);

            return new ChargeResponseDto
            {
                TransactionId = transaction.Id,
                AzulTransactionId = paymentResult.ExternalTransactionId,
                Status = transaction.Status.ToString(),
                ResponseCode = paymentResult.ResponseCode,
                ResponseMessage = paymentResult.ResponseMessage,
                AuthorizationCode = paymentResult.AuthorizationCode,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                TransactionDate = transaction.CreatedAt,
                IsSuccessful = paymentResult.Success,
                // Nuevos campos multi-proveedor
                Gateway = gateway.ToString(),
                ProviderName = provider.Name,
                Commission = paymentResult.Commission,
                CommissionPercentage = paymentResult.CommissionPercentage,
                NetAmount = paymentResult.NetAmount
            };
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not registered"))
        {
            _logger.LogWarning("❌ Proveedor {Gateway} no está registrado o configurado", gateway);
            throw new InvalidOperationException($"El proveedor {gateway} no está disponible. Intente con otro proveedor.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al procesar cobro para usuario {UserId} con proveedor {Gateway}", 
                chargeRequest.UserId, gateway);
            throw;
        }
    }

    /// <summary>
    /// Obtiene el proveedor default de la configuración
    /// </summary>
    private PaymentGateway GetDefaultGateway()
    {
        var defaultGateway = _configuration["PaymentGateway:Default"] ?? "Azul";
        
        if (Enum.TryParse<PaymentGateway>(defaultGateway, true, out var gateway))
        {
            return gateway;
        }
        
        _logger.LogWarning("⚠️ Gateway default '{Default}' no válido, usando AZUL", defaultGateway);
        return PaymentGateway.Azul;
    }
}
