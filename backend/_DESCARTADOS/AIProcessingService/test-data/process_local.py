#!/usr/bin/env python3
"""
Local Image Processor for AI Processing
Procesa imágenes locales directamente sin usar S3 ni API

Usage: python3 process_local.py [image_name]

Examples:
    python3 process_local.py                     # Process all images
    python3 process_local.py car_front_1.jpg     # Process single image
"""

import os
import sys
import cv2
import numpy as np
from pathlib import Path

# Paths
BASE_DIR = Path(__file__).parent
PHOTOS_DIR = BASE_DIR / "photos"
PROCESSED_DIR = BASE_DIR / "processed"

# Ensure processed directory exists
PROCESSED_DIR.mkdir(exist_ok=True)

def remove_background_simple(image_path: Path) -> tuple:
    """
    Simple background removal using GrabCut
    Returns (bg_removed_image, segmentation_mask)
    """
    # Read image
    img = cv2.imread(str(image_path))
    if img is None:
        print(f"  ❌ Could not read image: {image_path}")
        return None, None
    
    h, w = img.shape[:2]
    
    # Create mask for GrabCut
    mask = np.zeros((h, w), np.uint8)
    
    # Define rectangle (assume car is in center 80% of image)
    margin_x = int(w * 0.1)
    margin_y = int(h * 0.1)
    rect = (margin_x, margin_y, w - 2*margin_x, h - 2*margin_y)
    
    # Background model
    bgd_model = np.zeros((1, 65), np.float64)
    fgd_model = np.zeros((1, 65), np.float64)
    
    try:
        # Run GrabCut
        cv2.grabCut(img, mask, rect, bgd_model, fgd_model, 5, cv2.GC_INIT_WITH_RECT)
        
        # Create binary mask (0,2 = background, 1,3 = foreground)
        mask2 = np.where((mask == 2) | (mask == 0), 0, 1).astype('uint8')
        
        # Create white background image
        white_bg = np.ones_like(img) * 255
        
        # Apply mask
        bg_removed = np.where(mask2[:, :, np.newaxis] == 1, img, white_bg)
        
        # Create segmentation mask (white = vehicle, black = background)
        seg_mask = (mask2 * 255).astype(np.uint8)
        seg_mask_rgb = cv2.cvtColor(seg_mask, cv2.COLOR_GRAY2BGR)
        
        return bg_removed, seg_mask_rgb
        
    except Exception as e:
        print(f"  ❌ Error processing: {e}")
        return None, None

def process_image(image_name: str):
    """Process a single image"""
    image_path = PHOTOS_DIR / image_name
    
    if not image_path.exists():
        print(f"  ❌ Image not found: {image_path}")
        return False
    
    print(f"\n🔄 Processing: {image_name}")
    
    # Get base name without extension
    base_name = image_path.stem
    
    # Process
    bg_removed, seg_mask = remove_background_simple(image_path)
    
    if bg_removed is None or seg_mask is None:
        return False
    
    # Save results
    bg_output = PROCESSED_DIR / f"{base_name}_bg_removed.png"
    seg_output = PROCESSED_DIR / f"{base_name}_segmented.png"
    
    cv2.imwrite(str(bg_output), bg_removed)
    cv2.imwrite(str(seg_output), seg_mask)
    
    print(f"  ✅ Saved: {bg_output.name}")
    print(f"  ✅ Saved: {seg_output.name}")
    
    return True

def process_all():
    """Process all images in photos directory"""
    images = list(PHOTOS_DIR.glob("*.jpg")) + list(PHOTOS_DIR.glob("*.jpeg")) + list(PHOTOS_DIR.glob("*.png"))
    
    print(f"\n📷 Found {len(images)} images in {PHOTOS_DIR}")
    
    success = 0
    for img in images:
        if process_image(img.name):
            success += 1
    
    print(f"\n✅ Processed {success}/{len(images)} images")
    print(f"📁 Output saved to: {PROCESSED_DIR}")

if __name__ == "__main__":
    print("=" * 60)
    print("🖼️  AI Processing - Local Processor (Simple Mode)")
    print("=" * 60)
    
    if len(sys.argv) > 1:
        # Process specific image
        process_image(sys.argv[1])
    else:
        # Process all
        process_all()
    
    print("\n💡 View results at: http://localhost:8888")
    print("   (Run: python3 serve.py)")
