namespace Video360Service.Infrastructure.Configuration;

/// <summary>
/// Configuración de secretos centralizados para el servicio Video360.
/// Los secretos se almacenan separados de la configuración principal.
/// </summary>
public class SecretsSettings
{
    public const string SectionName = "Secrets";
    
    public FfmpegApiSecrets FfmpegApi { get; set; } = new();
    public ApyHubSecrets ApyHub { get; set; } = new();
    public CloudinarySecrets Cloudinary { get; set; } = new();
    public ImgixSecrets Imgix { get; set; } = new();
    public ShotstackSecrets Shotstack { get; set; } = new();
    public S3Secrets S3 { get; set; } = new();
}

/// <summary>
/// Secretos para FFmpeg-API.com
/// </summary>
public class FfmpegApiSecrets
{
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Secretos para ApyHub
/// </summary>
public class ApyHubSecrets
{
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Secretos para Cloudinary
/// </summary>
public class CloudinarySecrets
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}

/// <summary>
/// Secretos para Imgix
/// </summary>
public class ImgixSecrets
{
    public string ApiKey { get; set; } = string.Empty;
    public string SecureUrlToken { get; set; } = string.Empty;
}

/// <summary>
/// Secretos para Shotstack
/// </summary>
public class ShotstackSecrets
{
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Secretos para S3/Storage
/// </summary>
public class S3Secrets
{
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
