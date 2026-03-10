using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Domain.Entities.Privacy;

namespace UserService.Domain.Interfaces;

/// <summary>
/// Repository para ConsentRecord — auditoría de cambios de consentimiento (Ley 172-13).
/// </summary>
public interface IConsentRecordRepository
{
    /// <summary>
    /// Registra un nuevo cambio de consentimiento.
    /// </summary>
    Task AddAsync(ConsentRecord record);

    /// <summary>
    /// Obtiene el historial de consentimiento de un usuario.
    /// </summary>
    Task<IReadOnlyList<ConsentRecord>> GetByUserIdAsync(Guid userId, int limit = 100);

    /// <summary>
    /// Obtiene el último registro de un tipo de consentimiento específico.
    /// </summary>
    Task<ConsentRecord?> GetLatestAsync(Guid userId, string consentType);
}
