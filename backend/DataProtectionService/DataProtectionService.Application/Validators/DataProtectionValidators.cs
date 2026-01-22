using FluentValidation;
using DataProtectionService.Application.Commands;
using DataProtectionService.Domain.Entities;

namespace DataProtectionService.Application.Validators;

public class CreateConsentCommandValidator : AbstractValidator<CreateConsentCommand>
{
    public CreateConsentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId es requerido");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipo de consentimiento es requerido")
            .Must(BeValidConsentType).WithMessage("Tipo de consentimiento inválido");

        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Versión es requerida")
            .MaximumLength(20).WithMessage("Versión no puede exceder 20 caracteres");

        RuleFor(x => x.DocumentHash)
            .NotEmpty().WithMessage("Hash del documento es requerido")
            .MaximumLength(128).WithMessage("Hash no puede exceder 128 caracteres");
    }

    private static bool BeValidConsentType(string type)
    {
        return Enum.TryParse<ConsentType>(type, true, out _);
    }
}

public class RevokeConsentCommandValidator : AbstractValidator<RevokeConsentCommand>
{
    public RevokeConsentCommandValidator()
    {
        RuleFor(x => x.ConsentId)
            .NotEmpty().WithMessage("ConsentId es requerido");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId es requerido");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Razón de revocación es requerida")
            .MaximumLength(500).WithMessage("Razón no puede exceder 500 caracteres");
    }
}

public class CreateARCORequestCommandValidator : AbstractValidator<CreateARCORequestCommand>
{
    public CreateARCORequestCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId es requerido");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipo ARCO es requerido")
            .Must(BeValidARCOType).WithMessage("Tipo ARCO inválido. Debe ser: Access, Rectification, Cancellation, Opposition");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descripción es requerida")
            .MinimumLength(20).WithMessage("Descripción debe tener al menos 20 caracteres")
            .MaximumLength(2000).WithMessage("Descripción no puede exceder 2000 caracteres");

        RuleFor(x => x.SpecificDataRequested)
            .MaximumLength(1000).WithMessage("Datos específicos no puede exceder 1000 caracteres");

        RuleFor(x => x.ProposedChanges)
            .MaximumLength(2000).WithMessage("Cambios propuestos no puede exceder 2000 caracteres");

        RuleFor(x => x.OppositionReason)
            .MaximumLength(2000).WithMessage("Razón de oposición no puede exceder 2000 caracteres");

        // Validación condicional para Rectification
        When(x => x.Type == "Rectification", () =>
        {
            RuleFor(x => x.ProposedChanges)
                .NotEmpty().WithMessage("Para rectificación, debe especificar los cambios propuestos");
        });

        // Validación condicional para Opposition
        When(x => x.Type == "Opposition", () =>
        {
            RuleFor(x => x.OppositionReason)
                .NotEmpty().WithMessage("Para oposición, debe especificar la razón");
        });
    }

    private static bool BeValidARCOType(string type)
    {
        return Enum.TryParse<ARCOType>(type, true, out _);
    }
}

public class ProcessARCORequestCommandValidator : AbstractValidator<ProcessARCORequestCommand>
{
    public ProcessARCORequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("RequestId es requerido");

        RuleFor(x => x.ProcessedById)
            .NotEmpty().WithMessage("ProcessedById es requerido");

        RuleFor(x => x.ProcessedByName)
            .NotEmpty().WithMessage("Nombre del procesador es requerido")
            .MaximumLength(200).WithMessage("Nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Estado es requerido")
            .Must(BeValidARCOStatus).WithMessage("Estado inválido");

        // Si es rechazado, requiere razón
        When(x => x.Status == "Rejected", () =>
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Razón de rechazo es requerida cuando se rechaza una solicitud");
        });

        // Si es completado, requiere resolución
        When(x => x.Status == "Completed", () =>
        {
            RuleFor(x => x.Resolution)
                .NotEmpty().WithMessage("Resolución es requerida cuando se completa una solicitud");
        });
    }

    private static bool BeValidARCOStatus(string status)
    {
        return Enum.TryParse<ARCOStatus>(status, true, out _);
    }
}

public class CreatePrivacyPolicyCommandValidator : AbstractValidator<CreatePrivacyPolicyCommand>
{
    public CreatePrivacyPolicyCommandValidator()
    {
        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Versión es requerida")
            .Matches(@"^\d+\.\d+(\.\d+)?$").WithMessage("Versión debe seguir formato semántico (ej: 1.0, 2.1.3)");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("Tipo de documento es requerido")
            .Must(x => new[] { "PrivacyPolicy", "TermsOfService", "CookiePolicy", "DataProcessingAgreement" }.Contains(x))
            .WithMessage("Tipo de documento inválido");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Contenido es requerido")
            .MinimumLength(500).WithMessage("Contenido debe tener al menos 500 caracteres");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Idioma es requerido")
            .Must(x => new[] { "es", "en" }.Contains(x.ToLower()))
            .WithMessage("Idioma debe ser 'es' o 'en'");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty().WithMessage("Fecha de vigencia es requerida")
            .GreaterThan(DateTime.UtcNow.AddDays(-1)).WithMessage("Fecha de vigencia no puede ser en el pasado");

        RuleFor(x => x.CreatedById)
            .NotEmpty().WithMessage("CreatedById es requerido");
    }
}

public class AnonymizeUserDataCommandValidator : AbstractValidator<AnonymizeUserDataCommand>
{
    public AnonymizeUserDataCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId es requerido");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Razón no puede exceder 500 caracteres");
    }
}
