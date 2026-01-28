using MediatR;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Features.Transaction.Queries;

/// <summary>
/// Query para obtener una transacción por ID
/// </summary>
public class GetTransactionByIdQuery : IRequest<ChargeResponseDto>
{
    /// <summary>
    /// ID de la transacción
    /// </summary>
    public Guid TransactionId { get; set; }

    public GetTransactionByIdQuery(Guid transactionId)
    {
        TransactionId = transactionId;
    }
}
