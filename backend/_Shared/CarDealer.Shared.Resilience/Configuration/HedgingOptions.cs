namespace CarDealer.Shared.Resilience.Configuration;

/// <summary>
/// Opciones de Hedging (request hedging) para enviar requests duplicados
/// a réplicas alternativas cuando el primario es lento
/// </summary>
public class HedgingOptions
{
    /// <summary>
    /// Habilitar hedging
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Número máximo de acciones hedged (réplicas adicionales)
    /// </summary>
    public int MaxHedgedAttempts { get; set; } = 2;

    /// <summary>
    /// Delay antes de enviar el hedge request (milisegundos).
    /// Si el request original no responde en este tiempo, se envía uno adicional.
    /// </summary>
    public int DelayMilliseconds { get; set; } = 2000;
}
