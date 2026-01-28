using MediatR;
using Vehicle360ProcessingService.Application.DTOs;

namespace Vehicle360ProcessingService.Application.Features.Queries;

/// <summary>
/// Query para obtener un job por ID
/// </summary>
public class GetVehicle360JobQuery : IRequest<Vehicle360JobResponse?>
{
    public Guid JobId { get; set; }
}

/// <summary>
/// Query para obtener el estado de un job
/// </summary>
public class GetJobStatusQuery : IRequest<JobStatusResponse?>
{
    public Guid JobId { get; set; }
}

/// <summary>
/// Query para obtener el resultado del procesamiento
/// </summary>
public class GetProcessingResultQuery : IRequest<ProcessingResultResponse?>
{
    public Guid JobId { get; set; }
}

/// <summary>
/// Query para obtener los datos del visor 360 para un vehículo
/// </summary>
public class GetVehicle360ViewerQuery : IRequest<Vehicle360ViewerResponse?>
{
    public Guid VehicleId { get; set; }
}

/// <summary>
/// Query para obtener jobs de un vehículo
/// </summary>
public class GetJobsByVehicleQuery : IRequest<List<Vehicle360JobResponse>>
{
    public Guid VehicleId { get; set; }
}

/// <summary>
/// Query para obtener jobs de un usuario
/// </summary>
public class GetUserJobsQuery : IRequest<List<Vehicle360JobResponse>>
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Query para verificar la salud de los servicios dependientes
/// </summary>
public class CheckServicesHealthQuery : IRequest<ServicesHealthResponse>
{
}

public class ServicesHealthResponse
{
    public bool AllHealthy { get; set; }
    public Dictionary<string, ServiceHealthStatus> Services { get; set; } = new();
}

public class ServiceHealthStatus
{
    public string Name { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string? LastError { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public int? ResponseTimeMs { get; set; }
}
