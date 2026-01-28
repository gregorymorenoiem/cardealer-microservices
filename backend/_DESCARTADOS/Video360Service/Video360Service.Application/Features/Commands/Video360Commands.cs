using MediatR;
using Video360Service.Application.DTOs;
using Video360Service.Domain.Entities;

namespace Video360Service.Application.Features.Commands;

/// <summary>
/// Comando para crear un trabajo de procesamiento de video 360
/// </summary>
public record CreateVideo360JobCommand : IRequest<Video360JobResponse>
{
    public Guid UserId { get; init; }
    public Guid VehicleId { get; init; }
    public string VideoUrl { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public ProcessingOptions Options { get; init; } = new();
}

/// <summary>
/// Comando para procesar un video 360
/// </summary>
public record ProcessVideo360Command : IRequest<Video360JobResponse>
{
    public Guid JobId { get; init; }
}

/// <summary>
/// Comando para cancelar un trabajo
/// </summary>
public record CancelVideo360JobCommand : IRequest<bool>
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
}

/// <summary>
/// Comando para eliminar un trabajo y sus frames
/// </summary>
public record DeleteVideo360JobCommand : IRequest<bool>
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
}

/// <summary>
/// Comando para reprocesar un trabajo fallido
/// </summary>
public record RetryVideo360JobCommand : IRequest<Video360JobResponse>
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
}
