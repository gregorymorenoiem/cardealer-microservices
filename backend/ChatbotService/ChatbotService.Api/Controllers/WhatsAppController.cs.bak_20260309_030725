using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Application.Features.Sessions.Commands;
using ChatbotService.Domain.Enums;

namespace ChatbotService.Api.Controllers;

/// <summary>
/// WhatsApp webhook controller for Meta Cloud API integration.
/// 
/// Flow: Meta Cloud API → POST /api/whatsapp/webhook → ParseInbound → Pipeline → SendReply
/// 
/// Endpoints:
///   GET  /api/whatsapp/webhook  → Verificación del webhook (Meta challenge)
///   POST /api/whatsapp/webhook  → Recibir mensajes entrantes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // WhatsApp webhooks no tienen auth - se validan con verify token
public class WhatsAppController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(
        IMediator mediator,
        IWhatsAppService whatsAppService,
        IChatSessionRepository sessionRepository,
        ILogger<WhatsAppController> logger)
    {
        _mediator = mediator;
        _whatsAppService = whatsAppService;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Verificación del webhook de Meta (GET challenge).
    /// Meta envía esto una vez al configurar el webhook.
    /// </summary>
    [HttpGet("webhook")]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        if (_whatsAppService.VerifyWebhook(mode, token, challenge))
        {
            return Ok(challenge);
        }

        return Forbid();
    }

    /// <summary>
    /// Recibir mensajes entrantes de WhatsApp.
    /// Cada mensaje pasa por el pipeline completo:
    /// 1. Parse → 2. Rate limit → 3. Country filter → 4. Session lookup/create →
    /// 5. Handoff check → 6. SendMessage pipeline (security + LLM) → 7. Reply
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveMessage(CancellationToken ct)
    {
        // Meta siempre espera 200 OK rápido, procesar async
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync(ct);

            if (string.IsNullOrEmpty(body))
                return Ok();

            var payload = JsonSerializer.Deserialize<JsonElement>(body);

            // Parsear mensaje entrante
            var inbound = _whatsAppService.ParseInboundMessage(payload);
            if (inbound == null)
            {
                // Status update (delivered, read, etc.) — ignorar
                return Ok();
            }

            _logger.LogInformation(
                "WhatsApp inbound from {Phone} ({Name}): {Message}",
                inbound.From, inbound.ProfileName,
                inbound.Body.Length > 100 ? inbound.Body[..100] + "..." : inbound.Body);

            // Rate limiting
            if (!_whatsAppService.CheckRateLimit(inbound.From))
            {
                await _whatsAppService.SendTextMessageAsync(
                    inbound.From,
                    "⏳ Estás enviando mensajes muy rápido. Por favor espera un momento.",
                    ct);
                return Ok();
            }

            // Country filter
            if (!_whatsAppService.IsAllowedCountry(inbound.From))
            {
                _logger.LogWarning("WhatsApp message from unsupported country: {Phone}", inbound.From);
                await _whatsAppService.SendTextMessageAsync(
                    inbound.From,
                    "Lo siento, OKLA solo está disponible en República Dominicana. 🇩🇴",
                    ct);
                return Ok();
            }

            // Marcar como leído
            _ = _whatsAppService.MarkAsReadAsync(inbound.MessageId, ct);

            // Buscar sesión existente o crear nueva
            var session = await _sessionRepository.GetByChannelUserIdAsync(
                "whatsapp", inbound.From, ct);

            string sessionToken;

            if (session == null || session.Status == SessionStatus.Completed)
            {
                // Crear nueva sesión WhatsApp en modo DealerInventory (default)
                var startCmd = new StartSessionCommand(
                    UserId: null,
                    UserName: inbound.ProfileName,
                    UserEmail: null,
                    UserPhone: inbound.From,
                    SessionType: "WhatsApp",
                    Channel: "whatsapp",
                    ChannelUserId: inbound.From,
                    UserAgent: "WhatsApp",
                    IpAddress: null,
                    DeviceType: "mobile",
                    Language: "es",
                    DealerId: null, // TODO: determinar dealer por número destino
                    ChatMode: "dealer_inventory",
                    VehicleId: null);

                var startResult = await _mediator.Send(startCmd, ct);
                sessionToken = startResult.SessionToken;

                // Enviar mensaje de bienvenida
                await _whatsAppService.SendTextMessageAsync(
                    inbound.From, startResult.WelcomeMessage, ct);
            }
            else
            {
                sessionToken = session.SessionToken;
            }

            // Si la sesión está en modo humano, NO enviar al bot
            // El dealer maneja la conversación directamente
            if (session != null && !session.IsBotActive)
            {
                // Solo guardar el mensaje para que el dealer lo vea
                var saveCmd = new SendMessageCommand(
                    sessionToken,
                    inbound.Body,
                    "UserText",
                    inbound.MediaUrl);

                await _mediator.Send(saveCmd, ct);
                // No responder — el dealer responde manualmente
                return Ok();
            }

            // Enviar al pipeline del chatbot
            var messageCmd = new SendMessageCommand(
                sessionToken,
                inbound.Body,
                "UserText",
                inbound.MediaUrl);

            var result = await _mediator.Send(messageCmd, ct);

            // Responder por WhatsApp
            if (!string.IsNullOrEmpty(result.Response))
            {
                // Si el response es largo, truncar para WhatsApp (max ~4096 chars)
                var response = result.Response.Length > 4000
                    ? result.Response[..3997] + "..."
                    : result.Response;

                await _whatsAppService.SendTextMessageAsync(inbound.From, response, ct);
            }

            // Si es fallback, ofrecer hablar con un humano
            if (result.IsFallback)
            {
                await _whatsAppService.SendInteractiveMessageAsync(
                    inbound.From,
                    "¿Necesitas ayuda?",
                    "No pude responder a tu pregunta. ¿Te gustaría hablar con un asesor?",
                    new List<(string, string)>
                    {
                        ("agent_yes", "Sí, con un asesor"),
                        ("agent_no", "No, gracias")
                    },
                    ct);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WhatsApp webhook");
            // Siempre devolver 200 a Meta para evitar reintentos infinitos
            return Ok();
        }
    }
}
