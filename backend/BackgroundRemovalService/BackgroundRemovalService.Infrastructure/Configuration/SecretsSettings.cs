namespace BackgroundRemovalService.Infrastructure.Configuration;

/// <summary>
/// Configuración de secretos centralizados para el servicio.
/// Los secretos se almacenan separados de la configuración principal para facilitar
/// el uso de secrets managers (Azure Key Vault, AWS Secrets Manager, etc.)
/// </summary>
public class SecretsSettings
{
    public const string SectionName = "Secrets";
    
    public ProviderSecrets ClipDrop { get; set; } = new();
    public ProviderSecrets RemoveBg { get; set; } = new();
    public ProviderSecrets Photoroom { get; set; } = new();
    public ProviderSecrets Slazzer { get; set; } = new();
    public S3Secrets S3 { get; set; } = new();
}

/// <summary>
/// Secretos genéricos para proveedores de API
/// </summary>
public class ProviderSecrets
{
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Secretos específicos para S3/Storage
/// </summary>
public class S3Secrets
{
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
