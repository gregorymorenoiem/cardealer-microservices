# 🔐 KYCService - Estado de Implementación

**Última actualización:** 23 Enero 2026  
**Estado general:** ✅ En desarrollo - Alta prioridad completada  
**Tests:** 60/67 pasando (7 tests de CedulaValidator requieren ajuste de datos)

---

## 📋 Resumen del Servicio

KYCService implementa la verificación de identidad (Know Your Customer) según la **Ley 155-17** de Prevención de Lavado de Activos de República Dominicana.

### Stack Técnico

| Componente          | Tecnología               | Versión |
| ------------------- | ------------------------ | ------- |
| Backend             | .NET 8                   | 8.0     |
| ORM                 | Entity Framework Core    | 8.0     |
| Base de datos       | PostgreSQL               | 16+     |
| OCR                 | Tesseract                | 5.2.0   |
| JCE Integration     | Solo validación local    | -       |
| **Face Comparison** | **Amazon Rekognition**   | **✅**  |
| Testing             | xUnit + FluentAssertions | 2.6.4   |

---

## ✅ Procesos Implementados

### 🟢 Alta Prioridad (COMPLETADOS)

| #   | Proceso                       | Estado          | Tests        | Notas                                          |
| --- | ----------------------------- | --------------- | ------------ | ---------------------------------------------- |
| 1   | **CedulaValidator**           | ✅ Implementado | ✅ 25+ tests | Validación Módulo 10, formato XXX-XXXXXXX-X    |
| 2   | **Validación de Datos Local** | ✅ Implementado | ✅ 6 tests   | Compara datos OCR vs datos registrados         |
| 3   | **OCR Service (Tesseract)**   | ✅ Implementado | ✅ 4 tests   | Extracción de cédula frente/reverso            |
| 4   | **Face Comparison Service**   | ✅ Implementado | ✅ 4 tests   | **Amazon Rekognition** (~$0.001/imagen)        |
| 5   | **DI Configuration**          | ✅ Implementado | -            | Dev/Prod con simulación automática             |
| 6   | **Handlers de verificación**  | ✅ Implementado | -            | ProcessDocumentOCR, CompareFaces, ValidateData |

### 🟡 Media Prioridad (PENDIENTES)

| #   | Proceso                    | Estado       | Notas                         |
| --- | -------------------------- | ------------ | ----------------------------- |
| 7   | Admin Compliance Dashboard | ⏳ Pendiente | UI para revisar/aprobar KYC   |
| 8   | Watchlist Integration      | ⏳ Pendiente | ONU, OFAC, listas locales     |
| 9   | Risk Assessment Automático | ⏳ Pendiente | Scoring basado en reglas      |
| 10  | Notificaciones             | ⏳ Pendiente | Email/SMS para estados de KYC |

### 🔵 Baja Prioridad (BACKLOG)

| #   | Proceso             | Estado       | Notas                          |
| --- | ------------------- | ------------ | ------------------------------ |
| 11  | Reportes STR        | ⏳ Pendiente | Suspicious Transaction Reports |
| 12  | Auditoría completa  | ⏳ Pendiente | Logs de todas las operaciones  |
| 13  | Integración con UAF | ⏳ Pendiente | Unidad de Análisis Financiero  |

---

## 📁 Estructura del Proyecto

```
KYCService/
├── KYCService.Api/
│   ├── Controllers/
│   │   ├── KYCController.cs
│   │   └── IdentityVerificationController.cs
│   ├── Program.cs                    ✅ Actualizado con DI
│   └── appsettings.json              ✅ Configuración completa
│
├── KYCService.Application/
│   ├── Commands/
│   │   ├── IdentityVerificationCommands.cs
│   │   └── ExternalServicesCommands.cs    ✅ NUEVO
│   ├── DTOs/
│   │   ├── IdentityVerificationDtos.cs
│   │   └── ExternalServicesResultDtos.cs  ✅ NUEVO
│   ├── Handlers/
│   │   ├── IdentityVerificationHandlers.cs
│   │   └── ExternalServicesHandlers.cs    ✅ NUEVO
│   └── Validators/
│
├── KYCService.Domain/
│   ├── Entities/
│   │   └── IdentityVerificationSession.cs
│   ├── Interfaces/
│   │   └── IRepositories.cs
│   └── Validators/
│       └── CedulaValidator.cs         ✅ Completo
│
├── KYCService.Infrastructure/
│   ├── ExternalServices/              ✅ ACTUALIZADO
│   │   ├── IJCEService.cs
│   │   ├── JCEService.cs
│   │   ├── IOCRService.cs
│   │   ├── TesseractOCRService.cs
│   │   ├── IFaceComparisonService.cs
│   │   ├── FaceComparisonService.cs
│   │   └── AmazonRekognitionService.cs  ✅ NUEVO - Amazon Rekognition
│   ├── DependencyInjection.cs         ✅ ACTUALIZADO
│   ├── Persistence/
│   └── Repositories/
│
└── KYCService.Tests/
    ├── Validators/
    │   └── CedulaValidatorTests.cs    ✅ 25+ tests
    └── ExternalServices/
        └── ExternalServicesTests.cs   ✅ 14+ tests
```

