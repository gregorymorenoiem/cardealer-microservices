using MediatR;
using Video360Service.Application.DTOs;

namespace Video360Service.Application.Features.Queries;

/// <summary>
/// Query para obtener un trabajo por ID
/// </summary>
public record GetVideo360JobQuery : IRequest<Video360JobResponse?>
{
    public Guid JobId { get; init; }
    public bool IncludeFrames { get; init; } = true;
}

/// <summary>
/// Query para obtener el estado de un trabajo
/// </summary>
public record GetJobStatusQuery : IRequest<JobStatusResponse?>
{
    public Guid JobId { get; init; }
}

/// <summary>
/// Query para obtener trabajos de un vehículo
/// </summary>
public record GetJobsByVehicleQuery : IRequest<IEnumerable<Video360JobResponse>>
{
    public Guid VehicleId { get; init; }
}

/// <summary>
/// Query para obtener trabajos de un usuario
/// </summary>
public record GetJobsByUserQuery : IRequest<IEnumerable<Video360JobResponse>>
{
    public Guid UserId { get; init; }
    public int? Limit { get; init; }
}

/// <summary>
/// Query para obtener los datos del viewer 360 de un vehículo
/// </summary>
public record GetVehicle360ViewerQuery : IRequest<Vehicle360ViewerResponse?>
{
    public Guid VehicleId { get; init; }
}

/// <summary>
/// Query para obtener estadísticas del servicio
/// </summary>
public record GetServiceStatsQuery : IRequest<ServiceStatsResponse>;

public record ServiceStatsResponse
{
    public int PendingJobs { get; init; }
    public int ProcessingJobs { get; init; }
    public int CompletedToday { get; init; }
    public int FailedToday { get; init; }
    public double AverageProcessingTimeMs { get; init; }
}
