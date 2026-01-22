// ContractService - Domain Entities
// Ley 126-02 de Comercio Electrónico y Firma Digital de República Dominicana
// Gestión de contratos electrónicos con validez legal

namespace ContractService.Domain.Entities;

using System;
using System.Collections.Generic;

#region Enums

/// <summary>
/// Tipo de contrato electrónico
/// </summary>
public enum ContractType
{
    SaleAgreement = 1,           // Contrato de compraventa
    LeaseAgreement = 2,          // Contrato de arrendamiento
    ServiceAgreement = 3,        // Contrato de servicios
    FinancingAgreement = 4,      // Contrato de financiamiento
    ConsignmentAgreement = 5,    // Contrato de consignación
    TradeInAgreement = 6,        // Contrato de trade-in
    WarrantyAgreement = 7,       // Contrato de garantía
    InsuranceAgreement = 8,      // Contrato de seguro
    MaintenanceAgreement = 9,    // Contrato de mantenimiento
    NonDisclosure = 10,          // Acuerdo de confidencialidad
    TermsOfService = 11,         // Términos de servicio
    PrivacyPolicy = 12,          // Política de privacidad
    DealerAgreement = 13         // Acuerdo con dealers
}

/// <summary>
/// Estado del contrato
/// </summary>
public enum ContractStatus
{
    Draft = 1,                   // Borrador
    PendingSignatures = 2,       // Pendiente de firmas
    PartiallySigned = 3,         // Firmado parcialmente
    FullySigned = 4,             // Completamente firmado
    Active = 5,                  // Activo/En vigencia
    Expired = 6,                 // Expirado
    Terminated = 7,              // Terminado anticipadamente
    Cancelled = 8,               // Cancelado
    Disputed = 9,                // En disputa
    Renewed = 10,                // Renovado
    Archived = 11                // Archivado
}

/// <summary>
/// Tipo de parte del contrato
/// </summary>
public enum PartyType
{
    Individual = 1,              // Persona física
    Company = 2,                 // Empresa/Persona jurídica
    Dealer = 3,                  // Dealer verificado
    Platform = 4,                // La plataforma (OKLA)
    Guarantor = 5,               // Garante
    Witness = 6,                 // Testigo
    Agent = 7                    // Representante/Agente
}

/// <summary>
/// Rol en el contrato
/// </summary>
public enum PartyRole
{
    Seller = 1,                  // Vendedor
    Buyer = 2,                   // Comprador
    Lessor = 3,                  // Arrendador
    Lessee = 4,                  // Arrendatario
    ServiceProvider = 5,         // Proveedor de servicios
    Client = 6,                  // Cliente
    Financier = 7,               // Financiador
    Borrower = 8,                // Prestatario
    Guarantor = 9,               // Garante
    Witness = 10                 // Testigo
}

/// <summary>
/// Estado de firma
/// </summary>
public enum SignatureStatus
{
    Pending = 1,                 // Pendiente
    Requested = 2,               // Solicitud enviada
    Viewed = 3,                  // Documento visto
    Signed = 4,                  // Firmado
    Declined = 5,                // Rechazado
    Expired = 6                  // Expirado (tiempo límite alcanzado)
}

/// <summary>
/// Tipo de firma electrónica (per Ley 126-02)
/// </summary>
public enum SignatureType
{
    Simple = 1,                  // Firma electrónica simple
    Advanced = 2,                // Firma electrónica avanzada
    Qualified = 3,               // Firma digital certificada
    Biometric = 4                // Firma biométrica
}

/// <summary>
/// Estado de verificación de firma
/// </summary>
public enum SignatureVerificationStatus
{
    NotVerified = 1,
    Verified = 2,
    Failed = 3,
    Revoked = 4
}

/// <summary>
/// Tipo de cláusula
/// </summary>
public enum ClauseType
{
    Standard = 1,                // Cláusula estándar
    Mandatory = 2,               // Cláusula obligatoria (legal)
    Optional = 3,                // Cláusula opcional
    Negotiable = 4,              // Cláusula negociable
    LegalNotice = 5,             // Aviso legal requerido
    ArbitrationClause = 6,       // Cláusula de arbitraje
    JurisdictionClause = 7,      // Cláusula de jurisdicción
    ConfidentialityClause = 8,   // Cláusula de confidencialidad
    PenaltyClause = 9,           // Cláusula penal
    TerminationClause = 10       // Cláusula de terminación
}

/// <summary>
/// Estado de la versión del contrato
/// </summary>
public enum VersionStatus
{
    Draft = 1,
    Review = 2,
    Approved = 3,
    Current = 4,
    Superseded = 5,
    Archived = 6
}

/// <summary>
/// Tipo de evento de auditoría
/// </summary>
public enum ContractAuditEventType
{
    Created = 1,
    Updated = 2,
    SignatureRequested = 3,
    DocumentViewed = 4,
    Signed = 5,
    SignatureDeclined = 6,
    StatusChanged = 7,
    Terminated = 8,
    Renewed = 9,
    Archived = 10,
    Downloaded = 11,
    Shared = 12,
    Disputed = 13,
    ClauseModified = 14
}

