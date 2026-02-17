#!/usr/bin/env python3
"""
Remove Background V11 - YOLO + SAM + IC-Light Shadows
======================================================
Combina lo mejor de V7 (detección precisa) con V10 (sombras realistas).

Base: V7 (YOLO + SAM)
  - YOLO detecta vehículos (main vs background)
  - SAM segmenta con precisión pixel-perfect
  - Excluye automáticamente vehículos de fondo

+ Mejoras de V10 (Sombras IC-Light):
  - IC-Light genera sombras fotorrealistas (si disponible)
  - Sombra sintética profesional como fallback
  - Múltiples outputs con diferentes estilos de sombra

Outputs:
  📁 transparent/      - Vehículo sin fondo (PNG)
  📁 with_shadow/      - Vehículo + sombra realista (PNG)
  📁 white_shadow/     - Fondo blanco + sombra (JPG)
  📁 composite/        - Listo para usar con padding (JPG)
  📁 debug/            - Visualización de detección

Usage:
    python remove_background_v11.py
"""

import os
import sys
import time
from pathlib import Path
from PIL import Image, ImageFilter, ImageDraw
import numpy as np
from scipy.ndimage import binary_fill_holes, binary_dilation, binary_erosion, gaussian_filter
from scipy.ndimage import distance_transform_edt
import torch

# Directories
INPUT_DIR = Path("./input")
OUTPUT_DIR = Path("./output_v11")

# Model paths
SAM_CHECKPOINT = Path("./sam_vit_h_4b8939.pth")

# Shadow settings
SHADOW_STRENGTH = 0.7
SHADOW_BLUR = 10


def setup_directories():
    """Create output directories."""
    OUTPUT_DIR.mkdir(exist_ok=True)
    (OUTPUT_DIR / "transparent").mkdir(exist_ok=True)
    (OUTPUT_DIR / "with_shadow").mkdir(exist_ok=True)
    (OUTPUT_DIR / "white_shadow").mkdir(exist_ok=True)
    (OUTPUT_DIR / "composite").mkdir(exist_ok=True)
    (OUTPUT_DIR / "debug").mkdir(exist_ok=True)


# =============================================================================
# YOLO DETECTION (from V7)
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


# =============================================================================
# SAM SEGMENTATION (from V7)
# =============================================================================

def load_sam_model():
    """Load SAM model."""
    from segment_anything import sam_model_registry, SamPredictor
    
    if not SAM_CHECKPOINT.exists():
        raise FileNotFoundError(f"SAM checkpoint not found at {SAM_CHECKPOINT}")
    
    device = "cuda" if torch.cuda.is_available() else "cpu"
    sam = sam_model_registry["vit_h"](checkpoint=str(SAM_CHECKPOINT))
    sam.to(device=device)
    
    predictor = SamPredictor(sam)
    return predictor


def segment_with_sam(image_path: Path, box: list, sam_predictor) -> np.ndarray:
    """Use SAM to segment the vehicle using the bounding box as prompt."""
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


# =============================================================================
# MASK POST-PROCESSING (from V7)
# =============================================================================

def refine_mask(mask: np.ndarray) -> np.ndarray:
    """Refine the SAM mask with morphological operations."""
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


# =============================================================================
# IC-LIGHT SHADOW GENERATION (from V10)
# =============================================================================

