using MediatR;
using Microsoft.Extensions.Logging;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Enums;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Sessions.Commands;

/// <summary>
/// Handler para iniciar una nueva sesión de chat
/// </summary>
public class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, StartSessionResponse>
{
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatbotConfigurationRepository _configRepository;
    private readonly ILogger<StartSessionCommandHandler> _logger;

    public StartSessionCommandHandler(
        IChatSessionRepository sessionRepository,
        IChatbotConfigurationRepository configRepository,
        ILogger<StartSessionCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _configRepository = configRepository;
        _logger = logger;
    }

    public async Task<StartSessionResponse> Handle(StartSessionCommand request, CancellationToken ct)
    {
        // Obtener configuración del chatbot
        var config = request.DealerId.HasValue
            ? await _configRepository.GetByDealerIdAsync(request.DealerId.Value, ct)
            : await _configRepository.GetDefaultAsync(ct);

        if (config == null)
        {
            throw new InvalidOperationException("No chatbot configuration found");
        }

        // Crear nueva sesión
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            SessionToken = Guid.NewGuid().ToString("N"),
            UserId = request.UserId,
            UserName = request.UserName,
            UserEmail = request.UserEmail,
            UserPhone = request.UserPhone,
            SessionType = Enum.TryParse<SessionType>(request.SessionType, out var st) ? st : SessionType.WebChat,
            Channel = request.Channel ?? "web",
            ChannelUserId = request.ChannelUserId,
            Status = SessionStatus.Active,
            MessageCount = 0,
            InteractionCount = 0,
            MaxInteractionsPerSession = config.MaxInteractionsPerSession,
            InteractionLimitReached = false,
            Language = request.Language ?? "es",
            UserAgent = request.UserAgent,
            IpAddress = request.IpAddress,
            DeviceType = request.DeviceType,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        await _sessionRepository.CreateAsync(session, ct);
        _logger.LogInformation("Created new chat session {SessionId}", session.Id);

        return new StartSessionResponse
        {
            SessionId = session.Id,
            SessionToken = session.SessionToken,
            WelcomeMessage = config.WelcomeMessage,
            BotName = config.BotName,
            BotAvatarUrl = config.BotAvatarUrl,
            MaxInteractionsPerSession = config.MaxInteractionsPerSession,
            RemainingInteractions = config.MaxInteractionsPerSession
        };
    }
}

/// <summary>
/// Handler para enviar un mensaje
/// </summary>
public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ChatbotResponse>
{
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatMessageRepository _messageRepository;
    private readonly IChatbotConfigurationRepository _configRepository;
    private readonly IQuickResponseRepository _quickResponseRepository;
    private readonly IDialogflowService _dialogflowService;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IChatSessionRepository sessionRepository,
        IChatMessageRepository messageRepository,
        IChatbotConfigurationRepository configRepository,
        IQuickResponseRepository quickResponseRepository,
        IDialogflowService dialogflowService,
        ILogger<SendMessageCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _configRepository = configRepository;
        _quickResponseRepository = quickResponseRepository;
        _dialogflowService = dialogflowService;
        _logger = logger;
    }

    public async Task<ChatbotResponse> Handle(SendMessageCommand request, CancellationToken ct)
    {
        var startTime = DateTime.UtcNow;

        // Obtener sesión
        var session = await _sessionRepository.GetByTokenAsync(request.SessionToken, ct);
        if (session == null)
        {
            throw new InvalidOperationException("Session not found");
        }

        // Obtener configuración
        var config = await _configRepository.GetDefaultAsync(ct);
        if (config == null)
        {
            throw new InvalidOperationException("No chatbot configuration found");
        }

        // Verificar límite de interacciones
        if (session.InteractionLimitReached)
        {
            return new ChatbotResponse
            {
                MessageId = Guid.NewGuid(),
                Response = "Has alcanzado el límite de interacciones para esta sesión. Por favor, contacta con un agente.",
                IsFallback = true,
                RemainingInteractions = 0,
                ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }

        // Intentar respuesta rápida primero
        var quickResponse = await _quickResponseRepository.FindMatchingAsync(config.Id, request.Message, ct);
        
        string botResponse;
        string? intentName = null;
        IntentCategory intentCategory = IntentCategory.Other;
        decimal confidenceScore = 0;
        bool isFallback = false;
        bool consumedInteraction = false;

        if (quickResponse != null)
        {
            // Usar respuesta rápida (no consume interacción)
            botResponse = quickResponse.Response;
            intentName = quickResponse.Name;
            confidenceScore = 1.0m;
            _logger.LogInformation("Quick response matched: {QuickResponseName}", quickResponse.Name);
        }
        else
        {
            // Llamar a Dialogflow
            var dialogflowResult = await _dialogflowService.DetectIntentAsync(
                session.SessionToken,
                request.Message,
                session.Language ?? "es",
                ct);

            botResponse = dialogflowResult.FulfillmentText ?? "Lo siento, no entendí tu mensaje.";
            intentName = dialogflowResult.DetectedIntent;
            confidenceScore = (decimal)dialogflowResult.ConfidenceScore;
            isFallback = dialogflowResult.IsFallback;
            consumedInteraction = true;

            // Incrementar contador de interacciones
            session.InteractionCount++;
            if (session.InteractionCount >= session.MaxInteractionsPerSession)
            {
                session.InteractionLimitReached = true;
            }
        }

        // Crear mensaje de usuario
        var userMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            Type = Enum.TryParse<MessageType>(request.MessageType, out var mt) ? mt : MessageType.UserText,
            Content = request.Message,
            MediaUrl = request.MediaUrl,
            IsFromBot = false,
            ConsumedInteraction = false,
            CreatedAt = DateTime.UtcNow
        };

        // Crear respuesta del bot
        var botMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            Type = MessageType.BotText,
            Content = request.Message,
            BotResponse = botResponse,
            DialogflowIntentName = intentName,
            IntentCategory = intentCategory,
            ConfidenceScore = confidenceScore,
            IsFromBot = true,
            ConsumedInteraction = consumedInteraction,
            InteractionCost = consumedInteraction ? 0.002m : 0m,
            ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
            CreatedAt = DateTime.UtcNow
        };

        await _messageRepository.CreateAsync(userMessage, ct);
        await _messageRepository.CreateAsync(botMessage, ct);

        // Actualizar sesión
        session.MessageCount += 2;
        session.LastActivityAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(session, ct);

        return new ChatbotResponse
        {
            MessageId = botMessage.Id,
            Response = botResponse,
            IntentName = intentName,
            IntentCategory = intentCategory,
            ConfidenceScore = confidenceScore,
            IsFallback = isFallback,
            RemainingInteractions = session.MaxInteractionsPerSession - session.InteractionCount,
            ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds
        };
    }
}

