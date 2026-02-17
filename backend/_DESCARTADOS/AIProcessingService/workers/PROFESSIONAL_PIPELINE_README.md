# 🚗 Professional Vehicle Segmentation Pipeline

## Overview

A 7-stage AI pipeline for automotive dealership-quality vehicle cutouts. This pipeline combines multiple state-of-the-art models to achieve professional-grade background removal with precise edge handling, shadow detection, and wheel protection.

## Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  INPUT IMAGE                                                                 │
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 1: DETECTION (YOLOv8x-seg + GroundingDINO)                       ││
│  │  • YOLOv8x-seg: Fast instance segmentation                              ││
│  │  • GroundingDINO: Text-guided zero-shot detection                       ││
│  │  • Output: Combined bbox + initial mask + confidence                    ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 2: SHADOW DETECTION (ShadowFormer)                               ││
│  │  • Detect ground shadows under vehicle                                  ││
│  │  • Generate shadow mask for exclusion                                   ││
│  │  • Preserve wheel contact points                                        ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 3: PRIMARY SEGMENTATION (SAM2-H)                                 ││
│  │  • Use detection from Stage 1 as prompts                               ││
│  │  • Exclude shadow regions from Stage 2                                  ││
│  │  • 12 strategic positive + 8 negative points                           ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 4: PARTS ANALYSIS (CarParts-Seg)                                 ││
│  │  • Segment 8+ car parts: hood, bumper, fender, door, lights, etc.      ││
│  │  • Identify wheels for protection                                       ││
│  │  • Validate SAM2 mask contains all expected parts                      ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 5: SEMANTIC CORRECTION (SegFormer)                               ││
│  │  • Semantic segmentation for "car" class verification                   ││
│  │  • IoU-based correction strategy                                        ││
│  │  • Mask intersection for precision                                      ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 6: EDGE REFINEMENT (ISNet/DIS)                                   ││
│  │  • High-precision dichotomous segmentation                              ││
│  │  • Sub-pixel edge accuracy                                              ││
│  │  • Handle transparent/reflective surfaces                               ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 7: ENHANCEMENT (RealESRGAN + CodeFormer)                         ││
│  │  • Optional upscaling with RealESRGAN                                   ││
│  │  • Detail enhancement                                                   ││
│  │  • Professional alpha matting                                           ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  OUTPUT: Professional cutout with transparent/white background              │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Models Used

| Stage | Model         | Purpose                           | Size   |
| ----- | ------------- | --------------------------------- | ------ |
| 1     | YOLOv8x-seg   | Instance segmentation + detection | 137 MB |
| 1     | GroundingDINO | Zero-shot text-guided detection   | 900 MB |
| 2     | ShadowFormer  | Shadow detection (ONNX)           | 180 MB |
| 3     | SAM2-Large    | Primary segmentation              | 897 MB |
| 4     | CarParts-Seg  | Part-aware segmentation           | 100 MB |
| 5     | SegFormer-B5  | Semantic verification             | 380 MB |
| 6     | ISNet-DIS     | Edge refinement                   | 175 MB |
| 7     | RealESRGAN    | Upscaling (optional)              | 64 MB  |

**Total model size:** ~2.8 GB

## Installation

### 1. Install Dependencies

```bash
pip install -r requirements.txt
```

### 2. Download Models

```bash
# Download all models (~2.8 GB)
python download_pipeline_models.py --all

# Or download essential models only (~600 MB)
python download_pipeline_models.py --essential

# Check model availability
python download_pipeline_models.py --check
```

### 3. Verify Installation

```bash
python -c "from professional_pipeline import ProfessionalVehiclePipeline; print('OK')"
```

## Usage

### Command Line

```bash
# Process all images in input directory
python process_professional_batch.py

# Custom directories
python process_professional_batch.py --input ./my-images --output ./results

# Save intermediate masks from each stage
python process_professional_batch.py --save-intermediates

# Use simplified pipeline (faster, less quality)
python process_professional_batch.py --simplified

# Process single image
python process_professional_batch.py --single ./car.jpg
```

### Python API

```python
from professional_pipeline import ProfessionalVehiclePipeline, PipelineConfig
from PIL import Image
import numpy as np

# Configure
config = PipelineConfig(
    device="cuda",
    upscale_factor=1.0,  # No upscaling
    background_color=(255, 255, 255)  # White background
)

# Initialize pipeline
pipeline = ProfessionalVehiclePipeline(config)
pipeline.load_models()

# Process image
image = np.array(Image.open("car.jpg").convert("RGB"))
result = pipeline.process_image(
    image,
    output_path=Path("./output"),
    return_intermediates=True
)

# Access results
final_image = result["final_image"]  # RGB with white background
final_mask = result["final_mask"]    # Binary mask
alpha = result["alpha_matte"]        # Alpha channel

# Cleanup
pipeline.unload_models()
```

## Configuration Options

