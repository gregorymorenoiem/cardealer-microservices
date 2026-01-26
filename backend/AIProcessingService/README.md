# AIProcessingService - Vehicle Image AI Processing

## 🎯 Overview

AIProcessingService is an in-house AI-powered image processing service that replaces external services like Spyne. It provides:

- **Vehicle Segmentation** - Using Meta's SAM2 (Segment Anything Model 2)
- **Background Removal/Replacement** - Professional studio backgrounds
- **Image Classification** - Exterior/Interior/Engine/etc. using OpenAI CLIP
- **Angle Detection** - Front/Side/Rear view detection
- **License Plate Blurring** - Using YOLOv8 object detection
- **360° Spin Generation** - Create interactive vehicle tours

## 📊 Cost Comparison

| Feature                | Spyne Cost  | Our Cost      | Savings |
| ---------------------- | ----------- | ------------- | ------- |
| Background Replacement | $0.15/image | ~$0.01/image  | **93%** |
| Classification         | $0.08/image | ~$0.005/image | **94%** |
| Plate Blurring         | $0.10/image | ~$0.008/image | **92%** |
| 360° Spin (36 frames)  | $5.40       | ~$0.36        | **93%** |

**Estimated Annual Savings:** ~$50,000+ at scale

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    AIProcessingService API                       │
│                     (.NET 8 + MediatR)                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │ /process    │  │ /spin360    │  │ /backgrounds│             │
│  └──────┬──────┘  └──────┬──────┘  └─────────────┘             │
└─────────┼────────────────┼─────────────────────────────────────┘
          │                │
          ▼                ▼
     ┌────────────────────────────────────┐
     │           RabbitMQ Queues          │
     │  ┌──────────┐ ┌──────────┐ ┌─────┐ │
     │  │ segment  │ │ classify │ │ 360 │ │
     │  └────┬─────┘ └────┬─────┘ └──┬──┘ │
     └───────┼────────────┼──────────┼────┘
             │            │          │
     ┌───────▼────┐ ┌─────▼────┐ ┌───▼────────┐
     │ SAM2 Worker│ │CLIP Worker│ │YOLO Worker │
     │  (Python)  │ │  (Python) │ │  (Python)  │
     │ ┌────────┐ │ │ ┌───────┐ │ │ ┌────────┐ │
     │ │  GPU   │ │ │ │  GPU  │ │ │ │  GPU   │ │
     │ └────────┘ │ │ └───────┘ │ │ └────────┘ │
     └────────────┘ └───────────┘ └────────────┘
```

## 🚀 Quick Start

### Prerequisites

- Docker with NVIDIA GPU support
- nvidia-container-toolkit
- Minimum 8GB GPU VRAM (recommended: 16GB+)

### 1. Start Services

```bash
cd backend/AIProcessingService

# Start all services with GPU workers
docker-compose -f docker-compose.ai-workers.yaml up -d
```

### 2. Check Health

```bash
# API Health
curl http://localhost:5070/health

# RabbitMQ Management
open http://localhost:15673  # guest/guest
```

### 3. Test Processing

```bash
# Process an image
curl -X POST http://localhost:5070/api/aiprocessing/process \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "vehicleId": "123e4567-e89b-12d3-a456-426614174000",
    "imageUrl": "https://example.com/car.jpg",
    "processingType": "BackgroundReplacement",
    "options": {
      "backgroundCode": "white_studio",
      "quality": 90
    }
  }'
```

## 📡 API Endpoints

### Image Processing

| Method | Endpoint                             | Description             |
| ------ | ------------------------------------ | ----------------------- |
| POST   | `/api/aiprocessing/process`          | Process single image    |
| POST   | `/api/aiprocessing/process/batch`    | Process multiple images |
| POST   | `/api/aiprocessing/spin360/generate` | Generate 360° spin      |
| GET    | `/api/aiprocessing/jobs/{id}`        | Get job status          |
| POST   | `/api/aiprocessing/jobs/{id}/cancel` | Cancel job              |
| POST   | `/api/aiprocessing/jobs/{id}/retry`  | Retry failed job        |
| POST   | `/api/aiprocessing/analyze`          | Analyze image (CLIP)    |

### Backgrounds

| Method | Endpoint                  | Description               |
| ------ | ------------------------- | ------------------------- |
| GET    | `/api/backgrounds`        | Get available backgrounds |
| GET    | `/api/backgrounds/{code}` | Get specific background   |

## 🎨 Available Backgrounds

| Code            | Name            | Access  |
| --------------- | --------------- | ------- |
| `white_studio`  | Blanco Infinito | Free    |
| `gray_showroom` | Showroom Gris   | Dealers |
| `dark_studio`   | Estudio Oscuro  | Dealers |
| `outdoor_day`   | Exterior Día    | Dealers |

## 🔧 Configuration

### Environment Variables

```env
# Database
ConnectionStrings__DefaultConnection=Host=postgres;Database=aiprocessingservice;...

