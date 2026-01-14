using MediatR;
using AzulPaymentService.Application.DTOs;

namespace AzulPaymentService.Application.Features.Charge.Commands;

/// <summary>
/// Comando para procesar un cobro/pago
/// </summary>
public class ChargeCommand : IRequest<ChargeResponseDto>
{
    /// <summary>
    /// DTO con los datos del cobro
    /// </summary>
    public ChargeRequestDto ChargeRequest { get; set; } = null!;

    public ChargeCommand(ChargeRequestDto chargeRequest)
    {
        ChargeRequest = chargeRequest;
    }
}
