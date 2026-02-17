using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Domain.Entities.Privacy;

namespace UserService.Domain.Interfaces;

/// <summary>
/// Repositorio para gestionar solicitudes de privacidad ARCO (Ley 172-13)
/// </summary>
public interface IPrivacyRequestRepository
{
    /// <summary>
    /// Obtiene una solicitud por ID
    /// </summary>
    Task<PrivacyRequest?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtiene la solicitud de eliminación pendiente de un usuario
    /// </summary>
    Task<PrivacyRequest?> GetPendingDeletionRequestAsync(Guid userId);

    /// <summary>
    /// Obtiene todas las solicitudes de un usuario
    /// </summary>
    Task<IEnumerable<PrivacyRequest>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Agrega una nueva solicitud
    /// </summary>
    Task<PrivacyRequest> AddAsync(PrivacyRequest request);

    /// <summary>
    /// Actualiza una solicitud existente
    /// </summary>
    Task UpdateAsync(PrivacyRequest request);

    /// <summary>
    /// Verifica si existe una solicitud pendiente
    /// </summary>
    Task<bool> HasPendingRequestAsync(Guid userId, PrivacyRequestType type);

    /// <summary>
    /// Obtiene solicitudes pendientes que ya pasaron el período de gracia
    /// </summary>
    Task<IEnumerable<PrivacyRequest>> GetExpiredGracePeriodRequestsAsync();

    /// <summary>
    /// Verifica el código de confirmación
    /// </summary>
    Task<PrivacyRequest?> GetByConfirmationCodeAsync(Guid userId, string code);
}
