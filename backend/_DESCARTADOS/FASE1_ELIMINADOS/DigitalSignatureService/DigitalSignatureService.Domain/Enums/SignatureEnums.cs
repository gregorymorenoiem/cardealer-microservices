// =====================================================
// DigitalSignatureService - Enums
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

namespace DigitalSignatureService.Domain.Enums;

/// <summary>
/// Tipos de certificado según OGTIC
/// </summary>
public enum CertificateType
{
    Personal,       // Persona física
    Corporate,      // Persona jurídica
    Government,     // Entidades gubernamentales
    SSL,            // Certificado SSL/TLS
    CodeSigning     // Firma de código
}

/// <summary>
/// Estados del certificado
/// </summary>
public enum CertificateStatus
{
    Pending,        // Pendiente de emisión
    Active,         // Activo
    Expired,        // Expirado
    Revoked,        // Revocado
    Suspended       // Suspendido temporalmente
}

/// <summary>
/// Algoritmos de firma soportados
/// </summary>
public enum SignatureAlgorithm
{
    SHA256withRSA,      // RSA con SHA-256
    SHA384withRSA,      // RSA con SHA-384
    SHA512withRSA,      // RSA con SHA-512
    SHA256withECDSA,    // ECDSA con SHA-256
    SHA384withECDSA,    // ECDSA con SHA-384
    SHA512withECDSA     // ECDSA con SHA-512
}
