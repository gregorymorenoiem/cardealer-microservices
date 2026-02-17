using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Enums;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Domain.Models;

namespace ChatbotService.Infrastructure.Services.Strategies;

/// <summary>
/// Estrategia para chat general sin contexto de vehículo específico.
/// FAQ, soporte, información general del marketplace.
/// </summary>
public class GeneralChatStrategy : IChatModeStrategy
{
    public ChatMode Mode => ChatMode.General;

    public Task<string> BuildSystemPromptAsync(
        ChatSession session,
        ChatbotConfiguration config,
        string userMessage,
        CancellationToken ct = default)
    {
        var botName = config.BotName ?? "Ana";

        var prompt = $@"Eres {botName}, asistente virtual de OKLA, el marketplace de vehículos #1 en República Dominicana.

## 🎯 Tu rol
- Ayudar a los usuarios a navegar el marketplace
- Responder preguntas generales sobre compra/venta de vehículos en RD
- Dirigir a los usuarios a las secciones apropiadas del portal
- Dar información general sobre financiamiento y proceso de compra

## 📋 Reglas
1. NO tienes acceso a vehículos específicos en este modo.
2. Si preguntan por un vehículo específico, sugiéreles buscar en el catálogo o visitar el perfil del dealer.
3. Puedes responder preguntas generales sobre: financiamiento, documentación, proceso de compra, impuestos vehiculares en RD.
4. Responde en español dominicano, breve y amigable.
5. Si el usuario necesita asistencia específica, ofrece transferir a un agente.";

        if (!string.IsNullOrWhiteSpace(config.SystemPromptText))
        {
            prompt += $"\n\n## Instrucciones adicionales\n{config.SystemPromptText}";
        }

        return Task.FromResult(prompt);
    }

    public Task<List<FunctionDefinition>> GetAvailableFunctionsAsync(
        ChatSession session, CancellationToken ct = default)
    {
        return Task.FromResult(new List<FunctionDefinition>());
    }

    public Task<FunctionCallResult> ExecuteFunctionAsync(
        ChatSession session, FunctionCall functionCall, CancellationToken ct = default)
    {
        return Task.FromResult(new FunctionCallResult
        {
            Success = false,
            ErrorMessage = "Function calling no disponible en modo general"
        });
    }

    public Task<GroundingValidationResult> ValidateResponseGroundingAsync(
        ChatSession session, string llmResponse, CancellationToken ct = default)
    {
        return Task.FromResult(new GroundingValidationResult { IsGrounded = true });
    }
}
