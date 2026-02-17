using FluentValidation;
using StripePaymentService.Application.DTOs;

namespace StripePaymentService.Application.Validators;

/// <summary>
/// Validador para crear Payment Intent
/// </summary>
public class CreatePaymentIntentValidator : AbstractValidator<CreatePaymentIntentRequestDto>
{
    public CreatePaymentIntentValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto debe ser mayor a 0")
            .LessThanOrEqualTo(99999999)
            .WithMessage("El monto excede el límite");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("La moneda es requerida")
            .Length(3)
            .WithMessage("La moneda debe ser un código ISO de 3 caracteres");

        RuleFor(x => x.CustomerEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.CustomerEmail))
            .WithMessage("El email debe ser válido");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La descripción no puede exceder 1000 caracteres");
    }
}
