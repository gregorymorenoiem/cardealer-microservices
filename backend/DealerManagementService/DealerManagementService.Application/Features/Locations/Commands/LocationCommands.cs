using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Locations.Commands;

// ============================================
// Request DTOs
// ============================================
public record CreateLocationRequest(
    string Name,
    string Type,
    string Address,
    string City,
    string Province,
    string? ZipCode = null,
    string? Phone = null,
    string? Email = null,
    string? WorkingHours = null,
    bool IsPrimary = false,
    double? Latitude = null,
    double? Longitude = null,
    bool HasShowroom = false,
    bool HasServiceCenter = false,
    bool HasParking = false,
    int? ParkingSpaces = null);

public record UpdateLocationRequest(
    string? Name = null,
    string? Type = null,
    string? Address = null,
    string? City = null,
    string? Province = null,
    string? ZipCode = null,
    string? Phone = null,
    string? Email = null,
    string? WorkingHours = null,
    bool? IsActive = null,
    double? Latitude = null,
    double? Longitude = null,
    bool? HasShowroom = null,
    bool? HasServiceCenter = null,
    bool? HasParking = null,
    int? ParkingSpaces = null);

// ============================================
// Create Location Command
// ============================================
public record CreateLocationCommand(Guid DealerId, CreateLocationRequest Request) : IRequest<DealerLocationDto>;

public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, DealerLocationDto>
{
    private readonly IDealerLocationRepository _locationRepository;
    private readonly IDealerRepository _dealerRepository;

    public CreateLocationCommandHandler(
        IDealerLocationRepository locationRepository,
        IDealerRepository dealerRepository)
    {
        _locationRepository = locationRepository;
        _dealerRepository = dealerRepository;
    }

    public async Task<DealerLocationDto> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        // Verify dealer exists
        var dealer = await _dealerRepository.GetByIdAsync(command.DealerId, cancellationToken);
        if (dealer == null)
        {
            throw new KeyNotFoundException($"Dealer {command.DealerId} not found");
        }

        // Parse location type
        if (!Enum.TryParse<LocationType>(command.Request.Type, true, out var locationType))
        {
            locationType = LocationType.Branch;
        }

        // If setting as primary, unset current primary
        if (command.Request.IsPrimary)
        {
            var currentPrimary = await _locationRepository.GetPrimaryLocationAsync(command.DealerId, cancellationToken);
            if (currentPrimary != null)
            {
                currentPrimary.IsPrimary = false;
                await _locationRepository.UpdateAsync(currentPrimary, cancellationToken);
            }
        }

        var location = new DealerLocation
        {
            Id = Guid.NewGuid(),
            DealerId = command.DealerId,
            Name = command.Request.Name,
            Type = locationType,
            Address = command.Request.Address,
            City = command.Request.City,
            Province = command.Request.Province,
            ZipCode = command.Request.ZipCode,
            Phone = command.Request.Phone ?? string.Empty,
            Email = command.Request.Email,
            WorkingHours = command.Request.WorkingHours,
            IsPrimary = command.Request.IsPrimary,
            IsActive = true,
            Latitude = command.Request.Latitude,
            Longitude = command.Request.Longitude,
            HasShowroom = command.Request.HasShowroom,
            HasServiceCenter = command.Request.HasServiceCenter,
            HasParking = command.Request.HasParking,
            ParkingSpaces = command.Request.ParkingSpaces,
            CreatedAt = DateTime.UtcNow
        };

        await _locationRepository.AddAsync(location, cancellationToken);

        return MapToDto(location);
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
// Update Location Command
// ============================================
public record UpdateLocationCommand(Guid DealerId, Guid LocationId, UpdateLocationRequest Request) : IRequest<DealerLocationDto>;

public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, DealerLocationDto>
{
    private readonly IDealerLocationRepository _locationRepository;

    public UpdateLocationCommandHandler(IDealerLocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<DealerLocationDto> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        
        if (location == null || location.DealerId != command.DealerId || location.IsDeleted)
        {
            throw new KeyNotFoundException($"Location {command.LocationId} not found");
        }

        var req = command.Request;

        if (req.Name != null) location.Name = req.Name;
        if (req.Type != null && Enum.TryParse<LocationType>(req.Type, true, out var locationType))
            location.Type = locationType;
        if (req.Address != null) location.Address = req.Address;
        if (req.City != null) location.City = req.City;
        if (req.Province != null) location.Province = req.Province;
        if (req.ZipCode != null) location.ZipCode = req.ZipCode;
        if (req.Phone != null) location.Phone = req.Phone;
        if (req.Email != null) location.Email = req.Email;
        if (req.WorkingHours != null) location.WorkingHours = req.WorkingHours;
        if (req.IsActive.HasValue) location.IsActive = req.IsActive.Value;
        if (req.Latitude.HasValue) location.Latitude = req.Latitude;
        if (req.Longitude.HasValue) location.Longitude = req.Longitude;
        if (req.HasShowroom.HasValue) location.HasShowroom = req.HasShowroom.Value;
        if (req.HasServiceCenter.HasValue) location.HasServiceCenter = req.HasServiceCenter.Value;
        if (req.HasParking.HasValue) location.HasParking = req.HasParking.Value;
        if (req.ParkingSpaces.HasValue) location.ParkingSpaces = req.ParkingSpaces;

        location.UpdatedAt = DateTime.UtcNow;

        await _locationRepository.UpdateAsync(location, cancellationToken);

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

// ============================================
// Delete Location Command
// ============================================
public record DeleteLocationCommand(Guid DealerId, Guid LocationId) : IRequest<Unit>;

public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, Unit>
{
    private readonly IDealerLocationRepository _locationRepository;

    public DeleteLocationCommandHandler(IDealerLocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Unit> Handle(DeleteLocationCommand command, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        
        if (location == null || location.DealerId != command.DealerId)
        {
            throw new KeyNotFoundException($"Location {command.LocationId} not found");
        }

        if (location.IsPrimary)
        {
            throw new InvalidOperationException("Cannot delete primary location. Set another location as primary first.");
        }

        await _locationRepository.DeleteAsync(command.LocationId, cancellationToken);
        
        return Unit.Value;
    }
}

// ============================================
// Set Primary Location Command
// ============================================
public record SetPrimaryLocationCommand(Guid DealerId, Guid LocationId) : IRequest<Unit>;

public class SetPrimaryLocationCommandHandler : IRequestHandler<SetPrimaryLocationCommand, Unit>
{
    private readonly IDealerLocationRepository _locationRepository;

    public SetPrimaryLocationCommandHandler(IDealerLocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<Unit> Handle(SetPrimaryLocationCommand command, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        
        if (location == null || location.DealerId != command.DealerId || location.IsDeleted)
        {
            throw new KeyNotFoundException($"Location {command.LocationId} not found");
        }

        // Unset current primary
        var currentPrimary = await _locationRepository.GetPrimaryLocationAsync(command.DealerId, cancellationToken);
        if (currentPrimary != null && currentPrimary.Id != command.LocationId)
        {
            currentPrimary.IsPrimary = false;
            currentPrimary.UpdatedAt = DateTime.UtcNow;
            await _locationRepository.UpdateAsync(currentPrimary, cancellationToken);
        }

        // Set new primary
        location.IsPrimary = true;
        location.UpdatedAt = DateTime.UtcNow;
        await _locationRepository.UpdateAsync(location, cancellationToken);

        return Unit.Value;
    }
}
