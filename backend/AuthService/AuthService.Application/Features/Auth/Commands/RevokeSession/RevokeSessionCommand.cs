using MediatR;

namespace AuthService.Application.Features.Auth.Commands.RevokeSession;

/// <summary>
/// Command para revocar una sesión específica.
/// Proceso: AUTH-SEC-003
/// 
/// Seguridad:
/// - Verifica que la sesión pertenece al usuario
/// - No permite revocar sesión actual (debe usarse logout)
/// - Registra evento de auditoría
/// - Revoca también el refresh token asociado
/// </summary>
public record RevokeSessionCommand(
    string UserId,
    string SessionId,
    string? CurrentSessionId = null,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<RevokeSessionResponse>;

/// <summary>
/// Response de revocación de sesión.
/// </summary>
public record RevokeSessionResponse(
    bool Success,
    string Message,
    bool WasCurrentSession = false
);
