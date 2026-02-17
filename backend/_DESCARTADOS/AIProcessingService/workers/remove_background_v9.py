#!/usr/bin/env python3
"""
Remove Background V9 - Vehicle + Shadow Preservation
=====================================================
Mejora sobre V8: Preserva la sombra original del vehículo.

Estrategia de Sombra:
1. YOLO detecta el vehículo
2. SAM segmenta el vehículo (mask principal)
3. Detectamos la zona de sombra debajo del vehículo
4. SAM segmenta la sombra por separado
5. Combinamos: Vehículo opaco + Sombra semi-transparente

Outputs:
- transparent/: Vehículo sin fondo (PNG con alpha)
- with_shadow/: Vehículo + sombra original preservada
- white_shadow/: Fondo blanco + vehículo + sombra
- synthetic_shadow/: Sombra sintética profesional

Usage:
    python remove_background_v9.py
"""

import os
import sys
import time
from pathlib import Path
from PIL import Image, ImageFilter, ImageDraw, ImageEnhance
import numpy as np
from scipy.ndimage import binary_fill_holes, binary_dilation, binary_erosion, gaussian_filter
from scipy.ndimage import distance_transform_edt
import torch

# Directories
INPUT_DIR = Path("./input")
OUTPUT_DIR = Path("./output_v9")

# Model paths
SAM_CHECKPOINT = Path("./sam_vit_h_4b8939.pth")

# Shadow settings
SHADOW_EXPANSION_BOTTOM = 0.35  # Expand 35% below vehicle for shadow
SHADOW_OPACITY = 0.4  # Shadow transparency (0.0 = invisible, 1.0 = solid)
SHADOW_BLUR = 15  # Blur radius for shadow edges


def setup_directories():
    """Create output directories."""
    OUTPUT_DIR.mkdir(exist_ok=True)
    (OUTPUT_DIR / "transparent").mkdir(exist_ok=True)
    (OUTPUT_DIR / "with_shadow").mkdir(exist_ok=True)
    (OUTPUT_DIR / "white_shadow").mkdir(exist_ok=True)
    (OUTPUT_DIR / "synthetic_shadow").mkdir(exist_ok=True)
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


def expand_box_for_vehicle(box: list, image_size: tuple) -> list:
    """Expand bounding box for vehicle (mirrors, fenders, roof)."""
    width, height = image_size
    x1, y1, x2, y2 = box
    
    box_height = y2 - y1
    box_width = x2 - x1
    
    # Sides +25% for mirrors
    side_expand = int(box_width * 0.25)
    x1 = max(0, x1 - side_expand)
    x2 = min(width, x2 + side_expand)
    
    # Top +8% for roof/antennas
    top_expand = int(box_height * 0.08)
    y1 = max(0, y1 - top_expand)
    
    # Bottom +15% for fenders
    bottom_expand = int(box_height * 0.15)
    y2 = min(height, y2 + bottom_expand)
    
    return [x1, y1, x2, y2]


def get_shadow_box(vehicle_box: list, image_size: tuple) -> list:
    """
    Create a box for the shadow area below the vehicle.
    The shadow is typically on the ground, below and slightly wider than the vehicle.
    """
    width, height = image_size
    x1, y1, x2, y2 = vehicle_box
    
    vehicle_width = x2 - x1
    vehicle_height = y2 - y1
    
    # Shadow box starts at the bottom of the vehicle
    shadow_y1 = y2 - int(vehicle_height * 0.10)  # Overlap slightly with vehicle bottom
    
    # Shadow extends downward
    shadow_height = int(vehicle_height * SHADOW_EXPANSION_BOTTOM)
    shadow_y2 = min(height, y2 + shadow_height)
    
    # Shadow is slightly wider than vehicle (perspective effect)
    shadow_expand_x = int(vehicle_width * 0.15)
    shadow_x1 = max(0, x1 - shadow_expand_x)
    shadow_x2 = min(width, x2 + shadow_expand_x)
    
    return [shadow_x1, shadow_y1, shadow_x2, shadow_y2]


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


