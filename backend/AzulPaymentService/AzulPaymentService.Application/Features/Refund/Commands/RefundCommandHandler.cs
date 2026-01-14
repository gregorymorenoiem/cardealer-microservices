using MediatR;
using Microsoft.Extensions.Logging;
using AzulPaymentService.Application.DTOs;
using AzulPaymentService.Domain.Interfaces;
using AzulPaymentService.Domain.Entities;
using AzulPaymentService.Domain.Enums;

namespace AzulPaymentService.Application.Features.Refund.Commands;

/// <summary>
/// Handler para procesar un reembolso
/// </summary>
public class RefundCommandHandler : IRequestHandler<RefundCommand, ChargeResponseDto>
{
    private readonly IAzulTransactionRepository _transactionRepository;
    private readonly ILogger<RefundCommandHandler> _logger;

    public RefundCommandHandler(
        IAzulTransactionRepository transactionRepository,
        ILogger<RefundCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Procesa el reembolso
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

            // Crear nueva transacción de reembolso
            var refundTransaction = new AzulTransaction
            {
                Id = Guid.NewGuid(),
                UserId = originalTransaction.UserId,
                Amount = request.RefundRequest.PartialAmount ?? originalTransaction.Amount,
                Currency = originalTransaction.Currency,
                Status = TransactionStatus.Pending,
                PaymentMethod = originalTransaction.PaymentMethod,
                TransactionType = "Refund",
                CustomerEmail = originalTransaction.CustomerEmail,
                CustomerPhone = originalTransaction.CustomerPhone,
                CreatedAt = DateTime.UtcNow
            };

            // En este punto, se llamaría a AZUL API para procesar el reembolso
            // TODO: Implementar integración con AZUL en Infrastructure layer
            
            // Simular respuesta exitosa
            refundTransaction.Status = TransactionStatus.Refunded;
            refundTransaction.AzulTransactionId = Guid.NewGuid().ToString();
            refundTransaction.ResponseCode = "00";
            refundTransaction.ResponseMessage = "Reembolso Aprobado";
            refundTransaction.CompletedAt = DateTime.UtcNow;

            // Guardar transacción de reembolso
            await _transactionRepository.CreateAsync(refundTransaction, cancellationToken);

            // Actualizar transacción original
            originalTransaction.Status = TransactionStatus.Refunded;
            await _transactionRepository.UpdateAsync(originalTransaction, cancellationToken);

            _logger.LogInformation("Reembolso procesado exitosamente. ID: {RefundTransactionId}", refundTransaction.Id);

            return new ChargeResponseDto
            {
                TransactionId = refundTransaction.Id,
                AzulTransactionId = refundTransaction.AzulTransactionId,
                Status = refundTransaction.Status.ToString(),
                ResponseCode = refundTransaction.ResponseCode,
                ResponseMessage = refundTransaction.ResponseMessage,
                Amount = refundTransaction.Amount,
                Currency = refundTransaction.Currency,
                TransactionDate = refundTransaction.CreatedAt,
                IsSuccessful = refundTransaction.Status == TransactionStatus.Refunded
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar reembolso para transacción {TransactionId}", request.RefundRequest.TransactionId);
            throw;
        }
    }
}
