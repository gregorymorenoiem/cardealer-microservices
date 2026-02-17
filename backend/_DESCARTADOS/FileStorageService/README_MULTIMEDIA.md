# FileStorageService - Procesamiento Multimedia

## 🎥 Descripción

FileStorageService es un microservicio completo para almacenamiento y procesamiento de archivos multimedia que incluye capacidades avanzadas de procesamiento de video y audio mediante FFmpeg.

## ✨ Características

### Procesamiento de Video
- **Generación de Thumbnails**: Extrae fotogramas en timestamps específicos
- **Transcodificación**: Convierte videos entre formatos (mp4, webm, avi, mkv)
- **Generación de Variantes**: Crea múltiples resoluciones para adaptive streaming
- **Extracción de Audio**: Extrae pistas de audio de videos
- **Creación de Previews**: Genera vistas previas cortas
- **Marca de Agua**: Aplica watermarks con control de posición y opacidad
- **Validación**: Verifica la integridad de archivos de video
- **Metadata**: Extrae información completa (duración, resolución, codecs, bitrate, fps)

### Procesamiento de Audio
- **Conversión de Formato**: Soporta mp3, aac, opus, flac, wav
- **Normalización**: Ajusta volumen a niveles LUFS específicos
- **Recorte**: Extrae segmentos por tiempo
- **Efectos Fade**: Aplica fade in/out
- **Concatenación**: Une múltiples archivos de audio
- **Cambio de Velocidad**: Modifica la velocidad sin alterar el pitch
- **Generación de Waveform**: Crea imágenes de forma de onda
- **Validación**: Verifica la integridad de archivos de audio
- **Metadata**: Extrae información completa (duración, codec, bitrate, sample rate, canales)

### Procesamiento de Imágenes (Existente)
- Generación de thumbnails
- Optimización de imágenes
- Conversión de formatos
- Eliminación de metadatos EXIF
- Generación de variantes

### Almacenamiento
- Proveedores locales y en la nube
- Escaneo de virus
- Extracción de metadatos
- URLs pre-firmadas

## 📋 Requisitos Previos

### FFmpeg
El procesamiento de video y audio requiere **FFmpeg** y **FFprobe** instalados en el sistema:

#### Windows
```powershell
# Usando Chocolatey
choco install ffmpeg

# O descargar desde:
# https://www.gyan.dev/ffmpeg/builds/
```

#### Linux
```bash
# Ubuntu/Debian
sudo apt update
sudo apt install ffmpeg

# CentOS/RHEL
sudo yum install ffmpeg

# Arch Linux
sudo pacman -S ffmpeg
```

#### macOS
```bash
# Usando Homebrew
brew install ffmpeg
```

### Verificar Instalación
```bash
ffmpeg -version
ffprobe -version
```

## ⚙️ Configuración

### appsettings.json

```json
{
  "FFmpeg": {
    "FFmpegPath": "ffmpeg",
    "FFprobePath": "ffprobe",
    "WorkingDirectory": "./temp",
    "TimeoutSeconds": 300,
    "UseHardwareAcceleration": false,
    "HardwareAccelerationMethod": null,
    "Threads": 0
  },
  "Storage": {
    "ProviderType": "Local",
    "BasePath": "./uploads",
    "MaxFileSizeBytes": 104857600,
    "AllowedContentTypes": [
      "video/mp4",
      "video/webm",
      "video/quicktime",
      "audio/mpeg",
      "audio/wav",
      "audio/ogg",
      "image/jpeg",
      "image/png"
    ]
  }
}
```

### Opciones de FFmpeg

- **FFmpegPath**: Ruta al ejecutable ffmpeg (default: "ffmpeg")
- **FFprobePath**: Ruta al ejecutable ffprobe (default: "ffprobe")
- **WorkingDirectory**: Directorio para archivos temporales
- **TimeoutSeconds**: Timeout máximo de procesamiento (0 = sin límite)
- **UseHardwareAcceleration**: Habilitar aceleración por hardware
- **HardwareAccelerationMethod**: Método (cuda, qsv, vaapi, etc.)
- **Threads**: Número de threads (0 = automático)

