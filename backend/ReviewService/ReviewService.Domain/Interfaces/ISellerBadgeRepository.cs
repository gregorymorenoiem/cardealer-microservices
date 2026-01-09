using ReviewService.Domain.Entities;
using ReviewService.Domain.Base;

namespace ReviewService.Domain.Interfaces;

/// <summary>
/// Repositorio para badges de vendedores
/// </summary>
public interface ISellerBadgeRepository : IRepository<SellerBadge, Guid>
{
    /// <summary>
    /// Obtiene todos los badges activos de un vendedor
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de badges ordenados por DisplayOrder</returns>
    Task<List<SellerBadge>> GetActiveBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un badge específico de un vendedor por tipo
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <param name="badgeType">Tipo de badge</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Badge si existe, null si no</returns>
    Task<SellerBadge?> GetBySellerAndTypeAsync(Guid sellerId, BadgeType badgeType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Otorga un badge a un vendedor (o actualiza existente)
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <param name="badgeType">Tipo de badge</param>
    /// <param name="criteria">Criterios cumplidos</param>
    /// <param name="expiresAt">Fecha de expiración (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Badge otorgado</returns>
    Task<SellerBadge> GrantBadgeAsync(Guid sellerId, BadgeType badgeType, string criteria, DateTime? expiresAt = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoca un badge de un vendedor
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <param name="badgeType">Tipo de badge</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se revocó, false si no existía</returns>
    Task<bool> RevokeBadgeAsync(Guid sellerId, BadgeType badgeType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene badges que están próximos a expirar
    /// </summary>
    /// <param name="daysBeforeExpiry">Días antes de la expiración</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de badges próximos a expirar</returns>
    Task<List<SellerBadge>> GetExpiringBadgesAsync(int daysBeforeExpiry = 7, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de badges por tipo
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Diccionario con cuenta por tipo de badge</returns>
    Task<Dictionary<BadgeType, int>> GetBadgeStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un vendedor es elegible para un badge específico
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <param name="badgeType">Tipo de badge</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si es elegible</returns>
    Task<bool> IsEligibleForBadgeAsync(Guid sellerId, BadgeType badgeType, CancellationToken cancellationToken = default);
}