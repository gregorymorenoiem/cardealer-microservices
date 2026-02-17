namespace KYCService.Infrastructure.ExternalServices;

/// <summary>
/// Interface para el servicio de OCR (Reconocimiento Óptico de Caracteres)
/// </summary>
public interface IOCRService
{
    /// <summary>
    /// Extrae texto y datos de una imagen de documento
    /// </summary>
    Task<OCRResult> ExtractTextAsync(byte[] imageData, DocumentOCRType documentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extrae datos de una cédula dominicana (frente)
    /// </summary>
    Task<CedulaOCRResult> ExtractCedulaFrontAsync(byte[] imageData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extrae datos de una cédula dominicana (reverso)
    /// </summary>
    Task<CedulaOCRResult> ExtractCedulaBackAsync(byte[] imageData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica la calidad de la imagen para OCR
    /// </summary>
    Task<ImageQualityResult> CheckImageQualityAsync(byte[] imageData, CancellationToken cancellationToken = default);
}

/// <summary>
/// Tipo de documento para OCR
/// </summary>
public enum DocumentOCRType
{
    CedulaFront = 1,
    CedulaBack = 2,
    PassportFront = 3,
    PassportBack = 4,
    DriverLicenseFront = 5,
    DriverLicenseBack = 6,
    Generic = 0
}

/// <summary>
/// Resultado del procesamiento OCR
/// </summary>
public class OCRResult
{
    /// <summary>
    /// Indica si el procesamiento fue exitoso
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Texto completo extraído
    /// </summary>
    public string RawText { get; set; } = string.Empty;

    /// <summary>
    /// Líneas de texto detectadas
    /// </summary>
    public List<OCRTextLine> TextLines { get; set; } = new();

    /// <summary>
    /// Datos estructurados extraídos
    /// </summary>
    public Dictionary<string, string> ExtractedFields { get; set; } = new();

    /// <summary>
    /// Confianza general del OCR (0-100)
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Tiempo de procesamiento en ms
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// Idioma detectado
    /// </summary>
    public string? DetectedLanguage { get; set; }
}

/// <summary>
/// Línea de texto detectada por OCR
/// </summary>
public class OCRTextLine
{
    public string Text { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public OCRBoundingBox? BoundingBox { get; set; }
}

/// <summary>
/// Bounding box de un elemento detectado
/// </summary>
public class OCRBoundingBox
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// Resultado específico para cédula dominicana
/// </summary>
public class CedulaOCRResult : OCRResult
{
    /// <summary>
    /// Número de cédula detectado (formato: XXX-XXXXXXX-X)
    /// </summary>
    public string? CedulaNumber { get; set; }

    /// <summary>
    /// Nombre completo
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Primer nombre
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Segundo nombre
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Primer apellido
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Segundo apellido
    /// </summary>
    public string? SecondLastName { get; set; }

    /// <summary>
    /// Fecha de nacimiento
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Lugar de nacimiento
    /// </summary>
    public string? BirthPlace { get; set; }

    /// <summary>
    /// Nacionalidad
    /// </summary>
    public string? Nationality { get; set; }

    /// <summary>
    /// Sexo (M/F)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Estado civil
    /// </summary>
    public string? MaritalStatus { get; set; }

    /// <summary>
    /// Dirección (del reverso)
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Fecha de expedición
    /// </summary>
    public DateTime? IssueDate { get; set; }

    /// <summary>
    /// Fecha de expiración
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Código MRZ (Machine Readable Zone) si está disponible
    /// </summary>
    public string? MRZCode { get; set; }

    /// <summary>
    /// Indica si se detectó la foto en el documento
    /// </summary>
    public bool PhotoDetected { get; set; }

    /// <summary>
    /// Bounding box de la foto detectada
    /// </summary>
    public OCRBoundingBox? PhotoBoundingBox { get; set; }
}

/// <summary>
/// Resultado del análisis de calidad de imagen
/// </summary>
public class ImageQualityResult
{
    /// <summary>
    /// Indica si la imagen es adecuada para OCR
    /// </summary>
    public bool IsAcceptable { get; set; }

    /// <summary>
    /// Puntuación general de calidad (0-100)
    /// </summary>
    public decimal QualityScore { get; set; }

    /// <summary>
    /// Métricas individuales de calidad
    /// </summary>
    public ImageQualityMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Problemas detectados
    /// </summary>
    public List<string> Issues { get; set; } = new();

    /// <summary>
    /// Sugerencias para mejorar la imagen
    /// </summary>
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>
/// Métricas de calidad de imagen
/// </summary>
public class ImageQualityMetrics
{
    /// <summary>
    /// Nitidez de la imagen (0-100)
    /// </summary>
    public decimal Sharpness { get; set; }

    /// <summary>
    /// Nivel de luminosidad (0-100)
    /// </summary>
    public decimal Brightness { get; set; }

    /// <summary>
    /// Contraste de la imagen (0-100)
    /// </summary>
    public decimal Contrast { get; set; }

    /// <summary>
    /// Indica si hay reflejos/glare detectados
    /// </summary>
    public bool HasGlare { get; set; }

    /// <summary>
    /// Indica si la imagen está borrosa
    /// </summary>
    public bool IsBlurry { get; set; }

    /// <summary>
    /// Indica si el documento está cortado
    /// </summary>
    public bool IsDocumentCutOff { get; set; }

    /// <summary>
    /// Ángulo de inclinación detectado (grados)
    /// </summary>
    public decimal SkewAngle { get; set; }

    /// <summary>
    /// Resolución de la imagen (píxeles)
    /// </summary>
    public int Width { get; set; }
    public int Height { get; set; }

    /// <summary>
    /// DPI estimado
    /// </summary>
    public int EstimatedDPI { get; set; }
}
