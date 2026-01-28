using BackgroundRemovalService.Application.DTOs;
using MediatR;

namespace BackgroundRemovalService.Application.Features.Commands;

/// <summary>
/// Comando para crear y procesar un job de remoción de fondo
/// </summary>
public record CreateRemovalJobCommand : IRequest<RemovalJobResponse>
{
    public CreateRemovalJobRequest Request { get; init; } = null!;
    public Guid? UserId { get; init; }
    public string? TenantId { get; init; }
    public string? ClientIpAddress { get; init; }
    public string? UserAgent { get; init; }
}

/// <summary>
/// Comando para reintentar un job fallido
/// </summary>
public record RetryRemovalJobCommand : IRequest<RemovalJobResponse>
{
    public Guid JobId { get; init; }
    public Domain.Enums.BackgroundRemovalProvider? AlternateProvider { get; init; }
}

/// <summary>
/// Comando para cancelar un job pendiente
/// </summary>
public record CancelRemovalJobCommand : IRequest<bool>
{
    public Guid JobId { get; init; }
    public Guid? UserId { get; init; }
}

/// <summary>
/// Comando para limpiar jobs expirados
/// </summary>
public record CleanupExpiredJobsCommand : IRequest<int>
{
    public int DaysOld { get; init; } = 30;
}
