using MediatR;
using KYCService.Domain.Entities;
using KYCService.Application.DTOs;

namespace KYCService.Application.Queries;

/// <summary>
/// Obtener perfil KYC por ID
/// </summary>
public record GetKYCProfileByIdQuery(Guid Id) : IRequest<KYCProfileDto?>;

/// <summary>
/// Obtener perfil KYC por ID de usuario
/// </summary>
public record GetKYCProfileByUserIdQuery(Guid UserId) : IRequest<KYCProfileDto?>;

/// <summary>
/// Obtener perfil KYC por número de documento
/// </summary>
public record GetKYCProfileByDocumentQuery(string DocumentNumber) : IRequest<KYCProfileDto?>;

/// <summary>
/// Listar perfiles KYC con paginación
/// </summary>
public record GetKYCProfilesQuery : IRequest<PaginatedResult<KYCProfileSummaryDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public KYCStatus? Status { get; init; }
    public RiskLevel? RiskLevel { get; init; }
    public bool? IsPEP { get; init; }
    public EntityType? EntityType { get; init; }
}

/// <summary>
/// Obtener perfiles pendientes de revisión
/// </summary>
public record GetPendingKYCProfilesQuery : IRequest<PaginatedResult<KYCProfileSummaryDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Obtener perfiles próximos a expirar
/// </summary>
public record GetExpiringKYCProfilesQuery : IRequest<PaginatedResult<KYCProfileSummaryDto>>
{
    public int DaysUntilExpiry { get; init; } = 30;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Obtener documentos de un perfil KYC
/// </summary>
public record GetKYCDocumentsQuery(Guid KYCProfileId) : IRequest<List<KYCDocumentDto>>;

/// <summary>
/// Obtener verificaciones de un perfil KYC
/// </summary>
public record GetKYCVerificationsQuery(Guid KYCProfileId) : IRequest<List<KYCVerificationDto>>;

/// <summary>
/// Obtener historial de evaluaciones de riesgo
/// </summary>
public record GetKYCRiskAssessmentsQuery(Guid KYCProfileId) : IRequest<List<KYCRiskAssessmentDto>>;

/// <summary>
/// Obtener estadísticas KYC
/// </summary>
public record GetKYCStatisticsQuery : IRequest<KYCStatisticsDto>;

/// <summary>
/// Obtener reporte de transacción sospechosa por ID
/// </summary>
public record GetSTRByIdQuery(Guid Id) : IRequest<SuspiciousTransactionReportDto?>;

/// <summary>
/// Obtener reporte de transacción sospechosa por número
/// </summary>
public record GetSTRByNumberQuery(string ReportNumber) : IRequest<SuspiciousTransactionReportDto?>;

/// <summary>
/// Listar reportes de transacciones sospechosas
/// </summary>
public record GetSTRsQuery : IRequest<PaginatedResult<SuspiciousTransactionReportDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public STRStatus? Status { get; init; }
    public bool? IsOverdue { get; init; }
}

/// <summary>
/// Obtener estadísticas de STR
/// </summary>
public record GetSTRStatisticsQuery : IRequest<STRStatisticsDto>;

/// <summary>
/// Buscar en listas de control
/// </summary>
public record SearchWatchlistQuery : IRequest<List<WatchlistEntryDto>>
{
    public string SearchTerm { get; init; } = string.Empty;
    public WatchlistType? ListType { get; init; }
}

/// <summary>
/// Obtener entrada de lista de control por ID
/// </summary>
public record GetWatchlistEntryByIdQuery(Guid Id) : IRequest<WatchlistEntryDto?>;
