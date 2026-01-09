namespace LeadScoringService.Domain.Entities;

/// <summary>
/// Configuración de reglas de scoring
/// Permite ajustar los pesos y umbrales del modelo
/// </summary>
public class ScoringRule
{
    public Guid Id { get; set; }
    
    // Nombre de la regla
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Tipo de regla
    public ScoringRuleType RuleType { get; set; }
    
    // Peso en el score final (0-100)
    public int Weight { get; set; }
    
    // Fórmula o configuración (JSON)
    public string Configuration { get; set; } = string.Empty;
    
    // Estado
    public bool IsActive { get; set; }
    
    // Prioridad (mayor = se ejecuta primero)
    public int Priority { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

/// <summary>
/// Tipos de reglas de scoring
/// </summary>
public enum ScoringRuleType
{
    Engagement = 0,      // Basado en engagement (vistas, clicks)
    Recency = 1,         // Basado en recencia de interacciones
    Intent = 2,          // Basado en intent signals (test drive, financing)
    Demographic = 3,     // Basado en demografía del usuario
    Behavioral = 4,      // Basado en patrones de comportamiento
    Contextual = 5,      // Basado en contexto (hora, dispositivo)
    Custom = 99          // Regla personalizada
}