/// <summary>
/// Handler para finalizar una sesión
/// </summary>
public class EndSessionCommandHandler : IRequestHandler<EndSessionCommand, bool>
{
    private readonly IChatSessionRepository _sessionRepository;
    private readonly ILogger<EndSessionCommandHandler> _logger;

    public EndSessionCommandHandler(
        IChatSessionRepository sessionRepository,
        ILogger<EndSessionCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(EndSessionCommand request, CancellationToken ct)
    {
        var session = await _sessionRepository.GetByTokenAsync(request.SessionToken, ct);
        if (session == null) return false;

        session.Status = SessionStatus.Completed;
        session.EndedAt = DateTime.UtcNow;
        session.SessionDurationSeconds = (int)(DateTime.UtcNow - session.CreatedAt).TotalSeconds;

        await _sessionRepository.UpdateAsync(session, ct);
        _logger.LogInformation("Session {SessionId} ended", session.Id);

        return true;
    }
}

/// <summary>
/// Handler para transferir a agente humano
/// </summary>
public class TransferToAgentCommandHandler : IRequestHandler<TransferToAgentCommand, TransferToAgentResult>
{
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatLeadRepository _leadRepository;
    private readonly ILogger<TransferToAgentCommandHandler> _logger;

    public TransferToAgentCommandHandler(
        IChatSessionRepository sessionRepository,
        IChatLeadRepository leadRepository,
        ILogger<TransferToAgentCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _leadRepository = leadRepository;
        _logger = logger;
    }

    public async Task<TransferToAgentResult> Handle(TransferToAgentCommand request, CancellationToken ct)
    {
        var session = await _sessionRepository.GetByTokenAsync(request.SessionToken, ct);
        if (session == null)
        {
            return new TransferToAgentResult(
                Success: false,
                AgentName: null,
                Message: "Session not found",
                EstimatedWaitTimeMinutes: null
            );
        }

        session.Status = SessionStatus.TransferredToAgent;
        await _sessionRepository.UpdateAsync(session, ct);

        // Crear lead si hay datos de contacto
        if (!string.IsNullOrEmpty(session.UserPhone) || !string.IsNullOrEmpty(session.UserEmail))
        {
            var lead = new ChatLead
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                FullName = session.UserName ?? "Unknown",
                Email = session.UserEmail,
                Phone = session.UserPhone,
                PreferredContactMethod = "phone",
                Status = LeadStatus.New,
                Temperature = LeadTemperature.Warm,
                Notes = request.Reason,
                CreatedAt = DateTime.UtcNow
            };
            await _leadRepository.CreateAsync(lead, ct);
        }

        _logger.LogInformation("Session {SessionId} transferred to agent", session.Id);
        
        return new TransferToAgentResult(
            Success: true,
            AgentName: null,
            Message: "Your request has been transferred to a human agent. Please wait.",
            EstimatedWaitTimeMinutes: 5
        );
    }
}
