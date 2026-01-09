using ReviewService.Domain.Entities;

namespace ReviewService.Domain.Services;

/// <summary>
/// Servicio de dominio para cálculo de badges de vendedor
/// </summary>
public interface IBadgeCalculationService
{
    /// <summary>
    /// Calcula si un vendedor es elegible para un badge específico
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <param name="badgeType">Tipo de badge</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si es elegible</returns>
    Task<bool> IsEligibleForBadgeAsync(Guid sellerId, BadgeType badgeType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula y otorga badges automáticamente para un vendedor
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de badges otorgados</returns>
    Task<List<BadgeType>> CalculateAndGrantBadgesAsync(Guid sellerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula badges para todos los vendedores activos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Diccionario con vendedor y badges otorgados</returns>
    Task<Dictionary<Guid, List<BadgeType>>> CalculateBadgesForAllSellersAsync(CancellationToken cancellationToken = default);
}