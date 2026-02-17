namespace Video360Service.Infrastructure.Configuration;

/// <summary>
/// Configuración para FFmpeg-API.com (Starter Plan - $11/mes)
/// Calidad: Excelente
/// </summary>
public class FfmpegApiSettings
{
    public const string SectionName = "FfmpegApi";
    
    public string BaseUrl { get; set; } = "https://api.ffmpeg-api.com";
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 100; // Más alto = más prioritario
    public decimal CostPerVideoUsd { get; set; } = 0.011m;
    public int TimeoutSeconds { get; set; } = 120;
    public int MaxVideoSizeMb { get; set; } = 100;
    public int MaxVideoDurationSeconds { get; set; } = 120;
    public string[] SupportedFormats { get; set; } = ["mp4", "webm", "mov", "avi"];
}

/// <summary>
/// Configuración para ApyHub Video API ($9/mes)
/// Calidad: Muy buena
/// </summary>
public class ApyHubSettings
{
    public const string SectionName = "ApyHub";
    
    public string BaseUrl { get; set; } = "https://api.apyhub.com/processor/video";
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 90;
    public decimal CostPerVideoUsd { get; set; } = 0.009m;
    public int TimeoutSeconds { get; set; } = 120;
    public int MaxVideoSizeMb { get; set; } = 50;
    public int MaxVideoDurationSeconds { get; set; } = 60;
    public string[] SupportedFormats { get; set; } = ["mp4", "webm"];
}

/// <summary>
/// Configuración para Cloudinary ($12/mes)
/// Calidad: Buena
/// </summary>
public class CloudinarySettings
{
    public const string SectionName = "Cloudinary";
    
    public string BaseUrl { get; set; } = "https://api.cloudinary.com/v1_1";
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 70;
    public decimal CostPerVideoUsd { get; set; } = 0.012m;
    public int TimeoutSeconds { get; set; } = 180;
    public int MaxVideoSizeMb { get; set; } = 100;
    public int MaxVideoDurationSeconds { get; set; } = 120;
    public string[] SupportedFormats { get; set; } = ["mp4", "webm", "mov", "avi", "mkv"];
}

/// <summary>
/// Configuración para Imgix ($18/mes)
/// Calidad: Excelente
/// </summary>
public class ImgixSettings
{
    public const string SectionName = "Imgix";
    
    public string BaseUrl { get; set; } = "https://api.imgix.com";
    public string SourceDomain { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } = 80;
    public decimal CostPerVideoUsd { get; set; } = 0.018m;
    public int TimeoutSeconds { get; set; } = 120;
    public int MaxVideoSizeMb { get; set; } = 100;
    public int MaxVideoDurationSeconds { get; set; } = 120;
    public string[] SupportedFormats { get; set; } = ["mp4", "webm", "mov"];
}

/// <summary>
/// Configuración para Shotstack ($50/mes)
/// Calidad: Profesional
/// </summary>
public class ShotstackSettings
{
    public const string SectionName = "Shotstack";
    
    public string BaseUrl { get; set; } = "https://api.shotstack.io/stage";
    public bool IsEnabled { get; set; } = false; // Más caro, usar como premium
    public int Priority { get; set; } = 50;
    public decimal CostPerVideoUsd { get; set; } = 0.05m;
    public int TimeoutSeconds { get; set; } = 300;
    public int MaxVideoSizeMb { get; set; } = 500;
    public int MaxVideoDurationSeconds { get; set; } = 600;
    public string[] SupportedFormats { get; set; } = ["mp4", "webm", "mov", "avi", "mkv", "m4v"];
}

/// <summary>
/// Configuración de almacenamiento S3
/// </summary>
public class S3StorageSettings
{
    public const string SectionName = "S3Storage";
    
    public string BucketName { get; set; } = "okla-video360";
    public string ImagesBucketName { get; set; } = "okla-video360-images";
    public string Region { get; set; } = "us-east-1";
    public string Prefix { get; set; } = "video360";
    public string VideoPrefix { get; set; } = "videos";
    public string ImagesPrefix { get; set; } = "images";
    public bool UseLocalPath { get; set; } = false;
    public string LocalPath { get; set; } = "/app/media-files";
    public int SignedUrlExpirationMinutes { get; set; } = 60;
    public int PresignedUrlExpirationMinutes { get; set; } = 60;
    
    /// <summary>
    /// AWS Access Key ID
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;
    
    /// <summary>
    /// AWS Secret Access Key
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// URL del servicio S3 (para MinIO o S3-compatible)
    /// </summary>
    public string? ServiceUrl { get; set; }
    
    /// <summary>
    /// Si usar path-style en lugar de virtual-hosted-style
    /// </summary>
    public bool ForcePathStyle { get; set; } = false;
    
    /// <summary>
    /// URL base del CDN para servir los archivos
    /// </summary>
    public string? CdnBaseUrl { get; set; }
    
    /// <summary>
    /// Tamaño máximo de archivo en MB
    /// </summary>
    public int MaxFileSizeMb { get; set; } = 500;
    
    /// <summary>
    /// Si habilitar encriptación server-side
    /// </summary>
    public bool EnableEncryption { get; set; } = true;
    
    /// <summary>
    /// ACL por defecto para los archivos
    /// </summary>
    public string DefaultAcl { get; set; } = "private";
}
