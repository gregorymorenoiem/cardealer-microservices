// ComplianceReportingService - Validators
// Validación de comandos con FluentValidation

namespace ComplianceReportingService.Application.Validators;

using FluentValidation;
using ComplianceReportingService.Application.Features.Commands;

#region Report Validators

public class GenerateReportValidator : AbstractValidator<GenerateReportCommand>
{
    public GenerateReportValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Tipo de reporte inválido");

        RuleFor(x => x.PeriodStart)
            .NotEmpty()
            .WithMessage("Fecha de inicio del período es requerida")
            .LessThan(x => x.PeriodEnd)
            .WithMessage("La fecha de inicio debe ser anterior a la fecha fin");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty()
            .WithMessage("Fecha fin del período es requerida")
            .LessThanOrEqualTo(DateTime.UtcNow.AddMonths(1))
            .WithMessage("No se pueden generar reportes para períodos muy futuros");

        RuleFor(x => x.Format)
            .IsInEnum()
            .WithMessage("Formato de reporte inválido");

        RuleFor(x => x.Destination)
            .IsInEnum()
            .WithMessage("Destino de reporte inválido");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");
    }
}

public class SubmitReportValidator : AbstractValidator<SubmitReportCommand>
{
    public SubmitReportValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty()
            .WithMessage("ID del reporte es requerido");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");
    }
}

public class CancelReportValidator : AbstractValidator<CancelReportCommand>
{
    public CancelReportValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty()
            .WithMessage("ID del reporte es requerido");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Razón de cancelación es requerida")
            .MaximumLength(500)
            .WithMessage("La razón no debe exceder 500 caracteres");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");
    }
}

#endregion

#region Schedule Validators

public class CreateScheduleValidator : AbstractValidator<CreateScheduleCommand>
{
    public CreateScheduleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nombre de la programación es requerido")
            .MaximumLength(200)
            .WithMessage("El nombre no debe exceder 200 caracteres");

        RuleFor(x => x.ReportType)
            .IsInEnum()
            .WithMessage("Tipo de reporte inválido");

        RuleFor(x => x.Frequency)
            .IsInEnum()
            .WithMessage("Frecuencia inválida");

        RuleFor(x => x.Format)
            .IsInEnum()
            .WithMessage("Formato inválido");

        RuleFor(x => x.Destination)
            .IsInEnum()
            .WithMessage("Destino inválido");

        RuleFor(x => x.CronExpression)
            .NotEmpty()
            .WithMessage("Expresión cron es requerida")
            .MaximumLength(100)
            .WithMessage("La expresión cron no debe exceder 100 caracteres");

        RuleFor(x => x.NotificationEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.NotificationEmail))
            .WithMessage("Email de notificación inválido");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");
    }
}

public class ToggleScheduleValidator : AbstractValidator<ToggleScheduleCommand>
{
    public ToggleScheduleValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty()
            .WithMessage("ID de programación es requerido");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");
    }
}

#endregion

#region Template Validators

public class CreateTemplateValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Código de plantilla es requerido")
            .MaximumLength(50)
            .WithMessage("El código no debe exceder 50 caracteres")
            .Matches("^[A-Z0-9_]+$")
            .WithMessage("El código solo puede contener letras mayúsculas, números y guiones bajos");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nombre de plantilla es requerido")
            .MaximumLength(200)
            .WithMessage("El nombre no debe exceder 200 caracteres");

        RuleFor(x => x.ForReportType)
            .IsInEnum()
            .WithMessage("Tipo de reporte inválido");

        RuleFor(x => x.TemplateContent)
            .NotEmpty()
            .WithMessage("Contenido de plantilla es requerido")
            .MaximumLength(50000)
            .WithMessage("El contenido no debe exceder 50000 caracteres");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");
    }
}

#endregion

#region Subscription Validators

public class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionCommand>
{
    private readonly string[] _validDeliveryMethods = { "Email", "SMS", "Dashboard", "API" };

    public CreateSubscriptionValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");

        RuleFor(x => x.ReportType)
            .IsInEnum()
            .WithMessage("Tipo de reporte inválido");

        RuleFor(x => x.Frequency)
            .IsInEnum()
            .WithMessage("Frecuencia inválida");

        RuleFor(x => x.DeliveryMethod)
            .NotEmpty()
            .WithMessage("Método de entrega es requerido")
            .Must(m => _validDeliveryMethods.Contains(m))
            .WithMessage("Método de entrega debe ser: Email, SMS, Dashboard o API");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty()
            .WithMessage("Dirección de entrega es requerida")
            .EmailAddress()
            .When(x => x.DeliveryMethod == "Email")
            .WithMessage("Email de entrega inválido")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => x.DeliveryMethod == "SMS")
            .WithMessage("Número de teléfono inválido (formato E.164)");
    }
}

#endregion

#region DGII Validators