def segment_vehicle_with_sam(image_path: Path, box: list, sam_predictor) -> np.ndarray:
    """Segment the vehicle using SAM with bounding box prompt."""
    image = np.array(Image.open(image_path).convert('RGB'))
    
    sam_predictor.set_image(image)
    
    input_box = np.array(box)
    
    masks, scores, logits = sam_predictor.predict(
        point_coords=None,
        point_labels=None,
        box=input_box,
        multimask_output=True
    )
    
    best_idx = np.argmax(scores)
    return masks[best_idx]


def segment_shadow_with_sam(image_path: Path, shadow_box: list, vehicle_mask: np.ndarray,
                            sam_predictor) -> np.ndarray:
    """
    Segment the shadow area using SAM.
    Uses points in the shadow region as prompts.
    """
    image = np.array(Image.open(image_path).convert('RGB'))
    height, width = image.shape[:2]
    
    # Already set image from vehicle segmentation, but reset for shadow
    sam_predictor.set_image(image)
    
    sx1, sy1, sx2, sy2 = shadow_box
    
    # Create point prompts in the shadow zone
    # Points at the center-bottom of where shadow would be
    center_x = (sx1 + sx2) // 2
    shadow_points = []
    
    # Multiple points across the shadow area
    for x_offset in [-0.3, 0, 0.3]:
        px = int(center_x + (sx2 - sx1) * x_offset * 0.5)
        for y_offset in [0.3, 0.5, 0.7]:
            py = int(sy1 + (sy2 - sy1) * y_offset)
            if 0 <= px < width and 0 <= py < height:
                # Only add point if it's NOT inside the vehicle
                if not vehicle_mask[py, px]:
                    shadow_points.append([px, py])
    
    if len(shadow_points) < 2:
        # Fallback: just use box
        masks, scores, _ = sam_predictor.predict(
            point_coords=None,
            point_labels=None,
            box=np.array(shadow_box),
            multimask_output=True
        )
    else:
        point_coords = np.array(shadow_points)
        point_labels = np.ones(len(shadow_points))  # All positive (foreground)
        
        masks, scores, _ = sam_predictor.predict(
            point_coords=point_coords,
            point_labels=point_labels,
            box=np.array(shadow_box),
            multimask_output=True
        )
    
    # Select best mask
    best_idx = np.argmax(scores)
    shadow_mask = masks[best_idx]
    
    # Remove any overlap with vehicle
    shadow_mask = shadow_mask & ~vehicle_mask
    
    return shadow_mask


def detect_shadow_by_darkness(image_path: Path, vehicle_box: list, 
                               vehicle_mask: np.ndarray) -> np.ndarray:
    """
    Detect shadow by finding dark areas below the vehicle.
    This is a fallback/complement to SAM shadow detection.
    """
    img = Image.open(image_path).convert('RGB')
    img_array = np.array(img)
    height, width = img_array.shape[:2]
    
    # Convert to grayscale
    gray = np.mean(img_array, axis=2)
    
    # Get the bottom of the vehicle
    vehicle_rows = np.where(vehicle_mask.any(axis=1))[0]
    if len(vehicle_rows) == 0:
        return np.zeros((height, width), dtype=bool)
    
    vehicle_bottom = vehicle_rows[-1]
    
    # Get average brightness of the background (non-vehicle areas)
    non_vehicle = ~vehicle_mask
    if non_vehicle.sum() > 0:
        bg_brightness = np.mean(gray[non_vehicle])
    else:
        bg_brightness = 200
    
    # Shadow is darker than background
    # Look for pixels that are significantly darker
    shadow_threshold = bg_brightness * 0.7  # 70% of background brightness
    
    # Create shadow mask
    shadow_mask = gray < shadow_threshold
    
    # Only consider area below vehicle
    shadow_region = np.zeros((height, width), dtype=bool)
    shadow_region[vehicle_bottom:, :] = True
    
    # Also consider area slightly overlapping with vehicle bottom
    overlap_rows = max(0, vehicle_bottom - int(height * 0.05))
    shadow_region[overlap_rows:, :] = True
    
    shadow_mask = shadow_mask & shadow_region & ~vehicle_mask
    
    # Clean up with morphological operations
    shadow_mask = binary_erosion(shadow_mask, iterations=2)
    shadow_mask = binary_dilation(shadow_mask, iterations=3)
    shadow_mask = binary_fill_holes(shadow_mask)
    
    return shadow_mask


# =============================================================================
# MASK POST-PROCESSING
# =============================================================================

