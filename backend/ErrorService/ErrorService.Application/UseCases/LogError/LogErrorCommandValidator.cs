using FluentValidation;
using ErrorService.Application.DTOs;

namespace ErrorService.Application.UseCases.LogError
{
    public class LogErrorCommandValidator : AbstractValidator<LogErrorRequest>
    {
        public LogErrorCommandValidator()
        {
            RuleFor(x => x.ServiceName)
                .NotEmpty().WithMessage("Service name is required")
                .MaximumLength(100).WithMessage("Service name must not exceed 100 characters");

            RuleFor(x => x.ExceptionType)
                .NotEmpty().WithMessage("Exception type is required")
                .MaximumLength(200).WithMessage("Exception type must not exceed 200 characters");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Error message is required");

            RuleFor(x => x.Endpoint)
                .MaximumLength(500).WithMessage("Endpoint must not exceed 500 characters");

            RuleFor(x => x.HttpMethod)
                .MaximumLength(10).WithMessage("HTTP method must not exceed 10 characters");

            RuleFor(x => x.UserId)
                .MaximumLength(100).WithMessage("User ID must not exceed 100 characters");
        }
    }
}