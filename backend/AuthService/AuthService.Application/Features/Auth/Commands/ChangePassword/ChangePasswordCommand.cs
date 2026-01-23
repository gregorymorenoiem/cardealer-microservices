using MediatR;
using AuthService.Application.DTOs.Security;

namespace AuthService.Application.Features.Auth.Commands.ChangePassword;

/// <summary>
/// Command para cambiar la contraseña de un usuario autenticado.
/// Proceso: AUTH-SEC-001
/// 
/// Requiere:
/// - Usuario autenticado
/// - Contraseña actual correcta
/// - Nueva contraseña que cumpla requisitos de complejidad
/// 
/// Seguridad:
/// - Revoca todas las demás sesiones activas
/// - Envía notificación por email
/// - Registra evento de auditoría
/// </summary>
public record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<ChangePasswordResponse>;
