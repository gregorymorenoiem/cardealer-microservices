#!/usr/bin/env python3
"""
Professional Vehicle Segmentation Pipeline
YOLOv8x-seg + SAM2 for high-quality vehicle cutouts
"""

import os
import sys
from pathlib import Path
from typing import Optional, Tuple, List
import numpy as np
from PIL import Image
import cv2
import torch

# Paths
MODELS_DIR = Path("./models")
INPUT_DIR = Path("./input")
OUTPUT_DIR = Path("./output")
YOLO_MODEL = MODELS_DIR / "yolov8x-seg.pt"
SAM2_MODEL = MODELS_DIR / "sam2_hiera_base_plus.pt"

def load_models():
    """Load YOLO and optionally SAM2 models"""
    from ultralytics import YOLO
    
    print("\n📦 Loading models...")
    
    # Load YOLO
    yolo = YOLO(str(YOLO_MODEL))
    print("   ✅ YOLOv8x-seg loaded")
    
    # SAM2 requires special handling - skip for now
    sam2 = None
    
    return yolo, sam2


def refine_mask_edges(mask: np.ndarray, kernel_size: int = 5) -> np.ndarray:
    """Refine mask edges using morphological operations"""
    # Convert to uint8 if needed
    if mask.dtype != np.uint8:
        mask = (mask * 255).astype(np.uint8)
    
    # Morphological operations
    kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (kernel_size, kernel_size))
    
    # Close small holes
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel, iterations=2)
    
    # Open to remove noise
    mask = cv2.morphologyEx(mask, cv2.MORPH_OPEN, kernel, iterations=1)
    
    # Smooth edges with Gaussian blur and threshold
    mask_float = mask.astype(float) / 255.0
    mask_smooth = cv2.GaussianBlur(mask_float, (5, 5), 0)
    mask = (mask_smooth > 0.5).astype(np.uint8) * 255
    
    return mask


def protect_wheels(mask: np.ndarray, img_height: int, protection_ratio: float = 0.2) -> np.ndarray:
    """Protect wheel area from being cut off"""
    # The bottom 20% often contains wheels - ensure it's not cut
    bottom_start = int(img_height * (1 - protection_ratio))
    
    # Find the mask's bounding box
    if mask.max() > 0:
        rows = np.any(mask > 0, axis=1)
        if rows.any():
            y_min, y_max = np.where(rows)[0][[0, -1]]
            
            # If mask cuts off too early at the bottom, extend it slightly
            if y_max < img_height - 10:
                # Fill in the bottom area if there's content nearby
                cols = np.any(mask[y_max-20:y_max, :] > 0, axis=0)
                if cols.any():
                    x_min, x_max = np.where(cols)[0][[0, -1]]
                    # Extend mask down
                    extension = min(20, img_height - y_max - 1)
                    mask[y_max:y_max+extension, x_min:x_max] = mask[y_max, x_min:x_max]
    
    return mask


def create_transparent_cutout(image: np.ndarray, mask: np.ndarray) -> np.ndarray:
    """Create transparent PNG from image and mask"""
    # Ensure mask is 2D
    if len(mask.shape) == 3:
        mask = mask[:, :, 0]
    
    # Resize mask to match image if needed
    if mask.shape[:2] != image.shape[:2]:
        mask = cv2.resize(mask, (image.shape[1], image.shape[0]), interpolation=cv2.INTER_LINEAR)
    
    # Refine edges
    mask = refine_mask_edges(mask)
    
    # Protect wheels
    mask = protect_wheels(mask, image.shape[0])
    
    # Create alpha channel
    alpha = mask.astype(np.uint8)
    
    # Convert BGR to BGRA
    if image.shape[2] == 3:
        bgra = cv2.cvtColor(image, cv2.COLOR_BGR2BGRA)
    else:
        bgra = image.copy()
    
    # Apply alpha
    bgra[:, :, 3] = alpha
    
    return bgra


