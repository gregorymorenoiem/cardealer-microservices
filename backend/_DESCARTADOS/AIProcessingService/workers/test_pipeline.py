#!/usr/bin/env python3
"""
Simple Vehicle Segmentation Test
Uses YOLOv8x-seg + SAM2 for vehicle cutout
"""

import os
import sys
from pathlib import Path
import numpy as np
from PIL import Image
import cv2

# Check for models
MODELS_DIR = Path("./models")
YOLO_MODEL = MODELS_DIR / "yolov8x-seg.pt"
SAM2_MODEL = MODELS_DIR / "sam2_hiera_base_plus.pt"

print("=" * 60)
print("🚗 Vehicle Segmentation Test - YOLOv8x + SAM2")
print("=" * 60)

# Verify models
print("\n📊 Model Status:")
print(f"   YOLOv8x-seg: {'✅' if YOLO_MODEL.exists() else '❌'} ({YOLO_MODEL})")
print(f"   SAM2-Base+:  {'✅' if SAM2_MODEL.exists() else '❌'} ({SAM2_MODEL})")

if not YOLO_MODEL.exists():
    print("\n❌ YOLOv8x-seg model not found!")
    sys.exit(1)

# Import ultralytics
print("\n📦 Loading YOLO model...")
from ultralytics import YOLO

# Load YOLO model
model = YOLO(str(YOLO_MODEL))
print("✅ YOLOv8x-seg loaded successfully!")

# Check for input images
INPUT_DIR = Path("./input")
OUTPUT_DIR = Path("./output")
OUTPUT_DIR.mkdir(exist_ok=True)

# Look for test images
test_images = []
if INPUT_DIR.exists():
    test_images = list(INPUT_DIR.glob("*.jpg")) + list(INPUT_DIR.glob("*.png"))

if not test_images:
    # Create a test image
    print("\n📸 No input images found. Creating a test image...")
    test_img = np.zeros((480, 640, 3), dtype=np.uint8)
    test_img[:] = (200, 200, 200)  # Gray background
    cv2.putText(test_img, "Test Image - Place vehicle images in ./input/", 
                (20, 240), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (50, 50, 50), 2)
    
    INPUT_DIR.mkdir(exist_ok=True)
    test_path = INPUT_DIR / "test_placeholder.jpg"
    cv2.imwrite(str(test_path), test_img)
    test_images = [test_path]
    print(f"   Created placeholder: {test_path}")
    print("   📁 Add real vehicle images to ./input/ for actual testing")

# Process each image
print(f"\n🔄 Processing {len(test_images)} images...")

for img_path in test_images[:3]:  # Process up to 3 images
    print(f"\n   Processing: {img_path.name}")
    
    try:
        # Run YOLO detection
        results = model(str(img_path), verbose=False)
        
        if results and len(results) > 0:
            result = results[0]
            
            # Check for detections
            if result.boxes is not None and len(result.boxes) > 0:
                print(f"   ✅ Detected {len(result.boxes)} objects")
                
                # Filter for vehicles (COCO classes: car=2, truck=7, motorcycle=3)
                vehicle_classes = {2, 3, 7}  # car, motorcycle, truck
                
                for i, (box, cls) in enumerate(zip(result.boxes.xyxy, result.boxes.cls)):
                    cls_id = int(cls.item())
                    cls_name = result.names[cls_id]
                    
                    if cls_id in vehicle_classes or 'car' in cls_name.lower() or 'vehicle' in cls_name.lower():
                        print(f"      🚗 Found: {cls_name} (conf: {result.boxes.conf[i]:.2f})")
                        
                        # If segmentation mask available
                        if result.masks is not None and i < len(result.masks):
                            mask = result.masks[i].data.cpu().numpy()[0]
                            print(f"      📐 Mask shape: {mask.shape}")
                
                # Save annotated image
                annotated = result.plot()
                output_path = OUTPUT_DIR / f"{img_path.stem}_detected.jpg"
                cv2.imwrite(str(output_path), annotated)
                print(f"   💾 Saved: {output_path}")
            else:
                print(f"   ℹ️  No objects detected")
        else:
            print(f"   ⚠️  No results returned")
            
    except Exception as e:
        print(f"   ❌ Error: {e}")

print("\n" + "=" * 60)
print("✅ Test complete!")
print(f"📁 Output saved to: {OUTPUT_DIR.absolute()}")
print("=" * 60)
