namespace DealerManagementService.Domain.Entities;

public class DealerDocument
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Dealer Dealer { get; set; } = null!;
    
    public DocumentType Type { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? FileKey { get; set; } // S3 key
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    
    // Verification
    public DocumentVerificationStatus VerificationStatus { get; set; } = DocumentVerificationStatus.Pending;
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; } // Admin user ID
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    
    // Expiry (for licenses, permits, etc.)
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    
    // Timestamps
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public enum DocumentType
{
    // Required Documents
    RNC = 0,                        // Registro Nacional de Contribuyentes
    BusinessLicense = 1,            // Licencia Comercial
    IdentificationCard = 2,         // Cédula del dueño
    ProofOfAddress = 3,             // Comprobante de domicilio
    
    // Optional Documents
    BankStatement = 4,              // Estado de cuenta
    TaxCertificate = 5,             // Certificación tributaria
    IncorporationDocs = 6,          // Acta constitutiva
    PowerOfAttorney = 7,            // Poder notarial (if applicable)
    PreviousSalesRecords = 8,       // Récords de ventas anteriores
    InsurancePolicy = 9,            // Póliza de seguro
    
    // Additional
    Other = 99
}

public enum DocumentVerificationStatus
{
    Pending = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    Expired = 4
}