def refine_vehicle_mask(mask: np.ndarray) -> np.ndarray:
    """Refine the vehicle mask with morphological operations."""
    mask = binary_fill_holes(mask)
    mask = binary_dilation(mask, iterations=1)
    mask = binary_erosion(mask, iterations=1)
    return mask


def create_soft_alpha(mask: np.ndarray, edge_width: int = 2) -> np.ndarray:
    """Create alpha channel with soft edges."""
    alpha = mask.astype(np.float32) * 255
    
    dist_inside = distance_transform_edt(mask)
    dist_outside = distance_transform_edt(~mask)
    
    edge_inside = (dist_inside > 0) & (dist_inside <= edge_width)
    edge_outside = (dist_outside > 0) & (dist_outside <= edge_width)
    
    alpha[edge_inside] = 200 + (dist_inside[edge_inside] / edge_width) * 55
    alpha[edge_outside] = (1 - dist_outside[edge_outside] / edge_width) * 100
    
    alpha = gaussian_filter(alpha, sigma=0.5)
    
    return np.clip(alpha, 0, 255).astype(np.uint8)


def create_shadow_alpha(shadow_mask: np.ndarray, 
                        opacity: float = SHADOW_OPACITY) -> np.ndarray:
    """
    Create semi-transparent alpha for shadow.
    Shadow should be soft and fade out at edges.
    """
    # Start with the shadow mask
    shadow = shadow_mask.astype(np.float32)
    
    # Apply distance transform for soft edges
    dist = distance_transform_edt(shadow_mask)
    max_dist = dist.max() if dist.max() > 0 else 1
    
    # Normalize distance
    normalized_dist = dist / max_dist
    
    # Create gradient: center is more opaque, edges fade out
    shadow_alpha = normalized_dist * opacity * 255
    
    # Apply gaussian blur for smoothness
    shadow_alpha = gaussian_filter(shadow_alpha, sigma=SHADOW_BLUR)
    
    # Ensure it's within bounds
    shadow_alpha = np.clip(shadow_alpha, 0, 255).astype(np.uint8)
    
    return shadow_alpha


# =============================================================================
# SYNTHETIC SHADOW GENERATION
# =============================================================================

def create_synthetic_shadow(vehicle_mask: np.ndarray, 
                            image_size: tuple) -> np.ndarray:
    """
    Create a professional-looking synthetic shadow.
    This is used when natural shadow can't be detected.
    """
    height, width = vehicle_mask.shape
    
    # Find vehicle bounds
    rows_with_content = np.where(vehicle_mask.any(axis=1))[0]
    cols_with_content = np.where(vehicle_mask.any(axis=0))[0]
    
    if len(rows_with_content) == 0 or len(cols_with_content) == 0:
        return np.zeros((height, width), dtype=np.float32)
    
    bottom_y = rows_with_content[-1]
    left_x = cols_with_content[0]
    right_x = cols_with_content[-1]
    
    # Create shadow canvas
    shadow = np.zeros((height, width), dtype=np.float32)
    
    # Shadow parameters
    shadow_height = min(60, int((right_x - left_x) * 0.15))
    shadow_offset_y = 5  # Shadow slightly below vehicle
    
    # Create elliptical shadow
    shadow_center_x = (left_x + right_x) // 2
    shadow_center_y = bottom_y + shadow_offset_y + shadow_height // 3
    shadow_width = (right_x - left_x) * 0.9
    shadow_height_actual = shadow_height
    
    # Generate ellipse
    y_coords, x_coords = np.ogrid[:height, :width]
    
    # Ellipse equation: ((x-cx)/a)^2 + ((y-cy)/b)^2 <= 1
    a = shadow_width / 2
    b = shadow_height_actual / 2
    
    if a > 0 and b > 0:
        ellipse = ((x_coords - shadow_center_x) / a) ** 2 + \
                  ((y_coords - shadow_center_y) / b) ** 2
        
        # Gradient within ellipse (center = 1, edge = 0)
        shadow_gradient = np.maximum(0, 1 - ellipse)
        shadow_gradient = np.power(shadow_gradient, 0.5)  # Soften
        
        # Apply to shadow
        shadow = shadow_gradient * SHADOW_OPACITY
    
    # Add contact shadow (darker line right under vehicle)
    contact_shadow_height = 8
    for y in range(bottom_y, min(height, bottom_y + contact_shadow_height)):
        cols_at_y = np.where(vehicle_mask[min(y, bottom_y - 1), :])[0]
        if len(cols_at_y) > 0:
            intensity = 0.6 * (1 - (y - bottom_y) / contact_shadow_height)
            shadow[y, cols_at_y[0]:cols_at_y[-1]+1] = max(
                shadow[y, cols_at_y[0]:cols_at_y[-1]+1].max(),
                intensity
            )
    
    # Blur for softness
    shadow = gaussian_filter(shadow, sigma=12)
    
    return (shadow * 255).astype(np.uint8)


