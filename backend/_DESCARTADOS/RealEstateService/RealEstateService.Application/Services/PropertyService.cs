using RealEstateService.Application.DTOs;
using RealEstateService.Domain.Entities;
using RealEstateService.Domain.Interfaces;

namespace RealEstateService.Application.Services;

/// <summary>
/// Servicio de aplicación para propiedades inmobiliarias
/// </summary>
public class PropertyService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyImageRepository _imageRepository;
    private readonly IPropertyAmenityRepository _amenityRepository;

    public PropertyService(
        IPropertyRepository propertyRepository,
        IPropertyImageRepository imageRepository,
        IPropertyAmenityRepository amenityRepository)
    {
        _propertyRepository = propertyRepository;
        _imageRepository = imageRepository;
        _amenityRepository = amenityRepository;
    }

    public async Task<PropertyDto?> GetByIdAsync(Guid id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        return property != null ? MapToDto(property) : null;
    }

    public async Task<IEnumerable<PropertySummaryDto>> GetAllAsync()
    {
        var properties = await _propertyRepository.GetAllAsync();
        return properties.Select(MapToSummaryDto);
    }

    public async Task<IEnumerable<PropertySummaryDto>> GetByDealerIdAsync(Guid dealerId)
    {
        var properties = await _propertyRepository.GetByDealerIdAsync(dealerId);
        return properties.Select(MapToSummaryDto);
    }

    public async Task<IEnumerable<PropertySummaryDto>> GetFeaturedAsync(int limit = 10)
    {
        var properties = await _propertyRepository.GetFeaturedAsync(limit);
        return properties.Select(MapToSummaryDto);
    }

    public async Task<PropertySearchResponse> SearchAsync(PropertySearchRequest request)
    {
        var criteria = new PropertySearchCriteria
        {
            Query = request.Query,
            Type = request.Type,
            ListingType = request.ListingType,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            MinArea = request.MinArea,
            MaxArea = request.MaxArea,
            MinBedrooms = request.MinBedrooms,
            MaxBedrooms = request.MaxBedrooms,
            MinBathrooms = request.MinBathrooms,
            MaxBathrooms = request.MaxBathrooms,
            City = request.City,
            State = request.State,
            Neighborhood = request.Neighborhood,
            HasPool = request.HasPool,
            HasGarden = request.HasGarden,
            HasGym = request.HasGym,
            HasSecurity = request.HasSecurity,
            IsFurnished = request.IsFurnished,
            AllowsPets = request.AllowsPets,
            IsFeatured = request.IsFeatured,
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        var properties = await _propertyRepository.SearchAsync(criteria);
        var totalCount = await _propertyRepository.GetCountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new PropertySearchResponse(
            properties.Select(MapToSummaryDto),
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
    }

    public async Task<PropertyDto> CreateAsync(CreatePropertyRequest request, Guid dealerId, Guid sellerId, string sellerName)
    {
        var property = new Property
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            ListingType = request.ListingType,
            Status = PropertyStatus.Draft,
            Price = request.Price,
            Currency = request.Currency,
            MaintenanceFee = request.MaintenanceFee,
            IsNegotiable = request.IsNegotiable,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            Country = request.Country,
            Neighborhood = request.Neighborhood,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            TotalArea = request.TotalArea,
            BuiltArea = request.BuiltArea,
            LotArea = request.LotArea,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            HalfBathrooms = request.HalfBathrooms,
            ParkingSpaces = request.ParkingSpaces,
            Floor = request.Floor,
            TotalFloors = request.TotalFloors,
            YearBuilt = request.YearBuilt,
            HasGarden = request.HasGarden,
            HasPool = request.HasPool,
            HasGym = request.HasGym,
            HasElevator = request.HasElevator,
            HasSecurity = request.HasSecurity,
            IsFurnished = request.IsFurnished,
            AllowsPets = request.AllowsPets,
            SellerId = sellerId,
            SellerName = sellerName
        };

        property.PricePerSqMeter = property.TotalArea > 0
            ? Math.Round(property.Price / property.TotalArea, 2)
            : null;

        var created = await _propertyRepository.AddAsync(property);

        if (request.Amenities != null && request.Amenities.Any())
        {
            await _amenityRepository.SyncAmenitiesAsync(created.Id, request.Amenities);
        }

        return MapToDto(created);
    }

    public async Task<PropertyDto?> UpdateAsync(Guid id, UpdatePropertyRequest request)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null) return null;

        if (request.Title != null) property.Title = request.Title;
        if (request.Description != null) property.Description = request.Description;
        if (request.Type.HasValue) property.Type = request.Type.Value;
        if (request.ListingType.HasValue) property.ListingType = request.ListingType.Value;
        if (request.Price.HasValue) property.Price = request.Price.Value;
        if (request.MaintenanceFee.HasValue) property.MaintenanceFee = request.MaintenanceFee.Value;
        if (request.IsNegotiable.HasValue) property.IsNegotiable = request.IsNegotiable.Value;
        if (request.Address != null) property.Address = request.Address;
        if (request.City != null) property.City = request.City;
        if (request.State != null) property.State = request.State;
        if (request.ZipCode != null) property.ZipCode = request.ZipCode;
        if (request.Neighborhood != null) property.Neighborhood = request.Neighborhood;
        if (request.Latitude.HasValue) property.Latitude = request.Latitude.Value;
        if (request.Longitude.HasValue) property.Longitude = request.Longitude.Value;
        if (request.TotalArea.HasValue) property.TotalArea = request.TotalArea.Value;
        if (request.BuiltArea.HasValue) property.BuiltArea = request.BuiltArea.Value;
        if (request.LotArea.HasValue) property.LotArea = request.LotArea.Value;
        if (request.Bedrooms.HasValue) property.Bedrooms = request.Bedrooms.Value;
        if (request.Bathrooms.HasValue) property.Bathrooms = request.Bathrooms.Value;
        if (request.HalfBathrooms.HasValue) property.HalfBathrooms = request.HalfBathrooms.Value;
        if (request.ParkingSpaces.HasValue) property.ParkingSpaces = request.ParkingSpaces.Value;
        if (request.Floor.HasValue) property.Floor = request.Floor.Value;
        if (request.TotalFloors.HasValue) property.TotalFloors = request.TotalFloors.Value;
        if (request.YearBuilt.HasValue) property.YearBuilt = request.YearBuilt.Value;
        if (request.HasGarden.HasValue) property.HasGarden = request.HasGarden.Value;
        if (request.HasPool.HasValue) property.HasPool = request.HasPool.Value;
        if (request.HasGym.HasValue) property.HasGym = request.HasGym.Value;
        if (request.HasElevator.HasValue) property.HasElevator = request.HasElevator.Value;
        if (request.HasSecurity.HasValue) property.HasSecurity = request.HasSecurity.Value;
        if (request.IsFurnished.HasValue) property.IsFurnished = request.IsFurnished.Value;
        if (request.AllowsPets.HasValue) property.AllowsPets = request.AllowsPets.Value;

        property.UpdatedAt = DateTime.UtcNow;
        property.PricePerSqMeter = property.TotalArea > 0
            ? Math.Round(property.Price / property.TotalArea, 2)
            : null;

        var updated = await _propertyRepository.UpdateAsync(property);

        if (request.Amenities != null)
        {
            await _amenityRepository.SyncAmenitiesAsync(id, request.Amenities);
        }

        return MapToDto(updated);
    }

    public async Task<PropertyDto?> UpdateStatusAsync(Guid id, PropertyStatus status)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null) return null;

        property.Status = status;
        property.UpdatedAt = DateTime.UtcNow;

        if (status == PropertyStatus.Active && property.PublishedAt == null)
        {
            property.PublishedAt = DateTime.UtcNow;
        }

        var updated = await _propertyRepository.UpdateAsync(property);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await _propertyRepository.ExistsAsync(id))
            return false;

        await _propertyRepository.DeleteAsync(id);
        return true;
    }

    public async Task IncrementViewCountAsync(Guid id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property != null)
        {
            property.ViewCount++;
            await _propertyRepository.UpdateAsync(property);
        }
    }

    // ========================================
    // MAPPING
    // ========================================

    private PropertyDto MapToDto(Property p)
    {
        var images = p.Images?.Select(i => new PropertyImageDto(
            i.Id,
            i.Url,
            i.ThumbnailUrl,
            i.Caption,
            i.Category,
            i.SortOrder,
            i.IsPrimary
        )) ?? Enumerable.Empty<PropertyImageDto>();

        var amenitiesByCategory = (p.Amenities ?? Enumerable.Empty<PropertyAmenity>())
            .GroupBy(a => a.Category)
            .ToDictionary(g => g.Key, g => g.Select(a => a.Name));

        return new PropertyDto(
            p.Id,
            p.DealerId,
            p.Title,
            p.Description,
            p.Type,
            p.ListingType,
            p.Status,
            p.Price,
            p.Currency,
            p.PricePerSqMeter,
            p.MaintenanceFee,
            p.IsNegotiable,
            new PropertyLocationDto(
                p.Address,
                p.City,
                p.State,
                p.ZipCode,
                p.Country,
                p.Neighborhood,
                p.Latitude,
                p.Longitude
            ),
            new PropertyFeaturesDto(
                p.TotalArea,
                p.BuiltArea,
                p.LotArea,
                p.Bedrooms,
                p.Bathrooms,
                p.HalfBathrooms,
                p.ParkingSpaces,
                p.Floor,
                p.TotalFloors,
                p.YearBuilt,
                p.HasGarden,
                p.HasPool,
                p.HasGym,
                p.HasElevator,
                p.HasSecurity,
                p.IsFurnished,
                p.AllowsPets
            ),
            new PropertyAmenitiesDto(
                amenitiesByCategory.GetValueOrDefault(AmenityCategory.Security) ?? Enumerable.Empty<string>(),
                amenitiesByCategory.GetValueOrDefault(AmenityCategory.Recreation) ?? Enumerable.Empty<string>(),
                amenitiesByCategory.GetValueOrDefault(AmenityCategory.Comfort) ?? Enumerable.Empty<string>(),
                amenitiesByCategory.GetValueOrDefault(AmenityCategory.Services) ?? Enumerable.Empty<string>(),
                amenitiesByCategory.GetValueOrDefault(AmenityCategory.Outdoor) ?? Enumerable.Empty<string>(),
                amenitiesByCategory.GetValueOrDefault(AmenityCategory.Technology) ?? Enumerable.Empty<string>()
            ),
            new PropertySellerDto(
                p.SellerId,
                p.SellerName,
                p.SellerPhone,
                p.SellerEmail
            ),
            images,
            p.IsFeatured,
            p.ViewCount,
            p.FavoriteCount,
            p.InquiryCount,
            p.CreatedAt,
            p.UpdatedAt,
            p.PublishedAt
        );
    }

    private PropertySummaryDto MapToSummaryDto(Property p)
    {
        var primaryImage = p.Images?.FirstOrDefault(i => i.IsPrimary)?.Url
            ?? p.Images?.FirstOrDefault()?.Url;

        return new PropertySummaryDto(
            p.Id,
            p.Title,
            p.Type,
            p.ListingType,
            p.Price,
            p.Currency,
            p.City,
            p.State,
            p.TotalArea,
            p.Bedrooms,
            p.Bathrooms,
            p.ParkingSpaces,
            primaryImage,
            p.IsFeatured,
            p.CreatedAt
        );
    }
}
