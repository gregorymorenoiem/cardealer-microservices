using FluentValidation;
using SpyneIntegrationService.Application.Features.Chat.Commands;

namespace SpyneIntegrationService.Application.Validators;

/// <summary>
/// Validator for StartChatSessionCommand
/// 
/// ⚠️ FASE 4: Validador implementado pero el endpoint NO se consume en frontend
/// </summary>
public class StartChatSessionCommandValidator : AbstractValidator<StartChatSessionCommand>
{
    public StartChatSessionCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required");

        RuleFor(x => x.Language)
            .NotEmpty()
            .WithMessage("Language is required")
            .Must(lang => new[] { "es", "en" }.Contains(lang.ToLower()))
            .WithMessage("Language must be 'es' or 'en'");

        When(x => x.VehicleContext != null, () =>
        {
            RuleFor(x => x.VehicleContext!.Make)
                .NotEmpty()
                .When(x => x.VehicleContext != null)
                .WithMessage("Vehicle make is required when providing context");

            RuleFor(x => x.VehicleContext!.Model)
                .NotEmpty()
                .When(x => x.VehicleContext != null)
                .WithMessage("Vehicle model is required when providing context");
        });
    }
}
