using MediatR;
using Video360Service.Application.DTOs;
using Video360Service.Domain.Enums;

namespace Video360Service.Application.Features.Queries;

/// <summary>
/// Query para obtener un job por ID
/// </summary>
public record GetVideo360JobByIdQuery : IRequest<Video360JobResponse?>
{
    public Guid JobId { get; init; }
    public Guid? UserId { get; init; }
    public bool IncludeFrameBase64 { get; init; } = false;
}

/// <summary>
/// Query para listar jobs del usuario
/// </summary>
public record GetVideo360JobsQuery : IRequest<Video360JobListResponse>
{
    public Guid? UserId { get; init; }
    public Guid? VehicleId { get; init; }
    public ProcessingStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Query para obtener la vista 360 de un vehículo
/// </summary>
public record GetVehicle360ViewQuery : IRequest<Vehicle360ViewResponse?>
{
    public Guid VehicleId { get; init; }
}

/// <summary>
/// Query para obtener información de proveedores
/// </summary>
public record GetProvidersInfoQuery : IRequest<IEnumerable<ProviderInfoResponse>>
{
    public bool OnlyAvailable { get; init; } = false;
}

/// <summary>
/// Query para obtener estadísticas de uso
/// </summary>
public record GetUsageStatsQuery : IRequest<UsageStatsResponse>
{
    public Guid? UserId { get; init; }
    public string? TenantId { get; init; }
    public string? BillingPeriod { get; init; }
}
