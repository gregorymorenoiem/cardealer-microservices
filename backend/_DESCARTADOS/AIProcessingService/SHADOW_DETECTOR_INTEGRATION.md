# 🌑 Shadow Detector Integration

## Overview

The Shadow Detector module has been integrated into the AIProcessingService to improve vehicle segmentation quality by detecting and removing floor shadows that SAM2 incorrectly includes in the vehicle mask.

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                     VEHICLE IMAGE PROCESSING                         │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  📷 Input Image                                                     │
│        ↓                                                            │
│  🎯 YOLO v8m (Vehicle Detection)                                   │
│        ↓                                                            │
│  🔲 Bounding Box + 2% Tightening                                   │
│        ↓                                                            │
│  🎨 SAM2 (Segmentation with 9 strategic points)                    │
│        ↓                                                            │
│  🧹 Post-Processing:                                               │
│      ├── Keep largest contour                                      │
│      ├── 🌑 ShadowDetector (NEW)                                   │
│      │     ├── LAB + HSV color analysis                            │
│      │     ├── Process bottom 30% of mask                          │
│      │     └── Protect tire areas (V < 60)                         │
│      ├── Morphology (2x2 kernel)                                   │
│      └── Bilateral filter smoothing                                │
│        ↓                                                            │
│  ✨ Alpha Matting (Canny edge-only)                                │
│        ↓                                                            │
│  💾 Output: bg_removed.png + segmented.png + alpha.png             │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Files Added

| File                                | Size | Purpose                                |
| ----------------------------------- | ---- | -------------------------------------- |
| `workers/shadow_detector.py`        | 19KB | Main shadow detection module           |
| `workers/download_shadow_models.py` | 4KB  | Script to download pre-trained weights |

## Files Modified

| File                             | Changes                                    |
| -------------------------------- | ------------------------------------------ |
| `workers/process_local_batch.py` | Integrated ShadowDetector in SAM2Processor |

## ShadowDetector Class

### Initialization

```python
from shadow_detector import ShadowDetector

detector = ShadowDetector(
    device="cpu",        # "cpu" or "cuda"
    use_deep_model=True  # Try to load neural network weights
)
```

### Main Methods

#### `detect_shadows(image, mask=None, threshold=0.5)`

Detects shadows in an image using:

1. Deep learning model (if weights available)
2. Color-based analysis (LAB + HSV colorspace)

Returns:

- `shadow_mask`: Binary mask of shadow regions
- `metadata`: Dict with detection stats

#### `remove_shadows_from_mask(vehicle_mask, image, protect_wheels=True, bottom_region_ratio=0.30)`

Main function used after SAM2 segmentation. Removes floor shadows from vehicle mask.

Parameters:

- `vehicle_mask`: Binary mask from SAM2
- `image`: Original RGB image
- `protect_wheels`: Don't remove very dark areas (tires)
- `bottom_region_ratio`: Only process bottom X% of mask

Returns:

- `cleaned_mask`: Mask with shadows removed
- `removed_count`: Number of pixels removed

## Detection Methods

### 1. Color-Based (Default)

Uses LAB and HSV colorspace analysis:

```python
# Shadow characteristics:
# - Low luminance in LAB (L < 0.45)
# - Low saturation in HSV (S < 0.4)
# - Mid-range value (not pure black, not bright)

L_prob = clip(0.6 - L_norm, 0, 1) * 1.5  # Luminance contribution
S_prob = clip(0.4 - S_norm, 0, 1) * 2.0  # Saturation contribution
V_prob = gaussian(V_norm, center=0.4)     # Value contribution

shadow_prob = L_prob * 0.4 + S_prob * 0.4 + V_prob * 0.2
```

### 2. Deep Learning (When weights available)

Uses a lightweight U-Net architecture:

- Input: 256x256 RGB
- Encoder: 4 blocks (32→64→128→256→512 channels)
- Decoder: 4 blocks with skip connections
- Output: Shadow probability map

Pre-trained weights can be downloaded:

```bash
python download_shadow_models.py bdrar
```

## Protection Mechanisms

### Tire Protection

Very dark areas (V < 60 in HSV) are protected from removal:

```python
tire_protection = (V < 60) | (S > 50)  # Very dark OR saturated
shadows_to_remove = shadows_to_remove & ~tire_protection
```

### Connected Component Analysis

Large shadow blobs (>5% of mask area) are not removed:

```python
for component in connected_components(shadow_mask):
    if component.area > mask_area * 0.05:
        # Might be part of car, don't remove
        continue
```

## Usage Examples

### Basic Usage

```python
from shadow_detector import ShadowDetector
import numpy as np
import cv2

# Initialize
detector = ShadowDetector(device="cpu")

# Load image and mask
image = cv2.imread("vehicle.jpg")
image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
mask = cv2.imread("vehicle_mask.png", cv2.IMREAD_GRAYSCALE)

# Remove shadows
cleaned_mask, removed_count = detector.remove_shadows_from_mask(
    vehicle_mask=mask,
    image=image,
    protect_wheels=True,
    bottom_region_ratio=0.30
)

print(f"Removed {removed_count} shadow pixels")
```

### Get Shadow Probability Map

```python
shadow_mask, metadata = detector.detect_shadows(image, threshold=0.5)

print(f"Method: {metadata['method']}")
print(f"Shadow ratio: {metadata['shadow_ratio']:.2%}")
```

## Results

Shadow removal statistics from test images:

| Image        | SAM2 Score | Shadows Removed |
| ------------ | ---------- | --------------- |
| sports_car_1 | 0.846      | 846 pixels      |
| bmw_m4_blue  | 0.908      | 5,325 pixels    |
| ferrari_red  | 0.895      | 1,721 pixels    |
| porsche_911  | 0.878      | 2,080 pixels    |

## Training Custom Weights

To train the shadow detection model on your own dataset:

```python
from shadow_detector import train_shadow_model

train_shadow_model(
    dataset_path="/path/to/shadow_dataset",
    output_path="/models/shadow_detector.pt",
    epochs=50,
    batch_size=8,
    learning_rate=1e-4
)
```

Expected dataset structure:

```
shadow_dataset/
├── images/
│   ├── img001.jpg
│   └── ...
└── masks/
    ├── img001.png  # Shadow regions = white
    └── ...
```

## Docker Usage

The shadow detector is automatically loaded in the SAM2 worker:

```bash
docker run --rm \
  -v ./workers:/app \
  -v ./models:/models \
  -v ./test-data/originals:/app/input \
  -v ./test-data/processed:/app/output \
  sam2-worker-cpu:latest \
  python3 /app/process_local_batch.py
```

## Future Improvements

1. **Pre-trained Weights**: Download BDRAR or train custom weights on vehicle shadow dataset
2. **Instance Shadow Detection**: Link shadows to their source objects
3. **Shadow Removal**: Actually remove shadows from image (not just mask)
4. **Real-time Processing**: Optimize for GPU batch processing

## References

- BDRAR: "Bidirectional Feature Pyramid Network with Recurrent Attention Residual Modules for Shadow Detection" (ECCV 2018)
- MTMT: "A Multi-task Mean Teacher for Semi-supervised Shadow Detection" (CVPR 2020)
- SBU Shadow Dataset: https://www3.cs.stonybrook.edu/~cvl/dataset.html
