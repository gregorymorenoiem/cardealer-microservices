using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities;

/// <summary>
/// Preferencias de contacto para vendedores
/// </summary>
public class ContactPreferences
{
    public Guid Id { get; set; }
    public Guid SellerProfileId { get; set; }

    // ========================================
    // CANALES PERMITIDOS
    // ========================================
    public bool AllowPhoneCalls { get; set; } = true;
    public bool AllowWhatsApp { get; set; } = true;
    public bool AllowEmail { get; set; } = true;
    public bool AllowInAppChat { get; set; } = true;

    // ========================================
    // HORARIOS DE CONTACTO
    // ========================================
    public TimeSpan ContactHoursStart { get; set; } = new TimeSpan(8, 0, 0); // 8:00 AM
    public TimeSpan ContactHoursEnd { get; set; } = new TimeSpan(18, 0, 0); // 6:00 PM
    public string ContactDays { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday"; // JSON o CSV

    // ========================================
    // VISIBILIDAD DE INFORMACIÓN
    // ========================================
    public bool ShowPhoneNumber { get; set; } = true;
    public bool ShowWhatsAppNumber { get; set; } = true;
    public bool ShowEmail { get; set; } = false;

    // ========================================
    // PREFERENCIAS
    // ========================================
    public string PreferredContactMethod { get; set; } = "WhatsApp"; // phone, whatsapp, email, chat
    public string? AutoReplyMessage { get; set; }
    public string? AwayMessage { get; set; }

    // ========================================
    // FILTROS DE CONTACTO
    // ========================================
    public bool RequireVerifiedBuyers { get; set; } = false;
    public bool BlockAnonymousContacts { get; set; } = true;

    // ========================================
    // AUDITORÍA
    // ========================================
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ========================================
    // NAVEGACIÓN
    // ========================================
    public SellerProfile? SellerProfile { get; set; }
}

/// <summary>
/// Tipo de vendedor
/// </summary>
public enum SellerType
{
    Individual = 0,      // Vendedor particular
    Dealer = 1,          // Concesionario
    PremiumDealer = 2    // Dealer con plan Pro/Enterprise
}

/// <summary>
/// Badges del vendedor
/// </summary>
public enum SellerBadge
{
    Verified = 0,           // ✓ Identidad verificada
    TopSeller = 1,          // ⭐ Top vendedor del mes
    FastResponder = 2,      // ⚡ Responde en < 1 hora
    TrustedSeller = 3,      // 🛡️ +10 ventas, +4.5 rating
    FounderMember = 4,      // 🏆 Early Bird member
    SuperHost = 5,          // 🌟 5.0 rating, +20 reviews
    PowerSeller = 6,        // 💪 +50 ventas
    NewSeller = 7           // 🆕 Nuevo en OKLA
}

/// <summary>
/// Badge asignado a un vendedor
/// </summary>
public class SellerBadgeAssignment
{
    public Guid Id { get; set; }
    public Guid SellerProfileId { get; set; }
    public SellerBadge Badge { get; set; }
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; } // null = permanente
    public bool IsActive { get; set; } = true;
    public string? Reason { get; set; } // Razón de asignación

    // Navegación
    public SellerProfile? SellerProfile { get; set; }
}
