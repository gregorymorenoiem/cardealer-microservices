using BackgroundRemovalService.Application.DTOs;
using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Application.Interfaces;

/// <summary>
/// Servicio principal de orquestación para remoción de fondo.
/// Coordina entre proveedores, maneja reintentos y fallbacks.
/// </summary>
public interface IBackgroundRemovalOrchestrator
{
    /// <summary>
    /// Procesa una solicitud de remoción de fondo
    /// </summary>
    Task<RemovalJobResponse> ProcessRemovalAsync(
        CreateRemovalJobRequest request, 
        Guid? userId = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Procesa un job pendiente por ID
    /// </summary>
    Task<RemovalJobResponse> ProcessJobAsync(
        Guid jobId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reintenta un job fallido
    /// </summary>
    Task<RemovalJobResponse> RetryJobAsync(
        Guid jobId, 
        BackgroundRemovalProvider? alternateProvider = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancela un job pendiente
    /// </summary>
    Task<bool> CancelJobAsync(
        Guid jobId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene el estado de un job
    /// </summary>
    Task<RemovalJobResponse?> GetJobStatusAsync(
        Guid jobId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene jobs del usuario
    /// </summary>
    Task<RemovalJobListResponse> GetUserJobsAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene estadísticas de uso
    /// </summary>
    Task<UsageStatisticsResponse> GetUsageStatisticsAsync(
        Guid? userId = null,
        int? billingPeriod = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica la salud de todos los proveedores
    /// </summary>
    Task<IEnumerable<ProviderHealthResponse>> CheckProvidersHealthAsync(
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene información de todos los proveedores
    /// </summary>
    Task<IEnumerable<ProviderInfoResponse>> GetProvidersInfoAsync(
        CancellationToken cancellationToken = default);
}
