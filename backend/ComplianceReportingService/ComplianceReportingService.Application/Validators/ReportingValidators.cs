// =====================================================
// ComplianceReportingService - Validators
// Reportes Consolidados de Cumplimiento RD
// =====================================================

using FluentValidation;
using ComplianceReportingService.Application.DTOs;

namespace ComplianceReportingService.Application.Validators;

public class GenerateReportValidator : AbstractValidator<GenerateReportDto>
{
    public GenerateReportValidator()
    {
        RuleFor(x => x.ReportType)
            .IsInEnum().WithMessage("Tipo de reporte inválido");

        RuleFor(x => x.RegulatoryBody)
            .IsInEnum().WithMessage("Organismo regulador inválido");

        RuleFor(x => x.Period)
            .NotEmpty().WithMessage("El período es requerido")
            .Matches(@"^\d{6}$").WithMessage("Formato de período inválido (YYYYMM)");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("La fecha de inicio es requerida");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("La fecha de fin es requerida")
            .GreaterThan(x => x.StartDate).WithMessage("La fecha de fin debe ser posterior a la de inicio");
    }
}

public class SubmitReportValidator : AbstractValidator<SubmitReportDto>
{
    public SubmitReportValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty().WithMessage("El ID del reporte es requerido");

        RuleFor(x => x.SubmissionMethod)
            .NotEmpty().WithMessage("El método de envío es requerido")
            .Must(m => new[] { "API", "Web", "Email", "Manual" }.Contains(m))
            .WithMessage("Método de envío inválido");
    }
}

public class CreateScheduleValidator : AbstractValidator<CreateScheduleDto>
{
    public CreateScheduleValidator()
    {
        RuleFor(x => x.ReportType)
            .IsInEnum().WithMessage("Tipo de reporte inválido");

        RuleFor(x => x.RegulatoryBody)
            .IsInEnum().WithMessage("Organismo regulador inválido");

        RuleFor(x => x.CronExpression)
            .NotEmpty().WithMessage("La expresión cron es requerida")
            .Matches(@"^(\S+\s){4,5}\S+$").WithMessage("Formato de expresión cron inválido");
    }
}

public class CreateTemplateValidator : AbstractValidator<CreateTemplateDto>
{
    public CreateTemplateValidator()
    {
        RuleFor(x => x.ReportType)
            .IsInEnum().WithMessage("Tipo de reporte inválido");

        RuleFor(x => x.RegulatoryBody)
            .IsInEnum().WithMessage("Organismo regulador inválido");

        RuleFor(x => x.TemplateName)
            .NotEmpty().WithMessage("El nombre de la plantilla es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.TemplateVersion)
            .NotEmpty().WithMessage("La versión es requerida")
            .Matches(@"^\d+\.\d+(\.\d+)?$").WithMessage("Formato de versión inválido (ej: 1.0 o 1.0.0)");

        RuleFor(x => x.TemplateContent)
            .NotEmpty().WithMessage("El contenido de la plantilla es requerido");

        RuleFor(x => x.ValidFrom)
            .NotEmpty().WithMessage("La fecha de validez es requerida");
    }
}
