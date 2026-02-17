#!/usr/bin/env python3
"""
Remove Background V8 - YOLO + SAM (Fixed)
==========================================
Based on V7 with fixes for:
- Retrovisores (side mirrors) - expand box laterally
- Defensas laterales (side fenders) - expand box downward

Key difference from V7:
- Expanded bounding box to include mirrors and fenders
- NO aggressive removal of background vehicles (trust SAM)
- SAM only segments what's inside the expanded box

Strategy:
1. YOLO detects vehicles → Select main vehicle
2. EXPAND bounding box:
   - Sides +10% for mirrors
   - Bottom +12% for fenders/running boards
   - Top +5% for roof accessories
3. SAM segments ONLY within expanded box
4. Background vehicles are automatically excluded (outside box)

Usage:
    python remove_background_v8.py
"""

import os
import sys
import time
from pathlib import Path
from PIL import Image, ImageFilter, ImageDraw
import numpy as np
from scipy.ndimage import binary_fill_holes, binary_dilation, binary_erosion, gaussian_filter
import torch

# Directories
INPUT_DIR = Path("./input")
OUTPUT_DIR = Path("./output_v8")

# Model paths
SAM_CHECKPOINT = Path("./sam_vit_h_4b8939.pth")


def setup_directories():
    """Create output directories."""
    OUTPUT_DIR.mkdir(exist_ok=True)
    (OUTPUT_DIR / "transparent").mkdir(exist_ok=True)
    (OUTPUT_DIR / "white_bg").mkdir(exist_ok=True)
    (OUTPUT_DIR / "shadow").mkdir(exist_ok=True)
    (OUTPUT_DIR / "debug").mkdir(exist_ok=True)


# =============================================================================
# YOLO DETECTION
# =============================================================================

def detect_with_yolo(image_path: Path) -> list:
    """Detect vehicles using YOLO."""
    from ultralytics import YOLO
    
    model = YOLO("yolov8x-seg.pt")
    results = model(str(image_path), conf=0.25, verbose=False)[0]
    
    if results.boxes is None or len(results.boxes) == 0:
        return []
    
    vehicle_classes = {2: 'car', 3: 'motorcycle', 5: 'bus', 7: 'truck'}
    
    img = Image.open(image_path)
    width, height = img.size
    center_x, center_y = width / 2, height / 2
    
    vehicles = []
    for i, cls in enumerate(results.boxes.cls):
        class_id = int(cls.item())
        if class_id in vehicle_classes:
            box = results.boxes.xyxy[i].cpu().numpy()
            conf = float(results.boxes.conf[i].item())
            
            x1, y1, x2, y2 = int(box[0]), int(box[1]), int(box[2]), int(box[3])
            box_width = x2 - x1
            box_height = y2 - y1
            box_area = box_width * box_height
            box_center_x = (x1 + x2) / 2
            box_center_y = (y1 + y2) / 2
            
            size_score = box_area / (width * height)
            center_score = 1 - (abs(box_center_x - center_x) / center_x)
            bottom_score = box_center_y / height
            
            total_score = (size_score * 3) + (center_score * 1) + (bottom_score * 1) + (conf * 1)
            
            vehicles.append({
                'box': [x1, y1, x2, y2],
                'confidence': conf,
                'class_name': vehicle_classes[class_id],
                'score': total_score,
                'area': box_area
            })
    
    vehicles.sort(key=lambda x: x['score'], reverse=True)
    return vehicles


def expand_box_for_mirrors_and_fenders(box: list, image_size: tuple) -> list:
    """
    Expand bounding box to include:
    - Side mirrors (lateral expansion)
    - Fenders/running boards (bottom expansion)
    - Roof accessories (slight top expansion)
    """
    width, height = image_size
    x1, y1, x2, y2 = box
    
    box_height = y2 - y1
    box_width = x2 - x1
    
    # Expand SIDES by 25% for mirrors (increased from 18%)
    side_expand = int(box_width * 0.25)
    x1 = max(0, x1 - side_expand)
    x2 = min(width, x2 + side_expand)
    
    # Expand TOP by 8% for roof/antennas (increased from 5%)
    top_expand = int(box_height * 0.08)
    y1 = max(0, y1 - top_expand)
    
    # Expand BOTTOM by 15% for fenders/running boards (increased from 12%)
    bottom_expand = int(box_height * 0.15)
    y2 = min(height, y2 + bottom_expand)
    
    return [x1, y1, x2, y2]


