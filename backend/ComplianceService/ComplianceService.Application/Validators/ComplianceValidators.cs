// ComplianceService - Validators

namespace ComplianceService.Application.Validators;

using FluentValidation;
using ComplianceService.Application.Commands;
using ComplianceService.Domain.Interfaces;

#region Framework Validators

public class CreateFrameworkValidator : AbstractValidator<CreateFrameworkCommand>
{
    public CreateFrameworkValidator(IRegulatoryFrameworkRepository repository)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres")
            .MustAsync(async (code, ct) => !await repository.CodeExistsAsync(code, ct))
            .WithMessage("El código ya existe");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de regulación inválido");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty().WithMessage("La fecha efectiva es requerida");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(x => x.EffectiveDate)
            .When(x => x.ExpirationDate.HasValue)
            .WithMessage("La fecha de expiración debe ser posterior a la fecha efectiva");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El creador es requerido");
    }
}

public class UpdateFrameworkValidator : AbstractValidator<UpdateFrameworkCommand>
{
    public UpdateFrameworkValidator(IRegulatoryFrameworkRepository repository)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID es requerido")
            .MustAsync(async (id, ct) => await repository.ExistsAsync(id, ct))
            .WithMessage("El marco regulatorio no existe");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("El actualizador es requerido");
    }
}

#endregion

#region Requirement Validators

public class CreateRequirementValidator : AbstractValidator<CreateRequirementCommand>
{
    public CreateRequirementValidator(IRegulatoryFrameworkRepository frameworkRepository)
    {
        RuleFor(x => x.FrameworkId)
            .NotEmpty().WithMessage("El marco regulatorio es requerido")
            .MustAsync(async (id, ct) => await frameworkRepository.ExistsAsync(id, ct))
            .WithMessage("El marco regulatorio no existe");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(300).WithMessage("El título no puede exceder 300 caracteres");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es requerida");

        RuleFor(x => x.Criticality)
            .IsInEnum().WithMessage("Nivel de criticidad inválido");

        RuleFor(x => x.DeadlineDays)
            .GreaterThan(0).WithMessage("Los días de plazo deben ser mayor a 0");

        RuleFor(x => x.EvaluationFrequency)
            .IsInEnum().WithMessage("Frecuencia de evaluación inválida");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El creador es requerido");
    }
}

#endregion

#region Control Validators

public class CreateControlValidator : AbstractValidator<CreateControlCommand>
{
    public CreateControlValidator(IRegulatoryFrameworkRepository frameworkRepository)
    {
        RuleFor(x => x.FrameworkId)
            .NotEmpty().WithMessage("El marco regulatorio es requerido")
            .MustAsync(async (id, ct) => await frameworkRepository.ExistsAsync(id, ct))
            .WithMessage("El marco regulatorio no existe");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es requerida");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de control inválido");

        RuleFor(x => x.TestingFrequency)
            .IsInEnum().WithMessage("Frecuencia de prueba inválida");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El creador es requerido");
    }
}

public class CreateControlTestValidator : AbstractValidator<CreateControlTestCommand>
{
    public CreateControlTestValidator(IComplianceControlRepository controlRepository)
    {
        RuleFor(x => x.ControlId)
            .NotEmpty().WithMessage("El control es requerido")
            .MustAsync(async (id, ct) => 
            {
                var control = await controlRepository.GetByIdAsync(id, ct);
                return control != null;
            })
            .WithMessage("El control no existe");

        RuleFor(x => x.TestProcedure)
            .NotEmpty().WithMessage("El procedimiento de prueba es requerido");

        RuleFor(x => x.EffectivenessScore)
            .InclusiveBetween(0, 100)
            .When(x => x.EffectivenessScore.HasValue)
            .WithMessage("El puntaje de efectividad debe estar entre 0 y 100");

        RuleFor(x => x.TestedBy)
            .NotEmpty().WithMessage("El evaluador es requerido");
    }
}

#endregion

#region Assessment Validators

public class CreateAssessmentValidator : AbstractValidator<CreateAssessmentCommand>
{
    public CreateAssessmentValidator(IComplianceRequirementRepository requirementRepository)
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("El tipo de entidad es requerido")
            .MaximumLength(100).WithMessage("El tipo de entidad no puede exceder 100 caracteres");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("El ID de entidad es requerido");

        RuleFor(x => x.RequirementId)
            .NotEmpty().WithMessage("El requerimiento es requerido")
            .MustAsync(async (id, ct) =>
            {
                var req = await requirementRepository.GetByIdAsync(id, ct);
                return req != null;
            })
            .WithMessage("El requerimiento no existe");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Estado inválido");

        RuleFor(x => x.Score)
            .InclusiveBetween(0, 100)
            .When(x => x.Score.HasValue)
            .WithMessage("El puntaje debe estar entre 0 y 100");

        RuleFor(x => x.AssessedBy)
            .NotEmpty().WithMessage("El evaluador es requerido");
    }
}

#endregion

#region Finding Validators

public class CreateFindingValidator : AbstractValidator<CreateFindingCommand>
{
    public CreateFindingValidator(IComplianceAssessmentRepository assessmentRepository)
    {
        RuleFor(x => x.AssessmentId)
            .NotEmpty().WithMessage("La evaluación es requerida")
            .MustAsync(async (id, ct) =>
            {
                var assessment = await assessmentRepository.GetByIdAsync(id, ct);
                return assessment != null;
            })
            .WithMessage("La evaluación no existe");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(300).WithMessage("El título no puede exceder 300 caracteres");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es requerida");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de hallazgo inválido");