## 🚀 Uso

### API Endpoints

#### Video

**POST /api/multimedia/video/thumbnails**
```bash
curl -X POST "http://localhost:5012/api/multimedia/video/thumbnails" \
  -F "video=@video.mp4" \
  -F "timestamps=0,5,10,15" \
  -F "width=320" \
  -F "height=180"
```

**POST /api/multimedia/video/transcode**
```bash
curl -X POST "http://localhost:5012/api/multimedia/video/transcode" \
  -F "video=@video.mp4" \
  -F "outputFormat=webm" \
  -F "videoCodec=vp9" \
  -F "preset=fast" \
  -F "crf=28"
```

**POST /api/multimedia/video/extract-audio**
```bash
curl -X POST "http://localhost:5012/api/multimedia/video/extract-audio" \
  -F "video=@video.mp4" \
  -F "format=mp3" \
  -F "bitrate=192"
```

**POST /api/multimedia/video/watermark**
```bash
curl -X POST "http://localhost:5012/api/multimedia/video/watermark" \
  -F "video=@video.mp4" \
  -F "watermark=@logo.png" \
  -F "position=bottomright" \
  -F "opacity=50"
```

**POST /api/multimedia/video/metadata**
```bash
curl -X POST "http://localhost:5012/api/multimedia/video/metadata" \
  -F "video=@video.mp4"
```

**POST /api/multimedia/video/validate**
```bash
curl -X POST "http://localhost:5012/api/multimedia/video/validate" \
  -F "video=@video.mp4"
```

#### Audio

**POST /api/multimedia/audio/convert**
```bash
curl -X POST "http://localhost:5012/api/multimedia/audio/convert" \
  -F "audio=@audio.mp3" \
  -F "outputFormat=aac" \
  -F "bitrate=128" \
  -F "sampleRate=44100"
```

**POST /api/multimedia/audio/normalize**
```bash
curl -X POST "http://localhost:5012/api/multimedia/audio/normalize" \
  -F "audio=@audio.mp3" \
  -F "targetLevel=-16.0"
```

**POST /api/multimedia/audio/trim**
```bash
curl -X POST "http://localhost:5012/api/multimedia/audio/trim" \
  -F "audio=@audio.mp3" \
  -F "startTime=10.0" \
  -F "duration=30.0"
```

**POST /api/multimedia/audio/fade**
```bash
curl -X POST "http://localhost:5012/api/multimedia/audio/fade" \
  -F "audio=@audio.mp3" \
  -F "fadeInDuration=2.0" \
  -F "fadeOutDuration=3.0"
```

**POST /api/multimedia/audio/speed**
```bash
curl -X POST "http://localhost:5012/api/multimedia/audio/speed" \
  -F "audio=@audio.mp3" \
  -F "speed=1.5"
```

**POST /api/multimedia/audio/waveform**
```bash
curl -X POST "http://localhost:5012/api/multimedia/audio/waveform" \
  -F "audio=@audio.mp3" \
  -F "width=1200" \
  -F "height=200" \
  -F "foregroundColor=0066CC" \
  -F "backgroundColor=FFFFFF"
```

**POST /api/multimedia/audio/metadata**
```bash
curl -X POST "http://localhost:5012/api/multimedia/audio/metadata" \
  -F "audio=@audio.mp3"
```

**POST /api/multimedia/audio/validate**
```bash
curl -X POST "http://localhost:5012/api/multimedia/audio/validate" \
  -F "audio=@audio.mp3"
```

#### Sistema

**GET /api/multimedia/ffmpeg/available**
```bash
curl "http://localhost:5012/api/multimedia/ffmpeg/available"
```

### Uso Programático

