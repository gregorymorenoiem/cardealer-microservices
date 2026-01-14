using FluentValidation;
using AzulPaymentService.Application.DTOs;

namespace AzulPaymentService.Application.Validators;

/// <summary>
/// Validador para solicitud de suscripción
/// </summary>
public class SubscriptionRequestValidator : AbstractValidator<SubscriptionRequestDto>
{
    public SubscriptionRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("El ID de usuario es requerido");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto debe ser mayor a 0")
            .LessThanOrEqualTo(9999999.99m)
            .WithMessage("El monto no puede exceder 9,999,999.99");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("La moneda es requerida")
            .Length(3)
            .WithMessage("La moneda debe ser un código ISO de 3 caracteres");

        RuleFor(x => x.Frequency)
            .Must(x => new[] { "Daily", "Weekly", "BiWeekly", "Monthly", "Quarterly", "SemiAnnual", "Annual" }.Contains(x))
            .WithMessage("La frecuencia es inválida");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("La fecha de inicio debe ser hoy o en el futuro");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("La fecha de finalización debe ser posterior a la fecha de inicio");

        // Validar que tenga token o datos de tarjeta
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.CardToken) || 
                      (!string.IsNullOrEmpty(x.CardNumber) && 
                       !string.IsNullOrEmpty(x.CardExpiryMonth) &&
                       !string.IsNullOrEmpty(x.CardExpiryYear) &&
                       !string.IsNullOrEmpty(x.CardCVV)))
            .WithMessage("Debe proporcionar un token de tarjeta o datos completos de la tarjeta");

        RuleFor(x => x.CardNumber)
            .Matches(@"^\d{13,19}$")
            .When(x => !string.IsNullOrEmpty(x.CardNumber))
            .WithMessage("El número de tarjeta debe tener entre 13 y 19 dígitos");

        RuleFor(x => x.CardCVV)
            .Matches(@"^\d{3,4}$")
            .When(x => !string.IsNullOrEmpty(x.CardCVV))
            .WithMessage("El CVV debe tener 3 o 4 dígitos");

        RuleFor(x => x.CardExpiryMonth)
            .Matches(@"^(0[1-9]|1[0-2])$")
            .When(x => !string.IsNullOrEmpty(x.CardExpiryMonth))
            .WithMessage("El mes debe estar entre 01 y 12");

        RuleFor(x => x.CardExpiryYear)
            .Matches(@"^\d{4}$")
            .When(x => !string.IsNullOrEmpty(x.CardExpiryYear))
            .WithMessage("El año debe ser un valor de 4 dígitos");

        RuleFor(x => x.CustomerEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.CustomerEmail))
            .WithMessage("El email debe ser válido");

        RuleFor(x => x.PaymentMethod)
            .Must(x => new[] { "CreditCard", "DebitCard", "ACH", "MobilePayment", "EWallet", "TokenizedCard" }.Contains(x))
            .WithMessage("El método de pago es inválido");

        RuleFor(x => x.PlanName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.PlanName))
            .WithMessage("El nombre del plan no puede exceder 100 caracteres");
    }
}
