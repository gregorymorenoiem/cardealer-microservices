using UserService.Domain.Entities;

namespace UserService.Application.DTOs;

#region Seller Conversion DTOs

/// <summary>
/// Response DTO for buyer-to-seller conversion.
/// </summary>
public class SellerConversionResultDto
{
    public Guid ConversionId { get; set; }
    public Guid UserId { get; set; }
    public Guid SellerProfileId { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PreviousAccountType { get; set; } = string.Empty;
    public string NewAccountType { get; set; } = "Seller";
    public bool PendingVerification { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// The seller profile created or found.
    /// </summary>
    public SellerProfileDto? SellerProfile { get; set; }
}

/// <summary>
/// Request body for POST /api/sellers/convert.
/// Minimal — most data comes from the authenticated user's profile.
/// </summary>
public class ConvertBuyerToSellerRequest
{
    /// <summary>
    /// Preferred contact method for selling. Default: "whatsapp"
    /// </summary>
    public string? PreferredContactMethod { get; set; } = "whatsapp";

    /// <summary>
    /// Whether the seller accepts offers on listings.
    /// </summary>
    public bool AcceptsOffers { get; set; } = true;

    /// <summary>
    /// Whether to show phone number on public profile.
    /// </summary>
    public bool ShowPhone { get; set; } = true;

    /// <summary>
    /// Whether to show location on public profile.
    /// </summary>
    public bool ShowLocation { get; set; } = true;

    /// <summary>
    /// Optional short bio for the seller profile.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Accept terms and conditions for selling.
    /// </summary>
    public bool AcceptTerms { get; set; }
}

#endregion
