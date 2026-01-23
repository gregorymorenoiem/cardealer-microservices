using MediatR;
using AuthService.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Auth.Queries.GetActiveSessions;

/// <summary>
/// Handler para obtener sesiones activas.
/// Proceso: AUTH-SEC-002
/// 
/// Flujo:
/// 1. Validar que el userId no está vacío
/// 2. Obtener sesiones activas del repositorio
/// 3. Transformar a DTOs con información enmascarada
/// 4. Marcar sesión actual
/// 5. Retornar lista ordenada por última actividad
/// </summary>
public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, GetActiveSessionsResponse>
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly ILogger<GetActiveSessionsQueryHandler> _logger;

    public GetActiveSessionsQueryHandler(
        IUserSessionRepository sessionRepository,
        ILogger<GetActiveSessionsQueryHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<GetActiveSessionsResponse> Handle(
        GetActiveSessionsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "AUTH-SEC-002: Fetching active sessions for user {UserId}",
            request.UserId);

        try
        {
            var sessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(
                request.UserId,
                cancellationToken);

            var sessionList = sessions.ToList();
            var now = DateTime.UtcNow;

            var sessionDtos = sessionList.Select(s => new SessionDetailDto(
                Id: s.Id.ToString(),
                Device: SanitizeString(s.DeviceInfo),
                Browser: SanitizeString(s.Browser),
                OperatingSystem: SanitizeString(s.OperatingSystem),
                Location: GetSafeLocation(s.Location, s.City, s.Country),
                IpAddress: MaskIpAddress(s.IpAddress),
                CreatedAt: s.CreatedAt.ToString("o"),
                LastActiveAt: s.LastActiveAt.ToString("o"),
                IsCurrent: s.Id.ToString() == request.CurrentSessionId,
                IsExpiringSoon: s.ExpiresAt.HasValue && s.ExpiresAt.Value.Subtract(now).TotalHours < 1,
                ExpiresAt: s.ExpiresAt?.ToString("o")
            )).OrderByDescending(s => s.IsCurrent)
              .ThenByDescending(s => s.LastActiveAt)
              .ToList();

            _logger.LogInformation(
                "AUTH-SEC-002: Found {Count} active sessions for user {UserId}",
                sessionDtos.Count, request.UserId);

            return new GetActiveSessionsResponse(
                Success: true,
                Sessions: sessionDtos,
                TotalActiveSessions: sessionDtos.Count,
                Message: null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "AUTH-SEC-002: Error fetching sessions for user {UserId}",
                request.UserId);

            return new GetActiveSessionsResponse(
                Success: false,
                Sessions: new List<SessionDetailDto>(),
                TotalActiveSessions: 0,
                Message: "Error retrieving sessions"
            );
        }
    }

    /// <summary>
    /// Enmascara parcialmente la dirección IP por privacidad.
    /// IPv4: 192.168.1.100 → 192.168.1.***
    /// IPv6: se muestra solo el prefijo
    /// </summary>
    private static string MaskIpAddress(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown")
            return "Unknown";

        try
        {
            // IPv4
            if (ipAddress.Contains('.'))
            {
                var parts = ipAddress.Split('.');
                if (parts.Length == 4)
                {
                    return $"{parts[0]}.{parts[1]}.{parts[2]}.***";
                }
            }
            // IPv6
            else if (ipAddress.Contains(':'))
            {
                var parts = ipAddress.Split(':');
                if (parts.Length >= 2)
                {
                    return $"{parts[0]}:{parts[1]}:****:****";
                }
            }
        }
        catch
        {
            // En caso de error, retornar genérico
        }

        return "***.***.***";
    }

    /// <summary>
    /// Obtiene una ubicación segura para mostrar.
    /// </summary>
    private static string GetSafeLocation(string? location, string? city, string? country)
    {
        if (!string.IsNullOrEmpty(location))
            return SanitizeString(location);

        if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(country))
            return $"{SanitizeString(city)}, {SanitizeString(country)}";

        if (!string.IsNullOrEmpty(country))
            return SanitizeString(country);

        if (!string.IsNullOrEmpty(city))
            return SanitizeString(city);

        return "Unknown location";
    }

    /// <summary>
    /// Sanitiza strings para prevenir XSS en el frontend.
    /// </summary>
    private static string SanitizeString(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return "Unknown";

        // Remover caracteres potencialmente peligrosos
        return input
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;")
            .Replace("&", "&amp;");
    }
}
