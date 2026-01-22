using FluentValidation;
using SpyneIntegrationService.Application.Features.Webhooks.Commands;

namespace SpyneIntegrationService.Application.Validators;

public class ProcessSpyneWebhookCommandValidator : AbstractValidator<ProcessSpyneWebhookCommand>
{
    public ProcessSpyneWebhookCommandValidator()
    {
        RuleFor(x => x.EventType)
            .NotEmpty()
            .WithMessage("EventType is required");

        RuleFor(x => x.JobId)
            .NotEmpty()
            .WithMessage("JobId is required");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required");

        RuleFor(x => x.RawPayload)
            .NotEmpty()
            .WithMessage("RawPayload is required");
    }
}
