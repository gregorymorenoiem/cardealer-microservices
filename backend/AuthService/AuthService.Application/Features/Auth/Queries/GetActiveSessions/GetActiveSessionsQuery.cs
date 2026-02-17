using MediatR;
using AuthService.Application.DTOs.Security;

namespace AuthService.Application.Features.Auth.Queries.GetActiveSessions;

/// <summary>
/// Query para obtener las sesiones activas de un usuario.
/// Proceso: AUTH-SEC-002
/// 
/// Seguridad:
/// - Solo devuelve sesiones del usuario autenticado
/// - Marca la sesión actual para diferenciación
/// - No expone datos sensibles (tokens, hashes)
/// - Enmascara parcialmente IPs para privacidad
/// </summary>
public record GetActiveSessionsQuery(
    string UserId,
    string? CurrentSessionId = null
) : IRequest<GetActiveSessionsResponse>;

/// <summary>
/// Response con la lista de sesiones activas del usuario.
/// </summary>
public record GetActiveSessionsResponse(
    bool Success,
    List<SessionDetailDto> Sessions,
    int TotalActiveSessions,
    string? Message = null
);

/// <summary>
/// DTO con información detallada de una sesión.
/// Diseñado para UI de gestión de sesiones.
/// </summary>
public record SessionDetailDto(
    string Id,
    string Device,
    string Browser,
    string OperatingSystem,
    string Location,
    string IpAddress,           // Parcialmente enmascarada por seguridad
    string CreatedAt,
    string LastActiveAt,
    bool IsCurrent,
    bool IsExpiringSoon,        // True si expira en menos de 1 hora
    string? ExpiresAt
);
