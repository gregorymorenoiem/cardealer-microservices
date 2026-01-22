// =====================================================
// DigitalSignatureService - DTOs
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

using DigitalSignatureService.Domain.Enums;

namespace DigitalSignatureService.Application.DTOs;

// ==================== Certificados ====================
public record DigitalCertificateDto(
    Guid Id,
    string SerialNumber,
    string SubjectName,
    string SubjectIdentification,
    string IssuerName,
    DateTime IssuedAt,
    DateTime ExpiresAt,
    CertificateType CertificateType,
    CertificateStatus Status,
    bool IsExpiringSoon,
    DateTime CreatedAt
);

public record CreateCertificateDto(
    string SubjectName,
    string SubjectIdentification,
    CertificateType CertificateType,
    Guid UserId,
    Guid? OrganizationId,
    int ValidityYears = 2
);

public record RevokeCertificateDto(
    string RevocationReason
);

// ==================== Firmas ====================
public record DigitalSignatureDto(
    Guid Id,
    Guid CertificateId,
    Guid DocumentId,
    string DocumentHash,
    SignatureAlgorithm SignatureAlgorithm,
    DateTime SignedAt,
    string SignerName,
    string SignerIdentification,
    bool IsValid,
    string? ValidationMessage,
    DateTime? ValidatedAt
);

public record SignDocumentDto(
    Guid CertificateId,
    Guid DocumentId,
    string DocumentHash,
    SignatureAlgorithm Algorithm,
    string IpAddress
);

// ==================== Verificación ====================
public record VerificationResultDto(
    bool IsValid,
    string Message,
    DateTime VerifiedAt,
    string? SignerName,
    string? SignerIdentification,
    DateTime? SignedAt
);

public record VerifySignatureDto(
    Guid SignatureId,
    string IpAddress
);

// ==================== TimeStamp ====================
public record TimeStampDto(
    Guid Id,
    Guid SignatureId,
    DateTime Timestamp,
    string TsaName,
    bool IsValid
);

// ==================== Statistics ====================
public record SignatureStatisticsDto(
    int TotalCertificates,
    int ActiveCertificates,
    int ExpiringCertificates,
    int TotalSignatures,
    int ValidSignatures,
    int InvalidSignatures
);