class ICLightShadowGenerator:
    """
    Uses IC-Light to generate realistic shadows.
    Falls back to synthetic shadow if IC-Light is not available.
    """
    
    def __init__(self):
        self.pipe = None
        self.device = "cuda" if torch.cuda.is_available() else "cpu"
        self.available = False
    
    def load_model(self):
        """Attempt to load IC-Light model."""
        try:
            from diffusers import StableDiffusionPipeline, DPMSolverMultistepScheduler
            
            print("    Loading Stable Diffusion for relighting...")
            
            self.pipe = StableDiffusionPipeline.from_pretrained(
                "runwayml/stable-diffusion-v1-5",
                torch_dtype=torch.float16 if self.device == "cuda" else torch.float32,
                safety_checker=None,
            )
            
            # Check for IC-Light weights
            iclight_path = Path("./iclight_sd15_fc.safetensors")
            if iclight_path.exists():
                from safetensors.torch import load_file
                state_dict = load_file(str(iclight_path))
                self.pipe.unet.load_state_dict(state_dict, strict=False)
                print("    ✓ IC-Light weights loaded")
            else:
                print("    ⚠️ IC-Light weights not found, using base SD1.5")
            
            self.pipe.scheduler = DPMSolverMultistepScheduler.from_config(
                self.pipe.scheduler.config
            )
            self.pipe = self.pipe.to(self.device)
            
            if self.device == "cuda":
                self.pipe.enable_attention_slicing()
            
            self.available = True
            print("    ✓ Shadow generator ready")
            
        except Exception as e:
            print(f"    ⚠️ IC-Light not available: {e}")
            self.available = False
    
    def generate_shadow(self, vehicle_image: Image.Image, 
                        mask: np.ndarray) -> Image.Image:
        """Generate realistic shadow."""
        if self.available and self.pipe is not None:
            try:
                return self._generate_iclight_shadow(vehicle_image, mask)
            except Exception as e:
                print(f"    ⚠️ IC-Light failed: {e}, using synthetic")
        
        return self._generate_synthetic_shadow(vehicle_image, mask)
    
    def _generate_iclight_shadow(self, vehicle_image: Image.Image,
                                  mask: np.ndarray) -> Image.Image:
        """Generate shadow using IC-Light."""
        width, height = vehicle_image.size
        
        # Prepare foreground
        vehicle_rgba = vehicle_image.convert('RGBA')
        alpha = create_soft_alpha(mask)
        vehicle_rgba.putalpha(Image.fromarray(alpha))
        
        # Create gray floor background
        bg_color = (200, 200, 200)
        background = Image.new('RGB', (width, height), bg_color)
        
        # Composite
        composite = background.copy()
        composite.paste(vehicle_rgba, (0, 0), vehicle_rgba)
        
        # Resize for processing
        process_size = 512
        composite_resized = composite.resize((process_size, process_size), Image.LANCZOS)
        
        # Generate with lighting prompt
        prompt = "professional car photography, studio lighting from above, soft shadow on floor, photorealistic"
        negative_prompt = "harsh shadows, multiple shadows, dark"
        
        with torch.inference_mode():
            result = self.pipe(
                prompt=prompt,
                negative_prompt=negative_prompt,
                image=composite_resized,
                strength=0.3,
                num_inference_steps=20,
                guidance_scale=7.5,
            ).images[0]
        
        result = result.resize((width, height), Image.LANCZOS)
        
        # Extract shadow
        shadow = self._extract_shadow(result, bg_color, mask)
        
        return self._combine_vehicle_and_shadow(vehicle_image, mask, shadow)
    
    def _extract_shadow(self, relit_image: Image.Image, bg_color: tuple, 
                        mask: np.ndarray) -> np.ndarray:
        """Extract shadow from relit image."""
        relit_array = np.array(relit_image).astype(np.float32)
        
        bg_brightness = sum(bg_color) / 3
        relit_brightness = np.mean(relit_array, axis=2)
        
        darkness = bg_brightness - relit_brightness
        darkness = np.maximum(0, darkness)
        
        if darkness.max() > 0:
            darkness = darkness / darkness.max()
        
        # Remove vehicle area
        darkness[mask] = 0
        
        # Keep only shadow below vehicle
        vehicle_rows = np.where(mask.any(axis=1))[0]
        if len(vehicle_rows) > 0:
            vehicle_bottom = vehicle_rows[-1]
            shadow_start = max(0, vehicle_bottom - int(mask.shape[0] * 0.05))
            darkness[:shadow_start, :] = 0
        
        darkness = gaussian_filter(darkness, sigma=8)
        
        return (darkness * 255 * SHADOW_STRENGTH).astype(np.uint8)
    
    def _generate_synthetic_shadow(self, vehicle_image: Image.Image,
                                    mask: np.ndarray) -> Image.Image:
        """Generate professional synthetic shadow (fallback)."""
        height, width = mask.shape
        
        # Find vehicle bounds
        rows = np.where(mask.any(axis=1))[0]
        cols = np.where(mask.any(axis=0))[0]
        
        if len(rows) == 0 or len(cols) == 0:
            vehicle_rgba = vehicle_image.convert('RGBA')
            alpha = create_soft_alpha(mask)
            vehicle_rgba.putalpha(Image.fromarray(alpha))
            return vehicle_rgba
        
        bottom_y = rows[-1]
        left_x, right_x = cols[0], cols[-1]
        vehicle_width = right_x - left_x
        
        shadow = np.zeros((height, width), dtype=np.float32)
        
        # 1. Contact shadow (tight, dark line under vehicle)
        contact_height = max(8, int(vehicle_width * 0.02))
        for dy in range(contact_height):
            y = bottom_y + dy
            if y >= height:
                break
            
            intensity = 0.85 * (1 - dy / contact_height) ** 0.5
            shrink = 1 - (dy / contact_height) * 0.1
            cx = (left_x + right_x) // 2
            hw = int((vehicle_width / 2) * shrink)
            
            x1 = max(0, cx - hw)
            x2 = min(width, cx + hw)
            
            for x in range(x1, x2):
                edge_dist = min(x - x1, x2 - x) / max(1, (x2 - x1) / 2)
                edge_factor = min(1, edge_dist * 2)
                shadow[y, x] = max(shadow[y, x], intensity * edge_factor)
        
        # 2. Spread shadow (elliptical ground shadow)
        spread_height = max(40, int(vehicle_width * 0.12))
        shadow_center_x = (left_x + right_x) // 2
        shadow_center_y = bottom_y + contact_height + spread_height // 3
        
        for y in range(bottom_y, min(height, bottom_y + spread_height)):
            for x in range(max(0, left_x - 50), min(width, right_x + 50)):
                dx = (x - shadow_center_x) / (vehicle_width * 0.5)
                dy_norm = (y - shadow_center_y) / (spread_height * 0.5)
                dist = dx**2 + dy_norm**2
                
                if dist < 1:
                    intensity = (1 - dist) ** 1.5 * 0.4
                    shadow[y, x] = max(shadow[y, x], intensity)
        
        # 3. Ambient occlusion
        ao_distance = distance_transform_edt(~mask)
        y_coords = np.arange(height).reshape(-1, 1)
        ao_y_threshold = (rows[0] + bottom_y) // 2
        ao_mask = (ao_distance > 0) & (ao_distance < 30) & (y_coords >= ao_y_threshold)
        
        shadow[ao_mask] = np.maximum(
            shadow[ao_mask],
            (1 - ao_distance[ao_mask] / 30) * 0.2
        )
        
        # Blur for softness
        shadow = gaussian_filter(shadow, sigma=SHADOW_BLUR)
        
        # Remove shadow from vehicle
        shadow[mask] = 0
        
        shadow_alpha = (shadow * 255 * SHADOW_STRENGTH).astype(np.uint8)
        
        return self._combine_vehicle_and_shadow(vehicle_image, mask, shadow_alpha)
    
    def _combine_vehicle_and_shadow(self, vehicle_image: Image.Image,
                                     mask: np.ndarray,
                                     shadow_alpha: np.ndarray) -> Image.Image:
        """Combine vehicle and shadow into single RGBA image."""
        height, width = mask.shape
        
        # Create shadow layer (dark with blue tint)
        shadow_layer = np.zeros((height, width, 4), dtype=np.uint8)
        shadow_layer[:, :, 0] = 35   # R
        shadow_layer[:, :, 1] = 35   # G
        shadow_layer[:, :, 2] = 45   # B (slight blue for realism)
        shadow_layer[:, :, 3] = shadow_alpha
        
        # Create vehicle layer
        vehicle_rgba = vehicle_image.convert('RGBA')
        vehicle_alpha = create_soft_alpha(mask)
        vehicle_rgba.putalpha(Image.fromarray(vehicle_alpha))
        vehicle_array = np.array(vehicle_rgba)
        
        # Alpha compositing
        vehicle_alpha_float = vehicle_array[:, :, 3:4].astype(np.float32) / 255
        shadow_alpha_float = shadow_layer[:, :, 3:4].astype(np.float32) / 255
        
        combined_alpha = vehicle_alpha_float + shadow_alpha_float * (1 - vehicle_alpha_float)
        combined_alpha_safe = np.maximum(combined_alpha, 0.001)
        
        result_rgb = (
            vehicle_array[:, :, :3].astype(np.float32) * vehicle_alpha_float +
            shadow_layer[:, :, :3].astype(np.float32) * shadow_alpha_float * (1 - vehicle_alpha_float)
        ) / combined_alpha_safe
        
        result_array = np.zeros((height, width, 4), dtype=np.uint8)
        result_array[:, :, :3] = np.clip(result_rgb, 0, 255).astype(np.uint8)
        result_array[:, :, 3] = (combined_alpha[:, :, 0] * 255).astype(np.uint8)
        
        return Image.fromarray(result_array)


