using System;
using System.Collections.Generic;
using UserService.Domain.Entities;

namespace UserService.Application.DTOs;

#region Public Profile DTOs

/// <summary>
/// Perfil público de vendedor (vista de comprador)
/// </summary>
public class SellerPublicProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public SellerType Type { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public DateTime MemberSince { get; set; }
    public bool IsVerified { get; set; }
    public List<string> Badges { get; set; } = new();
    public SellerPublicStatsDto Stats { get; set; } = new();
    public SellerDealerInfoDto? Dealer { get; set; }
}

/// <summary>
/// Estadísticas públicas del vendedor
/// </summary>
public class SellerPublicStatsDto
{
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int SoldCount { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string ResponseTime { get; set; } = "N/A";
    public int ResponseRate { get; set; }
}

/// <summary>
/// Información de dealer (si aplica)
/// </summary>
public class SellerDealerInfoDto
{
    public string? BusinessName { get; set; }
    public string? Website { get; set; }
    public bool IsKYCVerified { get; set; }
}

#endregion

#region Contact Preferences DTOs

/// <summary>
/// Preferencias de contacto del vendedor
/// </summary>
public class ContactPreferencesDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }

    // Canales permitidos
    public bool AllowPhoneCalls { get; set; }
    public bool AllowWhatsApp { get; set; }
    public bool AllowEmail { get; set; }
    public bool AllowInAppChat { get; set; }

    // Horarios
    public string ContactHoursStart { get; set; } = "08:00";
    public string ContactHoursEnd { get; set; } = "18:00";
    public List<string> ContactDays { get; set; } = new();

    // Visibilidad
    public bool ShowPhoneNumber { get; set; }
    public bool ShowWhatsAppNumber { get; set; }
    public bool ShowEmail { get; set; }

    // Preferencias
    public string PreferredContactMethod { get; set; } = "WhatsApp";
    public string? AutoReplyMessage { get; set; }
    public string? AwayMessage { get; set; }

    // Filtros
    public bool RequireVerifiedBuyers { get; set; }
    public bool BlockAnonymousContacts { get; set; }
}

/// <summary>
/// Request para actualizar preferencias de contacto
/// </summary>
public class UpdateContactPreferencesRequest
{
    public bool? AllowPhoneCalls { get; set; }
    public bool? AllowWhatsApp { get; set; }
    public bool? AllowEmail { get; set; }
    public bool? AllowInAppChat { get; set; }
    public string? ContactHoursStart { get; set; }
    public string? ContactHoursEnd { get; set; }
    public List<string>? ContactDays { get; set; }
    public bool? ShowPhoneNumber { get; set; }
    public bool? ShowWhatsAppNumber { get; set; }
    public bool? ShowEmail { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? AutoReplyMessage { get; set; }
    public string? AwayMessage { get; set; }
    public bool? RequireVerifiedBuyers { get; set; }
    public bool? BlockAnonymousContacts { get; set; }
}

#endregion

#region Badge DTOs

/// <summary>
/// Badge del vendedor
/// </summary>
public class SellerBadgeDto
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EarnedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Request para asignar badge
/// </summary>
public class AssignBadgeRequest
{
    public Guid SellerProfileId { get; set; }
    public SellerBadge Badge { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Reason { get; set; }
}

#endregion

#region Listings DTOs

/// <summary>
/// Listado resumido del vendedor
/// </summary>
public class SellerListingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "DOP";
    public string Status { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public int Year { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string? Transmission { get; set; }
    public string? FuelType { get; set; }
    public int Views { get; set; }
    public int Favorites { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response de listados del vendedor con paginación
/// </summary>
public class SellerListingsResponse
{
    public List<SellerListingDto> Listings { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

#endregion

#region Review DTOs

/// <summary>
/// Reseña del vendedor
/// </summary>
public class SellerReviewDto
{
    public Guid Id { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string? ReviewerPhotoUrl { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? VehicleTitle { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public SellerReviewReplyDto? Reply { get; set; }
}

/// <summary>
/// Respuesta a una reseña
/// </summary>
public class SellerReviewReplyDto
{
    public string Content { get; set; } = string.Empty;
    public DateTime RepliedAt { get; set; }
}

/// <summary>
/// Response de reseñas con paginación
/// </summary>
public class SellerReviewsResponse
{
    public List<SellerReviewDto> Reviews { get; set; } = new();
    public double AverageRating { get; set; }
    public int TotalCount { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new(); // 1-5 stars count
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

#endregion

#region My Stats DTOs (Private)

/// <summary>
/// Estadísticas privadas del vendedor
/// </summary>
public class SellerMyStatsDto
{
    public Guid SellerId { get; set; }
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int PendingListings { get; set; }
    public int SoldListings { get; set; }
    public int ExpiredListings { get; set; }
    public int TotalViews { get; set; }
    public int ViewsThisMonth { get; set; }
    public int ViewsChange { get; set; } // % change vs last month
    public int TotalInquiries { get; set; }
    public int InquiriesThisMonth { get; set; }
    public int UnrespondedInquiries { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AveragePrice { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int ResponseTimeMinutes { get; set; }
    public int ResponseRate { get; set; }
    public int MaxActiveListings { get; set; }
    public int RemainingListings { get; set; }
    public bool CanSellHighValue { get; set; }
    public List<SellerBadgeDto> Badges { get; set; } = new();
    public SellerVerificationStatus VerificationStatus { get; set; }
}

#endregion

#region Update Profile DTOs

/// <summary>
/// Request para actualizar perfil propio
/// </summary>
public class UpdateMyProfileRequest
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Phone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Website { get; set; }
    public bool? AcceptsOffers { get; set; }
    public bool? ShowPhone { get; set; }
    public bool? ShowLocation { get; set; }
}

/// <summary>
/// Request para subir foto de perfil
/// </summary>
public class UpdateProfilePhotoRequest
{
    public string PhotoUrl { get; set; } = string.Empty;
    public bool IsCoverPhoto { get; set; } = false;
}

#endregion

#region Badge Criteria

/// <summary>
/// Criterios para badges automáticos
/// </summary>
public static class BadgeCriteria
{
    public static readonly Dictionary<SellerBadge, string> Descriptions = new()
    {
        { SellerBadge.Verified, "Identidad verificada por OKLA" },
        { SellerBadge.TopSeller, "Top 10 vendedores del mes" },
        { SellerBadge.FastResponder, "Responde en menos de 1 hora" },
        { SellerBadge.TrustedSeller, "Más de 10 ventas con rating 4.5+" },
        { SellerBadge.FounderMember, "Miembro fundador de OKLA" },
        { SellerBadge.SuperHost, "Rating 5.0 con más de 20 reseñas" },
        { SellerBadge.PowerSeller, "Más de 50 ventas totales" },
        { SellerBadge.NewSeller, "Nuevo vendedor en OKLA" }
    };

    public static readonly Dictionary<SellerBadge, string> Icons = new()
    {
        { SellerBadge.Verified, "✓" },
        { SellerBadge.TopSeller, "⭐" },
        { SellerBadge.FastResponder, "⚡" },
        { SellerBadge.TrustedSeller, "🛡️" },
        { SellerBadge.FounderMember, "🏆" },
        { SellerBadge.SuperHost, "🌟" },
        { SellerBadge.PowerSeller, "💪" },
        { SellerBadge.NewSeller, "🆕" }
    };
}

#endregion