def process_image(image_path: Path, yolo_model, sam2_model=None) -> Tuple[Optional[np.ndarray], dict]:
    """Process a single image and return transparent cutout"""
    
    info = {
        "filename": image_path.name,
        "detections": 0,
        "vehicles": [],
        "masks_found": False,
        "error": None
    }
    
    try:
        # Load image
        image = cv2.imread(str(image_path))
        if image is None:
            info["error"] = "Failed to load image"
            return None, info
        
        info["original_size"] = image.shape[:2]
        
        # Run YOLO detection + segmentation
        results = yolo_model(image, verbose=False)
        
        if not results or len(results) == 0:
            info["error"] = "No YOLO results"
            return None, info
        
        result = results[0]
        
        if result.boxes is None or len(result.boxes) == 0:
            info["error"] = "No detections"
            return None, info
        
        info["detections"] = len(result.boxes)
        
        # Find vehicles (COCO: car=2, motorcycle=3, truck=7, bus=5)
        vehicle_classes = {2, 3, 5, 7}
        vehicle_indices = []
        
        for i, (box, cls, conf) in enumerate(zip(result.boxes.xyxy, result.boxes.cls, result.boxes.conf)):
            cls_id = int(cls.item())
            cls_name = result.names[cls_id]
            
            is_vehicle = cls_id in vehicle_classes or any(v in cls_name.lower() for v in ['car', 'truck', 'vehicle', 'motorcycle', 'bus'])
            
            if is_vehicle:
                vehicle_indices.append(i)
                info["vehicles"].append({
                    "class": cls_name,
                    "confidence": float(conf.item()),
                    "box": [int(x) for x in box.tolist()]
                })
        
        if not vehicle_indices:
            info["error"] = f"No vehicles found (detected: {[result.names[int(c)] for c in result.boxes.cls]})"
            return None, info
        
        # Get segmentation mask
        if result.masks is None or len(result.masks) == 0:
            info["error"] = "No segmentation masks available"
            return None, info
        
        info["masks_found"] = True
        
        # Combine all vehicle masks
        combined_mask = None
        for idx in vehicle_indices:
            if idx < len(result.masks):
                mask_data = result.masks[idx].data.cpu().numpy()[0]
                mask_resized = cv2.resize(mask_data.astype(np.float32), (image.shape[1], image.shape[0]))
                
                if combined_mask is None:
                    combined_mask = mask_resized
                else:
                    combined_mask = np.maximum(combined_mask, mask_resized)
        
        if combined_mask is None:
            info["error"] = "Failed to extract masks"
            return None, info
        
        # Create transparent cutout
        cutout = create_transparent_cutout(image, combined_mask)
        
        return cutout, info
        
    except Exception as e:
        info["error"] = str(e)
        return None, info


def main():
    print("=" * 70)
    print("🚗 Professional Vehicle Segmentation Pipeline")
    print("   YOLOv8x-seg with Edge Refinement & Wheel Protection")
    print("=" * 70)
    
    # Check models
    print("\n📊 Model Status:")
    print(f"   YOLOv8x-seg: {'✅' if YOLO_MODEL.exists() else '❌'}")
    print(f"   SAM2-Base+:  {'✅' if SAM2_MODEL.exists() else '⚠️ (optional)'}")
    
    if not YOLO_MODEL.exists():
        print("\n❌ Required model not found!")
        print("   Run: python download_models_simple.py")
        sys.exit(1)
    
    # Load models
    yolo, sam2 = load_models()
    
    # Setup directories
    OUTPUT_DIR.mkdir(exist_ok=True)
    
    # Find images
    image_extensions = {'.jpg', '.jpeg', '.png', '.webp', '.bmp'}
    images = [f for f in INPUT_DIR.iterdir() if f.suffix.lower() in image_extensions]
    images = [f for f in images if 'placeholder' not in f.name.lower()]
    
    if not images:
        print(f"\n⚠️ No images found in {INPUT_DIR}")
        print("   Add vehicle images and run again")
        return
    
    print(f"\n🔄 Processing {len(images)} images...")
    
    # Process each image
    results_summary = []
    
    for i, img_path in enumerate(images, 1):
        print(f"\n[{i}/{len(images)}] {img_path.name}")
        
        cutout, info = process_image(img_path, yolo, sam2)
        
        if cutout is not None:
            # Save PNG with transparency
            output_path = OUTPUT_DIR / f"{img_path.stem}_cutout.png"
            cv2.imwrite(str(output_path), cutout)
            
            # Also save a preview with white background
            preview = cutout.copy()
            mask = preview[:, :, 3] == 0
            preview[mask] = [255, 255, 255, 255]
            preview_path = OUTPUT_DIR / f"{img_path.stem}_preview.jpg"
            cv2.imwrite(str(preview_path), cv2.cvtColor(preview, cv2.COLOR_BGRA2BGR))
            
            vehicles = ', '.join([v['class'] for v in info['vehicles']])
            print(f"   ✅ {vehicles}")
            print(f"   💾 Saved: {output_path.name}")
            info["output"] = str(output_path)
            info["success"] = True
        else:
            print(f"   ❌ {info.get('error', 'Unknown error')}")
            info["success"] = False
        
        results_summary.append(info)
    
    # Print summary
    print("\n" + "=" * 70)
    print("📊 PROCESSING SUMMARY")
    print("=" * 70)
    
    success = sum(1 for r in results_summary if r.get("success"))
    failed = len(results_summary) - success
    
    print(f"\n   ✅ Successful: {success}")
    print(f"   ❌ Failed: {failed}")
    print(f"   📁 Output: {OUTPUT_DIR.absolute()}")
    
    if failed > 0:
        print("\n   Failed images:")
        for r in results_summary:
            if not r.get("success"):
                print(f"      - {r['filename']}: {r.get('error', 'Unknown')}")
    
    print("\n" + "=" * 70)


if __name__ == "__main__":
    main()
