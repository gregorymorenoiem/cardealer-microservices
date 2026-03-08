using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Privacy.RequestAccountDeletion;

public class RequestAccountDeletionCommandValidator : AbstractValidator<RequestAccountDeletionCommand>
{
    public RequestAccountDeletionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Email!)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(254)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.OtherReason!)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.OtherReason));

        RuleFor(x => x.Feedback!)
            .MaximumLength(1000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Feedback));

        RuleFor(x => x.IpAddress!)
            .MaximumLength(45)
            .NoSqlInjection()
            .When(x => !string.IsNullOrWhiteSpace(x.IpAddress));

        RuleFor(x => x.UserAgent!)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.UserAgent));
    }
}
