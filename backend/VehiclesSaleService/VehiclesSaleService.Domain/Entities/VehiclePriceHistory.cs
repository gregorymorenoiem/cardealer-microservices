using CarDealer.Shared.MultiTenancy;

namespace VehiclesSaleService.Domain.Entities;

/// <summary>
/// Historial de cambios de precio de un vehículo.
/// Cada vez que un dealer/seller modifica el precio, se registra el cambio
/// para construir la línea de tiempo de precios visible al comprador (transparencia)
/// y como parte del OKLA Platform Score.
/// 
/// SWITCHING COST: Si el dealer retira el vehículo de OKLA, pierde este historial acumulado.
/// </summary>
public class VehiclePriceHistory : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DealerId { get; set; } // Multi-tenant

    /// <summary>Vehículo al que pertenece este cambio de precio</summary>
    public Guid VehicleId { get; set; }

    /// <summary>Precio anterior antes del cambio</summary>
    public decimal OldPrice { get; set; }

    /// <summary>Nuevo precio después del cambio</summary>
    public decimal NewPrice { get; set; }

    /// <summary>Diferencia de precio (NewPrice - OldPrice). Negativo = bajó.</summary>
    public decimal PriceDifference => NewPrice - OldPrice;

    /// <summary>Porcentaje de cambio respecto al precio anterior</summary>
    public decimal ChangePercentage => OldPrice > 0
        ? Math.Round((NewPrice - OldPrice) / OldPrice * 100, 2)
        : 0;

    /// <summary>Moneda del precio</summary>
    public string Currency { get; set; } = "DOP";

    /// <summary>Quién realizó el cambio (SellerId o AdminId)</summary>
    public Guid ChangedBy { get; set; }

    /// <summary>Razón del cambio de precio (opcional, visible al comprador)</summary>
    public string? Reason { get; set; }

    /// <summary>Tipo de cambio de precio</summary>
    public PriceChangeType ChangeType { get; set; } = PriceChangeType.Manual;

    /// <summary>Momento exacto del cambio</summary>
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Vehicle? Vehicle { get; set; }
}

/// <summary>
/// Tipos de cambio de precio
/// </summary>
public enum PriceChangeType
{
    /// <summary>Precio inicial al publicar</summary>
    InitialListing = 0,

    /// <summary>Cambio manual por el dealer/seller</summary>
    Manual = 1,

    /// <summary>Ajuste por campaña promocional</summary>
    CampaignAdjustment = 2,

    /// <summary>Reducción automática por tiempo en plataforma (futura feature)</summary>
    AutoReduction = 3,

    /// <summary>Ajuste por admin/moderador</summary>
    AdminAdjustment = 4
}
