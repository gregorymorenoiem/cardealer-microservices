// =====================================================
// DigitalSignatureService - Commands
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

using MediatR;
using DigitalSignatureService.Application.DTOs;
using DigitalSignatureService.Domain.Enums;

namespace DigitalSignatureService.Application.Commands;

// ==================== Certificados ====================
public record CreateCertificateCommand(CreateCertificateDto Data) : IRequest<DigitalCertificateDto>;
public record RevokeCertificateCommand(Guid CertificateId, RevokeCertificateDto Data) : IRequest<bool>;
public record SuspendCertificateCommand(Guid CertificateId, string Reason) : IRequest<bool>;
public record ReactivateCertificateCommand(Guid CertificateId) : IRequest<bool>;
public record RenewCertificateCommand(Guid CertificateId, int ValidityYears) : IRequest<DigitalCertificateDto>;

// ==================== Firmas ====================
public record SignDocumentCommand(SignDocumentDto Data) : IRequest<DigitalSignatureDto>;
public record ValidateSignatureCommand(Guid SignatureId) : IRequest<bool>;
public record BatchSignDocumentsCommand(Guid CertificateId, IEnumerable<SignDocumentDto> Documents) : IRequest<IEnumerable<DigitalSignatureDto>>;

// ==================== Verificación ====================
public record VerifySignatureCommand(VerifySignatureDto Data) : IRequest<VerificationResultDto>;
public record VerifyDocumentSignaturesCommand(Guid DocumentId) : IRequest<IEnumerable<VerificationResultDto>>;

// ==================== TimeStamp ====================
public record AddTimeStampCommand(Guid SignatureId) : IRequest<TimeStampDto>;
