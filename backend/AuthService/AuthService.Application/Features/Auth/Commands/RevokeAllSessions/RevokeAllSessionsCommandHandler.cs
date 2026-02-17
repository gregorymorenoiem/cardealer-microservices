using MediatR;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.Commands.RevokeAllSessions;

/// <summary>
/// Handler para revocar todas las sesiones de un usuario.
/// Proceso: AUTH-SEC-004
/// 
/// Flujo de seguridad:
/// 1. Obtener todas las sesiones activas del usuario
/// 2. Opcionalmente mantener la sesión actual
/// 3. Revocar todas las demás sesiones
/// 4. Revocar refresh tokens asociados
/// 5. Enviar notificación de seguridad por email
/// 6. Registrar en log de auditoría
/// </summary>
public class RevokeAllSessionsCommandHandler : IRequestHandler<RevokeAllSessionsCommand, RevokeAllSessionsResponse>
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthNotificationService _notificationService;
    private readonly ILogger<RevokeAllSessionsCommandHandler> _logger;

    public RevokeAllSessionsCommandHandler(
        IUserSessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IAuthNotificationService notificationService,
        ILogger<RevokeAllSessionsCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<RevokeAllSessionsResponse> Handle(
        RevokeAllSessionsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "AUTH-SEC-004: Revoke all sessions initiated by user {UserId}. " +
            "KeepCurrent: {KeepCurrent}, RevokeTokens: {RevokeTokens}, IP: {IpAddress}",
            request.UserId, request.KeepCurrentSession, request.RevokeRefreshTokens,
            request.IpAddress ?? "unknown");

        try
        {
            // 1. Obtener todas las sesiones activas
            var sessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(
                request.UserId,
                cancellationToken);

            var sessionList = sessions.ToList();
            int sessionsRevoked = 0;
            int refreshTokensRevoked = 0;

            // Parsear el ID de sesión actual
            Guid? currentSessionGuid = null;
            if (!string.IsNullOrEmpty(request.CurrentSessionId) &&
                Guid.TryParse(request.CurrentSessionId, out var parsed))
            {
                currentSessionGuid = parsed;
            }

            // 2. Revocar sesiones
            foreach (var session in sessionList)
            {
                // Si debemos mantener la sesión actual, saltarla
                if (request.KeepCurrentSession &&
                    currentSessionGuid.HasValue &&
                    session.Id == currentSessionGuid.Value)
                {
                    _logger.LogDebug(
                        "AUTH-SEC-004: Keeping current session {SessionId}",
                        session.Id);
                    continue;
                }

                // Revocar sesión
                await _sessionRepository.RevokeSessionAsync(
                    session.Id,
                    "User revoked all sessions",
                    cancellationToken);
                sessionsRevoked++;

                // 3. Revocar refresh token asociado (si está habilitado)
                if (request.RevokeRefreshTokens && !string.IsNullOrEmpty(session.RefreshTokenId))
                {
                    try
                    {
                        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
                            session.RefreshTokenId,
                            cancellationToken);

                        if (refreshToken != null && !refreshToken.IsRevoked)
                        {
                            refreshToken.Revoke("all_sessions_revoked", request.UserId);
                            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
                            refreshTokensRevoked++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "AUTH-SEC-004: Could not revoke refresh token for session {SessionId}",
                            session.Id);
                    }
                }
            }

            // 4. Si no mantuvimos la sesión actual, revocar TODOS los refresh tokens
            if (!request.KeepCurrentSession && request.RevokeRefreshTokens)
            {
                try
                {
                    await _refreshTokenRepository.RevokeAllForUserAsync(
                        request.UserId,
                        "all_sessions_revoked",
                        cancellationToken);

                    _logger.LogInformation(
                        "AUTH-SEC-004: All refresh tokens revoked for user {UserId}",
                        request.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "AUTH-SEC-004: Error revoking all refresh tokens for user {UserId}",
                        request.UserId);
                }
            }

            // 5. Enviar notificación de seguridad por email
            try
            {
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var alert = new SecurityAlertDto(
                        AlertType: "all_sessions_revoked",
                        IpAddress: request.IpAddress ?? "Unknown",
                        AttemptCount: sessionsRevoked,
                        Timestamp: DateTime.UtcNow,
                        Location: null,
                        DeviceInfo: request.UserAgent,
                        LockoutDuration: null
                    );

                    await _notificationService.SendSecurityAlertAsync(user.Email, alert);

                    _logger.LogInformation(
                        "AUTH-SEC-004: Security alert email sent to user {UserId}",
                        request.UserId);
                }
            }
            catch (Exception ex)
            {
                // No fallar por email
                _logger.LogWarning(ex,
                    "AUTH-SEC-004: Failed to send security alert email to user {UserId}",
                    request.UserId);
            }

            // 6. Log de auditoría
            _logger.LogInformation(
                "AUTH-SEC-004: Successfully revoked {SessionCount} sessions and {TokenCount} refresh tokens " +
                "for user {UserId}. CurrentKept: {CurrentKept}",
                sessionsRevoked, refreshTokensRevoked, request.UserId,
                request.KeepCurrentSession && currentSessionGuid.HasValue);

            string message;
            if (sessionsRevoked == 0)
            {
                message = request.KeepCurrentSession
                    ? "No other active sessions found."
                    : "No active sessions found.";
            }
            else if (request.KeepCurrentSession)
            {
                message = $"Successfully signed out of {sessionsRevoked} other device(s).";
            }
            else
            {
                message = $"Successfully signed out of all {sessionsRevoked} device(s). Please log in again.";
            }

            return new RevokeAllSessionsResponse(
                Success: true,
                Message: message,
                SessionsRevoked: sessionsRevoked,
                RefreshTokensRevoked: refreshTokensRevoked,
                CurrentSessionKept: request.KeepCurrentSession && currentSessionGuid.HasValue
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "AUTH-SEC-004: Unexpected error revoking all sessions for user {UserId}",
                request.UserId);

            return new RevokeAllSessionsResponse(
                Success: false,
                Message: "An error occurred while revoking sessions.",
                SessionsRevoked: 0,
                RefreshTokensRevoked: 0,
                CurrentSessionKept: false
            );
        }
    }
}
