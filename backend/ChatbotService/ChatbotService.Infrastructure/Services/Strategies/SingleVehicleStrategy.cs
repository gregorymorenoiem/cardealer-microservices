using Microsoft.Extensions.Logging;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Enums;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Domain.Models;

namespace ChatbotService.Infrastructure.Services.Strategies;

/// <summary>
/// Estrategia para chat sobre UN vehículo específico.
/// El usuario hizo clic en el chat de una publicación concreta.
/// Contexto fijo: solo datos de ese vehículo.
/// NO necesita function calling ni RAG.
/// </summary>
public class SingleVehicleStrategy : IChatModeStrategy
{
    private readonly IChatbotVehicleRepository _vehicleRepository;
    private readonly ILogger<SingleVehicleStrategy> _logger;
    
    public ChatMode Mode => ChatMode.SingleVehicle;

    public SingleVehicleStrategy(
        IChatbotVehicleRepository vehicleRepository,
        ILogger<SingleVehicleStrategy> logger)
    {
        _vehicleRepository = vehicleRepository;
        _logger = logger;
    }

    public async Task<string> BuildSystemPromptAsync(
        ChatSession session,
        ChatbotConfiguration config,
        string userMessage,
        CancellationToken ct = default)
    {
        if (!session.VehicleId.HasValue)
        {
            _logger.LogWarning("SingleVehicleStrategy: No VehicleId in session {SessionId}", session.Id);
            return BuildFallbackPrompt(config);
        }

        // Buscar el vehículo específico
        var vehicle = await _vehicleRepository.GetByVehicleIdAsync(
            session.ChatbotConfigurationId, session.VehicleId.Value, ct);

        if (vehicle == null)
        {
            _logger.LogWarning("SingleVehicleStrategy: Vehicle {VehicleId} not found for session {SessionId}",
                session.VehicleId, session.Id);
            return BuildFallbackPrompt(config);
        }

        return BuildSingleVehiclePrompt(config, vehicle);
    }

    public Task<List<FunctionDefinition>> GetAvailableFunctionsAsync(
        ChatSession session, CancellationToken ct = default)
    {
        // Modo SingleVehicle no necesita function calling
        // Todo el contexto está en el system prompt
        return Task.FromResult(new List<FunctionDefinition>());
    }

    public Task<FunctionCallResult> ExecuteFunctionAsync(
        ChatSession session, FunctionCall functionCall, CancellationToken ct = default)
    {
        // No se ejecutan funciones en este modo
        return Task.FromResult(new FunctionCallResult
        {
            Success = false,
            ErrorMessage = "Function calling no disponible en modo Single Vehicle"
        });
    }

    public async Task<GroundingValidationResult> ValidateResponseGroundingAsync(
        ChatSession session, string llmResponse, CancellationToken ct = default)
    {
        // En modo single vehicle, validar que el LLM no invente datos del vehículo
        if (!session.VehicleId.HasValue)
            return new GroundingValidationResult { IsGrounded = true };

        var vehicle = await _vehicleRepository.GetByVehicleIdAsync(
            session.ChatbotConfigurationId, session.VehicleId.Value, ct);

        if (vehicle == null)
            return new GroundingValidationResult { IsGrounded = true };

        var result = new GroundingValidationResult { IsGrounded = true };
        var lowerResponse = llmResponse.ToLowerInvariant();

        // Verificar que no mencione otras marcas/modelos como si fueran este vehículo
        // (Nota: el LLM podría mencionar otra marca en contexto de comparación,
        //  pero no debe decir "este Toyota" si el vehículo es un Honda)
        var vehicleMake = vehicle.Make.ToLowerInvariant();
        var vehicleModel = vehicle.Model.ToLowerInvariant();

        // Frases que indicarían que el LLM está confundiendo el vehículo
        var confusionPatterns = new[]
        {
            "este vehículo es un", "este carro es un", "este auto es un",
            "estás viendo un", "estás mirando un"
        };

        foreach (var pattern in confusionPatterns)
        {
            var idx = lowerResponse.IndexOf(pattern, StringComparison.Ordinal);
            if (idx >= 0)
            {
                var afterPattern = lowerResponse[(idx + pattern.Length)..].TrimStart();
                // Si menciona una marca diferente después del patrón, es un error
                if (!afterPattern.StartsWith(vehicleMake) && !afterPattern.StartsWith(vehicleModel))
                {
                    result.IsGrounded = false;
                    result.UngroundedClaims.Add($"Menciona vehículo incorrecto después de '{pattern}'");
                }
            }
        }

        return result;
    }

    // ══════════════════════════════════════════════════════════════
    // PROMPT BUILDERS
    // ══════════════════════════════════════════════════════════════

    private static string BuildSingleVehiclePrompt(ChatbotConfiguration config, ChatbotVehicle vehicle)
    {
        var botName = config.BotName ?? "Ana";
        var dealerName = config.Name ?? "OKLA";
        var saleInfo = vehicle.IsOnSale && vehicle.OriginalPrice.HasValue
            ? $"\n- 🏷️ EN OFERTA: Precio original RD${vehicle.OriginalPrice:N0}, ahora RD${vehicle.Price:N0}"
            : "";
        var mileage = vehicle.Mileage.HasValue ? $"{vehicle.Mileage.Value:N0} km" : "No especificado";
        var description = !string.IsNullOrEmpty(vehicle.Description) ? $"\n- Descripción: {vehicle.Description}" : "";

        return $@"Eres {botName}, asistente de ventas de {dealerName} en República Dominicana.
Estás respondiendo EXCLUSIVAMENTE sobre este vehículo:

## 🚗 Vehículo
- Marca: {vehicle.Make}
- Modelo: {vehicle.Model}
- Año: {vehicle.Year}
- Versión: {vehicle.Trim ?? "N/A"}
- Precio: RD${vehicle.Price:N0}{saleInfo}
- Kilometraje: {mileage}
- Transmisión: {vehicle.Transmission ?? "N/A"}
- Combustible: {vehicle.FuelType ?? "N/A"}
- Color exterior: {vehicle.ExteriorColor ?? vehicle.Color ?? "N/A"}
- Tipo de carrocería: {vehicle.BodyType ?? "N/A"}{description}

## 📋 Reglas ESTRICTAS
1. SOLO responde sobre el vehículo descrito arriba. NO inventes datos.
2. Si preguntan por OTRO vehículo, di: ""Este chat es sobre el {vehicle.Year} {vehicle.Make} {vehicle.Model}. Para ver otros vehículos, visita nuestro catálogo o el perfil del dealer.""
3. Puedes responder sobre: precio, financiamiento, ubicación, garantía, características, historial.
4. Puedes agendar cita para ver este vehículo.
5. NUNCA reveles precio mínimo, descuentos internos, o márgenes del dealer.
6. Si no tienes un dato específico (ej: historial de accidentes), di que no tienes esa información y sugiere contactar al dealer.
7. Responde en español dominicano, breve y amigable. Máximo 3-4 oraciones.
8. Usa emojis moderadamente (1-2 por respuesta).";
    }

    private static string BuildFallbackPrompt(ChatbotConfiguration config)
    {
        var botName = config.BotName ?? "Ana";
        return $@"Eres {botName}, asistente de ventas de vehículos en República Dominicana.
No pudimos cargar los datos del vehículo específico. 
Pide disculpas al usuario y sugiérele que recargue la página o visite el catálogo.
Responde en español dominicano, breve y amigable.";
    }
}
