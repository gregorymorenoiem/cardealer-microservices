using MediatR;

namespace AuthService.Application.Features.Auth.Commands.RevokeAllSessions;

/// <summary>
/// Command para revocar todas las sesiones de un usuario.
/// Proceso: AUTH-SEC-004
/// 
/// Opciones:
/// - KeepCurrentSession: mantiene la sesión actual activa (default: true)
/// - RevokeRefreshTokens: revoca también los refresh tokens (default: true)
/// 
/// Seguridad:
/// - Solo afecta sesiones del usuario autenticado
/// - Registra evento de seguridad
/// - Envía notificación por email
/// </summary>
public record RevokeAllSessionsCommand(
    string UserId,
    string? CurrentSessionId = null,
    bool KeepCurrentSession = true,
    bool RevokeRefreshTokens = true,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<RevokeAllSessionsResponse>;

/// <summary>
/// Response con estadísticas de revocación.
/// </summary>
public record RevokeAllSessionsResponse(
    bool Success,
    string Message,
    int SessionsRevoked,
    int RefreshTokensRevoked,
    bool CurrentSessionKept
);
