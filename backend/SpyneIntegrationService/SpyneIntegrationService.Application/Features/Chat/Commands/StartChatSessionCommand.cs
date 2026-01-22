using MediatR;
using SpyneIntegrationService.Application.DTOs;

namespace SpyneIntegrationService.Application.Features.Chat.Commands;

/// <summary>
/// Command to start a new chat session with Vini AI
/// 
/// ⚠️ FASE 4: Este endpoint está IMPLEMENTADO pero NO debe ser consumido 
/// por el frontend en esta versión. Está preparado para futuras iteraciones.
/// </summary>
public record StartChatSessionCommand : IRequest<ChatSessionDto>
{
    /// <summary>
    /// ID del vehículo sobre el que se inicia la conversación
    /// </summary>
    public Guid VehicleId { get; init; }
    
    /// <summary>
    /// ID del dealer propietario del vehículo (opcional)
    /// </summary>
    public Guid? DealerId { get; init; }
    
    /// <summary>
    /// ID del usuario que inicia el chat (puede ser anónimo)
    /// </summary>
    public Guid? UserId { get; init; }
    
    /// <summary>
    /// Identificador de sesión para usuarios anónimos
    /// </summary>
    public string? SessionIdentifier { get; init; }
    
    /// <summary>
    /// Contexto inicial del vehículo (marca, modelo, año, precio, etc.)
    /// </summary>
    public VehicleContextDto? VehicleContext { get; init; }
    
    /// <summary>
    /// Idioma preferido del chat (es, en)
    /// </summary>
    public string Language { get; init; } = "es";
}
