using FluentValidation;
using AdminService.Application.UseCases.Reports.ResolveReport;

namespace AdminService.Application.Validators.Reports;

/// <summary>
/// Validator for ResolveReportCommand.
/// Validates all string fields with NoSqlInjection/NoXss.
/// </summary>
public class ResolveReportCommandValidator : AbstractValidator<ResolveReportCommand>
{
    public ResolveReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty().WithMessage("Report ID is required.");

        RuleFor(x => x.ResolvedBy)
            .NotEmpty().WithMessage("ResolvedBy is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Resolution)
            .NotEmpty().WithMessage("Resolution is required.")
            .MaximumLength(5000)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.ReporterEmail)
            .NotEmpty().WithMessage("Reporter email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(254)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.ReportSubject)
            .NotEmpty().WithMessage("Report subject is required.")
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss();
    }
}