        RuleFor(x => x.Criticality)
            .IsInEnum().WithMessage("Nivel de criticidad inválido");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.DueDate.HasValue)
            .WithMessage("La fecha límite debe ser futura");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El creador es requerido");
    }
}

public class ResolveFindingValidator : AbstractValidator<ResolveFindingCommand>
{
    public ResolveFindingValidator(IComplianceFindingRepository findingRepository)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID es requerido")
            .MustAsync(async (id, ct) =>
            {
                var finding = await findingRepository.GetByIdAsync(id, ct);
                return finding != null;
            })
            .WithMessage("El hallazgo no existe");

        RuleFor(x => x.Resolution)
            .NotEmpty().WithMessage("La resolución es requerida")
            .MinimumLength(20).WithMessage("La resolución debe tener al menos 20 caracteres");

        RuleFor(x => x.ResolvedBy)
            .NotEmpty().WithMessage("El resolutor es requerido");
    }
}

#endregion

#region Report Validators

public class CreateReportValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportValidator()
    {
        RuleFor(x => x.ReportType)
            .IsInEnum().WithMessage("Tipo de reporte inválido");

        RuleFor(x => x.RegulationType)
            .IsInEnum().WithMessage("Tipo de regulación inválido");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(300).WithMessage("El título no puede exceder 300 caracteres");

        RuleFor(x => x.PeriodStart)
            .NotEmpty().WithMessage("La fecha de inicio del período es requerida");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty().WithMessage("La fecha de fin del período es requerida")
            .GreaterThan(x => x.PeriodStart)
            .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio");

        RuleFor(x => x.SubmissionDeadline)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.SubmissionDeadline.HasValue)
            .WithMessage("La fecha límite de envío debe ser futura");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El creador es requerido");
    }
}

public class SubmitReportValidator : AbstractValidator<SubmitReportCommand>
{
    public SubmitReportValidator(IRegulatoryReportRepository reportRepository)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID es requerido")
            .MustAsync(async (id, ct) =>
            {
                var report = await reportRepository.GetByIdAsync(id, ct);
                return report != null;
            })
            .WithMessage("El reporte no existe");

        RuleFor(x => x.SubmittedBy)
            .NotEmpty().WithMessage("El remitente es requerido");
    }
}

#endregion

#region Training Validators

public class CreateTrainingValidator : AbstractValidator<CreateTrainingCommand>
{
    public CreateTrainingValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(300).WithMessage("El título no puede exceder 300 caracteres");

        RuleFor(x => x.RegulationType)
            .IsInEnum().WithMessage("Tipo de regulación inválido");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("La duración debe ser mayor a 0 minutos");

        RuleFor(x => x.PassingScore)
            .InclusiveBetween(0, 100)
            .When(x => x.PassingScore.HasValue)
            .WithMessage("El puntaje mínimo debe estar entre 0 y 100");

        RuleFor(x => x.ContentUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.ContentUrl))
            .WithMessage("URL de contenido inválida");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El creador es requerido");
    }
}

public class RecordTrainingCompletionValidator : AbstractValidator<RecordTrainingCompletionCommand>
{
    public RecordTrainingCompletionValidator(IComplianceTrainingRepository trainingRepository)
    {
        RuleFor(x => x.TrainingId)
            .NotEmpty().WithMessage("El entrenamiento es requerido")
            .MustAsync(async (id, ct) =>
            {
                var training = await trainingRepository.GetByIdAsync(id, ct);
                return training != null;
            })
            .WithMessage("El entrenamiento no existe");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El usuario es requerido");

        RuleFor(x => x.Score)
            .InclusiveBetween(0, 100)
            .When(x => x.Score.HasValue)
            .WithMessage("El puntaje debe estar entre 0 y 100");
    }
}

#endregion

#region Calendar Validators

public class CreateCalendarItemValidator : AbstractValidator<CreateCalendarItemCommand>
{
    public CreateCalendarItemValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es requerido")
            .MaximumLength(300).WithMessage("El título no puede exceder 300 caracteres");

        RuleFor(x => x.RegulationType)
            .IsInEnum().WithMessage("Tipo de regulación inválido");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("La fecha límite es requerida")
            .GreaterThan(DateTime.UtcNow).WithMessage("La fecha límite debe ser futura");

        RuleFor(x => x.ReminderDaysBefore)
            .GreaterThanOrEqualTo(0).WithMessage("Los días de recordatorio deben ser 0 o más")
            .LessThanOrEqualTo(365).WithMessage("Los días de recordatorio no pueden exceder 365");

        RuleFor(x => x.RecurrencePattern)
            .IsInEnum()
            .When(x => x.IsRecurring && x.RecurrencePattern.HasValue)
            .WithMessage("Patrón de recurrencia inválido");

        RuleFor(x => x.RecurrencePattern)
            .NotNull()
            .When(x => x.IsRecurring)
            .WithMessage("El patrón de recurrencia es requerido cuando es recurrente");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("El creador es requerido");
    }
}

#endregion
