using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Locations.Queries;

// ============================================
// Get Dealer Locations Query
// ============================================
public record GetDealerLocationsQuery(Guid DealerId) : IRequest<List<DealerLocationDto>>;

public class GetDealerLocationsQueryHandler : IRequestHandler<GetDealerLocationsQuery, List<DealerLocationDto>>
{
    private readonly IDealerLocationRepository _locationRepository;

    public GetDealerLocationsQueryHandler(IDealerLocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<List<DealerLocationDto>> Handle(GetDealerLocationsQuery request, CancellationToken cancellationToken)
    {
        var locations = await _locationRepository.GetByDealerIdAsync(request.DealerId, cancellationToken);
        
        return locations.Select(MapToDto).ToList();
    }

    private static DealerLocationDto MapToDto(DealerLocation l) => new(
        l.Id,
        l.DealerId,
        l.Name,
        l.Type.ToString(),
        l.Address,
        l.City,
        l.Province,
        l.ZipCode,
        l.Country,
        l.Phone,
        l.Email,
        l.WorkingHours,
        l.IsPrimary,
        l.IsActive,
        l.Latitude,
        l.Longitude,
        l.HasShowroom,
        l.HasServiceCenter,
        l.HasParking,
        l.ParkingSpaces,
        l.CreatedAt,
        l.UpdatedAt);
}

// ============================================
// Get Location By Id Query
// ============================================
public record GetLocationByIdQuery(Guid DealerId, Guid LocationId) : IRequest<DealerLocationDto?>;

public class GetLocationByIdQueryHandler : IRequestHandler<GetLocationByIdQuery, DealerLocationDto?>
{
    private readonly IDealerLocationRepository _locationRepository;

    public GetLocationByIdQueryHandler(IDealerLocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<DealerLocationDto?> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(request.LocationId, cancellationToken);
        
        if (location == null || location.DealerId != request.DealerId || location.IsDeleted)
        {
            return null;
        }

        return new DealerLocationDto(
            location.Id,
            location.DealerId,
            location.Name,
            location.Type.ToString(),
            location.Address,
            location.City,
            location.Province,
            location.ZipCode,
            location.Country,
            location.Phone,
            location.Email,
            location.WorkingHours,
            location.IsPrimary,
            location.IsActive,
            location.Latitude,
            location.Longitude,
            location.HasShowroom,
            location.HasServiceCenter,
            location.HasParking,
            location.ParkingSpaces,
            location.CreatedAt,
            location.UpdatedAt);
    }
}