#endregion

#region Entities

/// <summary>
/// Plantilla de contrato reutilizable
/// </summary>
public class ContractTemplate
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ContractType Type { get; set; }
    public string ContentHtml { get; set; } = string.Empty;     // HTML del template
    public string? ContentJson { get; set; }                     // Estructura JSON para llenado
    public string? CssStyles { get; set; }                       // Estilos CSS
    public List<string> RequiredVariables { get; set; } = new(); // Variables a llenar
    public List<string> OptionalVariables { get; set; } = new();
    public string Language { get; set; } = "es-DO";
    public string? LegalBasis { get; set; }                      // Base legal (Ley 126-02, etc.)
    public bool RequiresWitness { get; set; } = false;
    public int MinimumSignatures { get; set; } = 2;
    public bool RequiresNotarization { get; set; } = false;
    public int? ValidityDays { get; set; }                       // Días de validez por defecto
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navegación
    public ICollection<TemplateClause> Clauses { get; set; } = new List<TemplateClause>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}

/// <summary>
/// Cláusula de plantilla
/// </summary>
public class TemplateClause
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public ClauseType Type { get; set; }
    public int Order { get; set; }
    public bool IsMandatory { get; set; } = true;
    public bool IsEditable { get; set; } = false;
    public string? LegalReference { get; set; }                  // Artículo de ley
    public string? HelpText { get; set; }
    
    // Navegación
    public ContractTemplate? Template { get; set; }
}

/// <summary>
/// Contrato electrónico - Entidad principal
/// </summary>
public class Contract
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;   // Número único de contrato
    public Guid? TemplateId { get; set; }
    public ContractType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Contenido del contrato
    public string ContentHtml { get; set; } = string.Empty;      // HTML generado
    public string? ContentPdf { get; set; }                       // Base64 del PDF firmado
    public string? ContentHash { get; set; }                      // Hash SHA-256 del contenido
    public string? DigitalSeal { get; set; }                      // Sello digital/timestamp
    
    // Fechas
    public DateTime EffectiveDate { get; set; }                   // Fecha de entrada en vigor
    public DateTime? ExpirationDate { get; set; }
    public DateTime? SignedAt { get; set; }                       // Fecha de firma completa
    public DateTime? TerminatedAt { get; set; }
    
    // Estado
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public string? TerminationReason { get; set; }
    
    // Objeto del contrato (puede ser vehículo, servicio, etc.)
    public string? SubjectType { get; set; }                      // "Vehicle", "Service", etc.
    public Guid? SubjectId { get; set; }
    public string? SubjectDescription { get; set; }
    public decimal? ContractValue { get; set; }                   // Valor del contrato
    public string Currency { get; set; } = "DOP";                 // Peso Dominicano por defecto
    
    // Compliance
    public string? LegalJurisdiction { get; set; }               // Ej: "República Dominicana"
    public string? ArbitrationClause { get; set; }
    public bool AcceptedTerms { get; set; } = false;
    public bool AcceptedPrivacyPolicy { get; set; } = false;
    public DateTime? LegalReviewedAt { get; set; }
    public string? LegalReviewedBy { get; set; }
    
    // Metadata
    public string? Tags { get; set; }                            // Etiquetas JSON
    public string? CustomFields { get; set; }                    // Campos personalizados JSON
    public string? Notes { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navegación
    public ContractTemplate? Template { get; set; }
    public ICollection<ContractParty> Parties { get; set; } = new List<ContractParty>();
    public ICollection<ContractSignature> Signatures { get; set; } = new List<ContractSignature>();
    public ICollection<ContractClause> Clauses { get; set; } = new List<ContractClause>();
    public ICollection<ContractVersion> Versions { get; set; } = new List<ContractVersion>();
    public ICollection<ContractDocument> Documents { get; set; } = new List<ContractDocument>();
    public ICollection<ContractAuditLog> AuditLogs { get; set; } = new List<ContractAuditLog>();
}

/// <summary>
/// Parte del contrato (persona o entidad)
/// </summary>
public class ContractParty
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public PartyType Type { get; set; }
    public PartyRole Role { get; set; }
    
    // Identificación
    public Guid? UserId { get; set; }                            // User ID en el sistema
    public string? ExternalId { get; set; }                      // ID externo si no es usuario
    public string FullName { get; set; } = string.Empty;
    public string? DocumentType { get; set; }                    // Cédula, RNC, Pasaporte
    public string? DocumentNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    // Dirección
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "DO";
    
    // Para empresas
    public string? CompanyName { get; set; }
    public string? RNC { get; set; }                             // Registro Nacional de Contribuyentes
    public string? RepresentativeName { get; set; }
    public string? RepresentativeTitle { get; set; }
    public string? LegalRepresentative { get; set; }             // Representante legal
    public string? PowerOfAttorneyNumber { get; set; }           // Número de poder de representación
    
    // Estado
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public bool HasSigned { get; set; } = false;
    public DateTime? SignedAt { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    
    // Navegación
    public Contract? Contract { get; set; }
    public ICollection<ContractSignature> Signatures { get; set; } = new List<ContractSignature>();
}

