using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

/// <summary>
/// Estado de verificación del vendedor individual
/// </summary>
public enum SellerVerificationStatus
{
    NotSubmitted = 0,       // No ha enviado documentos
    PendingReview = 1,      // Documentos enviados, pendiente revisión
    InReview = 2,           // En proceso de revisión
    Verified = 3,           // Verificado y aprobado
    Rejected = 4,           // Rechazado
    Expired = 5,            // Verificación expirada (necesita renovar)
    Suspended = 6           // Suspendido por violación de políticas
}

/// <summary>
/// Perfil de vendedor para usuarios individuales que quieren vender vehículos
/// </summary>
public class SellerProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // ========================================
    // INFORMACIÓN PERSONAL
    // ========================================
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }

    // ========================================
    // CONTACTO
    // ========================================
    public string Phone { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public string? WhatsApp { get; set; }
    public string Email { get; set; } = string.Empty;

    // ========================================
    // UBICACIÓN
    // ========================================
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public string Country { get; set; } = "DO";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // ========================================
    // VERIFICACIÓN
    // ========================================
    public SellerVerificationStatus VerificationStatus { get; set; } = SellerVerificationStatus.NotSubmitted;
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public string? VerificationNotes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? VerificationExpiresAt { get; set; } // Verificación válida por 1 año

    // ========================================
    // MÉTRICAS
    // ========================================
    public int TotalListings { get; set; } = 0;
    public int ActiveListings { get; set; } = 0;
    public int TotalSales { get; set; } = 0;
    public decimal AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;
    public int ResponseTimeMinutes { get; set; } = 0;

    // ========================================
    // CONFIGURACIÓN
    // ========================================
    public bool IsActive { get; set; } = true;
    public bool AcceptsOffers { get; set; } = true;
    public bool ShowPhone { get; set; } = true;
    public bool ShowLocation { get; set; } = true;
    public string? PreferredContactMethod { get; set; } // phone, whatsapp, email, chat

    // ========================================
    // LÍMITES (según verificación)
    // ========================================
    public int MaxActiveListings { get; set; } = 3; // Sin verificar: 3, Verificado: 10
    public bool CanSellHighValue { get; set; } = false; // >$50k requiere verificación

    // ========================================
    // TIPO Y BADGES
    // ========================================
    public SellerType SellerType { get; set; } = SellerType.Individual;
    public string? DisplayName { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public bool IsPhoneVerified { get; set; } = false;
    public bool IsIdentityVerified { get; set; } = false;
    public DateTime? LastActiveAt { get; set; }

    // Para Dealers
    public Guid? DealerId { get; set; }
    public string? BusinessName { get; set; }
    public string? Website { get; set; }

    // Métricas adicionales
    public int ResponseRate { get; set; } = 100; // Porcentaje
    public int ViewsThisMonth { get; set; } = 0;
    public int LeadsThisMonth { get; set; } = 0;

    // ========================================
    // AUDITORÍA
    // ========================================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // ========================================
    // NAVEGACIÓN
    // ========================================
    public ICollection<IdentityDocument> IdentityDocuments { get; set; } = new List<IdentityDocument>();
    public ICollection<SellerBadgeAssignment> Badges { get; set; } = new List<SellerBadgeAssignment>();
    public ContactPreferences? ContactPreferences { get; set; }
}

/// <summary>
/// Tipo de documento de identidad
/// </summary>
public enum IdentityDocumentType
{
    NationalId = 0,         // Cédula de identidad
    Passport = 1,           // Pasaporte
    DriversLicense = 2,     // Licencia de conducir
    ResidencyCard = 3,      // Carnet de residencia
    TaxId = 4               // RNC/NIF
}

/// <summary>
/// Estado de verificación del documento
/// </summary>
public enum DocumentVerificationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Expired = 3
}

/// <summary>
/// Documento de identidad para verificación de vendedores individuales
/// </summary>
public class IdentityDocument
{
    public Guid Id { get; set; }
    public Guid SellerProfileId { get; set; }

    // ========================================
    // INFORMACIÓN DEL DOCUMENTO
    // ========================================
    public IdentityDocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string? IssuingCountry { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // ========================================
    // IMÁGENES DEL DOCUMENTO
    // ========================================
    public string FrontImageUrl { get; set; } = string.Empty;
    public string? BackImageUrl { get; set; } // Algunos documentos no tienen reverso
    public string? SelfieWithDocumentUrl { get; set; } // Selfie sosteniendo el documento

    // ========================================
    // DATOS EXTRAÍDOS (OCR / Verificación manual)
    // ========================================
    public string? ExtractedFullName { get; set; }
    public DateTime? ExtractedDateOfBirth { get; set; }
    public string? ExtractedAddress { get; set; }

    // ========================================
    // VERIFICACIÓN
    // ========================================
    public DocumentVerificationStatus Status { get; set; } = DocumentVerificationStatus.Pending;
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public string? VerificationNotes { get; set; }
    public string? RejectionReason { get; set; }

    // ========================================
    // SEGURIDAD
    // ========================================
    public bool IsEncrypted { get; set; } = true; // Las imágenes se guardan encriptadas
    public int ViewCount { get; set; } = 0; // Cuántas veces se ha visto (auditoría)
    public DateTime? LastViewedAt { get; set; }
    public Guid? LastViewedByUserId { get; set; }

    // ========================================
    // AUDITORÍA
    // ========================================
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // ========================================
    // NAVEGACIÓN
    // ========================================
    public SellerProfile? SellerProfile { get; set; }
}
