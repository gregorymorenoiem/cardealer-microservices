using FluentValidation;
using AdminService.Application.UseCases.Messages;

namespace AdminService.Application.Validators.Messages;

/// <summary>
/// Validator for GetAdminMessagesQuery.
/// Validates Search, Status, Priority filters with NoSqlInjection/NoXss.
/// </summary>
public class GetAdminMessagesQueryValidator : AbstractValidator<GetAdminMessagesQuery>
{
    public GetAdminMessagesQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Search));

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Priority)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Priority));
    }
}
