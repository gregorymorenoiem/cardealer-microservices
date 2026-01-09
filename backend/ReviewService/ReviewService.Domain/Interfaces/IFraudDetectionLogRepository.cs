using ReviewService.Domain.Entities;
using ReviewService.Domain.Base;

namespace ReviewService.Domain.Interfaces;

/// <summary>
/// Repositorio para logs de detección de fraude
/// </summary>
public interface IFraudDetectionLogRepository : IRepository<FraudDetectionLog, Guid>
{
    /// <summary>
    /// Registra un resultado de verificación anti-fraude
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <param name="checkType">Tipo de verificación</param>
    /// <param name="result">Resultado de la verificación</param>
    /// <param name="confidenceScore">Score de confianza (0-100)</param>
    /// <param name="details">Detalles de la verificación</param>
    /// <param name="metadata">Metadata adicional (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Log creado</returns>
    Task<FraudDetectionLog> LogCheckAsync(
        Guid reviewId,
        FraudCheckType checkType,
        FraudCheckResult result,
        int confidenceScore,
        string details,
        string? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene logs de una review específica
    /// </summary>
    /// <param name="reviewId">ID de la review</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de logs ordenados por fecha</returns>
    Task<List<FraudDetectionLog>> GetByReviewIdAsync(Guid reviewId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene logs por tipo de verificación
    /// </summary>
    /// <param name="checkType">Tipo de verificación</param>
    /// <param name="result">Resultado específico (opcional)</param>
    /// <param name="fromDate">Fecha desde (opcional)</param>
    /// <param name="toDate">Fecha hasta (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de logs</returns>
    Task<List<FraudDetectionLog>> GetByCheckTypeAsync(
        FraudCheckType checkType,
        FraudCheckResult? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene reviews con múltiples flags de fraude
    /// </summary>
    /// <param name="minFailedChecks">Mínimo número de verificaciones fallidas</param>
    /// <param name="timeWindow">Ventana de tiempo en horas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de reviews sospechosas</returns>
    Task<List<Guid>> GetSuspiciousReviewsAsync(int minFailedChecks = 2, int timeWindow = 24, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de detección de fraude
    /// </summary>
    /// <param name="fromDate">Fecha desde (opcional)</param>
    /// <param name="toDate">Fecha hasta (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas por tipo de verificación y resultado</returns>
    Task<Dictionary<FraudCheckType, Dictionary<FraudCheckResult, int>>> GetStatsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene patrones sospechosos de IP
    /// </summary>
    /// <param name="ipAddress">Dirección IP</param>
    /// <param name="timeWindow">Ventana de tiempo en horas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de logs de esa IP</returns>
    Task<List<FraudDetectionLog>> GetByIpPatternAsync(string ipAddress, int timeWindow = 24, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene score promedio de confianza para reviews de un usuario
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Score promedio de confianza</returns>
    Task<double> GetAverageConfidenceScoreForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}