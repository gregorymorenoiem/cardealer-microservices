using ChatbotService.Domain.Enums;

namespace ChatbotService.Domain.Entities;

/// <summary>
/// Representa una sesión de conversación con el chatbot
/// </summary>
public class ChatSession
{
    public Guid Id { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    
    // Configuración del chatbot
    public Guid ChatbotConfigurationId { get; set; }
    
    // Usuario (puede ser anónimo)
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserPhone { get; set; }
    
    // Información del canal
    public SessionType SessionType { get; set; }
    public string Channel { get; set; } = "web"; // web, whatsapp, facebook, etc.
    public string? ChannelUserId { get; set; } // ID del usuario en el canal externo
    
    // Estado de la sesión
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public string? ContextData { get; set; } // JSON con contexto de Dialogflow
    
    // Métricas de la sesión
    public int MessageCount { get; set; }
    public int InteractionCount { get; set; } // Interacciones con Dialogflow
    public int MaxInteractionsPerSession { get; set; } = 10; // Límite configurable
    public bool InteractionLimitReached { get; set; }
    
    // Contexto del vehículo si están buscando uno específico
    public Guid? CurrentVehicleId { get; set; }
    public string? CurrentVehicleName { get; set; }
    
    // Lead generado
    public Guid? LeadId { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public int SessionDurationSeconds { get; set; }
    
    // Metadata
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceType { get; set; }
    public string? Language { get; set; } = "es";
    
    // Relaciones
    public virtual ChatbotConfiguration Configuration { get; set; } = null!;
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public virtual ChatLead? Lead { get; set; }
}

/// <summary>
/// Representa un mensaje individual en la conversación
/// </summary>
public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    
    // Contenido del mensaje
    public MessageType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; }
    
    // Información de Dialogflow
    public string? DialogflowIntentName { get; set; }
    public string? DialogflowIntentId { get; set; }
    public IntentCategory IntentCategory { get; set; }
    public decimal ConfidenceScore { get; set; }
    public ConfidenceLevel ConfidenceLevel { get; set; }
    public string? DialogflowParameters { get; set; } // JSON
    
    // Análisis de sentimiento
    public SentimentType Sentiment { get; set; } = SentimentType.Neutral;
    public decimal SentimentScore { get; set; }
    
    // Quick replies / Botones
    public string? QuickReplies { get; set; } // JSON array de opciones
    
    // Respuesta del bot
    public string? BotResponse { get; set; }
    public bool IsFromBot { get; set; }
    public int? ResponseTimeMs { get; set; }
    
    // Si se usó una interacción de Dialogflow
    public bool ConsumedInteraction { get; set; }
    public decimal? InteractionCost { get; set; } // Costo en USD
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Relaciones
    public virtual ChatSession Session { get; set; } = null!;
}

/// <summary>
/// Lead generado por el chatbot
/// </summary>
public class ChatLead
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    
    // Información del prospecto
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? WhatsApp { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? PreferredContactTime { get; set; }
    
    // Interés
    public Guid? InterestedVehicleId { get; set; }
    public string? InterestedVehicleName { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public bool InterestedInFinancing { get; set; }
    public bool InterestedInTradeIn { get; set; }
    public string? TradeInVehicleInfo { get; set; }
    
    // Calificación del lead
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadTemperature Temperature { get; set; } = LeadTemperature.Warm;
    public int QualificationScore { get; set; } // 0-100
    public string? QualificationNotes { get; set; }
    
    // Asignación
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public DateTime? AssignedAt { get; set; }
    
    // Seguimiento
    public DateTime? LastContactedAt { get; set; }
    public int ContactAttempts { get; set; }
    public string? Notes { get; set; }
    
    // Conversión
    public DateTime? ConvertedAt { get; set; }
    public Guid? ConvertedToSaleId { get; set; }
    public decimal? SaleAmount { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Relaciones
    public virtual ChatSession Session { get; set; } = null!;
}
