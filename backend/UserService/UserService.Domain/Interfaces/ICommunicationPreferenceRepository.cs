using System;
using System.Threading.Tasks;
using UserService.Domain.Entities.Privacy;

namespace UserService.Domain.Interfaces;

/// <summary>
/// Repository interface for CommunicationPreference (Derecho de Oposición ARCO).
/// </summary>
public interface ICommunicationPreferenceRepository
{
    /// <summary>
    /// Get communication preferences for a user. Returns null if not found (use defaults).
    /// </summary>
    Task<CommunicationPreference?> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Create or update communication preferences for a user (upsert).
    /// </summary>
    Task<CommunicationPreference> UpsertAsync(CommunicationPreference preference);
}