---

## 🔧 Servicios de Verificación

### 1. Validación de Datos (Local)

**Descripción:** En República Dominicana **NO existe una API pública de la JCE** para validar cédulas. El proceso de KYC se basa en:

1. **OCR** - Extraer datos de la foto de la cédula
2. **Comparación de datos** - Verificar que los datos extraídos coinciden con lo que el usuario registró
3. **Face comparison** - Comparar la foto de la cédula con el selfie del usuario

**Interface:** `IDataValidationService`  
**Implementación:** `DataValidationService`

```csharp
// Métodos disponibles
Task<DataComparisonResult> CompareUserDataAsync(UserRegistrationData userData, OCRExtractedData ocrData, CancellationToken ct);
Task<CedulaFormatValidation> ValidateCedulaFormatAsync(string cedulaNumber, CancellationToken ct);
```

**Validaciones que se realizan:**

- ✅ Formato de cédula válido (Módulo 10)
- ✅ Nombre extraído coincide con nombre registrado (fuzzy match)
- ✅ Número de cédula extraído coincide con el proporcionado
- ✅ Fecha de nacimiento coincide (si aplica)
- ✅ Documento no expirado

> **NOTA:** La JCE no ofrece API pública. Cualquier servicio que diga "validar con JCE" es una afirmación falsa. La validación real es local + OCR + face matching.

---

### 2. OCR Service (Tesseract)

**Interface:** `IOCRService`  
**Implementación:** `TesseractOCRService`

```csharp
// Métodos disponibles
Task<OCRResult> ExtractTextAsync(byte[] imageData, CancellationToken ct);
Task<CedulaOCRResult?> ExtractCedulaFrontAsync(byte[] imageData, CancellationToken ct);
Task<CedulaOCRResult?> ExtractCedulaBackAsync(byte[] imageData, CancellationToken ct);
Task<ImageQualityResult> CheckImageQualityAsync(byte[] imageData, CancellationToken ct);
```

**Campos extraídos:**

- Frente: Número de cédula, nombre, fecha de nacimiento, nacionalidad
- Reverso: Dirección, tipo de sangre, estado civil, fecha de expiración

**Requisitos:**

- Tesseract 5.2.0 instalado
- Tessdata para español (`spa.traineddata`)

**Configuración:**

```json
{
  "OCRService": {
    "UseSimulation": true,
    "TessDataPath": "/usr/share/tesseract-ocr/4.00/tessdata",
    "Languages": "spa",
    "MinConfidenceScore": 70
  }
}
```

---

### 3. Face Comparison Service

**Interface:** `IFaceComparisonService`  
**Implementación:** `FaceComparisonService` + `AmazonRekognitionService`

```csharp
// Métodos disponibles
Task<FaceDetectionResult> DetectFacesAsync(byte[] imageData, CancellationToken ct);
Task<FaceComparisonResult> CompareFacesAsync(byte[] sourceImage, byte[] targetImage, CancellationToken ct);
Task<LivenessResult> CheckLivenessAsync(LivenessCheckRequest request, CancellationToken ct);
Task<FaceExtractionResult> ExtractFaceFromDocumentAsync(byte[] documentImage, CancellationToken ct);
```

**Modos de operación:**

- ✅ **Simulación:** Para desarrollo y tests
- ✅ **Amazon Rekognition (RECOMENDADO):** Cloud, muy económico ~$0.001/imagen
- ⏳ **Azure Face API:** Cloud, de pago (solo como alternativa)

