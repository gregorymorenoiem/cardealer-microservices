using MediatR;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Features.Refund.Commands;

/// <summary>
/// Comando para procesar un reembolso
/// </summary>
public class RefundCommand : IRequest<ChargeResponseDto>
{
    /// <summary>
    /// DTO con los datos del reembolso
    /// </summary>
    public RefundRequestDto RefundRequest { get; set; } = null!;

    public RefundCommand(RefundRequestDto refundRequest)
    {
        RefundRequest = refundRequest;
    }
}
