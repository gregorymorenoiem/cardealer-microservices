using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

/// <summary>
/// Estado de verificación del dealer
/// </summary>
public enum DealerVerificationStatus
{
    Pending = 0,
    UnderReview = 1,
    Verified = 2,
    Rejected = 3,
    Suspended = 4
}

/// <summary>
/// Tipo de dealer/concesionario
/// </summary>
public enum DealerType
{
    Independent = 0,        // Lote independiente
    Franchise = 1,          // Concesionario oficial de marca
    MultiLocation = 2,      // Cadena con múltiples ubicaciones
    OnlineOnly = 3,         // Solo ventas online
    Wholesale = 4           // Venta mayorista
}

/// <summary>
/// Entidad Dealer - Representa un concesionario o vendedor profesional
/// </summary>
public class Dealer
{
    public Guid Id { get; set; }

    // ========================================
    // INFORMACIÓN BÁSICA
    // ========================================
    public string BusinessName { get; set; } = string.Empty;
    public string? TradeName { get; set; } // Nombre comercial si es diferente
    public string? Description { get; set; }
    public DealerType DealerType { get; set; } = DealerType.Independent;

    // ========================================
    // CONTACTO
    // ========================================
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? Website { get; set; }

    // ========================================
    // UBICACIÓN
    // ========================================
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public string Country { get; set; } = "DO"; // República Dominicana por defecto
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // ========================================
    // BRANDING
    // ========================================
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? PrimaryColor { get; set; } // Hex color

    // ========================================
    // DOCUMENTACIÓN LEGAL
    // ========================================
    public string? BusinessRegistrationNumber { get; set; } // RNC (República Dominicana)
    public string? TaxId { get; set; }
    public string? DealerLicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? BusinessLicenseDocumentUrl { get; set; }

    // ========================================
    // VERIFICACIÓN
    // ========================================
    public DealerVerificationStatus VerificationStatus { get; set; } = DealerVerificationStatus.Pending;
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public string? VerificationNotes { get; set; }
    public string? RejectionReason { get; set; }

    // ========================================
    // MÉTRICAS
    // ========================================
    public int TotalListings { get; set; } = 0;
    public int ActiveListings { get; set; } = 0;
    public int TotalSales { get; set; } = 0;
    public decimal AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;
    public int ResponseTimeMinutes { get; set; } = 0; // Promedio de tiempo de respuesta

    // ========================================
    // CONFIGURACIÓN
    // ========================================
    public bool IsActive { get; set; } = true;
    public bool AcceptsFinancing { get; set; } = false;
    public bool AcceptsTradeIn { get; set; } = false;
    public bool OffersWarranty { get; set; } = false;
    public bool HomeDelivery { get; set; } = false;
    public string? BusinessHours { get; set; } // JSON con horarios
    public string? SocialMediaLinks { get; set; } // JSON con links

    // ========================================
    // SUSCRIPCIÓN
    // ========================================
    public Guid? SubscriptionId { get; set; }
    public int MaxListings { get; set; } = 10; // Límite según plan
    public bool IsFeatured { get; set; } = false;
    public DateTime? FeaturedUntil { get; set; }

    // ========================================
    // AUDITORÍA
    // ========================================
    public Guid OwnerUserId { get; set; } // Usuario propietario del dealer
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // ========================================
    // NAVEGACIÓN
    // ========================================
    public ICollection<DealerEmployee> Employees { get; set; } = new List<DealerEmployee>();
}
