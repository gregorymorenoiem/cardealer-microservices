// DisputeService - Validators
// Pro-Consumidor RD + Ley 126-02

namespace DisputeService.Application.Validators;

using FluentValidation;
using DisputeService.Application.Commands;

public class FileDisputeValidator : AbstractValidator<FileDisputeCommand>
{
    public FileDisputeValidator()
    {
        RuleFor(x => x.ComplainantId).NotEmpty().WithMessage("El ID del reclamante es requerido");
        RuleFor(x => x.ComplainantName).NotEmpty().MaximumLength(200).WithMessage("El nombre del reclamante es requerido");
        RuleFor(x => x.ComplainantEmail).NotEmpty().EmailAddress().WithMessage("Email del reclamante inválido");
        RuleFor(x => x.RespondentId).NotEmpty().WithMessage("El ID del demandado es requerido");
        RuleFor(x => x.RespondentName).NotEmpty().MaximumLength(200).WithMessage("El nombre del demandado es requerido");
        RuleFor(x => x.RespondentEmail).NotEmpty().EmailAddress().WithMessage("Email del demandado inválido");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500).WithMessage("El título es requerido (máx. 500 caracteres)");
        RuleFor(x => x.Description).NotEmpty().MinimumLength(50).WithMessage("La descripción debe tener al menos 50 caracteres");
        RuleFor(x => x.DisputedAmount).GreaterThan(0).When(x => x.DisputedAmount.HasValue).WithMessage("El monto debe ser mayor a 0");
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3).WithMessage("La moneda es requerida (ej: DOP, USD)");
        RuleFor(x => x.Type).IsInEnum().WithMessage("Tipo de disputa inválido");
    }
}

public class AssignMediatorValidator : AbstractValidator<AssignMediatorCommand>
{
    public AssignMediatorValidator()
    {
        RuleFor(x => x.DisputeId).NotEmpty().WithMessage("El ID de la disputa es requerido");
        RuleFor(x => x.MediatorId).NotEmpty().WithMessage("El ID del mediador es requerido");
        RuleFor(x => x.MediatorName).NotEmpty().MaximumLength(200).WithMessage("El nombre del mediador es requerido");
    }
}

public class ResolveDisputeValidator : AbstractValidator<ResolveDisputeCommand>
{
    public ResolveDisputeValidator()
    {
        RuleFor(x => x.DisputeId).NotEmpty().WithMessage("El ID de la disputa es requerido");
        RuleFor(x => x.Resolution).IsInEnum().WithMessage("Tipo de resolución inválido");
        RuleFor(x => x.ResolutionSummary).NotEmpty().MinimumLength(20).WithMessage("El resumen de la resolución debe tener al menos 20 caracteres");
        RuleFor(x => x.ResolvedBy).NotEmpty().WithMessage("El nombre de quien resuelve es requerido");
    }
}

public class EscalateDisputeValidator : AbstractValidator<EscalateDisputeCommand>
{
    public EscalateDisputeValidator()
    {
        RuleFor(x => x.DisputeId).NotEmpty().WithMessage("El ID de la disputa es requerido");
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(10).WithMessage("La razón de escalamiento debe tener al menos 10 caracteres");
    }
}

public class ReferToProConsumidorValidator : AbstractValidator<ReferToProConsumidorCommand>
{
    public ReferToProConsumidorValidator()
    {
        RuleFor(x => x.DisputeId).NotEmpty().WithMessage("El ID de la disputa es requerido");
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(20).WithMessage("La razón de referencia a Pro-Consumidor debe tener al menos 20 caracteres");
    }
}

public class SubmitEvidenceValidator : AbstractValidator<SubmitEvidenceCommand>
{
    public SubmitEvidenceValidator()
    {
        RuleFor(x => x.DisputeId).NotEmpty().WithMessage("El ID de la disputa es requerido");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("El nombre de la evidencia es requerido");
        RuleFor(x => x.EvidenceType).NotEmpty().WithMessage("El tipo de evidencia es requerido");
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(500).WithMessage("El nombre del archivo es requerido");
        RuleFor(x => x.ContentType).NotEmpty().WithMessage("El tipo de contenido es requerido");
        RuleFor(x => x.FileSize).GreaterThan(0).LessThan(52428800).WithMessage("El tamaño del archivo debe ser mayor a 0 y menor a 50MB");
        RuleFor(x => x.StoragePath).NotEmpty().WithMessage("La ruta de almacenamiento es requerida");
        RuleFor(x => x.SubmittedById).NotEmpty().WithMessage("El ID del que presenta la evidencia es requerido");
        RuleFor(x => x.SubmittedByName).NotEmpty().WithMessage("El nombre del que presenta la evidencia es requerido");
        RuleFor(x => x.SubmitterRole).IsInEnum().WithMessage("Rol del presentante inválido");
    }
}

