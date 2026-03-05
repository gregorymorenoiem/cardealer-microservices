using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdvertisingService.Domain.Entities;

namespace AdvertisingService.Api.Controllers;

/// <summary>
/// Catálogo de productos publicitarios OKLA.
/// Endpoint público — los precios son visibles para todos.
/// </summary>
[ApiController]
[Route("api/advertising/catalog")]
public class CatalogController : ControllerBase
{
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(ILogger<CatalogController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los productos publicitarios disponibles
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public ActionResult<CatalogResponse> GetCatalog()
    {
        var products = AdvertisingProductCatalog.GetAll()
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(MapToDto)
            .ToList();

        return Ok(new CatalogResponse(
            Products: products,
            Currency: "USD",
            CoinConversion: "1 OKLA Coin = $0.01 USD",
            TotalProducts: products.Count
        ));
    }

    /// <summary>
    /// Obtiene un producto por su slug
    /// </summary>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    public ActionResult<AdProductDto> GetBySlug(string slug)
    {
        var product = AdvertisingProductCatalog.GetBySlug(slug);
        if (product == null)
            return NotFound(new { error = $"Product '{slug}' not found" });

        return Ok(MapToDto(product));
    }

    /// <summary>
    /// Obtiene productos por categoría
    /// </summary>
    [HttpGet("category/{category}")]
    [AllowAnonymous]
    public ActionResult<List<AdProductDto>> GetByCategory(string category)
    {
        if (!Enum.TryParse<AdProductCategory>(category, true, out var cat))
            return BadRequest(new { error = $"Invalid category '{category}'. Valid: Visibility, Display, DirectMarketing, Bundle" });

        var products = AdvertisingProductCatalog.GetByCategory(cat)
            .Where(p => p.IsActive)
            .Select(MapToDto)
            .ToList();

        return Ok(products);
    }

    /// <summary>
    /// Calcula precio estimado para un producto y duración
    /// </summary>
    [HttpGet("{slug}/estimate")]
    [AllowAnonymous]
    public ActionResult<PriceEstimateDto> GetPriceEstimate(
        string slug,
        [FromQuery] string duration = "month",
        [FromQuery] int quantity = 1)
    {
        var product = AdvertisingProductCatalog.GetBySlug(slug);
        if (product == null)
            return NotFound(new { error = $"Product '{slug}' not found" });

        decimal? price = duration.ToLower() switch
        {
            "day" => product.PricePerDay,
            "week" => product.PricePerWeek,
            "month" => product.PricePerMonth,
            _ => null
        };

        if (price == null)
            return BadRequest(new { error = $"Product '{slug}' is not available for '{duration}' pricing" });

        int? coinsPrice = duration.ToLower() switch
        {
            "day" => product.CoinsPricePerDay,
            "week" => product.CoinsPricePerWeek,
            "month" => product.CoinsPricePerMonth,
            _ => null
        };

        var totalUsd = price.Value * quantity;
        var totalCoins = (coinsPrice ?? 0) * quantity;

        return Ok(new PriceEstimateDto(
            ProductSlug: slug,
            ProductName: product.Name,
            Duration: duration,
            Quantity: quantity,
            UnitPriceUsd: price.Value,
            TotalPriceUsd: totalUsd,
            UnitPriceCoins: coinsPrice,
            TotalPriceCoins: totalCoins,
            EstimatedMargin: totalUsd - (product.EstimatedCost * quantity),
            Currency: "USD"
        ));
    }

    // ── Mapping ──

    private static AdProductDto MapToDto(AdvertisingProduct p) => new(
        Id: p.Id.ToString(),
        Slug: p.Slug,
        Name: p.Name,
        Description: p.Description,
        Category: p.Category.ToString(),
        PricePerDay: p.PricePerDay,
        PricePerWeek: p.PricePerWeek,
        PricePerMonth: p.PricePerMonth,
        CoinsPricePerDay: p.CoinsPricePerDay,
        CoinsPricePerWeek: p.CoinsPricePerWeek,
        CoinsPricePerMonth: p.CoinsPricePerMonth,
        MaxSimultaneous: p.MaxSimultaneous,
        Scope: p.Scope.ToString(),
        IsBundle: p.IsBundle,
        BundleSavings: p.BundleSavings,
        DisplayOrder: p.DisplayOrder
    );
}

// ── DTOs ──

public record CatalogResponse(
    List<AdProductDto> Products,
    string Currency,
    string CoinConversion,
    int TotalProducts
);

public record AdProductDto(
    string Id,
    string Slug,
    string Name,
    string Description,
    string Category,
    decimal? PricePerDay,
    decimal? PricePerWeek,
    decimal? PricePerMonth,
    int? CoinsPricePerDay,
    int? CoinsPricePerWeek,
    int? CoinsPricePerMonth,
    int? MaxSimultaneous,
    string Scope,
    bool IsBundle,
    decimal? BundleSavings,
    int DisplayOrder
);

public record PriceEstimateDto(
    string ProductSlug,
    string ProductName,
    string Duration,
    int Quantity,
    decimal UnitPriceUsd,
    decimal TotalPriceUsd,
    int? UnitPriceCoins,
    int TotalPriceCoins,
    decimal EstimatedMargin,
    string Currency
);
