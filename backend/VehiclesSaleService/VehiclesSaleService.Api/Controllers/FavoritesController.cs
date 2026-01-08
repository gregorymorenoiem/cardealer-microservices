using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VehiclesSaleService.Domain.Entities;
using VehiclesSaleService.Domain.Interfaces;
using System.Security.Claims;

namespace VehiclesSaleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(
        IFavoriteRepository favoriteRepository,
        IVehicleRepository vehicleRepository,
        ILogger<FavoritesController> logger)
    {
        _favoriteRepository = favoriteRepository;
        _vehicleRepository = vehicleRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene los vehículos favoritos del usuario actual
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetMyFavorites()
    {
        var userId = GetCurrentUserId();
        var vehicles = await _favoriteRepository.GetFavoriteVehiclesByUserIdAsync(userId);

        return Ok(vehicles.Select(MapToDto));
    }

    /// <summary>
    /// Obtiene el conteo de favoritos del usuario actual
    /// </summary>
    [HttpGet("count")]
    [ProducesResponseType(typeof(FavoriteCountResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FavoriteCountResponse>> GetCount()
    {
        var userId = GetCurrentUserId();
        var count = await _favoriteRepository.GetCountByUserIdAsync(userId);

        return Ok(new FavoriteCountResponse { Count = count });
    }

    /// <summary>
    /// Verifica si un vehículo está en favoritos
    /// </summary>
    [HttpGet("check/{vehicleId:guid}")]
    [ProducesResponseType(typeof(IsFavoriteResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<IsFavoriteResponse>> Check(Guid vehicleId)
    {
        var userId = GetCurrentUserId();
        var isFavorite = await _favoriteRepository.IsFavoriteAsync(userId, vehicleId);

        return Ok(new IsFavoriteResponse { IsFavorite = isFavorite });
    }

    /// <summary>
    /// Añade un vehículo a favoritos
    /// </summary>
    [HttpPost("{vehicleId:guid}")]
    [ProducesResponseType(typeof(FavoriteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FavoriteResponse>> AddFavorite(
        Guid vehicleId,
        [FromBody] AddFavoriteRequest? request = null)
    {
        var userId = GetCurrentUserId();

        // Verificar que el vehículo existe
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
        if (vehicle == null)
            return NotFound(new { message = "Vehículo no encontrado" });

        // Verificar si ya existe
        var existing = await _favoriteRepository.GetByUserAndVehicleAsync(userId, vehicleId);
        if (existing != null)
            return BadRequest(new { message = "El vehículo ya está en favoritos" });

        // Crear favorito
        var favorite = new Favorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VehicleId = vehicleId,
            CreatedAt = DateTime.UtcNow,
            Notes = request?.Notes,
            NotifyPriceChange = request?.NotifyPriceChange ?? false,
            DealerId = Guid.Empty // Usuario final
        };

        await _favoriteRepository.CreateAsync(favorite);

        _logger.LogInformation(
            "User {UserId} added vehicle {VehicleId} to favorites",
            userId, vehicleId);

        return CreatedAtAction(
            nameof(Check),
            new { vehicleId },
            new FavoriteResponse
            {
                Id = favorite.Id,
                UserId = favorite.UserId,
                VehicleId = favorite.VehicleId,
                CreatedAt = favorite.CreatedAt,
                Notes = favorite.Notes,
                NotifyPriceChange = favorite.NotifyPriceChange
            });
    }

    /// <summary>
    /// Elimina un vehículo de favoritos
    /// </summary>
    [HttpDelete("{vehicleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFavorite(Guid vehicleId)
    {
        var userId = GetCurrentUserId();

        var favorite = await _favoriteRepository.GetByUserAndVehicleAsync(userId, vehicleId);
        if (favorite == null)
            return NotFound(new { message = "Favorito no encontrado" });

        await _favoriteRepository.DeleteAsync(favorite.Id);

        _logger.LogInformation(
            "User {UserId} removed vehicle {VehicleId} from favorites",
            userId, vehicleId);

        return NoContent();
    }

    /// <summary>
    /// Actualiza las notas de un favorito
    /// </summary>
    [HttpPut("{vehicleId:guid}")]
    [ProducesResponseType(typeof(FavoriteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FavoriteResponse>> UpdateFavorite(
        Guid vehicleId,
        [FromBody] UpdateFavoriteRequest request)
    {
        var userId = GetCurrentUserId();

        var favorite = await _favoriteRepository.GetByUserAndVehicleAsync(userId, vehicleId);
        if (favorite == null)
            return NotFound(new { message = "Favorito no encontrado" });

        // Actualizar
        if (request.Notes != null)
            favorite.Notes = request.Notes;
        if (request.NotifyPriceChange.HasValue)
            favorite.NotifyPriceChange = request.NotifyPriceChange.Value;

        // Nota: En un repositorio completo tendríamos UpdateAsync,
        // pero como no está definido, recrearemos el objeto
        await _favoriteRepository.DeleteAsync(favorite.Id);
        await _favoriteRepository.CreateAsync(favorite);

        return Ok(new FavoriteResponse
        {
            Id = favorite.Id,
            UserId = favorite.UserId,
            VehicleId = favorite.VehicleId,
            CreatedAt = favorite.CreatedAt,
            Notes = favorite.Notes,
            NotifyPriceChange = favorite.NotifyPriceChange
        });
    }

    // Helper: Obtener UserId del token JWT
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value 
            ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }

        return userId;
    }

    // Helper: Mapear Vehicle a DTO
    private static VehicleDto MapToDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            Title = vehicle.Title,
            Description = vehicle.Description,
            Price = vehicle.Price,
            Currency = vehicle.Currency,
            Status = vehicle.Status.ToString(),
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Mileage = vehicle.Mileage,
            FuelType = vehicle.FuelType.ToString(),
            Transmission = vehicle.Transmission.ToString(),
            BodyStyle = vehicle.BodyStyle.ToString(),
            PrimaryImageUrl = vehicle.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.S3Url,
            SellerName = vehicle.SellerName,
            ViewCount = vehicle.ViewCount,
            FavoriteCount = vehicle.FavoriteCount,
            CreatedAt = vehicle.CreatedAt
        };
    }
}

#region DTOs

public record AddFavoriteRequest
{
    public string? Notes { get; init; }
    public bool NotifyPriceChange { get; init; } = false;
}

public record UpdateFavoriteRequest
{
    public string? Notes { get; init; }
    public bool? NotifyPriceChange { get; init; }
}

public record FavoriteResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid VehicleId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? Notes { get; init; }
    public bool NotifyPriceChange { get; init; }
}

public record FavoriteCountResponse
{
    public int Count { get; init; }
}

public record IsFavoriteResponse
{
    public bool IsFavorite { get; init; }
}

public record VehicleDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public string Status { get; init; } = string.Empty;
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public int? Mileage { get; init; }
    public string FuelType { get; init; } = string.Empty;
    public string Transmission { get; init; } = string.Empty;
    public string BodyStyle { get; init; } = string.Empty;
    public string? PrimaryImageUrl { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public int ViewCount { get; init; }
    public int FavoriteCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

#endregion