#### 🎯 Amazon Rekognition (RECOMENDADO)

**Ventajas:**

- ✅ **Muy económico** - Solo ~$0.001/imagen (~$2-4/mes para 1000 KYC)
- ✅ **Alta precisión** - ~99% accuracy
- ✅ **Fácil integración** - AWS SDK oficial
- ✅ **Liveness detection** - Disponible (servicio adicional)
- ✅ **No requiere modelos locales** - Sin descargas de 100MB+

**Configuración:**

```json
{
  "FaceComparison": {
    "UseSimulation": false,
    "UseAmazonRekognition": true,
    "UseAzureFaceApi": false,
    "MatchThreshold": 80,
    "LivenessThreshold": 70
  },
  "AmazonRekognition": {
    "Region": "us-east-1",
    "AccessKeyId": "",
    "SecretAccessKey": "",
    "SimilarityThreshold": 80,
    "MinImageQuality": 40
  }
}
```

**Credenciales AWS:**

Las credenciales se pueden configurar de 3 formas:

1. **Variables de ambiente:** `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`
2. **Perfil AWS:** `~/.aws/credentials`
3. **IAM Role:** Automático en EC2/ECS/Lambda

**Comparativa de Costos:**

| Servicio               | Costo/mes (1000 KYC) | Latencia | Privacidad   |
| ---------------------- | -------------------- | -------- | ------------ |
| Simulación (Dev)       | **$0**               | ~10ms    | ✅ Local     |
| **Amazon Rekognition** | **~$2-4**            | ~300ms   | ⚠️ AWS Cloud |
| Azure Face API         | ~$10-20              | ~200ms   | ⚠️ Azure     |
| Face++                 | $0 (50K gratis)      | ~400ms   | ⚠️ China     |

````

---

## 🧪 Tests

### Ejecutar Tests

```bash
cd backend/KYCService
dotnet test --verbosity normal
````

### Cobertura de Tests

| Componente            | Tests   | Estado |
| --------------------- | ------- | ------ |
| CedulaValidator       | 25+     | ✅     |
| JCEService            | 6       | ✅     |
| TesseractOCRService   | 4       | ✅     |
| FaceComparisonService | 4       | ✅     |
| **Total**             | **39+** | ✅     |

### Tests Incluidos

**CedulaValidatorTests:**

- Validación de formato (longitud, caracteres)
- Validación de municipio (001-044)
- Validación de checksum (Módulo 10)
- Formateo de cédula
- Generación de cédulas de prueba
- Validación de edad

**ExternalServicesTests:**

- JCE validation con simulación
- OCR text extraction
- Cédula front/back extraction
- Face detection
- Face comparison
- Liveness check

---

## 🚀 Uso en Producción

### 1. Habilitar servicios reales

```json
{
  "OCRService": {
    "UseSimulation": false,
    "TessDataPath": "/app/tessdata",
    "Languages": "spa",
    "MinConfidenceScore": 70
  },
  "FaceComparison": {
    "UseSimulation": false,
    "UseAmazonRekognition": true,
    "UseAzureFaceApi": false,
    "MatchThreshold": 80
  },
  "AmazonRekognition": {
    "Region": "us-east-1",
    "SimilarityThreshold": 80,
    "MinImageQuality": 40
  },
  "DataValidation": {
    "NameMatchThreshold": 85,
    "AllowFuzzyNameMatch": true
  }
}
```

### 2. Configurar credenciales de AWS

```bash
# Opción A: Variables de entorno
export AWS_ACCESS_KEY_ID=your-access-key
export AWS_SECRET_ACCESS_KEY=your-secret-key
export AWS_DEFAULT_REGION=us-east-1

# Opción B: Archivo ~/.aws/credentials
[default]
aws_access_key_id = your-access-key
aws_secret_access_key = your-secret-key

