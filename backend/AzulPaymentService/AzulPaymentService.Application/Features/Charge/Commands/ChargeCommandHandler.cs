using MediatR;
using Microsoft.Extensions.Logging;
using AzulPaymentService.Application.DTOs;
using AzulPaymentService.Domain.Interfaces;
using AzulPaymentService.Domain.Entities;
using AzulPaymentService.Domain.Enums;

namespace AzulPaymentService.Application.Features.Charge.Commands;

/// <summary>
/// Handler para procesar un cobro
/// </summary>
public class ChargeCommandHandler : IRequestHandler<ChargeCommand, ChargeResponseDto>
{
    private readonly IAzulTransactionRepository _transactionRepository;
    private readonly ILogger<ChargeCommandHandler> _logger;

    public ChargeCommandHandler(
        IAzulTransactionRepository transactionRepository,
        ILogger<ChargeCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Procesa el cobro
    /// </summary>
    public async Task<ChargeResponseDto> Handle(ChargeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Procesando cobro para usuario {UserId}", request.ChargeRequest.UserId);

        try
        {
            // Crear entidad de transacción
            var transaction = new AzulTransaction
            {
                Id = Guid.NewGuid(),
                UserId = request.ChargeRequest.UserId,
                Amount = request.ChargeRequest.Amount,
                Currency = request.ChargeRequest.Currency,
                Status = TransactionStatus.Pending,
                PaymentMethod = Enum.Parse<PaymentMethod>(request.ChargeRequest.PaymentMethod),
                TransactionType = request.ChargeRequest.TransactionType,
                CustomerEmail = request.ChargeRequest.CustomerEmail,
                CustomerPhone = request.ChargeRequest.CustomerPhone,
                CustomerIpAddress = request.ChargeRequest.CustomerIpAddress,
                InvoiceReference = request.ChargeRequest.InvoiceReference,
                IsRecurring = request.ChargeRequest.IsRecurring,
                SubscriptionId = request.ChargeRequest.SubscriptionId,
                CreatedAt = DateTime.UtcNow
            };

            // En este punto, se llamaría a AZUL API
            // TODO: Implementar integración con AZUL en Infrastructure layer
            // var azulResponse = await _azulClient.ProcessChargeAsync(request.ChargeRequest);
            
            // Por ahora, simular respuesta exitosa
            transaction.Status = TransactionStatus.Approved;
            transaction.AzulTransactionId = Guid.NewGuid().ToString();
            transaction.AuthorizationCode = GenerateAuthCode();
            transaction.ResponseCode = "00";
            transaction.ResponseMessage = "Aprobado";
            transaction.CompletedAt = DateTime.UtcNow;

            // Guardar transacción
            await _transactionRepository.CreateAsync(transaction, cancellationToken);

            _logger.LogInformation("Cobro procesado exitosamente. ID: {TransactionId}", transaction.Id);

            return new ChargeResponseDto
            {
                TransactionId = transaction.Id,
                AzulTransactionId = transaction.AzulTransactionId,
                Status = transaction.Status.ToString(),
                ResponseCode = transaction.ResponseCode,
                ResponseMessage = transaction.ResponseMessage,
                AuthorizationCode = transaction.AuthorizationCode,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                TransactionDate = transaction.CreatedAt,
                IsSuccessful = transaction.Status == TransactionStatus.Approved
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar cobro para usuario {UserId}", request.ChargeRequest.UserId);
            throw;
        }
    }

    private static string GenerateAuthCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}