# =============================================================================
# DEBUG VISUALIZATION
# =============================================================================

def save_debug_image(image_path: Path, vehicle_box: list, expanded_box: list,
                     shadow_box: list, vehicle_mask: np.ndarray, 
                     shadow_mask: np.ndarray):
    """Save debug visualization."""
    img = Image.open(image_path).convert('RGB')
    
    # Create overlay
    overlay = img.copy()
    draw = ImageDraw.Draw(overlay)
    
    # Draw boxes
    draw.rectangle(vehicle_box, outline='yellow', width=2)
    draw.rectangle(expanded_box, outline='lime', width=3)
    draw.rectangle(shadow_box, outline='cyan', width=2)
    
    # Save boxes debug
    boxes_path = OUTPUT_DIR / "debug" / f"{image_path.stem}_boxes.jpg"
    overlay.save(boxes_path, quality=90)
    
    # Create mask visualization
    height, width = vehicle_mask.shape
    mask_vis = np.zeros((height, width, 3), dtype=np.uint8)
    
    # Vehicle in green
    mask_vis[vehicle_mask] = [0, 255, 0]
    
    # Shadow in blue (semi-transparent)
    shadow_overlay = shadow_mask.astype(np.float32) * 0.5
    mask_vis[:, :, 2] = np.maximum(
        mask_vis[:, :, 2],
        (shadow_overlay * 255).astype(np.uint8)
    )
    
    mask_img = Image.fromarray(mask_vis)
    mask_path = OUTPUT_DIR / "debug" / f"{image_path.stem}_masks.png"
    mask_img.save(mask_path)
    
    return boxes_path, mask_path


# =============================================================================
# OUTPUT GENERATION
# =============================================================================

def create_transparent_output(original: Image.Image, 
                              vehicle_alpha: np.ndarray) -> Image.Image:
    """Create transparent PNG with just the vehicle."""
    result = original.convert('RGBA')
    result.putalpha(Image.fromarray(vehicle_alpha))
    return result


def create_with_shadow_output(original: Image.Image,
                               vehicle_mask: np.ndarray,
                               shadow_alpha: np.ndarray) -> Image.Image:
    """
    Create output with vehicle + preserved shadow.
    Vehicle is fully opaque, shadow is semi-transparent.
    """
    width, height = original.size
    
    # Create RGBA image
    result = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    
    # Get original as RGBA
    original_rgba = original.convert('RGBA')
    original_array = np.array(original_rgba)
    result_array = np.array(result)
    
    # Vehicle alpha (fully opaque where vehicle is)
    vehicle_alpha = create_soft_alpha(vehicle_mask)
    
    # Combine: Vehicle areas get full opacity, shadow areas get shadow opacity
    combined_alpha = np.maximum(vehicle_alpha, shadow_alpha)
    
    # Copy RGB from original
    result_array[:, :, :3] = original_array[:, :, :3]
    result_array[:, :, 3] = combined_alpha
    
    return Image.fromarray(result_array)


def create_white_with_shadow(vehicle_shadow_image: Image.Image) -> Image.Image:
    """Composite vehicle+shadow onto white background."""
    white_bg = Image.new('RGBA', vehicle_shadow_image.size, (255, 255, 255, 255))
    white_bg.paste(vehicle_shadow_image, (0, 0), vehicle_shadow_image)
    return white_bg.convert('RGB')