# RabbitMQ
RabbitMQ__Host=rabbitmq
RabbitMQ__Username=guest
RabbitMQ__Password=guest

# S3 Storage
S3__BucketName=okla-processed-images
S3__AccessKey=xxx
S3__SecretKey=xxx
S3__Region=us-east-1

# JWT
Jwt__Secret=your-secret-key
Jwt__Issuer=OKLA
Jwt__Audience=OKLA-Users
```

### Worker Configuration

Each Python worker supports:

```env
RABBITMQ_HOST=rabbitmq
DEVICE=cuda  # or cpu for testing
MODEL_PATH=/models/model.pt
API_CALLBACK_URL=http://aiprocessingservice:8080
```

## 🐍 Python Workers

### SAM2 Worker (sam2_worker.py)

- **Model:** SAM2 Hiera Large (~2.5GB)
- **GPU Memory:** ~6GB
- **Queue:** `ai-processing-segmentation`
- **Features:**
  - Vehicle segmentation
  - Background removal (transparent PNG)
  - Background replacement with shadows

### CLIP Worker (clip_worker.py)

- **Model:** ViT-L/14@336px (~1.7GB)
- **GPU Memory:** ~4GB
- **Queue:** `ai-processing-classification`
- **Features:**
  - Image type classification
  - Angle detection
  - Quality assessment
  - Damage detection

### YOLO Worker (yolo_worker.py)

- **Model:** YOLOv8x (~130MB)
- **GPU Memory:** ~2GB
- **Queue:** `ai-processing-detection`
- **Features:**
  - License plate detection
  - Automatic plate blurring
  - Object detection

## 📈 Performance

### Processing Times (RTX 3080)

| Operation              | Time   |
| ---------------------- | ------ |
| Segmentation (SAM2)    | ~800ms |
| Classification (CLIP)  | ~150ms |
| Plate Detection (YOLO) | ~200ms |
| Full Processing        | ~1.5s  |
| 360° Spin (36 frames)  | ~30s   |

### Throughput

- Single GPU: ~2,400 images/hour
- Multi-GPU: Scales linearly

## 🔐 Security

- JWT authentication required for most endpoints
- Role-based access for premium backgrounds
- S3 signed URLs for processed images
- Rate limiting in Gateway

## 📁 Project Structure

```
AIProcessingService/
├── AIProcessingService.Api/         # REST API
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
├── AIProcessingService.Application/ # CQRS/MediatR
│   ├── DTOs/
│   ├── Features/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── Handlers/
│   └── Validators/
├── AIProcessingService.Domain/      # Entities & Interfaces
│   ├── Entities/
│   └── Interfaces/
├── AIProcessingService.Infrastructure/ # Persistence
│   └── Persistence/
│       ├── AIProcessingDbContext.cs
│       └── Repositories/
├── workers/                         # Python AI Workers
│   ├── sam2_worker.py
│   ├── clip_worker.py
│   ├── yolo_worker.py
│   ├── requirements.txt
│   ├── Dockerfile.sam2
│   ├── Dockerfile.clip
│   └── Dockerfile.yolo
├── Dockerfile                       # .NET API image
├── docker-compose.ai-workers.yaml   # Full stack with GPU
└── README.md
```

## 🧪 Testing

```bash
# Run .NET tests
cd AIProcessingService
dotnet test

# Test worker locally (requires GPU)
cd workers
python sam2_worker.py
```

## 📝 License

- **SAM2:** Apache 2.0 (Meta)
- **CLIP:** MIT (OpenAI)
- **YOLOv8:** AGPL-3.0 (Ultralytics)

---

**Maintained by:** OKLA Development Team  
**Last Updated:** January 2026