public class GenerateDGII606Validator : AbstractValidator<GenerateDGII606Command>
{
    public GenerateDGII606Validator()
    {
        RuleFor(x => x.PeriodStart)
            .NotEmpty()
            .WithMessage("Fecha de inicio del período es requerida")
            .Must(d => d.Day == 1)
            .WithMessage("Formato 606 debe iniciar el primer día del mes");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty()
            .WithMessage("Fecha fin del período es requerida")
            .GreaterThan(x => x.PeriodStart)
            .WithMessage("La fecha fin debe ser posterior a la fecha inicio");

        RuleFor(x => x.RNC)
            .NotEmpty()
            .WithMessage("RNC es requerido")
            .Matches(@"^\d{9,11}$")
            .WithMessage("RNC debe tener entre 9 y 11 dígitos");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");
    }
}

public class GenerateDGII607Validator : AbstractValidator<GenerateDGII607Command>
{
    public GenerateDGII607Validator()
    {
        RuleFor(x => x.PeriodStart)
            .NotEmpty()
            .WithMessage("Fecha de inicio del período es requerida")
            .Must(d => d.Day == 1)
            .WithMessage("Formato 607 debe iniciar el primer día del mes");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty()
            .WithMessage("Fecha fin del período es requerida")
            .GreaterThan(x => x.PeriodStart)
            .WithMessage("La fecha fin debe ser posterior a la fecha inicio");

        RuleFor(x => x.RNC)
            .NotEmpty()
            .WithMessage("RNC es requerido")
            .Matches(@"^\d{9,11}$")
            .WithMessage("RNC debe tener entre 9 y 11 dígitos");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");
    }
}

#endregion

#region UAF Validators

public class GenerateROSValidator : AbstractValidator<GenerateROSCommand>
{
    public GenerateROSValidator()
    {
        RuleFor(x => x.SubjectName)
            .NotEmpty()
            .WithMessage("Nombre del sujeto es requerido - Art. 33 Ley 155-17")
            .MaximumLength(200)
            .WithMessage("El nombre no debe exceder 200 caracteres");

        RuleFor(x => x.SubjectIdType)
            .NotEmpty()
            .WithMessage("Tipo de documento del sujeto es requerido")
            .Must(t => new[] { "Cedula", "Pasaporte", "RNC", "Otro" }.Contains(t))
            .WithMessage("Tipo de documento inválido");

        RuleFor(x => x.SubjectIdNumber)
            .NotEmpty()
            .WithMessage("Número de documento del sujeto es requerido");

        RuleFor(x => x.TransactionType)
            .NotEmpty()
            .WithMessage("Tipo de transacción es requerido")
            .MaximumLength(100);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Monto debe ser mayor a cero");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Moneda es requerida")
            .Must(c => new[] { "DOP", "USD", "EUR" }.Contains(c))
            .WithMessage("Moneda debe ser DOP, USD o EUR");

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .WithMessage("Fecha de transacción es requerida")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Fecha de transacción no puede ser futura");

        RuleFor(x => x.SuspicionIndicators)
            .NotEmpty()
            .WithMessage("Indicadores de sospecha son requeridos - Art. 33 Ley 155-17")
            .MaximumLength(2000)
            .WithMessage("Los indicadores no deben exceder 2000 caracteres");

        RuleFor(x => x.Narrative)
            .NotEmpty()
            .WithMessage("Narrativa del caso es requerida - Art. 33 Ley 155-17")
            .MinimumLength(100)
            .WithMessage("La narrativa debe tener al menos 100 caracteres")
            .MaximumLength(10000)
            .WithMessage("La narrativa no debe exceder 10000 caracteres");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");
    }
}

public class GenerateCTRValidator : AbstractValidator<GenerateCTRCommand>
{
    public GenerateCTRValidator()
    {
        RuleFor(x => x.PeriodStart)
            .NotEmpty()
            .WithMessage("Fecha de inicio es requerida");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty()
            .WithMessage("Fecha fin es requerida")
            .GreaterThan(x => x.PeriodStart)
            .WithMessage("La fecha fin debe ser posterior a la fecha inicio");

        RuleFor(x => x.ThresholdAmount)
            .GreaterThan(0)
            .WithMessage("Monto umbral debe ser mayor a cero")
            .LessThanOrEqualTo(1000000000)
            .WithMessage("Monto umbral no puede exceder RD$1,000,000,000");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");
    }
}

#endregion

#region Compliance Validators

public class RecordMetricValidator : AbstractValidator<RecordMetricCommand>
{
    public RecordMetricValidator()
    {
        RuleFor(x => x.MetricCode)
            .NotEmpty()
            .WithMessage("Código de métrica es requerido")
            .MaximumLength(50)
            .WithMessage("El código no debe exceder 50 caracteres")
            .Matches("^[A-Z0-9_]+$")
            .WithMessage("El código solo puede contener letras mayúsculas, números y guiones bajos");

        RuleFor(x => x.MetricName)
            .NotEmpty()
            .WithMessage("Nombre de métrica es requerido")
            .MaximumLength(200)
            .WithMessage("El nombre no debe exceder 200 caracteres");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Categoría es requerida")
            .MaximumLength(100)
            .WithMessage("La categoría no debe exceder 100 caracteres");

        RuleFor(x => x.Value)
            .NotNull()
            .WithMessage("Valor es requerido");

        RuleFor(x => x.Threshold)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Threshold.HasValue)
            .WithMessage("Umbral debe ser mayor o igual a cero");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID de usuario es requerido");
    }
}

#endregion
