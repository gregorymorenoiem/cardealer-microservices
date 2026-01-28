namespace ChatbotService.Domain.Entities;

/// <summary>
/// Análisis de intención del usuario realizado por el LLM
/// </summary>
public class IntentAnalysis
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public Guid MessageId { get; set; }
    
    // Intención principal detectada
    public IntentType IntentType { get; set; }
    public double Confidence { get; set; } // 0.0 - 1.0
    
    // Señales de compra detectadas
    public List<BuyingSignal> BuyingSignals { get; set; } = new();
    
    // Extracción de información
    public string? ExtractedUrgency { get; set; } // "inmediato", "esta semana", "este mes", etc.
    public string? ExtractedBudget { get; set; }
    public bool? HasTradeIn { get; set; }
    public string? TradeInDetails { get; set; }
    
    // Score parcial de este mensaje
    public int PartialScore { get; set; } // Contribución al lead score total
    
    // Timestamp
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public enum IntentType
{
    Unknown,
    InformationSeeking,   // Solo pregunta
    PriceInquiry,         // Pregunta por precio
    FeatureComparison,    // Compara características
    AvailabilityCheck,    // ¿Está disponible?
    TestDriveRequest,     // Quiere probar el carro
    FinancingInquiry,     // Pregunta por financiamiento
    TradeInInterest,      // Quiere entregar su carro
    ReadyToBuy,           // Listo para comprar
    Objection,            // Pone objeciones
    Farewell              // Se despide
}

public class BuyingSignal
{
    public string Signal { get; set; } = string.Empty;
    public SignalType Type { get; set; }
    public int Weight { get; set; } // Peso en el scoring (positivo o negativo)
}

public enum SignalType
{
    Urgency,      // +25: "lo necesito ya"
    Financial,    // +20: "tengo el dinero"
    TradeIn,      // +15: "tengo carro para entregar"
    Commitment,   // +25: "quiero test drive"
    Engagement,   // +10: respuestas largas
    Negative      // -10 a -15: "solo mirando"
}
