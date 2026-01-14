using MediatR;
using Serilog;
using AzulPaymentService.Application.DTOs;
using AzulPaymentService.Domain.Interfaces;

namespace AzulPaymentService.Application.Features.Transaction.Queries;

/// <summary>
/// Handler para obtener una transacción
/// </summary>
public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, ChargeResponseDto>
{
    private readonly IAzulTransactionRepository _transactionRepository;
    private readonly ILogger<GetTransactionByIdQueryHandler> _logger;

    public GetTransactionByIdQueryHandler(
        IAzulTransactionRepository transactionRepository,
        ILogger<GetTransactionByIdQueryHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la transacción
    /// </summary>
    public async Task<ChargeResponseDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.Information("Obteniendo transacción {TransactionId}", request.TransactionId);

        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);

            if (transaction == null)
            {
                throw new InvalidOperationException($"Transacción no encontrada: {request.TransactionId}");
            }

            return new ChargeResponseDto
            {
                TransactionId = transaction.Id,
                AzulTransactionId = transaction.AzulTransactionId,
                Status = transaction.Status.ToString(),
                ResponseCode = transaction.ResponseCode,
                ResponseMessage = transaction.ResponseMessage,
                AuthorizationCode = transaction.AuthorizationCode,
                CardLast4 = ExtractLast4Digits(transaction.AzulTransactionId),
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                TransactionDate = transaction.CreatedAt,
                IsSuccessful = transaction.Status.ToString() == "Approved"
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al obtener transacción {TransactionId}", request.TransactionId);
            throw;
        }
    }

    private static string? ExtractLast4Digits(string azulTransactionId)
    {
        return azulTransactionId.Length >= 4 ? azulTransactionId.Substring(azulTransactionId.Length - 4) : null;
    }
}
