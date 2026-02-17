using MediatR;
using KYCService.Application.DTOs;

namespace KYCService.Application.Queries;

/// <summary>
/// Query para obtener el estado de una sesión de verificación
/// </summary>
public class GetVerificationSessionQuery : IRequest<VerificationSessionStatusDto?>
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }

    public GetVerificationSessionQuery(Guid sessionId, Guid userId)
    {
        SessionId = sessionId;
        UserId = userId;
    }
}

/// <summary>
/// Query para obtener la sesión activa de un usuario
/// </summary>
public class GetActiveVerificationSessionQuery : IRequest<VerificationSessionStatusDto?>
{
    public Guid UserId { get; set; }

    public GetActiveVerificationSessionQuery(Guid userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Query para obtener historial de sesiones de verificación
/// </summary>
public class GetVerificationHistoryQuery : IRequest<List<VerificationSessionSummaryDto>>
{
    public Guid UserId { get; set; }
    public int? Limit { get; set; } = 10;

    public GetVerificationHistoryQuery(Guid userId, int? limit = 10)
    {
        UserId = userId;
        Limit = limit;
    }
}

/// <summary>
/// Query para verificar si el usuario puede iniciar una nueva sesión
/// </summary>
public class CanStartVerificationQuery : IRequest<CanStartVerificationResult>
{
    public Guid UserId { get; set; }

    public CanStartVerificationQuery(Guid userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Resultado de la query de si puede iniciar verificación
/// </summary>
public class CanStartVerificationResult
{
    public bool CanStart { get; set; }
    public string? Reason { get; set; }
    public DateTime? CanRetryAfter { get; set; }
    public bool HasActiveSession { get; set; }
    public Guid? ActiveSessionId { get; set; }
    public bool HasApprovedKYC { get; set; }
    public int TotalAttemptsToday { get; set; }
}