# =============================================================================
# SIMPLE PROFESSIONAL SHADOW (fast fallback, no ML)
# =============================================================================

def create_simple_professional_shadow(vehicle_image: Image.Image,
                                       mask: np.ndarray) -> Image.Image:
    """Create professional shadow without any ML models."""
    height, width = mask.shape
    
    vehicle_alpha = create_soft_alpha(mask)
    
    rows = np.where(mask.any(axis=1))[0]
    cols = np.where(mask.any(axis=0))[0]
    
    if len(rows) == 0:
        vehicle_rgba = vehicle_image.convert('RGBA')
        vehicle_rgba.putalpha(Image.fromarray(vehicle_alpha))
        return vehicle_rgba
    
    bottom_y = rows[-1]
    left_x, right_x = cols[0], cols[-1]
    vehicle_width = right_x - left_x
    
    shadow = np.zeros((height, width), dtype=np.float32)
    
    # Contact shadow
    contact_height = max(10, int(vehicle_width * 0.025))
    for dy in range(contact_height):
        y = bottom_y + dy
        if y >= height:
            break
        
        intensity = 0.9 * (1 - dy / contact_height) ** 0.5
        shrink = 1 - (dy / contact_height) * 0.15
        cx = (left_x + right_x) // 2
        hw = int((vehicle_width / 2) * shrink)
        
        x1 = max(0, cx - hw)
        x2 = min(width, cx + hw)
        
        for x in range(x1, x2):
            edge_dist = min(x - x1, x2 - x) / max(1, (x2 - x1) / 2)
            edge_factor = min(1, edge_dist * 2.5)
            shadow[y, x] = max(shadow[y, x], intensity * edge_factor)
    
    # Spread shadow
    spread_height = max(50, int(vehicle_width * 0.15))
    shadow_cx = (left_x + right_x) // 2
    shadow_cy = bottom_y + contact_height + spread_height // 3
    
    for y in range(bottom_y, min(height, bottom_y + spread_height)):
        for x in range(max(0, left_x - 60), min(width, right_x + 60)):
            dx = (x - shadow_cx) / (vehicle_width * 0.55)
            dy = (y - shadow_cy) / (spread_height * 0.5)
            dist = dx**2 + dy**2
            
            if dist < 1:
                intensity = (1 - dist) ** 1.8 * 0.35
                shadow[y, x] = max(shadow[y, x], intensity)
    
    shadow = gaussian_filter(shadow, sigma=12)
    shadow[mask] = 0
    
    shadow_alpha = (shadow * 255 * 0.7).astype(np.uint8)
    
    # Create result
    result = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    result_array = np.array(result)
    
    result_array[:, :, 0] = 30
    result_array[:, :, 1] = 30
    result_array[:, :, 2] = 40
    result_array[:, :, 3] = shadow_alpha
    
    shadow_image = Image.fromarray(result_array)
    
    vehicle_rgba = vehicle_image.convert('RGBA')
    vehicle_rgba.putalpha(Image.fromarray(vehicle_alpha))
    
    result = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    result.paste(shadow_image, (0, 0), shadow_image)
    result.paste(vehicle_rgba, (0, 0), vehicle_rgba)
    
    return result


