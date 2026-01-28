using MediatR;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Sessions.Commands;

/// <summary>
/// Comando para iniciar una nueva sesión de chat
/// </summary>
public record StartSessionCommand(
    Guid? UserId,
    string? UserName,
    string? UserEmail,
    string? UserPhone,
    string SessionType,
    string Channel,
    string? ChannelUserId,
    string? UserAgent,
    string? IpAddress,
    string? DeviceType,
    string Language,
    Guid? DealerId
) : IRequest<StartSessionResponse>;

/// <summary>
/// Comando para enviar un mensaje al chatbot
/// </summary>
public record SendMessageCommand(
    string SessionToken,
    string Message,
    string MessageType,
    string? MediaUrl
) : IRequest<ChatbotResponse>;

/// <summary>
/// Comando para finalizar una sesión
/// </summary>
public record EndSessionCommand(
    string SessionToken,
    string? Reason
) : IRequest<bool>;

/// <summary>
/// Comando para transferir a un agente humano
/// </summary>
public record TransferToAgentCommand(
    string SessionToken,
    string? Reason,
    Guid? PreferredAgentId
) : IRequest<TransferToAgentResult>;

public record TransferToAgentResult(
    bool Success,
    string? AgentName,
    string? Message,
    int? EstimatedWaitTimeMinutes
);