# =============================================================================
# SAM SEGMENTATION
# =============================================================================

def load_sam_model():
    """Load SAM model."""
    from segment_anything import sam_model_registry, SamPredictor
    
    if not SAM_CHECKPOINT.exists():
        raise FileNotFoundError(f"SAM checkpoint not found at {SAM_CHECKPOINT}")
    
    sam = sam_model_registry["vit_h"](checkpoint=str(SAM_CHECKPOINT))
    sam.to(device="cpu")
    
    predictor = SamPredictor(sam)
    return predictor


def segment_with_sam(image_path: Path, box: list, sam_predictor) -> np.ndarray:
    """
    Use SAM to segment the vehicle using the bounding box as prompt.
    Returns binary mask.
    """
    image = np.array(Image.open(image_path).convert('RGB'))
    
    sam_predictor.set_image(image)
    
    input_box = np.array(box)
    
    # Predict mask using box prompt only
    masks, scores, logits = sam_predictor.predict(
        point_coords=None,
        point_labels=None,
        box=input_box,
        multimask_output=True
    )
    
    # Select the mask with highest score
    best_idx = np.argmax(scores)
    best_mask = masks[best_idx]
    
    return best_mask


# =============================================================================
# DEBUG VISUALIZATION
# =============================================================================

def save_debug_image(image_path: Path, main_vehicle: dict, expanded_box: list, 
                     background_vehicles: list):
    """Save debug image with detection boxes."""
    img = Image.open(image_path).convert('RGB')
    draw = ImageDraw.Draw(img)
    
    # Draw background vehicles in RED
    for v in background_vehicles:
        x1, y1, x2, y2 = v['box']
        draw.rectangle([x1, y1, x2, y2], outline='red', width=3)
        draw.text((x1, y1 - 18), f"BG: {v['class_name']}", fill='red')
    
    # Draw original YOLO box in YELLOW
    ox1, oy1, ox2, oy2 = main_vehicle['box']
    draw.rectangle([ox1, oy1, ox2, oy2], outline='yellow', width=2)
    draw.text((ox1, oy1 - 20), "YOLO original", fill='yellow')
    
    # Draw EXPANDED box in GREEN (this is what SAM uses)
    ex1, ey1, ex2, ey2 = expanded_box
    draw.rectangle([ex1, ey1, ex2, ey2], outline='lime', width=4)
    draw.text((ex1, ey1 - 40), "EXPANDED (SAM uses this)", fill='lime')
    
    debug_path = OUTPUT_DIR / "debug" / f"{image_path.stem}_detection.jpg"
    img.save(debug_path, quality=90)
    
    return debug_path


# =============================================================================
# MASK POST-PROCESSING
# =============================================================================

def refine_mask(mask: np.ndarray) -> np.ndarray:
    """Refine the SAM mask with morphological operations."""
    # Fill holes
    mask = binary_fill_holes(mask)
    
    # Slight smoothing
    mask = binary_dilation(mask, iterations=1)
    mask = binary_erosion(mask, iterations=1)
    
    return mask


def create_soft_alpha(mask: np.ndarray) -> np.ndarray:
    """Create alpha channel with soft edges."""
    from scipy.ndimage import distance_transform_edt
    
    alpha = mask.astype(np.float32) * 255
    
    dist_inside = distance_transform_edt(mask)
    dist_outside = distance_transform_edt(~mask)
    
    edge_width = 2
    edge_inside = (dist_inside > 0) & (dist_inside <= edge_width)
    edge_outside = (dist_outside > 0) & (dist_outside <= edge_width)
    
    alpha[edge_inside] = 200 + (dist_inside[edge_inside] / edge_width) * 55
    alpha[edge_outside] = (1 - dist_outside[edge_outside] / edge_width) * 100
    
    alpha = gaussian_filter(alpha, sigma=0.5)
    
    return np.clip(alpha, 0, 255).astype(np.uint8)


