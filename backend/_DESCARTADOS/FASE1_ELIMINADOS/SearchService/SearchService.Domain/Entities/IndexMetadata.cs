using SearchService.Domain.Enums;

namespace SearchService.Domain.Entities;

/// <summary>
/// Representa la configuración y metadatos de un índice de Elasticsearch
/// </summary>
public class IndexMetadata
{
    /// <summary>
    /// Nombre del índice
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del índice
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual del índice
    /// </summary>
    public IndexStatus Status { get; set; } = IndexStatus.Active;

    /// <summary>
    /// Número de documentos en el índice
    /// </summary>
    public long DocumentCount { get; set; }

    /// <summary>
    /// Tamaño del índice en bytes
    /// </summary>
    public long SizeInBytes { get; set; }

    /// <summary>
    /// Fecha de creación del índice
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Última actualización del índice
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Configuración de mappings (esquema)
    /// </summary>
    public Dictionary<string, object> Mappings { get; set; } = new();

    /// <summary>
    /// Configuración de settings (réplicas, shards, etc.)
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();

    /// <summary>
    /// Aliases del índice
    /// </summary>
    public List<string> Aliases { get; set; } = new();

    /// <summary>
    /// Número de shards primarios
    /// </summary>
    public int PrimaryShards { get; set; } = 1;

    /// <summary>
    /// Número de réplicas
    /// </summary>
    public int ReplicaCount { get; set; } = 1;

    /// <summary>
    /// Calcula el tamaño formateado del índice
    /// </summary>
    public string GetFormattedSize()
    {
        if (SizeInBytes < 1024)
            return $"{SizeInBytes} B";
        if (SizeInBytes < 1024 * 1024)
            return $"{SizeInBytes / 1024.0:F2} KB";
        if (SizeInBytes < 1024 * 1024 * 1024)
            return $"{SizeInBytes / (1024.0 * 1024.0):F2} MB";
        return $"{SizeInBytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }

    /// <summary>
    /// Verifica si el índice está saludable
    /// </summary>
    public bool IsHealthy() => Status == IndexStatus.Active;

    /// <summary>
    /// Verifica si el índice necesita reindexación
    /// </summary>
    public bool NeedsReindexing() => Status == IndexStatus.Reindexing;
}
