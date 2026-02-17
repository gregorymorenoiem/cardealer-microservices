// =====================================================
// DigitalSignatureService - Entities
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

namespace DigitalSignatureService.Domain.Entities;

/// <summary>
/// Certificado digital X.509 según OGTIC
/// </summary>
public class DigitalCertificate
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectIdentification { get; set; } = string.Empty;
    public string IssuerName { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Enums.CertificateType CertificateType { get; set; }
    public Enums.CertificateStatus Status { get; set; }
    public string PublicKey { get; set; } = string.Empty;
    public string? PrivateKeyReference { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrganizationId { get; set; }
    public string? RevocationReason { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<DigitalSignature> Signatures { get; set; } = new List<DigitalSignature>();
}

/// <summary>
/// Firma digital aplicada a documento
/// </summary>
public class DigitalSignature
{
    public Guid Id { get; set; }
    public Guid CertificateId { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentHash { get; set; } = string.Empty;
    public string SignatureValue { get; set; } = string.Empty;
    public Enums.SignatureAlgorithm SignatureAlgorithm { get; set; }
    public DateTime SignedAt { get; set; }
    public string SignerName { get; set; } = string.Empty;
    public string SignerIdentification { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? ValidationMessage { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public DigitalCertificate? Certificate { get; set; }
}

/// <summary>
/// Verificación de firma por terceros
/// </summary>
public class SignatureVerification
{
    public Guid Id { get; set; }
    public Guid SignatureId { get; set; }
    public Guid VerifiedBy { get; set; }
    public bool IsValid { get; set; }
    public string VerificationDetails { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    
    public DigitalSignature? Signature { get; set; }
}

/// <summary>
/// Sello de tiempo certificado (TimeStamp)
/// </summary>
public class TimeStamp
{
    public Guid Id { get; set; }
    public Guid SignatureId { get; set; }
    public string TimeStampToken { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string TsaName { get; set; } = string.Empty; // Time Stamp Authority
    public bool IsValid { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public DigitalSignature? Signature { get; set; }
}