# =============================================================================
# OUTPUT GENERATION
# =============================================================================

def add_professional_shadow(image: Image.Image) -> Image.Image:
    """Add a subtle contact shadow."""
    width, height = image.size
    padding = 50
    new_width = width + padding * 2
    new_height = height + padding * 2
    
    result = Image.new('RGBA', (new_width, new_height), (255, 255, 255, 255))
    
    alpha = np.array(image.split()[3])
    
    rows_with_content = np.where(alpha.max(axis=1) > 128)[0]
    if len(rows_with_content) > 0:
        bottom_y = rows_with_content[-1]
    else:
        bottom_y = height - 1
    
    shadow_height = 30
    shadow_array = np.zeros((height, width), dtype=np.uint8)
    
    for y in range(max(0, bottom_y - shadow_height), min(height, bottom_y + 5)):
        cols_with_content = np.where(alpha[y] > 128)[0]
        if len(cols_with_content) > 0:
            left_x = cols_with_content[0]
            right_x = cols_with_content[-1]
            
            dist_from_bottom = abs(y - bottom_y)
            intensity = max(0, 60 - dist_from_bottom * 3)
            
            for x in range(left_x, right_x + 1):
                edge_dist = min(x - left_x, right_x - x)
                edge_falloff = min(1.0, edge_dist / 20)
                shadow_array[y, x] = int(intensity * edge_falloff)
    
    shadow = Image.fromarray(shadow_array)
    shadow = shadow.filter(ImageFilter.GaussianBlur(radius=8))
    
    result.paste(Image.new('RGB', (width, height), (0, 0, 0)), 
                 (padding, padding + 3), shadow)
    result.paste(image, (padding, padding), image)
    
    return result


def create_white_background(image: Image.Image) -> Image.Image:
    white_bg = Image.new('RGBA', image.size, (255, 255, 255, 255))
    white_bg.paste(image, (0, 0), image)
    return white_bg.convert('RGB')


# =============================================================================
# MAIN PROCESSING
# =============================================================================

def process_image(image_path: Path, sam_predictor) -> dict:
    """Process a single image with YOLO + SAM."""
    results = {}
    
    try:
        img = Image.open(image_path)
        image_size = img.size
        
        # Step 1: Detect vehicles
        print(f"  → Detecting vehicles...")
        vehicles = detect_with_yolo(image_path)
        
        if not vehicles:
            print(f"    ⚠️ No vehicles detected")
            results['success'] = False
            results['error'] = "No vehicles detected"
            return results
        
        main_vehicle = vehicles[0]
        background_vehicles = vehicles[1:]
        
        print(f"    ✓ Main: {main_vehicle['class_name']} (conf={main_vehicle['confidence']:.2f})")
        
        if background_vehicles:
            print(f"    🔴 Background: {len(background_vehicles)} (will be excluded by SAM)")
        
        # Step 2: EXPAND bounding box for mirrors and fenders
        print(f"  → Expanding box for mirrors/fenders...")
        expanded_box = expand_box_for_mirrors_and_fenders(main_vehicle['box'], image_size)
        
        ox1, oy1, ox2, oy2 = main_vehicle['box']
        ex1, ey1, ex2, ey2 = expanded_box
        print(f"    Original: ({ox1},{oy1})-({ox2},{oy2})")
        print(f"    Expanded: ({ex1},{ey1})-({ex2},{ey2})")
        
        # Save debug image
        save_debug_image(image_path, main_vehicle, expanded_box, background_vehicles)
        
        # Step 3: Segment with SAM using EXPANDED box
        print(f"  → Segmenting with SAM...")
        mask = segment_with_sam(image_path, expanded_box, sam_predictor)
        
        # Step 4: Refine mask (NO aggressive removal of background vehicles)
        print(f"  → Refining mask...")
        mask = refine_mask(mask)
        
        # Step 5: Create soft alpha
        print(f"  → Creating soft edges...")
        alpha = create_soft_alpha(mask)
        
        # Step 6: Apply to original image
        original = Image.open(image_path).convert('RGBA')
        original.putalpha(Image.fromarray(alpha))
        
        # Step 7: Save outputs
        stem = image_path.stem
        
        # Transparent
        transparent_path = OUTPUT_DIR / "transparent" / f"{stem}_transparent.png"
        original.save(transparent_path, 'PNG', optimize=True)
        results['transparent'] = transparent_path
        
        # White background
        white_bg = create_white_background(original)
        white_path = OUTPUT_DIR / "white_bg" / f"{stem}_white.png"
        white_bg.save(white_path, 'PNG', optimize=True)
        results['white_bg'] = white_path
        
        # With shadow
        with_shadow = add_professional_shadow(original)
        shadow_path = OUTPUT_DIR / "shadow" / f"{stem}_shadow.png"
        with_shadow.save(shadow_path, 'PNG', optimize=True)
        results['shadow'] = shadow_path
        
        results['success'] = True
        results['main_vehicle'] = main_vehicle
        results['background_count'] = len(background_vehicles)
        
    except Exception as e:
        results['success'] = False
        results['error'] = str(e)
        import traceback
        traceback.print_exc()
    
    return results


