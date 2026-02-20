using MediatR;
using Video360Service.Application.DTOs;

namespace Video360Service.Application.Features.Commands;

/// <summary>
/// Comando para crear un nuevo job de extracción 360
/// </summary>
public record CreateVideo360JobCommand : IRequest<Video360JobResponse>
{
    public CreateVideo360JobRequest Request { get; init; } = null!;
    public Guid? UserId { get; init; }
    public string? TenantId { get; init; }
    public byte[]? VideoBytes { get; init; }
}

/// <summary>
/// Comando para cancelar un job en progreso
/// </summary>
public record CancelVideo360JobCommand : IRequest<bool>
{
    public Guid JobId { get; init; }
    public Guid? UserId { get; init; }
}

/// <summary>
/// Comando para reintentar un job fallido
/// </summary>
public record RetryVideo360JobCommand : IRequest<Video360JobResponse>
{
    public Guid JobId { get; init; }
    public Guid? UserId { get; init; }
}

/// <summary>
/// Comando para eliminar un job y sus archivos
/// </summary>
public record DeleteVideo360JobCommand : IRequest<bool>
{
    public Guid JobId { get; init; }
    public Guid? UserId { get; init; }
}