```python
@dataclass
class PipelineConfig:
    # Paths
    models_dir: str = "/models"
    device: str = "cuda"  # or "cpu"

    # Stage 1: Detection
    yolo_confidence: float = 0.45
    grounding_dino_threshold: float = 0.35
    detection_prompts: List[str] = ["car.", "vehicle.", "automobile."]

    # Stage 2: Shadow Detection
    shadow_threshold: float = 0.5

    # Stage 3: SAM2
    sam2_multimask: bool = True

    # Stage 5: SegFormer
    segformer_car_class_id: int = 20  # ADE20K class ID

    # Stage 6: ISNet
    isnet_input_size: Tuple[int, int] = (1024, 1024)

    # Stage 7: Enhancement
    upscale_factor: float = 1.0  # 1.0 = no upscale

    # Output
    output_format: str = "png"
    output_quality: int = 95
    background_color: Tuple[int, int, int] = (255, 255, 255)
```

## Output Files

For each processed image, the pipeline generates:

```
output/
├── {name}/
│   ├── result_bg_removed.png    # Final image with white background
│   ├── result_transparent.png   # RGBA with transparency
│   ├── result_mask.png          # Binary segmentation mask
│   ├── result_alpha.png         # Alpha matte
│   ├── result_summary.json      # Processing metadata
│   └── intermediate_stage*.png  # (if --save-intermediates)
```

## Performance

### Processing Time (GPU)

| Image Size | Time per Image | GPU Memory |
| ---------- | -------------- | ---------- |
| 1920x1080  | ~3-5s          | ~6 GB      |
| 3840x2160  | ~8-12s         | ~10 GB     |
| 4000x3000  | ~10-15s        | ~12 GB     |

### Processing Time (CPU)

| Image Size | Time per Image | RAM    |
| ---------- | -------------- | ------ |
| 1920x1080  | ~30-60s        | ~8 GB  |
| 3840x2160  | ~90-120s       | ~16 GB |

## Fallback Behavior

The pipeline gracefully handles missing models:

- **ShadowFormer missing:** Falls back to color-based shadow detection
- **CarParts-Seg missing:** Skips part analysis, uses SAM2 mask directly
- **SegFormer missing:** Skips semantic verification
- **ISNet missing:** Falls back to GrabCut edge refinement
- **RealESRGAN missing:** Uses basic CLAHE enhancement

## Docker

```dockerfile
FROM pytorch/pytorch:2.1.0-cuda12.1-cudnn8-runtime

WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y \
    libgl1-mesa-glx \
    libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

# Install Python dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy pipeline code
COPY *.py .

# Download essential models
RUN python download_pipeline_models.py --essential

# Create directories
RUN mkdir -p /app/input /app/output /models

CMD ["python", "process_professional_batch.py"]
```

## Comparison: Original vs Professional

| Feature             | Original (YOLO+SAM2)   | Professional (7-Stage)               |
| ------------------- | ---------------------- | ------------------------------------ |
| Detection           | YOLOv8m only           | YOLOv8x-seg + GroundingDINO          |
| Shadow handling     | Basic color analysis   | ShadowFormer deep learning           |
| Segmentation        | SAM2 Base+             | SAM2 Large with shadow-aware prompts |
| Wheel protection    | Manual bbox adjustment | CarParts-Seg automatic               |
| Semantic validation | None                   | SegFormer IoU correction             |
| Edge refinement     | Bilateral filter       | ISNet dichotomous segmentation       |
| Enhancement         | None                   | RealESRGAN + CLAHE                   |
| Quality             | Good                   | Professional dealership-grade        |
| Speed (GPU)         | ~1-2s                  | ~3-5s                                |

## Troubleshooting

### CUDA Out of Memory

```bash
# Use CPU instead
python process_professional_batch.py --cpu

# Or reduce batch size
export PYTORCH_CUDA_ALLOC_CONF=max_split_size_mb:512
```

### Model Download Failed

```bash
# Retry with force
python download_pipeline_models.py --model isnet --force

# Check available models
python download_pipeline_models.py --check
```

### Missing Dependencies

```bash
# Install HuggingFace hub
pip install huggingface_hub transformers

# Install ONNX runtime
pip install onnxruntime-gpu  # or onnxruntime for CPU
```

## Credits

- **SAM2**: Meta AI Research
- **YOLOv8**: Ultralytics
- **GroundingDINO**: IDEA Research
- **ShadowFormer**: PINTO_model_zoo
- **ISNet/DIS**: Xuebin Qin (ECCV 2022)
- **SegFormer**: NVIDIA
- **RealESRGAN**: Xintao Wang

## License

This pipeline combines multiple models with different licenses:

- SAM2: Apache 2.0
- YOLO: AGPL-3.0
- ISNet: Apache 2.0
- GroundingDINO: Apache 2.0
- RealESRGAN: BSD 3-Clause

Check individual model licenses before commercial use.
