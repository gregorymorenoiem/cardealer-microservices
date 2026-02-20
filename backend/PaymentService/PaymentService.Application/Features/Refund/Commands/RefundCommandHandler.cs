using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Models;

namespace PaymentService.Application.Features.Refund.Commands;

/// <summary>
/// Handler para procesar un reembolso (multi-proveedor)
/// </summary>
public class RefundCommandHandler : IRequestHandler<RefundCommand, ChargeResponseDto>
{
    private readonly IAzulTransactionRepository _transactionRepository;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefundCommandHandler> _logger;

    public RefundCommandHandler(
        IAzulTransactionRepository transactionRepository,
        IPaymentGatewayFactory gatewayFactory,
        IConfiguration configuration,
        ILogger<RefundCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _gatewayFactory = gatewayFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Procesa el reembolso a través del proveedor correspondiente
    /// </summary>
    public async Task<ChargeResponseDto> Handle(RefundCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Procesando reembolso para transacción {TransactionId}", request.RefundRequest.TransactionId);

        try
        {
            // Obtener transacción original
            var originalTransaction = await _transactionRepository.GetByIdAsync(
                request.RefundRequest.TransactionId, 
                cancellationToken);

            if (originalTransaction == null)
            {
                throw new InvalidOperationException($"Transacción no encontrada: {request.RefundRequest.TransactionId}");
            }

            // Verificar que la transacción pueda ser reembolsada
            if (originalTransaction.Status != TransactionStatus.Approved && originalTransaction.Status != TransactionStatus.Captured)
            {
                throw new InvalidOperationException($"La transacción no puede ser reembolsada. Estado actual: {originalTransaction.Status}");
            }

            // Determinar el gateway usado en la transacción original
            var gateway = DetermineGatewayFromTransaction(originalTransaction);
            _logger.LogInformation("Usando gateway {Gateway} para reembolso", gateway);

            // Obtener el proveedor correcto
            var provider = _gatewayFactory.GetProvider(gateway);

            // Crear solicitud de reembolso
            var refundRequest = new RefundRequest
            {
                OriginalTransactionId = originalTransaction.AzulTransactionId ?? originalTransaction.Id.ToString(),
                Amount = request.RefundRequest.PartialAmount ?? originalTransaction.Amount,
                Currency = originalTransaction.Currency,
                Reason = request.RefundRequest.Reason ?? "Reembolso solicitado por el usuario"
            };

            // Crear nueva transacción de reembolso
            var refundTransaction = new AzulTransaction
            {
                Id = Guid.NewGuid(),
                UserId = originalTransaction.UserId,
                Amount = refundRequest.Amount,
                Currency = originalTransaction.Currency,
                Status = TransactionStatus.Pending,
                PaymentMethod = originalTransaction.PaymentMethod,
                TransactionType = "Refund",
                CustomerEmail = originalTransaction.CustomerEmail,
                CustomerPhone = originalTransaction.CustomerPhone,
                CreatedAt = DateTime.UtcNow,
                Gateway = gateway.ToString()
            };

            // Procesar reembolso a través del proveedor
            var refundResult = await provider.RefundAsync(refundRequest, cancellationToken);

            // Actualizar transacción con resultado
            refundTransaction.Status = refundResult.Success ? TransactionStatus.Refunded : TransactionStatus.Error;
            refundTransaction.AzulTransactionId = refundResult.TransactionId ?? string.Empty;
            refundTransaction.ResponseCode = refundResult.ResponseCode;
            refundTransaction.ResponseMessage = refundResult.Message;
            refundTransaction.CompletedAt = refundResult.Success ? DateTime.UtcNow : null;

            // Guardar transacción de reembolso
            await _transactionRepository.CreateAsync(refundTransaction, cancellationToken);

            // Actualizar transacción original si el reembolso fue exitoso
            if (refundResult.Success)
            {
                originalTransaction.Status = TransactionStatus.Refunded;
                await _transactionRepository.UpdateAsync(originalTransaction, cancellationToken);
            }

            _logger.LogInformation(
                "Reembolso {Result} vía {Gateway}. ID: {RefundTransactionId}", 
                refundResult.Success ? "procesado" : "fallido",
                gateway,
                refundTransaction.Id);

            return new ChargeResponseDto
            {
                TransactionId = refundTransaction.Id,
                AzulTransactionId = refundTransaction.AzulTransactionId ?? string.Empty,
                Status = refundTransaction.Status.ToString(),
                ResponseCode = refundTransaction.ResponseCode,
                ResponseMessage = refundTransaction.ResponseMessage,
                Amount = refundTransaction.Amount,
                Currency = refundTransaction.Currency,
                TransactionDate = refundTransaction.CreatedAt,
                IsSuccessful = refundResult.Success,
                Gateway = gateway.ToString(),
                ProviderName = provider.Name,
                Commission = null, // No hay comisión en reembolsos
                CommissionPercentage = null,
                NetAmount = refundTransaction.Amount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar reembolso para transacción {TransactionId}", request.RefundRequest.TransactionId);
            throw;
        }
    }

    /// <summary>
    /// Determina el gateway usado en la transacción original
    /// </summary>
    private PaymentGateway DetermineGatewayFromTransaction(AzulTransaction transaction)
    {
        // Si la transacción tiene el gateway guardado, usarlo
        if (!string.IsNullOrEmpty(transaction.Gateway) && 
            Enum.TryParse<PaymentGateway>(transaction.Gateway, true, out var parsedGateway))
        {
            return parsedGateway;
        }

        // Fallback: determinar por el nombre del campo (legacy)
        if (!string.IsNullOrEmpty(transaction.AzulTransactionId))
        {
            if (transaction.AzulTransactionId.StartsWith("PP-"))
                return PaymentGateway.PayPal;
            if (transaction.AzulTransactionId.StartsWith("PX-"))
                return PaymentGateway.PixelPay;
            if (transaction.AzulTransactionId.StartsWith("FY-"))
                return PaymentGateway.Fygaro;
            if (transaction.AzulTransactionId.StartsWith("CN-"))
                return PaymentGateway.CardNET;
        }

        // Default: AZUL (el proveedor original)
        var defaultGateway = _configuration["PaymentGateway:Default"] ?? "Azul";
        return Enum.TryParse<PaymentGateway>(defaultGateway, true, out var defaultParsed) 
            ? defaultParsed 
            : PaymentGateway.Azul;
    }
}
