using HealthCheckService.Domain.Entities;
using MediatR;

namespace HealthCheckService.Application.Queries;

/// <summary>
/// Query to get the overall system health
/// </summary>
public record GetSystemHealthQuery : IRequest<SystemHealth>;

/// <summary>
/// Query to get health status of a specific service
/// </summary>
public record GetServiceHealthQuery(string ServiceName) : IRequest<ServiceHealth?>;

/// <summary>
/// Query to get all registered services
/// </summary>
public record GetRegisteredServicesQuery : IRequest<IEnumerable<string>>;
