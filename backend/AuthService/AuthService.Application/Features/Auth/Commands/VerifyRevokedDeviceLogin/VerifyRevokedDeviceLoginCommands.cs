using MediatR;

namespace AuthService.Application.Features.Auth.Commands.VerifyRevokedDeviceLogin;

/// <summary>
/// Command para solicitar código de verificación cuando un dispositivo revocado intenta iniciar sesión.
/// Proceso: AUTH-SEC-005
/// 
/// Flujo de seguridad:
/// 1. Se detecta que el dispositivo fue previamente revocado
/// 2. Se genera código de 6 dígitos
/// 3. Se envía email de alerta + código
/// 4. Usuario debe ingresar código para continuar con el login
/// </summary>
public record RequestRevokedDeviceLoginCommand(
    string UserId,
    string Email,
    string DeviceFingerprint,
    string? IpAddress = null,
    string? UserAgent = null,
    string? Browser = null,
    string? OperatingSystem = null
) : IRequest<RequestRevokedDeviceLoginResponse>;

public record RequestRevokedDeviceLoginResponse(
    bool RequiresVerification,
    string Message,
    string? VerificationToken = null,
    DateTime? CodeExpiresAt = null
);

/// <summary>
/// Command para verificar el código y completar el login desde dispositivo revocado.
/// </summary>
public record VerifyRevokedDeviceLoginCommand(
    string VerificationToken,
    string Code,
    string? IpAddress = null
) : IRequest<VerifyRevokedDeviceLoginResponse>;

public record VerifyRevokedDeviceLoginResponse(
    bool Success,
    string Message,
    bool DeviceCleared = false,
    int? RemainingAttempts = null
);