# =============================================================================
# DEBUG VISUALIZATION (from V7)
# =============================================================================

def save_debug_image(image_path: Path, main_vehicle: dict, background_vehicles: list):
    """Save debug image with detection boxes."""
    img = Image.open(image_path).convert('RGB')
    draw = ImageDraw.Draw(img)
    
    # Background vehicles in RED
    for v in background_vehicles:
        x1, y1, x2, y2 = v['box']
        draw.rectangle([x1, y1, x2, y2], outline='red', width=3)
        label = f"BG: {v.get('class_name', 'vehicle')} ({v['confidence']:.2f})"
        draw.text((x1, y1 - 18), label, fill='red')
    
    # Main vehicle in GREEN
    if main_vehicle:
        x1, y1, x2, y2 = main_vehicle['box']
        draw.rectangle([x1, y1, x2, y2], outline='lime', width=5)
        label = f"MAIN: {main_vehicle.get('class_name', 'vehicle')} ({main_vehicle['confidence']:.2f})"
        draw.text((x1, y1 - 22), label, fill='lime')
    
    debug_path = OUTPUT_DIR / "debug" / f"{image_path.stem}_detection.jpg"
    img.save(debug_path, quality=90)


def create_white_background(rgba_image: Image.Image) -> Image.Image:
    """Composite on white background."""
    white = Image.new('RGBA', rgba_image.size, (255, 255, 255, 255))
    white.paste(rgba_image, (0, 0), rgba_image)
    return white.convert('RGB')