```csharp
// Inyectar servicios
public class MyService
{
    private readonly IVideoProcessingService _videoProcessing;
    private readonly IAudioProcessingService _audioProcessing;
    
    public MyService(
        IVideoProcessingService videoProcessing,
        IAudioProcessingService audioProcessing)
    {
        _videoProcessing = videoProcessing;
        _audioProcessing = audioProcessing;
    }
    
    public async Task ProcessVideoAsync(Stream videoStream)
    {
        // Extraer metadata
        var metadata = await _videoProcessing.ExtractVideoMetadataAsync(videoStream);
        
        // Generar thumbnails
        var timestamps = new[] { 0.0, metadata.DurationSeconds / 2, metadata.DurationSeconds };
        var thumbnails = await _videoProcessing.GenerateVideoThumbnailsAsync(
            videoStream, timestamps, width: 320, height: 180);
        
        // Transcodificar
        var transcodedStream = await _videoProcessing.TranscodeVideoAsync(
            videoStream,
            outputFormat: "webm",
            videoCodec: "vp9",
            audioCodec: "opus",
            preset: "fast",
            crf: 28);
        
        // Generar variantes para adaptive streaming
        var variants = new List<VideoVariantConfig>
        {
            new() { Name = "720p", Width = 1280, Height = 720, VideoBitrate = 2500, AudioBitrate = 128 },
            new() { Name = "480p", Width = 854, Height = 480, VideoBitrate = 1000, AudioBitrate = 96 },
            new() { Name = "360p", Width = 640, Height = 360, VideoBitrate = 600, AudioBitrate = 96 }
        };
        
        var variantStreams = await _videoProcessing.GenerateVideoVariantsAsync(
            videoStream, variants);
    }
    
    public async Task ProcessAudioAsync(Stream audioStream)
    {
        // Extraer metadata
        var metadata = await _audioProcessing.ExtractAudioMetadataAsync(audioStream);
        
        // Normalizar volumen
        var normalizedStream = await _audioProcessing.NormalizeAudioAsync(
            audioStream, targetLevel: -16.0);
        
        // Convertir formato
        var convertedStream = await _audioProcessing.ConvertAudioAsync(
            audioStream,
            outputFormat: "aac",
            bitrate: 128,
            sampleRate: 44100);
        
        // Generar waveform
        var waveformStream = await _audioProcessing.GenerateWaveformAsync(
            audioStream,
            width: 1200,
            height: 200,
            foregroundColor: "0066CC",
            backgroundColor: "FFFFFF");
    }
}
```

## 🏗️ Arquitectura

### Servicios Principales

```
FileStorageService.Core/
├── Interfaces/
│   ├── IVideoProcessingService.cs       # Procesamiento de video
│   ├── IAudioProcessingService.cs       # Procesamiento de audio
│   ├── IImageProcessingService.cs       # Procesamiento de imágenes
│   └── IStorageProvider.cs             # Almacenamiento
├── Services/
│   ├── VideoProcessingService.cs       # Implementación FFmpeg video
│   ├── AudioProcessingService.cs       # Implementación FFmpeg audio
│   ├── ImageProcessingService.cs       # Implementación ImageSharp
│   └── LocalStorageProvider.cs         # Almacenamiento local
└── Models/
    ├── MultimediaModels.cs             # Modelos multimedia
    ├── FileMetadata.cs                 # Metadatos
    └── VideoVariantConfig.cs           # Configuración variantes
```

### Flujo de Procesamiento

```
1. Recepción de archivo (API Controller)
2. Validación de formato y tamaño
3. Guardado temporal
4. Procesamiento (FFmpeg/ImageSharp)
5. Almacenamiento del resultado
6. Limpieza de archivos temporales
7. Retorno del resultado
```

## 🔧 Optimización

### Aceleración por Hardware

Para procesamiento más rápido, habilita aceleración por hardware:

#### NVIDIA (CUDA)
```json
{
  "FFmpeg": {
    "UseHardwareAcceleration": true,
    "HardwareAccelerationMethod": "cuda"
  }
}
```

#### Intel (Quick Sync)
```json
{
  "FFmpeg": {
    "UseHardwareAcceleration": true,
    "HardwareAccelerationMethod": "qsv"
  }
}
```

