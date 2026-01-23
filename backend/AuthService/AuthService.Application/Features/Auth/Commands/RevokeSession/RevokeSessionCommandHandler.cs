using MediatR;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.Commands.RevokeSession;

/// <summary>
/// Handler para revocar una sesión específica.
/// Proceso: AUTH-SEC-003
/// 
/// Flujo de seguridad:
/// 1. Validar formato de session ID
/// 2. Obtener sesión del repositorio
/// 3. Verificar que la sesión existe
/// 4. Verificar que la sesión pertenece al usuario (CRÍTICO)
/// 5. Verificar que no es la sesión actual
/// 6. Revocar sesión
/// 7. Revocar refresh token asociado (si existe)
/// 8. Registrar en log de auditoría
/// </summary>
public class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand, RevokeSessionResponse>
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<RevokeSessionCommandHandler> _logger;

    public RevokeSessionCommandHandler(
        IUserSessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<RevokeSessionCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<RevokeSessionResponse> Handle(
        RevokeSessionCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "AUTH-SEC-003: Session revocation initiated by user {UserId} for session {SessionId} from IP {IpAddress}",
            request.UserId, request.SessionId, request.IpAddress ?? "unknown");

        try
        {
            // 1. Validar formato de GUID
            if (!Guid.TryParse(request.SessionId, out var sessionGuid))
            {
                _logger.LogWarning(
                    "AUTH-SEC-003: Invalid session ID format: {SessionId}",
                    request.SessionId);
                throw new BadRequestException("Invalid session ID format.");
            }

            // 2. Obtener sesión
            var session = await _sessionRepository.GetByIdAsync(sessionGuid, cancellationToken);

            // 3. Verificar que existe
            if (session == null)
            {
                _logger.LogWarning(
                    "AUTH-SEC-003: Session {SessionId} not found for user {UserId}",
                    request.SessionId, request.UserId);
                throw new NotFoundException("Session not found.");
            }

            // 4. CRÍTICO: Verificar que la sesión pertenece al usuario
            if (session.UserId != request.UserId)
            {
                _logger.LogWarning(
                    "AUTH-SEC-003: SECURITY ALERT - User {UserId} attempted to revoke session {SessionId} " +
                    "belonging to user {SessionOwner}. IP: {IpAddress}",
                    request.UserId, request.SessionId, session.UserId, request.IpAddress);

                // No revelar que la sesión existe pero pertenece a otro usuario
                throw new NotFoundException("Session not found.");
            }

            // 5. Verificar si ya está revocada
            if (session.IsRevoked)
            {
                _logger.LogInformation(
                    "AUTH-SEC-003: Session {SessionId} already revoked",
                    request.SessionId);
                return new RevokeSessionResponse(
                    Success: true,
                    Message: "Session was already revoked.",
                    WasCurrentSession: false
                );
            }

            // 6. Advertir si es la sesión actual
            bool isCurrentSession = request.SessionId == request.CurrentSessionId;
            if (isCurrentSession)
            {
                _logger.LogInformation(
                    "AUTH-SEC-003: User {UserId} is revoking their current session",
                    request.UserId);
            }

            // 7. Revocar la sesión
            await _sessionRepository.RevokeSessionAsync(
                sessionGuid,
                "User revoked session remotely",
                cancellationToken);

            // 8. Revocar el refresh token asociado (si existe)
            if (!string.IsNullOrEmpty(session.RefreshTokenId))
            {
                try
                {
                    var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
                        session.RefreshTokenId,
                        cancellationToken);

                    if (refreshToken != null && !refreshToken.IsRevoked)
                    {
                        refreshToken.Revoke("session_revoked", request.UserId);
                        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

                        _logger.LogInformation(
                            "AUTH-SEC-003: Associated refresh token revoked for session {SessionId}",
                            request.SessionId);
                    }
                }
                catch (Exception ex)
                {
                    // No fallar si el refresh token ya no existe
                    _logger.LogWarning(ex,
                        "AUTH-SEC-003: Could not revoke associated refresh token for session {SessionId}",
                        request.SessionId);
                }
            }

            // 9. Log de auditoría exitoso
            _logger.LogInformation(
                "AUTH-SEC-003: Session {SessionId} successfully revoked by user {UserId}. " +
                "Device: {Device}, Location: {Location}, IP: {SessionIp}",
                request.SessionId, request.UserId,
                session.DeviceInfo, session.Location ?? "Unknown", session.IpAddress);

            return new RevokeSessionResponse(
                Success: true,
                Message: isCurrentSession
                    ? "Current session revoked. You will be logged out."
                    : "Session revoked successfully.",
                WasCurrentSession: isCurrentSession
            );
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "AUTH-SEC-003: Unexpected error revoking session {SessionId} for user {UserId}",
                request.SessionId, request.UserId);
            throw;
        }
    }
}