public class ReviewEvidenceValidator : AbstractValidator<ReviewEvidenceCommand>
{
    public ReviewEvidenceValidator()
    {
        RuleFor(x => x.EvidenceId).NotEmpty().WithMessage("El ID de la evidencia es requerido");
        RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Estado de evidencia inválido");
        RuleFor(x => x.ReviewedBy).NotEmpty().WithMessage("El nombre del revisor es requerido");
    }
}

public class AddCommentValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentValidator()
    {
        RuleFor(x => x.DisputeId).NotEmpty().WithMessage("El ID de la disputa es requerido");
        RuleFor(x => x.AuthorId).NotEmpty().WithMessage("El ID del autor es requerido");
        RuleFor(x => x.AuthorName).NotEmpty().MaximumLength(200).WithMessage("El nombre del autor es requerido");
        RuleFor(x => x.AuthorRole).IsInEnum().WithMessage("Rol del autor inválido");
        RuleFor(x => x.Content).NotEmpty().MinimumLength(5).MaximumLength(5000).WithMessage("El contenido debe tener entre 5 y 5000 caracteres");
    }
}

public class ScheduleMediationValidator : AbstractValidator<ScheduleMediationCommand>
{
    public ScheduleMediationValidator()
    {
        RuleFor(x => x.DisputeId).NotEmpty().WithMessage("El ID de la disputa es requerido");
        RuleFor(x => x.ScheduledAt).GreaterThan(DateTime.UtcNow).WithMessage("La fecha programada debe ser en el futuro");
        RuleFor(x => x.DurationMinutes).InclusiveBetween(15, 480).WithMessage("La duración debe ser entre 15 y 480 minutos");
        RuleFor(x => x.Channel).IsInEnum().WithMessage("Canal de comunicación inválido");
        RuleFor(x => x.MediatorId).NotEmpty().WithMessage("El ID del mediador es requerido");
        RuleFor(x => x.MediatorName).NotEmpty().MaximumLength(200).WithMessage("El nombre del mediador es requerido");
        
        RuleFor(x => x.MeetingLink)
            .NotEmpty()
            .When(x => x.Channel == Domain.Entities.CommunicationChannel.VideoCall)
            .WithMessage("El enlace de reunión es requerido para videollamadas");
    }
}

public class CompleteMediationValidator : AbstractValidator<CompleteMediationCommand>
{
    public CompleteMediationValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("El ID de la sesión es requerido");
        RuleFor(x => x.Summary).NotEmpty().MinimumLength(20).WithMessage("El resumen de la mediación debe tener al menos 20 caracteres");
    }
}

public class AddParticipantValidator : AbstractValidator<AddParticipantCommand>
{
    public AddParticipantValidator()
    {
        RuleFor(x => x.DisputeId).NotEmpty().WithMessage("El ID de la disputa es requerido");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El ID del usuario es requerido");
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(200).WithMessage("El nombre del usuario es requerido");
        RuleFor(x => x.UserEmail).NotEmpty().EmailAddress().WithMessage("Email del usuario inválido");
        RuleFor(x => x.Role).IsInEnum().WithMessage("Rol del participante inválido");
    }
}

public class CreateResolutionTemplateValidator : AbstractValidator<CreateResolutionTemplateCommand>
{
    public CreateResolutionTemplateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("El nombre de la plantilla es requerido");
        RuleFor(x => x.ForDisputeType).IsInEnum().WithMessage("Tipo de disputa inválido");
        RuleFor(x => x.ResolutionType).IsInEnum().WithMessage("Tipo de resolución inválido");
        RuleFor(x => x.TemplateContent).NotEmpty().MinimumLength(50).WithMessage("El contenido de la plantilla debe tener al menos 50 caracteres");
        RuleFor(x => x.CreatedBy).NotEmpty().WithMessage("El creador es requerido");
    }
}

public class CreateSlaConfigurationValidator : AbstractValidator<CreateSlaConfigurationCommand>
{
    public CreateSlaConfigurationValidator()
    {
        RuleFor(x => x.DisputeType).IsInEnum().WithMessage("Tipo de disputa inválido");
        RuleFor(x => x.Priority).IsInEnum().WithMessage("Prioridad inválida");
        RuleFor(x => x.ResponseDeadlineHours).InclusiveBetween(1, 720).WithMessage("El plazo de respuesta debe ser entre 1 y 720 horas");
        RuleFor(x => x.ResolutionDeadlineHours).InclusiveBetween(24, 2160).WithMessage("El plazo de resolución debe ser entre 24 y 2160 horas (90 días)");
        RuleFor(x => x.EscalationThresholdHours).InclusiveBetween(24, 720).WithMessage("El umbral de escalamiento debe ser entre 24 y 720 horas");
    }
}
