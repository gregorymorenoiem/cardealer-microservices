using RealEstateService.Domain.Entities;

namespace RealEstateService.Application.DTOs;

// ========================================
// PROPERTY DTOs
// ========================================

public record PropertyDto(
    Guid Id,
    Guid DealerId,
    string Title,
    string Description,
    PropertyType Type,
    ListingType ListingType,
    PropertyStatus Status,
    decimal Price,
    string Currency,
    decimal? PricePerSqMeter,
    decimal? MaintenanceFee,
    bool IsNegotiable,
    PropertyLocationDto Location,
    PropertyFeaturesDto Features,
    PropertyAmenitiesDto Amenities,
    PropertySellerDto Seller,
    IEnumerable<PropertyImageDto> Images,
    bool IsFeatured,
    int ViewCount,
    int FavoriteCount,
    int InquiryCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt
);

public record PropertyLocationDto(
    string Address,
    string City,
    string State,
    string ZipCode,
    string Country,
    string? Neighborhood,
    double? Latitude,
    double? Longitude
);

public record PropertyFeaturesDto(
    decimal TotalArea,
    decimal? BuiltArea,
    decimal? LotArea,
    int Bedrooms,
    int Bathrooms,
    int? HalfBathrooms,
    int? ParkingSpaces,
    int? Floor,
    int? TotalFloors,
    int? YearBuilt,
    bool HasGarden,
    bool HasPool,
    bool HasGym,
    bool HasElevator,
    bool HasSecurity,
    bool IsFurnished,
    bool AllowsPets
);

public record PropertyAmenitiesDto(
    IEnumerable<string> Security,
    IEnumerable<string> Recreation,
    IEnumerable<string> Comfort,
    IEnumerable<string> Services,
    IEnumerable<string> Outdoor,
    IEnumerable<string> Technology
);

public record PropertySellerDto(
    Guid Id,
    string Name,
    string? Phone,
    string? Email
);

public record PropertyImageDto(
    Guid Id,
    string Url,
    string? ThumbnailUrl,
    string? Caption,
    ImageCategory Category,
    int SortOrder,
    bool IsPrimary
);

// ========================================
// PROPERTY SUMMARY (para listados)
// ========================================

public record PropertySummaryDto(
    Guid Id,
    string Title,
    PropertyType Type,
    ListingType ListingType,
    decimal Price,
    string Currency,
    string City,
    string State,
    decimal TotalArea,
    int Bedrooms,
    int Bathrooms,
    int? ParkingSpaces,
    string? PrimaryImageUrl,
    bool IsFeatured,
    DateTime CreatedAt
);

// ========================================
// CREATE/UPDATE DTOs
// ========================================

public record CreatePropertyRequest(
    string Title,
    string Description,
    PropertyType Type,
    ListingType ListingType,
    decimal Price,
    string Currency,
    decimal? MaintenanceFee,
    bool IsNegotiable,
    // Location
    string Address,
    string City,
    string State,
    string ZipCode,
    string Country,
    string? Neighborhood,
    double? Latitude,
    double? Longitude,
    // Features
    decimal TotalArea,
    decimal? BuiltArea,
    decimal? LotArea,
    int Bedrooms,
    int Bathrooms,
    int? HalfBathrooms,
    int? ParkingSpaces,
    int? Floor,
    int? TotalFloors,
    int? YearBuilt,
    bool HasGarden,
    bool HasPool,
    bool HasGym,
    bool HasElevator,
    bool HasSecurity,
    bool IsFurnished,
    bool AllowsPets,
    // Amenities
    IEnumerable<string>? Amenities,
    // Images
    IEnumerable<Guid>? ImageIds
);

public record UpdatePropertyRequest(
    string? Title,
    string? Description,
    PropertyType? Type,
    ListingType? ListingType,
    decimal? Price,
    decimal? MaintenanceFee,
    bool? IsNegotiable,
    // Location
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Neighborhood,
    double? Latitude,
    double? Longitude,
    // Features
    decimal? TotalArea,
    decimal? BuiltArea,
    decimal? LotArea,
    int? Bedrooms,
    int? Bathrooms,
    int? HalfBathrooms,
    int? ParkingSpaces,
    int? Floor,
    int? TotalFloors,
    int? YearBuilt,
    bool? HasGarden,
    bool? HasPool,
    bool? HasGym,
    bool? HasElevator,
    bool? HasSecurity,
    bool? IsFurnished,
    bool? AllowsPets,
    // Amenities
    IEnumerable<string>? Amenities
);

public record UpdatePropertyStatusRequest(
    PropertyStatus Status
);

// ========================================
// SEARCH DTOs
// ========================================

public record PropertySearchRequest(
    string? Query,
    PropertyType? Type,
    ListingType? ListingType,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? MinArea,
    decimal? MaxArea,
    int? MinBedrooms,
    int? MaxBedrooms,
    int? MinBathrooms,
    int? MaxBathrooms,
    string? City,
    string? State,
    string? Neighborhood,
    bool? HasPool,
    bool? HasGarden,
    bool? HasGym,
    bool? HasSecurity,
    bool? IsFurnished,
    bool? AllowsPets,
    bool? IsFeatured,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = false
);

public record PropertySearchResponse(
    IEnumerable<PropertySummaryDto> Properties,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

// ========================================
// IMAGE DTOs
// ========================================

public record AddPropertyImageRequest(
    Guid PropertyId,
    string Url,
    string? ThumbnailUrl,
    string? Caption,
    ImageCategory Category,
    bool IsPrimary
);

public record ReorderImagesRequest(
    IEnumerable<Guid> OrderedImageIds
);