def create_synthetic_shadow_output(original: Image.Image,
                                   vehicle_mask: np.ndarray) -> Image.Image:
    """Create output with synthetic shadow (fallback)."""
    width, height = original.size
    
    # Create result with padding for shadow
    padding = 80
    result = Image.new('RGBA', (width + padding * 2, height + padding), 
                       (255, 255, 255, 255))
    
    # Generate synthetic shadow
    synthetic = create_synthetic_shadow(vehicle_mask, (width, height))
    
    # Create shadow layer
    shadow_layer = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    shadow_array = np.array(shadow_layer)
    shadow_array[:, :, 0] = 0  # R
    shadow_array[:, :, 1] = 0  # G  
    shadow_array[:, :, 2] = 0  # B
    shadow_array[:, :, 3] = synthetic  # A
    shadow_layer = Image.fromarray(shadow_array)
    
    # Paste shadow
    result.paste(Image.new('RGB', (width, height), (0, 0, 0)), 
                 (padding, padding + 5), shadow_layer)
    
    # Create vehicle layer
    vehicle_alpha = create_soft_alpha(vehicle_mask)
    vehicle_layer = original.convert('RGBA')
    vehicle_layer.putalpha(Image.fromarray(vehicle_alpha))
    
    # Paste vehicle
    result.paste(vehicle_layer, (padding, padding), vehicle_layer)
    
    return result


# =============================================================================
# MAIN PROCESSING
# =============================================================================

def process_image(image_path: Path, sam_predictor) -> dict:
    """Process a single image: vehicle + shadow preservation."""
    results = {}
    
    try:
        img = Image.open(image_path)
        image_size = img.size
        width, height = image_size
        
        # Step 1: Detect vehicles with YOLO
        print(f"  → Detecting vehicles...")
        vehicles = detect_with_yolo(image_path)
        
        if not vehicles:
            print(f"    ⚠️ No vehicles detected")
            results['success'] = False
            results['error'] = "No vehicles detected"
            return results
        
        main_vehicle = vehicles[0]
        print(f"    ✓ Main: {main_vehicle['class_name']} (conf={main_vehicle['confidence']:.2f})")
        
        # Step 2: Expand box for vehicle
        vehicle_box = main_vehicle['box']
        expanded_box = expand_box_for_vehicle(vehicle_box, image_size)
        
        # Step 3: Get shadow box
        shadow_box = get_shadow_box(expanded_box, image_size)
        print(f"  → Shadow zone: y={shadow_box[1]}-{shadow_box[3]}")
        
        # Step 4: Segment vehicle with SAM
        print(f"  → Segmenting vehicle...")
        vehicle_mask = segment_vehicle_with_sam(image_path, expanded_box, sam_predictor)
        vehicle_mask = refine_vehicle_mask(vehicle_mask)
        
        # Step 5: Segment/detect shadow
        print(f"  → Detecting shadow...")
        
        # Try SAM-based shadow detection
        sam_shadow = segment_shadow_with_sam(
            image_path, shadow_box, vehicle_mask, sam_predictor
        )
        
        # Also try darkness-based detection
        dark_shadow = detect_shadow_by_darkness(
            image_path, vehicle_box, vehicle_mask
        )
        
        # Combine both shadow detections
        combined_shadow = sam_shadow | dark_shadow
        
        # Check if we found any shadow
        shadow_pixels = combined_shadow.sum()
        has_natural_shadow = shadow_pixels > (width * height * 0.005)  # At least 0.5% of image
        
        if has_natural_shadow:
            print(f"    ✓ Natural shadow detected ({shadow_pixels} pixels)")
            shadow_mask = combined_shadow
        else:
            print(f"    ⚠️ No natural shadow, using synthetic")
            shadow_mask = np.zeros_like(vehicle_mask)
        
        # Step 6: Save debug images
        save_debug_image(image_path, vehicle_box, expanded_box, shadow_box,
                        vehicle_mask, shadow_mask if has_natural_shadow else np.zeros_like(vehicle_mask))
        
        # Step 7: Create outputs
        print(f"  → Creating outputs...")
        stem = image_path.stem
        original = Image.open(image_path).convert('RGB')
        
        # 7a: Transparent (just vehicle, no shadow)
        vehicle_alpha = create_soft_alpha(vehicle_mask)
        transparent = create_transparent_output(original, vehicle_alpha)
        transparent_path = OUTPUT_DIR / "transparent" / f"{stem}_transparent.png"
        transparent.save(transparent_path, 'PNG', optimize=True)
        results['transparent'] = transparent_path
        
        # 7b: With natural shadow (if detected)
        if has_natural_shadow:
            shadow_alpha = create_shadow_alpha(shadow_mask)
            with_shadow = create_with_shadow_output(original, vehicle_mask, shadow_alpha)
            shadow_path = OUTPUT_DIR / "with_shadow" / f"{stem}_with_shadow.png"
            with_shadow.save(shadow_path, 'PNG', optimize=True)
            results['with_shadow'] = shadow_path
            
            # White background with shadow
            white_shadow = create_white_with_shadow(with_shadow)
            white_shadow_path = OUTPUT_DIR / "white_shadow" / f"{stem}_white_shadow.jpg"
            white_shadow.save(white_shadow_path, 'JPEG', quality=95)
            results['white_shadow'] = white_shadow_path
        
        # 7c: Synthetic shadow (always generate as fallback/option)
        synthetic = create_synthetic_shadow_output(original, vehicle_mask)
        synthetic_path = OUTPUT_DIR / "synthetic_shadow" / f"{stem}_synthetic.png"
        synthetic.save(synthetic_path, 'PNG', optimize=True)
        results['synthetic_shadow'] = synthetic_path
        
        results['success'] = True
        results['has_natural_shadow'] = has_natural_shadow
        
    except Exception as e:
        results['success'] = False
        results['error'] = str(e)
        import traceback
        traceback.print_exc()
    
    return results


