namespace KYCService.Infrastructure.ExternalServices;

/// <summary>
/// Interface para el servicio de comparación facial
/// </summary>
public interface IFaceComparisonService
{
    /// <summary>
    /// Compara dos rostros y retorna el nivel de similitud
    /// </summary>
    Task<FaceComparisonResult> CompareFacesAsync(byte[] image1, byte[] image2, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compara un rostro con la foto de un documento
    /// </summary>
    Task<FaceComparisonResult> CompareWithDocumentAsync(byte[] selfieImage, byte[] documentImage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Detecta rostros en una imagen
    /// </summary>
    Task<FaceDetectionResult> DetectFacesAsync(byte[] imageData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extrae la foto del documento
    /// </summary>
    Task<FaceExtractionResult> ExtractFaceFromDocumentAsync(byte[] documentImage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica liveness (que es una persona real, no una foto)
    /// </summary>
    Task<LivenessResult> CheckLivenessAsync(LivenessCheckRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado de la comparación facial
/// </summary>
public class FaceComparisonResult
{
    /// <summary>
    /// Indica si la comparación fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Indica si los rostros coinciden (score >= threshold)
    /// </summary>
    public bool IsMatch { get; set; }

    /// <summary>
    /// Puntuación de similitud (0-100)
    /// </summary>
    public decimal SimilarityScore { get; set; }

    /// <summary>
    /// Umbral de similitud usado para la comparación
    /// </summary>
    public decimal Threshold { get; set; }

    /// <summary>
    /// Nivel de confianza en el resultado
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Detalles adicionales de la comparación
    /// </summary>
    public FaceComparisonDetails? Details { get; set; }

    /// <summary>
    /// Tiempo de procesamiento en ms
    /// </summary>
    public long ProcessingTimeMs { get; set; }
}

/// <summary>
/// Detalles de la comparación facial
/// </summary>
public class FaceComparisonDetails
{
    /// <summary>
    /// Rostro 1 detectado correctamente
    /// </summary>
    public bool Face1Detected { get; set; }

    /// <summary>
    /// Rostro 2 detectado correctamente
    /// </summary>
    public bool Face2Detected { get; set; }

    /// <summary>
    /// Calidad del rostro 1 (0-100)
    /// </summary>
    public decimal Face1Quality { get; set; }

    /// <summary>
    /// Calidad del rostro 2 (0-100)
    /// </summary>
    public decimal Face2Quality { get; set; }

    /// <summary>
    /// Distancia euclidiana entre embeddings
    /// </summary>
    public decimal EuclideanDistance { get; set; }

    /// <summary>
    /// Similitud coseno entre embeddings
    /// </summary>
    public decimal CosineSimilarity { get; set; }

    /// <summary>
    /// Información de pose del rostro 1
    /// </summary>
    public FacePoseInfo? Face1Pose { get; set; }

    /// <summary>
    /// Información de pose del rostro 2
    /// </summary>
    public FacePoseInfo? Face2Pose { get; set; }
}

/// <summary>
/// Información de pose facial
/// </summary>
public class FacePoseInfo
{
    public decimal Yaw { get; set; }    // Rotación izquierda/derecha
    public decimal Pitch { get; set; }  // Rotación arriba/abajo
    public decimal Roll { get; set; }   // Inclinación
}

/// <summary>
/// Resultado de la detección de rostros
/// </summary>
public class FaceDetectionResult
{
    /// <summary>
    /// Indica si la detección fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Número de rostros detectados
    /// </summary>
    public int FaceCount { get; set; }

    /// <summary>
    /// Lista de rostros detectados
    /// </summary>
    public List<DetectedFace> Faces { get; set; } = new();

    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Tiempo de procesamiento en ms
    /// </summary>
    public long ProcessingTimeMs { get; set; }
}

/// <summary>
/// Información de un rostro detectado
/// </summary>
public class DetectedFace
{
    /// <summary>
    /// Índice del rostro
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// ID único del rostro (generado por el servicio)
    /// </summary>
    public string FaceId { get; set; } = string.Empty;

    /// <summary>
    /// Bounding box del rostro
    /// </summary>
    public FaceBoundingBox BoundingBox { get; set; } = new();

    /// <summary>
    /// Confianza de la detección (0-100)
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// Puntos de referencia faciales
    /// </summary>
    public FaceLandmarks? Landmarks { get; set; }

    /// <summary>
    /// Calidad del rostro (0-100) - valor simple
    /// </summary>
    public decimal Quality { get; set; }

    /// <summary>
    /// Información de pose
    /// </summary>
    public FacePoseInfo? Pose { get; set; }

    /// <summary>
    /// Edad estimada
    /// </summary>
    public int? EstimatedAge { get; set; }

    /// <summary>
    /// Alias para EstimatedAge (compatibilidad con Amazon Rekognition)
    /// </summary>
    public int? Age { get => EstimatedAge; set => EstimatedAge = value; }

    /// <summary>
    /// Género detectado
    /// </summary>
    public string? DetectedGender { get; set; }

    /// <summary>
    /// Alias para DetectedGender (compatibilidad con Amazon Rekognition)
    /// </summary>
    public string? Gender { get => DetectedGender; set => DetectedGender = value; }

    /// <summary>
    /// Expresión facial detectada
    /// </summary>
    public string? Expression { get; set; }

    /// <summary>
    /// Alias para Expression (compatibilidad con Amazon Rekognition)
    /// </summary>
    public string? Emotion { get => Expression; set => Expression = value; }

    /// <summary>
    /// Si los ojos están abiertos
    /// </summary>
    public bool EyesOpen { get; set; }

    /// <summary>
    /// Si la boca está abierta
    /// </summary>
    public bool MouthOpen { get; set; }

    /// <summary>
    /// Si está sonriendo
    /// </summary>
    public bool Smile { get; set; }

    /// <summary>
    /// Si usa gafas de sol
    /// </summary>
    public bool Sunglasses { get; set; }
}

/// <summary>
/// Bounding box de un rostro
/// </summary>
public class FaceBoundingBox
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// Puntos de referencia faciales
/// </summary>
public class FaceLandmarks
{
    public Point? LeftEye { get; set; }
    public Point? RightEye { get; set; }
    public Point? Nose { get; set; }
    public Point? MouthLeft { get; set; }
    public Point? MouthRight { get; set; }
}

/// <summary>
/// Punto en 2D
/// </summary>
public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

/// <summary>
/// Resultado de extracción de rostro de documento
/// </summary>
public class FaceExtractionResult
{
    /// <summary>
    /// Indica si la extracción fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Imagen del rostro extraída (bytes)
    /// </summary>
    public byte[]? FaceImage { get; set; }

    /// <summary>
    /// Bounding box del rostro en el documento original
    /// </summary>
    public FaceBoundingBox? BoundingBox { get; set; }

    /// <summary>
    /// Calidad del rostro extraído (0-100)
    /// </summary>
    public decimal Quality { get; set; }

    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request para verificación de liveness
/// </summary>
public class LivenessCheckRequest
{
    /// <summary>
    /// Imagen principal (selfie)
    /// </summary>
    public byte[] SelfieImage { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Frames de video capturados durante challenges
    /// </summary>
    public List<byte[]> VideoFrames { get; set; } = new();

    /// <summary>
    /// Resultados de los challenges realizados
    /// </summary>
    public List<LivenessChallengeResult> ChallengeResults { get; set; } = new();

    /// <summary>
    /// Datos del dispositivo (acelerómetro, giroscopio)
    /// </summary>
    public DeviceMotionData? MotionData { get; set; }
}

/// <summary>
/// Resultado de un challenge de liveness
/// </summary>
public class LivenessChallengeResult
{
    public string ChallengeType { get; set; } = string.Empty; // "blink", "smile", "turn_left", etc.
    public bool Passed { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Confidence { get; set; }
}

/// <summary>
/// Datos de movimiento del dispositivo
/// </summary>
public class DeviceMotionData
{
    public List<MotionSample> AccelerometerSamples { get; set; } = new();
    public List<MotionSample> GyroscopeSamples { get; set; } = new();
}

/// <summary>
/// Muestra de datos de movimiento
/// </summary>
public class MotionSample
{
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal Z { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Resultado de verificación de liveness
/// </summary>
public class LivenessResult
{
    /// <summary>
    /// Indica si la verificación fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Indica si se detectó una persona real (no foto de foto)
    /// </summary>
    public bool IsLive { get; set; }

    /// <summary>
    /// Puntuación de liveness (0-100)
    /// </summary>
    public decimal LivenessScore { get; set; }

    /// <summary>
    /// Umbral de liveness usado
    /// </summary>
    public decimal Threshold { get; set; }

    /// <summary>
    /// Detalle de cada challenge
    /// </summary>
    public List<LivenessChallengeDetail> ChallengeDetails { get; set; } = new();

    /// <summary>
    /// Razón del fallo (si falló)
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Mensaje de error si hubo error técnico
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Tiempo de procesamiento en ms
    /// </summary>
    public long ProcessingTimeMs { get; set; }
}

/// <summary>
/// Detalle de un challenge de liveness
/// </summary>
public class LivenessChallengeDetail
{
    public string ChallengeType { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public decimal Score { get; set; }
    public string? Notes { get; set; }
}
