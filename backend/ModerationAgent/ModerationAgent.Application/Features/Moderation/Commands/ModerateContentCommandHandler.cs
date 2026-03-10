using MediatR;
using Microsoft.Extensions.Logging;
using ModerationAgent.Application.DTOs;
using ModerationAgent.Domain.Interfaces;
using ModerationAgent.Domain.Models;

namespace ModerationAgent.Application.Features.Moderation.Commands;

public sealed class ModerateContentCommandHandler
    : IRequestHandler<ModerateContentCommand, ModerationResponse>
{
    private readonly ILlmModerationService _moderationService;
    private readonly ILogger<ModerateContentCommandHandler> _logger;

    public ModerateContentCommandHandler(ILlmModerationService moderationService, ILogger<ModerateContentCommandHandler> logger)
    {
        _moderationService = moderationService;
        _logger = logger;
    }

    public async Task<ModerationResponse> Handle(ModerateContentCommand command, CancellationToken ct)
    {
        var req = command.Request;
        _logger.LogInformation("ModerationAgent: Moderating {ContentType} {ContentId}", req.ContentType, req.ContentId);

        var input = new ModerationInput
        {
            ContentId = req.ContentId,
            ContentType = req.ContentType,
            Title = req.Title,
            Body = req.Body,
            SellerName = req.SellerName,
            Price = req.Price,
            Currency = req.Currency,
            PhotoCount = req.PhotoCount,
            ImageLabels = req.ImageLabels
        };

        var verdict = await _moderationService.ModerateContentAsync(input, ct);

        _logger.LogInformation(
            "ModerationAgent: {ContentId} → Acción: {Action}, Riesgo: {Risk}, Estafa: {Scam}",
            req.ContentId, verdict.Accion, verdict.PuntajeRiesgo, verdict.PosibleEstafa);

        return new ModerationResponse
        {
            Accion = verdict.Accion,
            Confianza = verdict.Confianza,
            PuntajeRiesgo = verdict.PuntajeRiesgo,
            Violaciones = verdict.Violaciones,
            PosibleEstafa = verdict.PosibleEstafa,
            IndicadoresEstafa = verdict.IndicadoresEstafa,
            ContienePii = verdict.ContienePii,
            CamposPii = verdict.CamposPii,
            ContenidoCorregido = verdict.ContenidoCorregido,
            Explicacion = verdict.Explicacion,
            MensajeParaUsuario = verdict.MensajeParaUsuario,
            ModelUsed = "cascade",
            FallbackLevel = 0,
            LatencyMs = 0
        };
    }
}
