namespace CarDealer.Shared.Resilience.Configuration;

/// <summary>
/// Opciones de Bulkhead Isolation para limitar concurrencia
/// y evitar que un servicio lento afecte a todo el sistema
/// </summary>
public class BulkheadOptions
{
    /// <summary>
    /// Habilitar bulkhead isolation
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Número máximo de ejecuciones concurrentes permitidas
    /// </summary>
    public int MaxParallelization { get; set; } = 25;

    /// <summary>
    /// Número máximo de acciones en cola esperando ejecución
    /// </summary>
    public int MaxQueuingActions { get; set; } = 50;
}
