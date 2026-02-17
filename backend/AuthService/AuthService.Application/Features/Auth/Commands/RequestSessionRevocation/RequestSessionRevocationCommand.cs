using MediatR;

namespace AuthService.Application.Features.Auth.Commands.RequestSessionRevocation;

/// <summary>
/// Command para solicitar la revocación de una sesión.
/// Proceso: AUTH-SEC-003-A
/// 
/// Flujo de seguridad:
/// 1. Valida que la sesión existe y pertenece al usuario
/// 2. Valida que NO es la sesión actual (bloqueado)
/// 3. Genera código de 6 dígitos
/// 4. Envía email con el código
/// 5. Almacena código en cache (Redis) con TTL de 5 minutos
/// 
/// Anti-secuestro:
/// - Rate limiting: máximo 3 solicitudes por hora
/// - Código expira en 5 minutos
/// - Notificación al usuario del dispositivo afectado
/// </summary>
public record RequestSessionRevocationCommand(
    string UserId,
    string SessionId,
    string? CurrentSessionId = null,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<RequestSessionRevocationResponse>;

/// <summary>
/// Response de solicitud de revocación.
/// </summary>
public record RequestSessionRevocationResponse(
    bool Success,
    string Message,
    string? SessionId = null,
    DateTime? CodeExpiresAt = null,
    int? RemainingAttempts = null
);
