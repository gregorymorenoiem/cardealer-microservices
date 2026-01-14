using FluentValidation;
using StripePaymentService.Application.DTOs;

namespace StripePaymentService.Application.Validators;

/// <summary>
/// Validador para crear suscripción
/// </summary>
public class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionRequestDto>
{
    public CreateSubscriptionValidator()
    {
        RuleFor(x => x.StripeCustomerId)
            .NotEmpty()
            .WithMessage("El ID del cliente es requerido");

        RuleFor(x => x.StripePriceId)
            .NotEmpty()
            .WithMessage("El ID del precio es requerido");

        RuleFor(x => x.TrialDays)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TrialDays.HasValue)
            .WithMessage("Los días de prueba no pueden ser negativos");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.StartDate.HasValue)
            .WithMessage("La fecha de inicio debe ser en el futuro");
    }
}