def main():
    print("=" * 70)
    print("🚗 Background Removal V9 - Vehicle + Shadow Preservation")
    print("=" * 70)
    print()
    print("Features:")
    print("  ✓ Preserves natural shadow from original image")
    print("  ✓ SAM + darkness detection for shadow")
    print("  ✓ Semi-transparent shadow layer")
    print("  ✓ Synthetic shadow fallback")
    print()
    print("Outputs:")
    print("  📁 transparent/     - Vehicle only (no shadow)")
    print("  📁 with_shadow/     - Vehicle + natural shadow (PNG)")
    print("  📁 white_shadow/    - White BG + vehicle + shadow (JPG)")
    print("  📁 synthetic_shadow/- Synthetic professional shadow")
    print()
    print("=" * 70)
    
    setup_directories()
    
    # Check SAM model
    if not SAM_CHECKPOINT.exists():
        print(f"\n❌ SAM model not found at {SAM_CHECKPOINT}")
        print("Download from: https://dl.fbaipublicfiles.com/segment_anything/sam_vit_h_4b8939.pth")
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
    with_natural_shadow = 0
    
    for i, image_path in enumerate(sorted(images), 1):
        print(f"[{i}/{len(images)}] {image_path.name}")
        img_start = time.time()
        
        results = process_image(image_path, sam_predictor)
        
        img_time = time.time() - img_start
        
        if results['success']:
            shadow_status = "🌓 natural shadow" if results.get('has_natural_shadow') else "⚪ synthetic shadow"
            print(f"  ✅ Done ({shadow_status}) in {img_time:.1f}s\n")
            successful += 1
            if results.get('has_natural_shadow'):
                with_natural_shadow += 1
        else:
            print(f"  ❌ Failed: {results.get('error', 'Unknown')}\n")
    
    total_time = time.time() - start_time
    
    print("=" * 70)
    print("📊 SUMMARY")
    print("=" * 70)
    print(f"✅ Successful: {successful}/{len(images)}")
    print(f"🌓 Natural shadow preserved: {with_natural_shadow}/{successful}")
    print(f"⚪ Synthetic shadow used: {successful - with_natural_shadow}/{successful}")
    print(f"⏱️  Total time: {total_time:.1f}s ({total_time/len(images):.1f}s avg)")
    print(f"\n📂 Output: {OUTPUT_DIR}/")
    print(f"   ├── transparent/      (PNG, vehicle only)")
    print(f"   ├── with_shadow/      (PNG, vehicle + natural shadow)")
    print(f"   ├── white_shadow/     (JPG, white bg + shadow)")
    print(f"   ├── synthetic_shadow/ (PNG, professional shadow)")
    print(f"   └── debug/            (visualization)")
    print("=" * 70)


if __name__ == "__main__":
    main()
