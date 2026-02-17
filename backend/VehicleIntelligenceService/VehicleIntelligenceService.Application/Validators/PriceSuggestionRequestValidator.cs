using FluentValidation;
using VehicleIntelligenceService.Application.DTOs;

namespace VehicleIntelligenceService.Application.Validators;

public class PriceSuggestionRequestValidator : AbstractValidator<PriceSuggestionRequestDto>
{
    public PriceSuggestionRequestValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("La marca es requerida")
            .MaximumLength(50).WithMessage("La marca no puede exceder 50 caracteres");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("El modelo es requerido")
            .MaximumLength(100).WithMessage("El modelo no puede exceder 100 caracteres");

        RuleFor(x => x.Year)
            .InclusiveBetween(1990, DateTime.Now.Year + 1)
            .WithMessage($"El año debe estar entre 1990 y {DateTime.Now.Year + 1}");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo")
            .LessThanOrEqualTo(500000).WithMessage("El kilometraje no puede exceder 500,000 km");

        RuleFor(x => x.AskingPrice)
            .GreaterThan(0).When(x => x.AskingPrice.HasValue)
            .WithMessage("El precio debe ser mayor a 0");
    }
}
