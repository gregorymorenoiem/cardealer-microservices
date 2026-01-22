// =====================================================
// DigitalSignatureService - Validators
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

using FluentValidation;
using DigitalSignatureService.Application.DTOs;

namespace DigitalSignatureService.Application.Validators;

public class CreateCertificateValidator : AbstractValidator<CreateCertificateDto>
{
    public CreateCertificateValidator()
    {
        RuleFor(x => x.SubjectName)
            .NotEmpty().WithMessage("El nombre del titular es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.SubjectIdentification)
            .NotEmpty().WithMessage("La identificación del titular es requerida")
            .Matches(@"^\d{9,11}$").WithMessage("La cédula/RNC debe tener entre 9 y 11 dígitos");

        RuleFor(x => x.CertificateType)
            .IsInEnum().WithMessage("Tipo de certificado no válido");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido");

        RuleFor(x => x.ValidityYears)
            .InclusiveBetween(1, 5).WithMessage("La validez debe ser entre 1 y 5 años (Ley 339-22)");
    }
}

public class SignDocumentValidator : AbstractValidator<SignDocumentDto>
{
    public SignDocumentValidator()
    {
        RuleFor(x => x.CertificateId)
            .NotEmpty().WithMessage("El ID del certificado es requerido");

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("El ID del documento es requerido");

        RuleFor(x => x.DocumentHash)
            .NotEmpty().WithMessage("El hash del documento es requerido")
            .MinimumLength(32).WithMessage("El hash debe tener al menos 32 caracteres");

        RuleFor(x => x.Algorithm)
            .IsInEnum().WithMessage("Algoritmo de firma no válido");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("La dirección IP es requerida para trazabilidad");
    }
}

public class VerifySignatureValidator : AbstractValidator<VerifySignatureDto>
{
    public VerifySignatureValidator()
    {
        RuleFor(x => x.SignatureId)
            .NotEmpty().WithMessage("El ID de la firma es requerido");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("La dirección IP es requerida para auditoría");
    }
}

public class RevokeCertificateValidator : AbstractValidator<RevokeCertificateDto>
{
    public RevokeCertificateValidator()
    {
        RuleFor(x => x.RevocationReason)
            .NotEmpty().WithMessage("La razón de revocación es requerida")
            .MaximumLength(500).WithMessage("La razón no puede exceder 500 caracteres");
    }
}
