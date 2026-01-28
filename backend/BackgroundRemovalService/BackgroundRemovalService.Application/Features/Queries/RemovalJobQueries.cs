using BackgroundRemovalService.Application.DTOs;
using BackgroundRemovalService.Domain.Enums;
using MediatR;

namespace BackgroundRemovalService.Application.Features.Queries;

/// <summary>
/// Query para obtener un job por ID
/// </summary>
public record GetRemovalJobByIdQuery : IRequest<RemovalJobResponse?>
{
    public Guid JobId { get; init; }
}

/// <summary>
/// Query para obtener un job por CorrelationId
/// </summary>
public record GetRemovalJobByCorrelationIdQuery : IRequest<RemovalJobResponse?>
{
    public string CorrelationId { get; init; } = string.Empty;
}

/// <summary>
/// Query para listar jobs del usuario
/// </summary>
public record GetUserRemovalJobsQuery : IRequest<RemovalJobListResponse>
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public ProcessingStatus? StatusFilter { get; init; }
}

/// <summary>
/// Query para obtener estadísticas de uso
/// </summary>
public record GetUsageStatisticsQuery : IRequest<UsageStatisticsResponse>
{
    public Guid? UserId { get; init; }
    public int? BillingPeriod { get; init; }
}

/// <summary>
/// Query para obtener información de proveedores
/// </summary>
public record GetProvidersInfoQuery : IRequest<IEnumerable<ProviderInfoResponse>>
{
    public bool OnlyEnabled { get; init; } = true;
}

/// <summary>
/// Query para health check de proveedores
/// </summary>
public record GetProvidersHealthQuery : IRequest<IEnumerable<ProviderHealthResponse>>
{
}