/// <summary>
/// Firma electrónica del contrato (per Ley 126-02)
/// </summary>
public class ContractSignature
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Guid PartyId { get; set; }
    public Guid? CertificationAuthorityId { get; set; }          // FK a autoridad certificadora
    
    // Tipo y estado de firma
    public SignatureType Type { get; set; } = SignatureType.Advanced;
    public SignatureStatus Status { get; set; } = SignatureStatus.Pending;
    public SignatureVerificationStatus VerificationStatus { get; set; } = SignatureVerificationStatus.NotVerified;
    
    // Datos de la firma
    public string? SignatureData { get; set; }                   // Datos de firma (base64)
    public string? SignatureImage { get; set; }                  // Imagen de firma si aplica
    public string? CertificateData { get; set; }                 // Certificado digital
    public string? CertificateIssuer { get; set; }               // Entidad certificadora
    public string? CertificateSerialNumber { get; set; }
    public DateTime? CertificateValidFrom { get; set; }
    public DateTime? CertificateValidTo { get; set; }
    
    // Hash y verificación
    public string? DocumentHash { get; set; }                    // Hash del documento al firmar
    public string? SignatureHash { get; set; }                   // Hash de la firma
    public string? TimestampToken { get; set; }                  // Token de timestamp (TSA)
    public DateTime? TimestampDate { get; set; }
    public string? VerificationDetails { get; set; }             // Detalles de verificación
    
    // Metadata de firma
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? GeoLocation { get; set; }                     // Lat/Long
    public string? DeviceFingerprint { get; set; }
    
    // Fechas
    public DateTime? RequestedAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? SignedAt { get; set; }
    public DateTime? DeclinedAt { get; set; }
    public string? DeclineReason { get; set; }
    public DateTime? ExpiresAt { get; set; }                     // Tiempo límite para firmar
    
    // Verificación biométrica (si aplica)
    public bool BiometricVerified { get; set; } = false;
    public string? BiometricType { get; set; }                   // "face", "fingerprint"
    public string? BiometricScore { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    
    // Navegación
    public Contract? Contract { get; set; }
    public ContractParty? Party { get; set; }
    public CertificationAuthority? CertificationAuthority { get; set; }
}

/// <summary>
/// Cláusula específica del contrato
/// </summary>
public class ContractClause
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public Guid? TemplateClauseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;         // Contenido (puede ser modificado)
    public string? OriginalContent { get; set; }                // Contenido original del template
    public ClauseType Type { get; set; }
    public int Order { get; set; }
    public bool WasModified { get; set; } = false;
    public string? ModificationReason { get; set; }
    public string? ModifiedBy { get; set; }                     // Quién modificó
    public bool IsAccepted { get; set; } = false;
    public DateTime? AcceptedAt { get; set; }
    public string? AcceptedBy { get; set; }
    
    // Navegación
    public Contract? Contract { get; set; }
    public TemplateClause? TemplateClause { get; set; }
}

/// <summary>
/// Versión del contrato para tracking de cambios
/// </summary>
public class ContractVersion
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public int VersionNumber { get; set; }
    public string ContentHtml { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public VersionStatus Status { get; set; }
    public string? ChangeDescription { get; set; }
    public string? ChangedBy { get; set; }
    public string? CreatedBy { get; set; }                       // Quién creó esta versión
    public DateTime CreatedAt { get; set; }
    
    // Navegación
    public Contract? Contract { get; set; }
}

/// <summary>
/// Documento adjunto al contrato
/// </summary>
public class ContractDocument
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DocumentType { get; set; } = string.Empty;     // "id", "vehicle_title", "insurance", etc.
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;      // Path en S3/storage
    public string? FileHash { get; set; }                        // SHA-256
    public bool IsRequired { get; set; } = false;
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    
    // Auditoría
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    
    // Navegación
    public Contract? Contract { get; set; }
}

/// <summary>
/// Log de auditoría del contrato (per Ley 126-02)
/// </summary>
public class ContractAuditLog
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public ContractAuditEventType EventType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }                        // JSON
    public string? NewValue { get; set; }                        // JSON
    public string PerformedBy { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime PerformedAt { get; set; }
    
    // Navegación
    public Contract? Contract { get; set; }
}

/// <summary>
/// Configuración de entidad certificadora (per Ley 126-02)
/// </summary>
public class CertificationAuthority
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Country { get; set; }                         // País de la entidad
    public string? Website { get; set; }                         // Sitio web
    public string? WebsiteUrl { get; set; }                      // URL alternativa
    public string? CertificateUrl { get; set; }                  // URL del certificado
    public string? PublicKey { get; set; }                       // Clave pública
    public string? AccreditationNumber { get; set; }             // Número de acreditación
    public string? ContactEmail { get; set; }                    // Email de contacto
    public string? SupportPhone { get; set; }                    // Teléfono de soporte
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; }                          // Encriptado
    public bool IsActive { get; set; } = true;
    public bool IsGovernmentApproved { get; set; } = false;      // Aprobada por INDOTEL
    public string? CertificateChain { get; set; }                // Cadena de certificados
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime? ValidUntil { get; set; }                    // Alias para ValidTo
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

#endregion