# =============================================================================
# MAIN PROCESSING
# =============================================================================

def process_image(image_path: Path, sam_predictor, shadow_generator) -> dict:
    """Process single image with YOLO + SAM + realistic shadow."""
    results = {}
    
    try:
        img = Image.open(image_path)
        image_size = img.size
        
        # Step 1: Detect vehicles with YOLO (from V7)
        print(f"  → Detecting vehicles...")
        vehicles = detect_with_yolo(image_path)
        
        if not vehicles:
            results['success'] = False
            results['error'] = "No vehicles detected"
            return results
        
        main_vehicle = vehicles[0]
        background_vehicles = vehicles[1:]
        
        print(f"    ✓ Main: {main_vehicle.get('class_name', 'vehicle')} (conf={main_vehicle['confidence']:.2f})")
        
        if background_vehicles:
            print(f"    🔴 Background: {len(background_vehicles)} (will be excluded)")
        
        # Save debug
        save_debug_image(image_path, main_vehicle, background_vehicles)
        
        # Step 2: Segment main vehicle with SAM (from V7)
        print(f"  → Segmenting with SAM...")
        mask = segment_with_sam(image_path, main_vehicle['box'], sam_predictor)
        mask = refine_mask(mask)
        
        # Step 3: Create outputs
        print(f"  → Creating outputs...")
        original = Image.open(image_path).convert('RGB')
        stem = image_path.stem
        
        # 3a: Transparent (no shadow)
        vehicle_alpha = create_soft_alpha(mask)
        transparent = original.convert('RGBA')
        transparent.putalpha(Image.fromarray(vehicle_alpha))
        
        transparent_path = OUTPUT_DIR / "transparent" / f"{stem}_transparent.png"
        transparent.save(transparent_path, 'PNG', optimize=True)
        results['transparent'] = transparent_path
        
        # 3b: Generate realistic shadow (from V10)
        print(f"  → Generating shadow...")
        
        if shadow_generator is not None:
            with_shadow = shadow_generator.generate_shadow(original, mask)
            method = "IC-Light" if shadow_generator.available else "synthetic"
        else:
            with_shadow = create_simple_professional_shadow(original, mask)
            method = "simple"
        
        shadow_path = OUTPUT_DIR / "with_shadow" / f"{stem}_shadow.png"
        with_shadow.save(shadow_path, 'PNG', optimize=True)
        results['shadow'] = shadow_path
        results['shadow_method'] = method
        
        # 3c: White background with shadow
        white_bg = create_white_background(with_shadow)
        white_path = OUTPUT_DIR / "white_shadow" / f"{stem}_white.jpg"
        white_bg.save(white_path, 'JPEG', quality=95)
        results['white_bg'] = white_path
        
        # 3d: Composite with padding (ready to use)
        padding = 60
        composite_size = (image_size[0] + padding * 2, image_size[1] + padding)
        composite = Image.new('RGB', composite_size, (255, 255, 255))
        composite.paste(with_shadow, (padding, padding), with_shadow)
        
        composite_path = OUTPUT_DIR / "composite" / f"{stem}_composite.jpg"
        composite.save(composite_path, 'JPEG', quality=95)
        results['composite'] = composite_path
        
        results['success'] = True
        results['background_count'] = len(background_vehicles)
        
    except Exception as e:
        results['success'] = False
        results['error'] = str(e)
        import traceback
        traceback.print_exc()
    
    return results