#### AMD (VAAPI)
```json
{
  "FFmpeg": {
    "UseHardwareAcceleration": true,
    "HardwareAccelerationMethod": "vaapi"
  }
}
```

### Presets de Codificación

- **ultrafast**: Máxima velocidad, mayor tamaño
- **superfast**: Muy rápido
- **veryfast**: Rápido
- **faster**: Rápido con buena compresión
- **fast**: Balance velocidad/calidad
- **medium**: Balance óptimo (default)
- **slow**: Mejor compresión
- **slower**: Excelente compresión
- **veryslow**: Máxima compresión

### CRF (Constant Rate Factor)

Controla la calidad del video (0-51):
- **0-17**: Calidad visualmente sin pérdidas
- **18-23**: Alta calidad (recomendado)
- **24-28**: Calidad media
- **29-51**: Baja calidad

## 📊 Formatos Soportados

### Video
- **Entrada**: mp4, webm, avi, mkv, mov, flv, wmv
- **Salida**: mp4, webm, avi, mkv
- **Codecs**: h264, h265, vp8, vp9, av1, mpeg4

### Audio
- **Entrada**: mp3, aac, wav, ogg, opus, flac, m4a
- **Salida**: mp3, aac, wav, ogg, opus, flac
- **Codecs**: mp3, aac, opus, vorbis, flac, pcm

### Imágenes
- **Entrada**: jpg, png, gif, webp, bmp, tiff
- **Salida**: jpg, png, webp, gif

## 🐳 Docker

### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5012

# Instalar FFmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FileStorageService.Api/FileStorageService.Api.csproj", "FileStorageService.Api/"]
COPY ["FileStorageService.Core/FileStorageService.Core.csproj", "FileStorageService.Core/"]
RUN dotnet restore "FileStorageService.Api/FileStorageService.Api.csproj"
COPY . .
WORKDIR "/src/FileStorageService.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileStorageService.Api.dll"]
```

### docker-compose.yml
```yaml
version: '3.8'
services:
  filestorage:
    build: .
    ports:
      - "5012:5012"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - FFmpeg__FFmpegPath=ffmpeg
      - FFmpeg__FFprobePath=ffprobe
      - FFmpeg__WorkingDirectory=/tmp
      - FFmpeg__TimeoutSeconds=300
    volumes:
      - ./uploads:/app/uploads
      - ./temp:/tmp
```

## 🧪 Testing

```bash
# Ejecutar tests
dotnet test

# Tests específicos
dotnet test --filter "Category=VideoProcessing"
dotnet test --filter "Category=AudioProcessing"
```

## 📈 Rendimiento

### Benchmarks Típicos (CPU i7)

| Operación | Archivo | Tiempo |
|-----------|---------|--------|
| Thumbnail | 1080p 1min | ~2s |
| Transcode 1080p→720p | 1min | ~30s |
| Audio normalize | 5min | ~5s |
| Waveform generation | 5min | ~3s |
| Metadata extraction | Any | <1s |

*Con aceleración por hardware los tiempos pueden reducirse 5-10x*

## 🔐 Seguridad

- Validación de tipos MIME
- Límites de tamaño de archivo
- Escaneo de virus (opcional)
- Limpieza automática de archivos temporales
- Timeouts de procesamiento
- Aislamiento de procesos FFmpeg

## 📝 Licencia

MIT License - Ver LICENSE file

## 🤝 Contribución

Las contribuciones son bienvenidas. Por favor:
1. Fork el proyecto
2. Crea una feature branch
3. Commit tus cambios
4. Push a la branch
5. Abre un Pull Request

## 📧 Soporte

Para preguntas o problemas, abre un issue en GitHub.

## 🎯 Roadmap

- [ ] Soporte para streaming en tiempo real
- [ ] Procesamiento por lotes
- [ ] Cola de trabajos con prioridades
- [ ] Dashboard de monitoreo
- [ ] Integración con CDN
- [ ] Detección de escenas en video
- [ ] Subtítulos automáticos
- [ ] Transcripción de audio
