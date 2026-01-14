using FluentValidation;
using AzulPaymentService.Application.DTOs;

namespace AzulPaymentService.Application.Validators;

/// <summary>
/// Validador para solicitud de reembolso
/// </summary>
public class RefundRequestValidator : AbstractValidator<RefundRequestDto>
{
    public RefundRequestValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("El ID de transacción es requerido");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("La razón del reembolso es requerida")
            .MaximumLength(255)
            .WithMessage("La razón no puede exceder 255 caracteres");

        RuleFor(x => x.PartialAmount)
            .GreaterThan(0)
            .When(x => x.PartialAmount.HasValue)
            .WithMessage("El monto parcial debe ser mayor a 0");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Las notas no pueden exceder 500 caracteres");
    }
}
