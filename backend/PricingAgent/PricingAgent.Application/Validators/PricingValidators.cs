using FluentValidation;
using PricingAgent.Application.Features.Pricing.Queries;

namespace PricingAgent.Application.Validators;

public sealed class AnalyzeVehiclePriceQueryValidator : AbstractValidator<AnalyzeVehiclePriceQuery>
{
    private static readonly HashSet<string> ValidConditions = new(StringComparer.OrdinalIgnoreCase)
    {
        "new", "used", "certified", "nuevo", "usado", "certificado"
    };

    public AnalyzeVehiclePriceQueryValidator()
    {
        RuleFor(x => x.Request.Make)
            .NotEmpty().WithMessage("La marca es requerida.")
            .MaximumLength(50);

        RuleFor(x => x.Request.Model)
            .NotEmpty().WithMessage("El modelo es requerido.")
            .MaximumLength(100);

        RuleFor(x => x.Request.Year)
            .InclusiveBetween(1990, DateTime.UtcNow.Year + 1)
            .WithMessage("El año debe estar entre 1990 y el próximo año.");

        RuleFor(x => x.Request.Mileage)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Request.Mileage.HasValue)
            .WithMessage("El kilometraje no puede ser negativo.");

        RuleFor(x => x.Request.AskingPrice)
            .GreaterThan(0)
            .When(x => x.Request.AskingPrice.HasValue)
            .WithMessage("El precio debe ser mayor a 0.");

        RuleFor(x => x.Request.Condition)
            .Must(c => c is null || ValidConditions.Contains(c))
            .WithMessage("Condición inválida. Use: new, used, certified.");
    }
}
