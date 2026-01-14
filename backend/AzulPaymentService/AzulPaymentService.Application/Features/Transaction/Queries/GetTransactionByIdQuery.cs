using MediatR;
using AzulPaymentService.Application.DTOs;

namespace AzulPaymentService.Application.Features.Transaction.Queries;

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
