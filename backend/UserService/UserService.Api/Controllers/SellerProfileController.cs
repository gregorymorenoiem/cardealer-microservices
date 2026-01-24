using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller para gestión de perfiles de vendedores
/// Implementa procesos SELLER-001 a SELLER-005 y PROF-001 a PROF-004
/// </summary>
[ApiController]
[Route("api/sellers")]
public class SellerProfileController : ControllerBase
{
    private readonly ISellerProfileRepository _sellerProfileRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SellerProfileController> _logger;

    public SellerProfileController(
        ISellerProfileRepository sellerProfileRepository,
        IUserRepository userRepository,
        IEventPublisher eventPublisher,
        ILogger<SellerProfileController> logger)
    {
        _sellerProfileRepository = sellerProfileRepository;
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    #region Endpoints Públicos

    /// <summary>
    /// SELLER-001: Ver perfil público de vendedor
    /// GET /api/sellers/{sellerId}/profile
    /// </summary>
    [HttpGet("{sellerId}/profile")]
    [ProducesResponseType(typeof(SellerPublicProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSellerProfile(Guid sellerId)
    {
        _logger.LogInformation("SELLER-001: Getting public profile for seller {SellerId}", sellerId);

        var profile = await _sellerProfileRepository.GetByIdAsync(sellerId);
        if (profile == null || !profile.IsActive || profile.IsDeleted)
        {
            return NotFound(new { message = "Vendedor no encontrado" });
        }

        // Actualizar última actividad
        await _sellerProfileRepository.UpdateLastActiveAsync(sellerId);

        var badges = await _sellerProfileRepository.GetBadgesAsync(sellerId);
        
        var response = MapToPublicProfile(profile, badges);
        return Ok(response);
    }

    /// <summary>
    /// SELLER-001: Ver listados del vendedor
    /// GET /api/sellers/{sellerId}/listings
    /// </summary>
    [HttpGet("{sellerId}/listings")]
    [ProducesResponseType(typeof(SellerListingsResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSellerListings(
        Guid sellerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? status = null)
    {
        var profile = await _sellerProfileRepository.GetByIdAsync(sellerId);
        if (profile == null || !profile.IsActive)
        {
            return NotFound(new { message = "Vendedor no encontrado" });
        }

        // TODO: Integrar con VehiclesSaleService para obtener listados reales
        var response = new SellerListingsResponse
        {
            Listings = new List<SellerListingDto>(),
            Page = page,
            PageSize = pageSize,
            TotalCount = 0
        };

        return Ok(response);
    }

    /// <summary>
    /// SELLER-001: Ver reseñas del vendedor
    /// GET /api/sellers/{sellerId}/reviews
    /// </summary>
    [HttpGet("{sellerId}/reviews")]
    [ProducesResponseType(typeof(SellerReviewsResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSellerReviews(
        Guid sellerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? rating = null)
    {
        var profile = await _sellerProfileRepository.GetByIdAsync(sellerId);
        if (profile == null || !profile.IsActive)
        {
            return NotFound(new { message = "Vendedor no encontrado" });
        }

        // TODO: Integrar con ReviewService para obtener reseñas reales
        var response = new SellerReviewsResponse
        {
            Reviews = new List<SellerReviewDto>(),
            AverageRating = (double)profile.AverageRating,
            TotalCount = profile.TotalReviews,
            RatingDistribution = new Dictionary<int, int>
            {
                { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 }
            },
            Page = page,
            PageSize = pageSize
        };

        return Ok(response);
    }

    /// <summary>
    /// SELLER-001: Ver estadísticas públicas del vendedor
    /// GET /api/sellers/{sellerId}/stats
    /// </summary>
    [HttpGet("{sellerId}/stats")]
    [ProducesResponseType(typeof(SellerPublicStatsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSellerStats(Guid sellerId)
    {
        var profile = await _sellerProfileRepository.GetByIdAsync(sellerId);
        if (profile == null || !profile.IsActive)
        {
            return NotFound(new { message = "Vendedor no encontrado" });
        }

        var stats = new SellerPublicStatsDto
        {
            TotalListings = profile.TotalListings,
            ActiveListings = profile.ActiveListings,
            SoldCount = profile.TotalSales,
            AverageRating = (double)profile.AverageRating,
            ReviewCount = profile.TotalReviews,
            ResponseTime = FormatResponseTime(profile.ResponseTimeMinutes),
            ResponseRate = profile.ResponseRate
        };

        return Ok(stats);
    }

    /// <summary>
    /// SELLER-003: Ver preferencias de contacto del vendedor
    /// GET /api/sellers/{sellerId}/contact-preferences
    /// </summary>
    [HttpGet("{sellerId}/contact-preferences")]
    [ProducesResponseType(typeof(ContactPreferencesDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSellerContactPreferences(Guid sellerId)
    {
        var profile = await _sellerProfileRepository.GetByIdAsync(sellerId);
        if (profile == null || !profile.IsActive)
        {
            return NotFound(new { message = "Vendedor no encontrado" });
        }

        var preferences = await _sellerProfileRepository.GetContactPreferencesAsync(sellerId);
        if (preferences == null)
        {
            return Ok(new ContactPreferencesDto
            {
                SellerId = sellerId,
                AllowPhoneCalls = profile.ShowPhone,
                AllowWhatsApp = true,
                AllowEmail = true,
                AllowInAppChat = true,
                ContactHoursStart = "08:00",
                ContactHoursEnd = "18:00",
                ContactDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" },
                ShowPhoneNumber = profile.ShowPhone,
                ShowWhatsAppNumber = true,
                ShowEmail = false,
                PreferredContactMethod = profile.PreferredContactMethod ?? "WhatsApp"
            });
        }

        return Ok(MapToContactPreferencesDto(preferences));
    }

    /// <summary>
    /// Buscar vendedores
    /// GET /api/sellers/search
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedSellersResponse), 200)]
    public async Task<IActionResult> SearchSellers(
        [FromQuery] string? q = null,
        [FromQuery] string? city = null,
        [FromQuery] SellerType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var sellers = await _sellerProfileRepository.SearchAsync(q, type, city, page, pageSize);
        var totalCount = await _sellerProfileRepository.GetTotalCountAsync(q, type, city);

        var response = new PaginatedSellersResponse
        {
            Sellers = sellers.Select(s => new SellerProfileSummaryDto
            {
                Id = s.Id,
                UserId = s.UserId,
                FullName = s.DisplayName ?? s.FullName,
                AvatarUrl = s.AvatarUrl,
                City = s.City,
                State = s.State,
                VerificationStatus = s.VerificationStatus,
                AverageRating = s.AverageRating,
                TotalReviews = s.TotalReviews,
                ActiveListings = s.ActiveListings,
                TotalSales = s.TotalSales,
                CreatedAt = s.CreatedAt,
                ResponseTimeMinutes = s.ResponseTimeMinutes
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(response);
    }

    /// <summary>
    /// Obtener top vendedores
    /// GET /api/sellers/top
    /// </summary>
    [HttpGet("top")]
    [ProducesResponseType(typeof(List<SellerProfileSummaryDto>), 200)]
    public async Task<IActionResult> GetTopSellers(
        [FromQuery] int count = 10,
        [FromQuery] string? city = null)
    {
        var sellers = await _sellerProfileRepository.GetTopSellersAsync(count, city);

        var response = sellers.Select(s => new SellerProfileSummaryDto
        {
            Id = s.Id,
            UserId = s.UserId,
            FullName = s.DisplayName ?? s.FullName,
            AvatarUrl = s.AvatarUrl,
            City = s.City,
            State = s.State,
            VerificationStatus = s.VerificationStatus,
            AverageRating = s.AverageRating,
            TotalReviews = s.TotalReviews,
            ActiveListings = s.ActiveListings,
            TotalSales = s.TotalSales,
            CreatedAt = s.CreatedAt,
            ResponseTimeMinutes = s.ResponseTimeMinutes
        }).ToList();

        return Ok(response);
    }

    #endregion

    #region Endpoints Autenticados (Mi Perfil)

    /// <summary>
    /// SELLER-002: Obtener mi perfil de vendedor
    /// GET /api/sellers/profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(SellerProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var profile = await _sellerProfileRepository.GetByUserIdAsync(userId.Value);
        if (profile == null)
        {
            return NotFound(new { message = "No tienes un perfil de vendedor. Crea uno primero." });
        }

        return Ok(MapToSellerProfileDto(profile));
    }

    /// <summary>
    /// SELLER-002: Actualizar mi perfil
    /// PUT /api/sellers/profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(SellerProfileDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        _logger.LogInformation("SELLER-002: Updating profile for user {UserId}", userId);

        var profile = await _sellerProfileRepository.GetByUserIdAsync(userId.Value);
        if (profile == null)
        {
            return NotFound(new { message = "No tienes un perfil de vendedor" });
        }

        if (request.DisplayName != null) profile.DisplayName = request.DisplayName;
        if (request.Bio != null) profile.Bio = request.Bio;
        if (request.City != null) profile.City = request.City;
        if (request.Province != null) profile.State = request.Province;
        if (request.Phone != null) profile.Phone = request.Phone;
        if (request.WhatsApp != null) profile.WhatsApp = request.WhatsApp;
        if (request.Website != null) profile.Website = request.Website;
        if (request.AcceptsOffers.HasValue) profile.AcceptsOffers = request.AcceptsOffers.Value;
        if (request.ShowPhone.HasValue) profile.ShowPhone = request.ShowPhone.Value;
        if (request.ShowLocation.HasValue) profile.ShowLocation = request.ShowLocation.Value;

        await _sellerProfileRepository.UpdateAsync(profile);

        await _eventPublisher.PublishAsync("seller.profile.updated", new
        {
            SellerId = profile.Id,
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        });

        return Ok(MapToSellerProfileDto(profile));
    }

    /// <summary>
    /// SELLER-002: Subir foto de perfil
    /// PUT /api/sellers/profile/photo
    /// </summary>
    [HttpPut("profile/photo")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateProfilePhoto([FromBody] UpdateProfilePhotoRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var profile = await _sellerProfileRepository.GetByUserIdAsync(userId.Value);
        if (profile == null)
        {
            return NotFound(new { message = "No tienes un perfil de vendedor" });
        }

        if (request.IsCoverPhoto)
        {
            profile.CoverPhotoUrl = request.PhotoUrl;
        }
        else
        {
            profile.AvatarUrl = request.PhotoUrl;
        }

        await _sellerProfileRepository.UpdateAsync(profile);

        return Ok(new { 
            message = request.IsCoverPhoto ? "Foto de portada actualizada" : "Foto de perfil actualizada",
            photoUrl = request.PhotoUrl
        });
    }

    /// <summary>
    /// SELLER-003: Actualizar preferencias de contacto
    /// PUT /api/sellers/contact-preferences
    /// </summary>
    [HttpPut("contact-preferences")]
    [Authorize]
    [ProducesResponseType(typeof(ContactPreferencesDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateContactPreferences([FromBody] UpdateContactPreferencesRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        _logger.LogInformation("SELLER-003: Updating contact preferences for user {UserId}", userId);

        var profile = await _sellerProfileRepository.GetByUserIdAsync(userId.Value);
        if (profile == null)
        {
            return NotFound(new { message = "No tienes un perfil de vendedor" });
        }

        var preferences = await _sellerProfileRepository.GetContactPreferencesAsync(profile.Id);
        
        if (preferences == null)
        {
            preferences = new ContactPreferences
            {
                Id = Guid.NewGuid(),
                SellerProfileId = profile.Id
            };
        }

        if (request.AllowPhoneCalls.HasValue) preferences.AllowPhoneCalls = request.AllowPhoneCalls.Value;
        if (request.AllowWhatsApp.HasValue) preferences.AllowWhatsApp = request.AllowWhatsApp.Value;
        if (request.AllowEmail.HasValue) preferences.AllowEmail = request.AllowEmail.Value;
        if (request.AllowInAppChat.HasValue) preferences.AllowInAppChat = request.AllowInAppChat.Value;
        if (request.ContactHoursStart != null) 
            preferences.ContactHoursStart = TimeSpan.Parse(request.ContactHoursStart);
        if (request.ContactHoursEnd != null) 
            preferences.ContactHoursEnd = TimeSpan.Parse(request.ContactHoursEnd);
        if (request.ContactDays != null) 
            preferences.ContactDays = string.Join(",", request.ContactDays);
        if (request.ShowPhoneNumber.HasValue) preferences.ShowPhoneNumber = request.ShowPhoneNumber.Value;
        if (request.ShowWhatsAppNumber.HasValue) preferences.ShowWhatsAppNumber = request.ShowWhatsAppNumber.Value;
        if (request.ShowEmail.HasValue) preferences.ShowEmail = request.ShowEmail.Value;
        if (request.PreferredContactMethod != null) preferences.PreferredContactMethod = request.PreferredContactMethod;
        if (request.AutoReplyMessage != null) preferences.AutoReplyMessage = request.AutoReplyMessage;
        if (request.AwayMessage != null) preferences.AwayMessage = request.AwayMessage;
        if (request.RequireVerifiedBuyers.HasValue) preferences.RequireVerifiedBuyers = request.RequireVerifiedBuyers.Value;
        if (request.BlockAnonymousContacts.HasValue) preferences.BlockAnonymousContacts = request.BlockAnonymousContacts.Value;

        if (preferences.CreatedAt == default)
        {
            await _sellerProfileRepository.CreateContactPreferencesAsync(preferences);
        }
        else
        {
            await _sellerProfileRepository.UpdateContactPreferencesAsync(preferences);
        }

        await _eventPublisher.PublishAsync("seller.preferences.updated", new
        {
            SellerId = profile.Id,
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        });

        return Ok(MapToContactPreferencesDto(preferences));
    }

    /// <summary>
    /// SELLER-005: Obtener mis estadísticas
    /// GET /api/sellers/my-stats
    /// </summary>
    [HttpGet("my-stats")]
    [Authorize]
    [ProducesResponseType(typeof(SellerMyStatsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMyStats()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var profile = await _sellerProfileRepository.GetByUserIdAsync(userId.Value);
        if (profile == null)
        {
            return NotFound(new { message = "No tienes un perfil de vendedor" });
        }

        var badges = await _sellerProfileRepository.GetBadgesAsync(profile.Id);

        var stats = new SellerMyStatsDto
        {
            SellerId = profile.Id,
            TotalListings = profile.TotalListings,
            ActiveListings = profile.ActiveListings,
            PendingListings = 0,
            SoldListings = profile.TotalSales,
            ExpiredListings = 0,
            TotalViews = profile.ViewsThisMonth * 12,
            ViewsThisMonth = profile.ViewsThisMonth,
            ViewsChange = 0,
            TotalInquiries = profile.LeadsThisMonth * 12,
            InquiriesThisMonth = profile.LeadsThisMonth,
            UnrespondedInquiries = 0,
            TotalValue = 0,
            AveragePrice = 0,
            AverageRating = (double)profile.AverageRating,
            ReviewCount = profile.TotalReviews,
            ResponseTimeMinutes = profile.ResponseTimeMinutes,
            ResponseRate = profile.ResponseRate,
            MaxActiveListings = profile.MaxActiveListings,
            RemainingListings = profile.MaxActiveListings - profile.ActiveListings,
            CanSellHighValue = profile.CanSellHighValue,
            Badges = badges.Select(b => new SellerBadgeDto
            {
                Name = b.Badge.ToString(),
                Icon = BadgeCriteria.Icons.GetValueOrDefault(b.Badge, "🏷️"),
                Description = BadgeCriteria.Descriptions.GetValueOrDefault(b.Badge, ""),
                EarnedAt = b.EarnedAt,
                ExpiresAt = b.ExpiresAt
            }).ToList(),
            VerificationStatus = profile.VerificationStatus
        };

        return Ok(stats);
    }

    /// <summary>
    /// Crear perfil de vendedor
    /// POST /api/sellers/profile
    /// </summary>
    [HttpPost("profile")]
    [Authorize]
    [ProducesResponseType(typeof(SellerProfileDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateSellerProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var existing = await _sellerProfileRepository.GetByUserIdAsync(userId.Value);
        if (existing != null)
        {
            return BadRequest(new { message = "Ya tienes un perfil de vendedor" });
        }

        var profile = new SellerProfile
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId != Guid.Empty ? request.UserId : userId.Value,
            FullName = request.FullName,
            DisplayName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Nationality = request.Nationality,
            Phone = request.Phone,
            AlternatePhone = request.AlternatePhone,
            WhatsApp = request.WhatsApp,
            Email = request.Email,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            Country = request.Country,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AcceptsOffers = request.AcceptsOffers,
            ShowPhone = request.ShowPhone,
            ShowLocation = request.ShowLocation,
            PreferredContactMethod = request.PreferredContactMethod,
            SellerType = SellerType.Individual,
            VerificationStatus = SellerVerificationStatus.NotSubmitted
        };

        await _sellerProfileRepository.CreateAsync(profile);

        await _sellerProfileRepository.AssignBadgeAsync(new SellerBadgeAssignment
        {
            Id = Guid.NewGuid(),
            SellerProfileId = profile.Id,
            Badge = SellerBadge.NewSeller,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Reason = "Nuevo vendedor en OKLA"
        });

        await _eventPublisher.PublishAsync("seller.profile.created", new
        {
            SellerId = profile.Id,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        return CreatedAtAction(nameof(GetMyProfile), MapToSellerProfileDto(profile));
    }

    #endregion

    #region Endpoints Admin

    /// <summary>
    /// SELLER-004: Asignar badge a vendedor
    /// POST /api/sellers/{sellerId}/badges
    /// </summary>
    [HttpPost("{sellerId}/badges")]
    [Authorize(Roles = "Admin,Moderator")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AssignBadge(Guid sellerId, [FromBody] AssignBadgeRequest request)
    {
        _logger.LogInformation("SELLER-004: Assigning badge {Badge} to seller {SellerId}", request.Badge, sellerId);

        var profile = await _sellerProfileRepository.GetByIdAsync(sellerId);
        if (profile == null)
        {
            return NotFound(new { message = "Vendedor no encontrado" });
        }

        var badge = await _sellerProfileRepository.AssignBadgeAsync(new SellerBadgeAssignment
        {
            Id = Guid.NewGuid(),
            SellerProfileId = sellerId,
            Badge = request.Badge,
            ExpiresAt = request.ExpiresAt,
            Reason = request.Reason
        });

        await _eventPublisher.PublishAsync("seller.badge.earned", new
        {
            SellerId = sellerId,
            Badge = request.Badge.ToString(),
            EarnedAt = DateTime.UtcNow
        });

        return Ok(new { 
            message = $"Badge {request.Badge} asignado exitosamente",
            badge = new SellerBadgeDto
            {
                Name = badge.Badge.ToString(),
                Icon = BadgeCriteria.Icons.GetValueOrDefault(badge.Badge, "🏷️"),
                Description = BadgeCriteria.Descriptions.GetValueOrDefault(badge.Badge, ""),
                EarnedAt = badge.EarnedAt,
                ExpiresAt = badge.ExpiresAt
            }
        });
    }

    /// <summary>
    /// SELLER-004: Quitar badge de vendedor
    /// DELETE /api/sellers/{sellerId}/badges/{badge}
    /// </summary>
    [HttpDelete("{sellerId}/badges/{badge}")]
    [Authorize(Roles = "Admin,Moderator")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveBadge(Guid sellerId, SellerBadge badge)
    {
        var removed = await _sellerProfileRepository.RemoveBadgeAsync(sellerId, badge);
        if (!removed)
        {
            return NotFound(new { message = "Badge no encontrado" });
        }

        await _eventPublisher.PublishAsync("seller.badge.lost", new
        {
            SellerId = sellerId,
            Badge = badge.ToString(),
            RemovedAt = DateTime.UtcNow
        });

        return Ok(new { message = $"Badge {badge} removido exitosamente" });
    }

    /// <summary>
    /// Verificar vendedor (Admin)
    /// POST /api/sellers/{sellerId}/verify
    /// </summary>
    [HttpPost("{sellerId}/verify")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> VerifySeller(Guid sellerId, [FromBody] VerifySellerProfileRequest request)
    {
        var success = await _sellerProfileRepository.VerifyAsync(sellerId, request.VerifiedByUserId, request.Notes);
        if (!success)
        {
            return NotFound(new { message = "Vendedor no encontrado" });
        }

        await _eventPublisher.PublishAsync("seller.verified", new
        {
            SellerId = sellerId,
            VerifiedAt = DateTime.UtcNow
        });

        return Ok(new { message = "Vendedor verificado exitosamente" });
    }

    /// <summary>
    /// Obtener vendedores pendientes de verificación
    /// GET /api/sellers/pending-verifications
    /// </summary>
    [HttpGet("pending-verifications")]
    [Authorize(Roles = "Admin,Moderator")]
    [ProducesResponseType(typeof(List<SellerProfileDto>), 200)]
    public async Task<IActionResult> GetPendingVerifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var sellers = await _sellerProfileRepository.GetPendingVerificationsAsync(page, pageSize);
        var response = sellers.Select(MapToSellerProfileDto).ToList();
        return Ok(response);
    }

    #endregion

    #region Helpers

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    private static string FormatResponseTime(int minutes)
    {
        if (minutes <= 0) return "N/A";
        if (minutes < 60) return $"{minutes} minutos";
        if (minutes < 1440) return $"{minutes / 60} horas";
        return $"{minutes / 1440} días";
    }

    private static SellerPublicProfileDto MapToPublicProfile(SellerProfile profile, List<SellerBadgeAssignment> badges)
    {
        return new SellerPublicProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            DisplayName = profile.DisplayName ?? profile.FullName,
            Type = profile.SellerType,
            Bio = profile.Bio,
            ProfilePhotoUrl = profile.AvatarUrl,
            CoverPhotoUrl = profile.CoverPhotoUrl,
            City = profile.City,
            Province = profile.State,
            MemberSince = profile.CreatedAt,
            IsVerified = profile.VerificationStatus == SellerVerificationStatus.Verified,
            Badges = badges.Select(b => b.Badge.ToString()).ToList(),
            Stats = new SellerPublicStatsDto
            {
                TotalListings = profile.TotalListings,
                ActiveListings = profile.ActiveListings,
                SoldCount = profile.TotalSales,
                AverageRating = (double)profile.AverageRating,
                ReviewCount = profile.TotalReviews,
                ResponseTime = FormatResponseTime(profile.ResponseTimeMinutes),
                ResponseRate = profile.ResponseRate
            },
            Dealer = profile.DealerId.HasValue ? new SellerDealerInfoDto
            {
                BusinessName = profile.BusinessName,
                Website = profile.Website,
                IsKYCVerified = profile.IsIdentityVerified
            } : null
        };
    }

    private static SellerProfileDto MapToSellerProfileDto(SellerProfile profile)
    {
        return new SellerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = profile.FullName,
            DateOfBirth = profile.DateOfBirth,
            Nationality = profile.Nationality,
            Bio = profile.Bio,
            AvatarUrl = profile.AvatarUrl,
            Phone = profile.Phone,
            AlternatePhone = profile.AlternatePhone,
            WhatsApp = profile.WhatsApp,
            Email = profile.Email,
            Address = profile.Address,
            City = profile.City,
            State = profile.State,
            ZipCode = profile.ZipCode,
            Country = profile.Country,
            Latitude = profile.Latitude,
            Longitude = profile.Longitude,
            VerificationStatus = profile.VerificationStatus,
            VerifiedAt = profile.VerifiedAt,
            RejectionReason = profile.RejectionReason,
            TotalListings = profile.TotalListings,
            ActiveListings = profile.ActiveListings,
            TotalSales = profile.TotalSales,
            AverageRating = profile.AverageRating,
            TotalReviews = profile.TotalReviews,
            ResponseTimeMinutes = profile.ResponseTimeMinutes,
            IsActive = profile.IsActive,
            AcceptsOffers = profile.AcceptsOffers,
            ShowPhone = profile.ShowPhone,
            ShowLocation = profile.ShowLocation,
            PreferredContactMethod = profile.PreferredContactMethod,
            MaxActiveListings = profile.MaxActiveListings,
            CanSellHighValue = profile.CanSellHighValue,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }

    private static ContactPreferencesDto MapToContactPreferencesDto(ContactPreferences preferences)
    {
        return new ContactPreferencesDto
        {
            Id = preferences.Id,
            SellerId = preferences.SellerProfileId,
            AllowPhoneCalls = preferences.AllowPhoneCalls,
            AllowWhatsApp = preferences.AllowWhatsApp,
            AllowEmail = preferences.AllowEmail,
            AllowInAppChat = preferences.AllowInAppChat,
            ContactHoursStart = preferences.ContactHoursStart.ToString(@"hh\:mm"),
            ContactHoursEnd = preferences.ContactHoursEnd.ToString(@"hh\:mm"),
            ContactDays = preferences.ContactDays?.Split(',').ToList() ?? new List<string>(),
            ShowPhoneNumber = preferences.ShowPhoneNumber,
            ShowWhatsAppNumber = preferences.ShowWhatsAppNumber,
            ShowEmail = preferences.ShowEmail,
            PreferredContactMethod = preferences.PreferredContactMethod,
            AutoReplyMessage = preferences.AutoReplyMessage,
            AwayMessage = preferences.AwayMessage,
            RequireVerifiedBuyers = preferences.RequireVerifiedBuyers,
            BlockAnonymousContacts = preferences.BlockAnonymousContacts
        };
    }

    #endregion
}
