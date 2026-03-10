using ModerationAgent.Domain.Models;

namespace ModerationAgent.Application.DTOs;

public sealed class ModerationRequest
{
    public required string ContentId { get; init; }
    public required string ContentType { get; init; }
    public string? Title { get; init; }
    public string? Body { get; init; }
    public string? SellerName { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public int? PhotoCount { get; init; }
    public IReadOnlyList<string>? ImageLabels { get; init; }
}

public sealed class ModerationResponse
{
    public required string Accion { get; init; }
    public required double Confianza { get; init; }
    public required int PuntajeRiesgo { get; init; }
    public required IReadOnlyList<Violation> Violaciones { get; init; }
    public required bool PosibleEstafa { get; init; }
    public required IReadOnlyList<string> IndicadoresEstafa { get; init; }
    public required bool ContienePii { get; init; }
    public required IReadOnlyList<string> CamposPii { get; init; }
    public string? ContenidoCorregido { get; init; }
    public required string Explicacion { get; init; }
    public string? MensajeParaUsuario { get; init; }
    public required string ModelUsed { get; init; }
    public required int FallbackLevel { get; init; }
    public required double LatencyMs { get; init; }
}