def main():
    print("=" * 70)
    print("🚗 Background Removal V8 - YOLO + SAM (Mirrors & Fenders Fixed)")
    print("=" * 70)
    print()
    print("Fixes over V7:")
    print("  ✓ Expanded box SIDES +10% → includes mirrors")
    print("  ✓ Expanded box BOTTOM +12% → includes fenders/running boards")
    print("  ✓ Expanded box TOP +5% → includes roof accessories")
    print("  ✓ NO aggressive removal → trusts SAM segmentation")
    print()
    print("Strategy:")
    print("  1. YOLO detects main vehicle")
    print("  2. EXPAND box for mirrors/fenders")
    print("  3. SAM segments ONLY within expanded box")
    print("  4. Background vehicles outside box = automatically excluded")
    print()
    print("=" * 70)
    
    setup_directories()
    
    # Check SAM model
    if not SAM_CHECKPOINT.exists():
        print(f"\n❌ SAM model not found at {SAM_CHECKPOINT}")
        sys.exit(1)
    
    # Load SAM model
    print("\n📦 Loading SAM model...")
    sam_predictor = load_sam_model()
    print("   ✓ SAM loaded\n")
    
    # Get images
    extensions = ['*.jpg', '*.jpeg', '*.png', '*.webp']
    images = []
    for ext in extensions:
        images.extend(INPUT_DIR.glob(ext))
    
    if not images:
        print(f"\n❌ No images found in {INPUT_DIR}")
        sys.exit(1)
    
    print(f"📁 Found {len(images)} images\n")
    
    start_time = time.time()
    successful = 0
    total_bg_excluded = 0
    
    for i, image_path in enumerate(sorted(images), 1):
        print(f"[{i}/{len(images)}] {image_path.name}")
        img_start = time.time()
        
        results = process_image(image_path, sam_predictor)
        
        img_time = time.time() - img_start
        
        if results['success']:
            bg_count = results.get('background_count', 0)
            total_bg_excluded += bg_count
            
            status = f"✅ Done"
            if bg_count > 0:
                status += f" ({bg_count} bg vehicle{'s' if bg_count > 1 else ''} excluded)"
            print(f"  {status} in {img_time:.1f}s\n")
            successful += 1
        else:
            print(f"  ❌ Failed: {results.get('error', 'Unknown')}\n")
    
    total_time = time.time() - start_time
    
    print("=" * 70)
    print("📊 SUMMARY")
    print("=" * 70)
    print(f"✅ Successful: {successful}/{len(images)}")
    print(f"🚗 Background vehicles excluded: {total_bg_excluded}")
    print(f"⏱️  Total time: {total_time:.1f}s ({total_time/len(images):.1f}s avg)")
    print(f"📂 Output: {OUTPUT_DIR}/")
    print(f"   ├── transparent/  (PNG with alpha)")
    print(f"   ├── white_bg/     (PNG with white)")
    print(f"   ├── shadow/       (PNG with shadow)")
    print(f"   └── debug/        (yellow=YOLO, green=EXPANDED)")
    print("=" * 70)


if __name__ == "__main__":
    main()
