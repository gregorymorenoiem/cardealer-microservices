using FluentValidation;
using VehicleIntelligenceService.Application.DTOs;

namespace VehicleIntelligenceService.Application.Validators;

public class PriceSuggestionRequestValidator : AbstractValidator<PriceSuggestionRequestDto>
{
    public PriceSuggestionRequestValidator()
    {
        RuleFor(x => x.Make).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Year).InclusiveBetween(1970, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Mileage).InclusiveBetween(0, 2_000_000);
        RuleFor(x => x.AskingPrice).GreaterThan(0).When(x => x.AskingPrice.HasValue);
    }
}
