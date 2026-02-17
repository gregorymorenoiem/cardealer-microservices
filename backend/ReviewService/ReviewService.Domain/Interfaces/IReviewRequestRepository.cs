using ReviewService.Domain.Entities;
using ReviewService.Domain.Base;

namespace ReviewService.Domain.Interfaces;

/// <summary>
/// Repositorio para solicitudes de review
/// </summary>
public interface IReviewRequestRepository : IRepository<ReviewRequest, Guid>
{
    /// <summary>
    /// Obtiene solicitudes pendientes que deben enviarse
    /// </summary>
    /// <param name="daysAfterPurchase">Días después de la compra para enviar</param>
    /// <param name="limit">Límite de solicitudes a obtener</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de solicitudes pendientes</returns>
    Task<List<ReviewRequest>> GetPendingRequestsAsync(int daysAfterPurchase = 7, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene solicitudes que necesitan recordatorio
    /// </summary>
    /// <param name="daysAfterRequest">Días después de la solicitud inicial</param>
    /// <param name="maxReminders">Máximo número de recordatorios</param>
    /// <param name="limit">Límite de solicitudes</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de solicitudes para recordatorio</returns>
    Task<List<ReviewRequest>> GetReminderRequestsAsync(int daysAfterRequest = 3, int maxReminders = 2, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una solicitud por token
    /// </summary>
    /// <param name="token">Token único de la solicitud</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Solicitud si existe, null si no</returns>
    Task<ReviewRequest?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene solicitudes de un comprador
    /// </summary>
    /// <param name="buyerId">ID del comprador</param>
    /// <param name="status">Estado específico (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de solicitudes del comprador</returns>
    Task<List<ReviewRequest>> GetByBuyerIdAsync(Guid buyerId, ReviewRequestStatus? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene solicitudes de un vendedor
    /// </summary>
    /// <param name="sellerId">ID del vendedor</param>
    /// <param name="status">Estado específico (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de solicitudes del vendedor</returns>
    Task<List<ReviewRequest>> GetBySellerIdAsync(Guid sellerId, ReviewRequestStatus? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca una solicitud como completada
    /// </summary>
    /// <param name="requestId">ID de la solicitud</param>
    /// <param name="reviewId">ID de la review creada</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se actualizó</returns>
    Task<bool> MarkAsCompletedAsync(Guid requestId, Guid reviewId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Expira solicitudes vencidas
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de solicitudes expiradas</returns>
    Task<int> ExpireOldRequestsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de solicitudes
    /// </summary>
    /// <param name="sellerId">ID del vendedor (opcional)</param>
    /// <param name="fromDate">Fecha desde (opcional)</param>
    /// <param name="toDate">Fecha hasta (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas de solicitudes</returns>
    Task<Dictionary<ReviewRequestStatus, int>> GetStatsAsync(Guid? sellerId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si ya existe una solicitud para una compra
    /// </summary>
    /// <param name="orderId">ID de la orden</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si ya existe</returns>
    Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}