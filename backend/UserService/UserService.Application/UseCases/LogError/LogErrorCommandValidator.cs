using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.LogError;

public class LogErrorCommandValidator : AbstractValidator<LogErrorCommand>
{
    public LogErrorCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Request body is required");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.ServiceName)
                .NotEmpty().WithMessage("Service name is required")
                .MaximumLength(100)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Request.ExceptionType)
                .NotEmpty().WithMessage("Exception type is required")
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Request.Message)
                .NotEmpty().WithMessage("Error message is required")
                .MaximumLength(4000)
                .NoSqlInjection()
                .NoXss();

            RuleFor(x => x.Request.StackTrace!)
                .MaximumLength(8000)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.StackTrace));

            RuleFor(x => x.Request.Endpoint!)
                .MaximumLength(500)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Endpoint));

            RuleFor(x => x.Request.HttpMethod!)
                .MaximumLength(10)
                .NoSqlInjection()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.HttpMethod));

            RuleFor(x => x.Request.UserId!)
                .MaximumLength(100)
                .NoSqlInjection()
                .NoXss()
                .When(x => !string.IsNullOrWhiteSpace(x.Request.UserId));
        });
    }
}