def main():
    print("=" * 70)
    print("🚗 Background Removal V11 - YOLO + SAM + IC-Light Shadows")
    print("=" * 70)
    print()
    print("Based on V7 (best detection) + V10 (best shadows)")
    print()
    print("Features:")
    print("  ✓ YOLO detects main vehicle (excludes background vehicles)")
    print("  ✓ SAM provides pixel-perfect segmentation")
    print("  ✓ IC-Light generates photorealistic shadows")
    print("  ✓ Professional synthetic shadow fallback")
    print()
    print("Outputs:")
    print("  📁 transparent/   - Vehicle only (PNG)")
    print("  📁 with_shadow/   - Vehicle + shadow (PNG)")
    print("  📁 white_shadow/  - White BG + shadow (JPG)")
    print("  📁 composite/     - Ready to use (JPG)")
    print("  📁 debug/         - Detection visualization")
    print()
    print("=" * 70)
    
    setup_directories()
    
    # Check SAM model
    if not SAM_CHECKPOINT.exists():
        print(f"\n❌ SAM model not found at {SAM_CHECKPOINT}")
        print("Download: https://dl.fbaipublicfiles.com/segment_anything/sam_vit_h_4b8939.pth")
        sys.exit(1)
    
    # Load SAM
    print("\n📦 Loading SAM model...")
    sam_predictor = load_sam_model()
    print("   ✓ SAM loaded")
    
    # Load shadow generator
    print("\n📦 Loading shadow generator...")
    shadow_generator = ICLightShadowGenerator()
    
    try:
        shadow_generator.load_model()
    except Exception as e:
        print(f"   ⚠️ Advanced shadows not available: {e}")
        print("   Using simple professional shadows")
        shadow_generator = None
    
    print()
    
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
    shadow_methods = {}
    
    for i, image_path in enumerate(sorted(images), 1):
        print(f"[{i}/{len(images)}] {image_path.name}")
        img_start = time.time()
        
        results = process_image(image_path, sam_predictor, shadow_generator)
        
        img_time = time.time() - img_start
        
        if results['success']:
            bg_count = results.get('background_count', 0)
            total_bg_excluded += bg_count
            
            method = results.get('shadow_method', 'unknown')
            shadow_methods[method] = shadow_methods.get(method, 0) + 1
            
            status = f"✅ Done ({method} shadow)"
            if bg_count > 0:
                status += f" [excluded {bg_count} bg]"
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
    print(f"\nShadow methods:")
    for method, count in shadow_methods.items():
        print(f"   {method}: {count}")
    print(f"\n⏱️  Total time: {total_time:.1f}s ({total_time/len(images):.1f}s avg)")
    print(f"\n📂 Output: {OUTPUT_DIR}/")
    print("=" * 70)
    
    # IC-Light setup hint
    if shadow_generator is None or not shadow_generator.available:
        print("\n💡 To enable IC-Light AI shadows:")
        print("   1. pip install diffusers transformers accelerate safetensors")
        print("   2. Download: https://huggingface.co/lllyasviel/ic-light")
        print("   3. Place 'iclight_sd15_fc.safetensors' in workers folder")
        print("=" * 70)


if __name__ == "__main__":
    main()
