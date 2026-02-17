namespace KYCService.Application.DTOs;

/// <summary>
/// Resultado de verificación con JCE
/// </summary>
public class JCEVerificationResultDto
{
    public bool Success { get; set; }
    public bool IsValid { get; set; }
    public string? JCEStatus { get; set; }
    public string Source { get; set; } = "LOCAL";
    public List<string>? ValidationErrors { get; set; }
    public JCEVerifiedDataDto? VerifiedData { get; set; }
    public JCEMatchResultsDto? MatchResults { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

/// <summary>
/// Datos verificados del ciudadano según JCE
/// </summary>
public class JCEVerifiedDataDto
{
    public string FullName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? BirthPlace { get; set; }
    public string? Nationality { get; set; }
    public string? Gender { get; set; }
    public string? PhotoUrl { get; set; }
    public string? MunicipalityCode { get; set; }
}

/// <summary>
/// Resultados de coincidencia con datos proporcionados
/// </summary>
public class JCEMatchResultsDto
{
    public bool NameMatch { get; set; }
    public bool DateOfBirthMatch { get; set; }
    public bool OverallMatch { get; set; }
}

/// <summary>
/// Resultado del procesamiento OCR
/// </summary>
public class OCRProcessingResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public decimal Confidence { get; set; }
    public ExtractedDocumentDataDto? ExtractedData { get; set; }
    public List<string>? CedulaValidationErrors { get; set; }
    public List<string>? QualityIssues { get; set; }
    public List<string>? Suggestions { get; set; }
    public ImageQualityDto? ImageQuality { get; set; }
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// Datos extraídos del documento
/// </summary>
public class ExtractedDocumentDataDto
{
    public string? CedulaNumber { get; set; }
    public string? FullName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Nationality { get; set; }
    public string? Address { get; set; }
    public string? BloodType { get; set; }
    public string? MaritalStatus { get; set; }
}

/// <summary>
/// Información de calidad de imagen
/// </summary>
public class ImageQualityDto
{
    public bool IsAcceptable { get; set; }
    public decimal Brightness { get; set; }
    public decimal Contrast { get; set; }
    public decimal Sharpness { get; set; }
    public string Resolution { get; set; } = string.Empty;
}

/// <summary>
/// Resultado de comparación facial
/// </summary>
public class FaceComparisonResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public bool FaceDetectedInDocument { get; set; }
    public bool FaceDetectedInSelfie { get; set; }
    public bool IsMatch { get; set; }
    public decimal MatchScore { get; set; }
    public decimal Confidence { get; set; }
    public LivenessResultDto? Liveness { get; set; }
    public FaceDetailsDto? DocumentFaceDetails { get; set; }
    public FaceDetailsDto? SelfieFaceDetails { get; set; }
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// Resultado de verificación de liveness
/// </summary>
public class LivenessResultDto
{
    public bool Passed { get; set; }
    public decimal Confidence { get; set; }
    public List<string>? ChallengesCompleted { get; set; }
}

/// <summary>
/// Detalles de rostro detectado
/// </summary>
public class FaceDetailsDto
{
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public bool? Glasses { get; set; }
}