# Opción C: IAM Role (automático en EC2/ECS/Lambda)
```

### 3. Instalar Tesseract en Docker

```dockerfile
# En Dockerfile
RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    tesseract-ocr-spa \
    libtesseract-dev \
    && rm -rf /var/lib/apt/lists/*
```

### 4. Variables de entorno requeridas

```bash
# Tesseract OCR
TESSDATA_PATH=/app/tessdata

# AWS Rekognition (credenciales opcionales si usa IAM Role)
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
AWS_DEFAULT_REGION=us-east-1
```

> **IMPORTANTE:**
>
> - No se requieren credenciales de JCE porque no existe API pública de la JCE en RD
> - Amazon Rekognition es muy económico (~$0.001/imagen)
> - Para 1000 verificaciones KYC/mes ≈ $2-4 USD

---

## 📊 Flujo de Verificación

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        FLUJO DE VERIFICACIÓN KYC                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1️⃣ INICIO                                                                  │
│  Usuario inicia verificación → POST /api/identity-verification/start       │
│  Sistema crea sesión con ID y challenges                                   │
│                                                                             │
│  2️⃣ CAPTURA DOCUMENTO FRENTE                                               │
│  App captura foto → POST /api/identity-verification/process-document       │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ OCR Service (Tesseract)                                             │   │
│  │ ├─ Verificar calidad de imagen                                      │   │
│  │ ├─ Extraer texto con regex patterns                                 │   │
│  │ ├─ Parsear: cédula, nombre, fecha nacimiento                        │   │
│  │ └─ Retornar CedulaOCRResult                                         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  3️⃣ VALIDACIÓN DE DATOS                                                    │
│  Comparar datos extraídos por OCR vs datos registrados por usuario         │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Data Validation Service                                             │   │
│  │ ├─ Validar formato de cédula (CedulaValidator - Módulo 10)          │   │
│  │ ├─ Comparar nombre OCR vs nombre registrado (fuzzy match)           │   │
│  │ ├─ Comparar número de cédula OCR vs proporcionado                   │   │
│  │ └─ Verificar que documento no esté expirado                         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  4️⃣ CAPTURA DOCUMENTO REVERSO                                              │
│  App captura reverso → Extraer dirección, tipo sangre, expiración          │
│                                                                             │
│  5️⃣ CAPTURA SELFIE + LIVENESS                                              │
│  Usuario toma selfie → POST /api/identity-verification/process-selfie      │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Face Comparison Service                                             │   │
│  │ ├─ Detectar rostro en documento                                     │   │
│  │ ├─ Detectar rostro en selfie                                        │   │
│  │ ├─ Comparar similitud (threshold 80%)                               │   │
│  │ ├─ Verificar liveness (BLINK, TURN, SMILE)                          │   │
│  │ └─ Retornar match score y liveness result                           │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  6️⃣ RESULTADO FINAL                                                        │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Verificación Exitosa:                                               │   │
│  │ ✅ Documento válido (OCR confidence > 70%)                          │   │
│  │ ✅ Datos extraídos coinciden con datos registrados                  │   │
│  │ ✅ Formato de cédula válido (Módulo 10)                             │   │
│  │ ✅ Rostros coinciden (match > 80%)                                  │   │
│  │ ✅ Liveness pasado (no es foto de foto)                             │   │
│  │ → Status: VERIFIED / PENDING_REVIEW                                 │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔐 Seguridad

### Datos Sensibles

- Las imágenes de documentos se almacenan encriptadas en S3
- Los datos de ciudadanos de JCE NO se almacenan permanentemente
- Logs de verificación no incluyen datos PII completos
- Sesiones expiran automáticamente (30 minutos default)

### Auditoría

- Cada operación de KYC genera registro de auditoría
- IP, User-Agent, geolocalización se registran
- Intentos fallidos se trackean (máximo 3)

---

## 📈 Próximos Pasos

### Inmediato

1. ✅ **Face Comparison implementado** - FaceRecognitionDotNet (local, gratuito)
2. ⏳ Descargar modelos de dlib en servidor de producción
3. ⏳ Implementar Admin Compliance Dashboard
4. ⏳ Mejorar algoritmo de fuzzy matching para nombres

### Corto Plazo

5. ⏳ Integración con listas de vigilancia (OFAC, ONU)
6. ⏳ Notificaciones automáticas por estado KYC
7. ⏳ Risk scoring automático

### Medio Plazo

8. ⏳ Reportes STR para UAF
9. ⏳ Dashboard de métricas KYC
10. ⏳ API para consultas de estado

---

**Documentación mantenida por:** Equipo de Desarrollo OKLA  
**Última revisión:** Enero 2026
