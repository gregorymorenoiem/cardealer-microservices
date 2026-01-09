using ReviewService.Domain.Entities;
using ReviewService.Domain.Base;

namespace ReviewService.Domain.Interfaces;

/// <summary>
/// Repositorio para votos de utilidad de reviews
/// </summary>
public interface IReviewHelpfulVoteRepository : IRepository<ReviewHelpfulVote, Guid>
{
    /// <summary>
    /// Obtiene el voto de un usuario específico para una review
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Voto si existe, null si no</returns>
    Task<ReviewHelpfulVote?> GetByReviewAndUserAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de votos para una review
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tupla con (helpful votes, total votes)</returns>
    Task<(int helpfulVotes, int totalVotes)> GetVoteStatsAsync(Guid reviewId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los votos de una review
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de votos</returns>
    Task<List<ReviewHelpfulVote>> GetByReviewIdAsync(Guid reviewId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un usuario ya votó en una review
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si ya votó</returns>
    Task<bool> HasUserVotedAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene votos sospechosos (misma IP, patrones)
    /// </summary>
    /// <param name="ipAddress">Dirección IP a verificar</param>
    /// <param name="timeWindow">Ventana de tiempo en horas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de votos sospechosos</returns>
    Task<List<ReviewHelpfulVote>> GetSuspiciousVotesByIpAsync(string ipAddress, int timeWindow = 24, CancellationToken cancellationToken = default);
}