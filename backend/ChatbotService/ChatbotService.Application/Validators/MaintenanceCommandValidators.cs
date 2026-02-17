using FluentValidation;
using ChatbotService.Application.Features.Maintenance.Commands;

namespace ChatbotService.Application.Validators;

/// <summary>
/// Validator for CreateOrUpdateConfigurationCommand — validates all string inputs
/// against SQL injection and XSS patterns.
/// </summary>
public class CreateOrUpdateConfigurationCommandValidator : AbstractValidator<CreateOrUpdateConfigurationCommand>
{
    public CreateOrUpdateConfigurationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200)
            .NoSecurityThreats();

        RuleFor(x => x.LlmServerUrl)
            .NotEmpty().WithMessage("LlmServerUrl is required")
            .MaximumLength(500)
            .NoSecurityThreats();

        RuleFor(x => x.LlmModelId)
            .NotEmpty().WithMessage("LlmModelId is required")
            .MaximumLength(200)
            .NoSecurityThreats();

        RuleFor(x => x.LlmLanguageCode)
            .NotEmpty().WithMessage("LlmLanguageCode is required")
            .MaximumLength(10)
            .NoSecurityThreats();

        When(x => !string.IsNullOrWhiteSpace(x.LlmSystemPrompt), () =>
        {
            RuleFor(x => x.LlmSystemPrompt!)
                .MaximumLength(5000)
                .NoSqlInjection();
        });

        RuleFor(x => x.BotName)
            .NotEmpty().WithMessage("BotName is required")
            .MaximumLength(100)
            .NoSecurityThreats();

        When(x => !string.IsNullOrWhiteSpace(x.BotAvatarUrl), () =>
        {
            RuleFor(x => x.BotAvatarUrl!)
                .MaximumLength(2048)
                .NoSecurityThreats();
        });

        RuleFor(x => x.WelcomeMessage)
            .NotEmpty().WithMessage("WelcomeMessage is required")
            .MaximumLength(1000)
            .NoSqlInjection()
            .NoXssAdvanced();

        When(x => !string.IsNullOrWhiteSpace(x.QuickRepliesJson), () =>
        {
            RuleFor(x => x.QuickRepliesJson!)
                .MaximumLength(10000)
                .NoSqlInjection();
        });

        RuleFor(x => x.MaxInteractionsPerSession)
            .GreaterThan(0).WithMessage("MaxInteractionsPerSession must be positive")
            .LessThanOrEqualTo(100);

        RuleFor(x => x.MaxInteractionsPerUserPerDay)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000);

        RuleFor(x => x.MaxInteractionsPerUserPerMonth)
            .GreaterThan(0)
            .LessThanOrEqualTo(50000);

        RuleFor(x => x.MaxGlobalInteractionsPerDay)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000000);

        RuleFor(x => x.MaxGlobalInteractionsPerMonth)
            .GreaterThan(0)
            .LessThanOrEqualTo(10000000);

        RuleFor(x => x.InventorySyncIntervalMinutes)
            .GreaterThanOrEqualTo(5)
            .LessThanOrEqualTo(1440);
    }
}

/// <summary>
/// Validator for CreateQuickResponseCommand.
/// </summary>
public class CreateQuickResponseCommandValidator : AbstractValidator<CreateQuickResponseCommand>
{
    public CreateQuickResponseCommandValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100)
            .NoSecurityThreats();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200)
            .NoSecurityThreats();

        RuleFor(x => x.Triggers)
            .NotEmpty().WithMessage("At least one trigger is required");

        RuleForEach(x => x.Triggers)
            .NotEmpty()
            .MaximumLength(200)
            .NoSecurityThreats();

        RuleFor(x => x.Response)
            .NotEmpty().WithMessage("Response is required")
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXssAdvanced();

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1000);
    }
}
