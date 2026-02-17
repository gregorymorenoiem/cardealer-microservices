#!/usr/bin/env python3
"""
Remove Background V10 - YOLO + SAM + IC-Light (Realistic Shadows)
=================================================================
Uses IC-Light from Hugging Face to generate photorealistic shadows.

IC-Light is a diffusion model that can relight images with consistent
lighting and shadows. We use it to generate realistic ground shadows.

Strategy:
1. YOLO detects vehicle → SAM segments it
2. Create transparent vehicle image
3. IC-Light generates realistic shadow with top-down lighting
4. Combine: Original vehicle + Generated shadow

Requirements:
    pip install diffusers transformers accelerate torch
    pip install ultralytics segment-anything

Models (auto-downloaded from Hugging Face):
    - lllyasviel/ic-light (IC-Light FC model for foreground relighting)

Usage:
    python remove_background_v10.py
"""

import os
import sys
import time
from pathlib import Path
from PIL import Image, ImageFilter, ImageDraw, ImageOps
import numpy as np
from scipy.ndimage import binary_fill_holes, binary_dilation, binary_erosion, gaussian_filter
from scipy.ndimage import distance_transform_edt
import torch

# Directories
INPUT_DIR = Path("./input")
OUTPUT_DIR = Path("./output_v10")

# Model paths
SAM_CHECKPOINT = Path("./sam_vit_h_4b8939.pth")

# IC-Light settings
ICLIGHT_MODEL = "lllyasviel/ic-light-fbc"  # Foreground-background conditioning
LIGHT_DIRECTION = "top"  # Light from top = shadow below
SHADOW_STRENGTH = 0.7
OUTPUT_SIZE = (1024, 1024)  # IC-Light works best at this size


def setup_directories():
    """Create output directories."""
    OUTPUT_DIR.mkdir(exist_ok=True)
    (OUTPUT_DIR / "transparent").mkdir(exist_ok=True)
    (OUTPUT_DIR / "iclight_shadow").mkdir(exist_ok=True)
    (OUTPUT_DIR / "white_shadow").mkdir(exist_ok=True)
    (OUTPUT_DIR / "composite").mkdir(exist_ok=True)
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
    
    side_expand = int(box_width * 0.25)
    x1 = max(0, x1 - side_expand)
    x2 = min(width, x2 + side_expand)
    
    top_expand = int(box_height * 0.08)
    y1 = max(0, y1 - top_expand)
    
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
    device = "cuda" if torch.cuda.is_available() else "cpu"
    sam.to(device=device)
    
    predictor = SamPredictor(sam)
    return predictor


def segment_vehicle_with_sam(image_path: Path, box: list, sam_predictor) -> np.ndarray:
    """Segment the vehicle using SAM."""
    image = np.array(Image.open(image_path).convert('RGB'))
    
    sam_predictor.set_image(image)
    
    masks, scores, _ = sam_predictor.predict(
        point_coords=None,
        point_labels=None,
        box=np.array(box),
        multimask_output=True
    )
    
    best_idx = np.argmax(scores)
    return masks[best_idx]


def refine_mask(mask: np.ndarray) -> np.ndarray:
    """Refine the mask with morphological operations."""
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
# IC-LIGHT SHADOW GENERATION
# =============================================================================

class ICLightShadowGenerator:
    """
    Uses IC-Light to generate realistic shadows.
    
    IC-Light can relight foreground objects with different lighting conditions.
    We use it to simulate top-down lighting which creates ground shadows.
    """
    
    def __init__(self):
        self.pipe = None
        self.device = "cuda" if torch.cuda.is_available() else "cpu"
        print(f"    Using device: {self.device}")
    
    def load_model(self):
        """Load IC-Light model from Hugging Face."""
        from diffusers import StableDiffusionPipeline, DPMSolverMultistepScheduler
        
        print("    Loading IC-Light model from Hugging Face...")
        print("    (First run will download ~5GB)")
        
        # IC-Light is based on SD 1.5 with custom training
        # We'll use the fbc (foreground-background conditioning) variant
        try:
            # Try loading IC-Light specific model
            self.pipe = StableDiffusionPipeline.from_pretrained(
                "runwayml/stable-diffusion-v1-5",
                torch_dtype=torch.float16 if self.device == "cuda" else torch.float32,
                safety_checker=None,
            )
            
            # Load IC-Light weights if available
            iclight_path = Path("./iclight_sd15_fc.safetensors")
            if iclight_path.exists():
                from safetensors.torch import load_file
                state_dict = load_file(str(iclight_path))
                self.pipe.unet.load_state_dict(state_dict, strict=False)
                print("    ✓ IC-Light weights loaded")
            else:
                print("    ⚠️ IC-Light weights not found, using base SD1.5")
                print(f"    Download from: https://huggingface.co/lllyasviel/ic-light")
                print(f"    Place 'iclight_sd15_fc.safetensors' in {Path.cwd()}")
            
            self.pipe.scheduler = DPMSolverMultistepScheduler.from_config(
                self.pipe.scheduler.config
            )
            self.pipe = self.pipe.to(self.device)
            
            if self.device == "cuda":
                self.pipe.enable_attention_slicing()
            
            print("    ✓ Model loaded")
            
        except Exception as e:
            print(f"    ⚠️ Could not load IC-Light: {e}")
            print("    Falling back to synthetic shadow generation")
            self.pipe = None
    
    def generate_shadow(self, vehicle_image: Image.Image, 
                        mask: np.ndarray) -> Image.Image:
        """
        Generate realistic shadow using IC-Light or fallback.
        
        Args:
            vehicle_image: Original image with vehicle
            mask: Binary mask of vehicle
            
        Returns:
            Image with vehicle + realistic shadow on transparent background
        """
        if self.pipe is None:
            return self._generate_synthetic_shadow(vehicle_image, mask)
        
        try:
            return self._generate_iclight_shadow(vehicle_image, mask)
        except Exception as e:
            print(f"    ⚠️ IC-Light failed: {e}")
            return self._generate_synthetic_shadow(vehicle_image, mask)
    
    def _generate_iclight_shadow(self, vehicle_image: Image.Image,
                                  mask: np.ndarray) -> Image.Image:
        """Generate shadow using IC-Light diffusion model."""
        width, height = vehicle_image.size
        
        # Prepare foreground (vehicle on transparent)
        vehicle_rgba = vehicle_image.convert('RGBA')
        alpha = create_soft_alpha(mask)
        vehicle_rgba.putalpha(Image.fromarray(alpha))
        
        # Create a simple gray floor background for relighting
        bg_color = (200, 200, 200)  # Light gray floor
        background = Image.new('RGB', (width, height), bg_color)
        
        # Composite vehicle on background
        composite = background.copy()
        composite.paste(vehicle_rgba, (0, 0), vehicle_rgba)
        
        # Resize for IC-Light (works best at 512x512 or 1024x1024)
        process_size = 512
        composite_resized = composite.resize((process_size, process_size), Image.LANCZOS)
        
        # Generate with top-down lighting prompt
        prompt = "professional car photography, studio lighting from above, soft shadow on floor, photorealistic, 8k"
        negative_prompt = "harsh shadows, multiple shadows, dark, unrealistic"
        
        # Run IC-Light inference
        with torch.inference_mode():
            result = self.pipe(
                prompt=prompt,
                negative_prompt=negative_prompt,
                image=composite_resized,
                strength=0.3,  # Low strength to preserve vehicle appearance
                num_inference_steps=20,
                guidance_scale=7.5,
            ).images[0]
        
        # Resize back
        result = result.resize((width, height), Image.LANCZOS)
        
        # Extract shadow by comparing with original
        shadow = self._extract_shadow_from_relit(
            result, vehicle_image, mask, bg_color
        )
        
        # Combine: original vehicle + extracted shadow
        final = self._combine_vehicle_and_shadow(vehicle_image, mask, shadow)
        
        return final
    
    def _extract_shadow_from_relit(self, relit_image: Image.Image,
                                    original: Image.Image,
                                    mask: np.ndarray,
                                    bg_color: tuple) -> np.ndarray:
        """Extract shadow from IC-Light output."""
        relit_array = np.array(relit_image).astype(np.float32)
        
        # Shadow is where the relit image is darker than background
        bg_brightness = sum(bg_color) / 3
        relit_brightness = np.mean(relit_array, axis=2)
        
        # Areas darker than background (potential shadow)
        darkness = bg_brightness - relit_brightness
        darkness = np.maximum(0, darkness)
        
        # Normalize
        if darkness.max() > 0:
            darkness = darkness / darkness.max()
        
        # Remove vehicle area from shadow
        darkness[mask] = 0
        
        # Only keep shadow below vehicle
        vehicle_rows = np.where(mask.any(axis=1))[0]
        if len(vehicle_rows) > 0:
            vehicle_bottom = vehicle_rows[-1]
            # Allow some overlap
            shadow_start = max(0, vehicle_bottom - int(mask.shape[0] * 0.05))
            darkness[:shadow_start, :] = 0
        
        # Apply blur for soft shadow edges
        darkness = gaussian_filter(darkness, sigma=8)
        
        return (darkness * 255 * SHADOW_STRENGTH).astype(np.uint8)
    
    def _generate_synthetic_shadow(self, vehicle_image: Image.Image,
                                    mask: np.ndarray) -> Image.Image:
        """Fallback: Generate professional synthetic shadow."""
        height, width = mask.shape
        
        # Find vehicle bounds
        rows_with_content = np.where(mask.any(axis=1))[0]
        cols_with_content = np.where(mask.any(axis=0))[0]
        
        if len(rows_with_content) == 0 or len(cols_with_content) == 0:
            # No vehicle found, return original
            vehicle_rgba = vehicle_image.convert('RGBA')
            alpha = create_soft_alpha(mask)
            vehicle_rgba.putalpha(Image.fromarray(alpha))
            return vehicle_rgba
        
        bottom_y = rows_with_content[-1]
        left_x = cols_with_content[0]
        right_x = cols_with_content[-1]
        vehicle_width = right_x - left_x
        
        # Create shadow array
        shadow = np.zeros((height, width), dtype=np.float32)
        
        # === SHADOW COMPONENTS ===
        
        # 1. Contact shadow (dark line right under vehicle)
        contact_height = 12
        for y in range(bottom_y, min(height, bottom_y + contact_height)):
            cols_at_bottom = np.where(mask[min(bottom_y - 1, y), :])[0]
            if len(cols_at_bottom) > 0:
                intensity = 0.8 * (1 - (y - bottom_y) / contact_height)
                for x in range(cols_at_bottom[0], cols_at_bottom[-1] + 1):
                    shadow[y, x] = max(shadow[y, x], intensity)
        
        # 2. Ground shadow (elliptical, spreads from bottom)
        shadow_center_x = (left_x + right_x) // 2
        shadow_center_y = bottom_y + 25
        shadow_width = vehicle_width * 0.85
        shadow_height = min(80, int(vehicle_width * 0.2))
        
        y_coords, x_coords = np.ogrid[:height, :width]
        
        a = shadow_width / 2
        b = shadow_height / 2
        
        if a > 0 and b > 0:
            ellipse = ((x_coords - shadow_center_x) / a) ** 2 + \
                      ((y_coords - shadow_center_y) / b) ** 2
            
            shadow_gradient = np.maximum(0, 1 - ellipse)
            shadow_gradient = np.power(shadow_gradient, 0.6)
            
            # Add to shadow (don't overwrite contact shadow)
            shadow = np.maximum(shadow, shadow_gradient * 0.5)
        
        # 3. Ambient occlusion (soft darkness around bottom of vehicle)
        ao_distance = distance_transform_edt(~mask)
        ao_mask = (ao_distance > 0) & (ao_distance < 30)
        ao_intensity = (1 - ao_distance[ao_mask] / 30) * 0.3
        
        # Only apply AO below vehicle center
        ao_y_threshold = (rows_with_content[0] + bottom_y) // 2
        ao_region = ao_mask & (y_coords >= ao_y_threshold)
        shadow[ao_region] = np.maximum(
            shadow[ao_region],
            (1 - ao_distance[ao_region] / 30) * 0.25
        )
        
        # Apply gaussian blur for soft edges
        shadow = gaussian_filter(shadow, sigma=10)
        
        # Remove shadow from vehicle area
        shadow[mask] = 0
        
        # Convert to alpha
        shadow_alpha = (shadow * 255 * SHADOW_STRENGTH).astype(np.uint8)
        
        # Create final image with vehicle + shadow
        return self._combine_vehicle_and_shadow(vehicle_image, mask, shadow_alpha)
    
    def _combine_vehicle_and_shadow(self, vehicle_image: Image.Image,
                                     mask: np.ndarray,
                                     shadow_alpha: np.ndarray) -> Image.Image:
        """Combine vehicle and shadow into single RGBA image."""
        height, width = mask.shape
        
        # Create result canvas
        result = Image.new('RGBA', (width, height), (0, 0, 0, 0))
        result_array = np.array(result)
        
        # Add shadow layer (black with alpha)
        shadow_layer = np.zeros((height, width, 4), dtype=np.uint8)
        shadow_layer[:, :, 0] = 40   # Slight blue tint for realistic shadow
        shadow_layer[:, :, 1] = 40
        shadow_layer[:, :, 2] = 50
        shadow_layer[:, :, 3] = shadow_alpha
        
        result_array = shadow_layer
        
        # Add vehicle layer on top
        vehicle_rgba = vehicle_image.convert('RGBA')
        vehicle_alpha = create_soft_alpha(mask)
        vehicle_rgba.putalpha(Image.fromarray(vehicle_alpha))
        
        vehicle_array = np.array(vehicle_rgba)
        
        # Alpha compositing: shadow + vehicle
        # Where vehicle is visible, use vehicle; else use shadow
        vehicle_alpha_float = vehicle_array[:, :, 3:4].astype(np.float32) / 255
        shadow_alpha_float = result_array[:, :, 3:4].astype(np.float32) / 255
        
        # Combined alpha
        combined_alpha = vehicle_alpha_float + shadow_alpha_float * (1 - vehicle_alpha_float)
        
        # Avoid division by zero
        combined_alpha_safe = np.maximum(combined_alpha, 0.001)
        
        # Blend RGB
        result_rgb = (
            vehicle_array[:, :, :3].astype(np.float32) * vehicle_alpha_float +
            result_array[:, :, :3].astype(np.float32) * shadow_alpha_float * (1 - vehicle_alpha_float)
        ) / combined_alpha_safe
        
        result_array[:, :, :3] = np.clip(result_rgb, 0, 255).astype(np.uint8)
        result_array[:, :, 3] = (combined_alpha[:, :, 0] * 255).astype(np.uint8)
        
        return Image.fromarray(result_array)


# =============================================================================
# SIMPLE FALLBACK SHADOW (no ML required)
# =============================================================================

def create_simple_professional_shadow(vehicle_image: Image.Image,
                                       mask: np.ndarray) -> Image.Image:
    """
    Create a simple but professional-looking shadow without any ML models.
    This is the fastest option and works well for most cases.
    """
    height, width = mask.shape
    
    # Vehicle alpha
    vehicle_alpha = create_soft_alpha(mask)
    
    # Find vehicle bounds
    rows = np.where(mask.any(axis=1))[0]
    cols = np.where(mask.any(axis=0))[0]
    
    if len(rows) == 0:
        vehicle_rgba = vehicle_image.convert('RGBA')
        vehicle_rgba.putalpha(Image.fromarray(vehicle_alpha))
        return vehicle_rgba
    
    bottom_y = rows[-1]
    left_x, right_x = cols[0], cols[-1]
    vehicle_width = right_x - left_x
    
    # Create shadow
    shadow = np.zeros((height, width), dtype=np.float32)
    
    # Parameters based on vehicle size
    contact_shadow_height = max(8, int(vehicle_width * 0.02))
    spread_shadow_height = max(40, int(vehicle_width * 0.12))
    
    # 1. Contact shadow (tight, dark)
    for dy in range(contact_shadow_height):
        y = bottom_y + dy
        if y >= height:
            break
        
        # Get vehicle width at this row (approximation)
        intensity = 0.9 * (1 - dy / contact_shadow_height) ** 0.5
        
        # Shrink shadow width slightly as it goes down
        shrink = 1 - (dy / contact_shadow_height) * 0.1
        cx = (left_x + right_x) // 2
        hw = int((vehicle_width / 2) * shrink)
        
        x1 = max(0, cx - hw)
        x2 = min(width, cx + hw)
        
        for x in range(x1, x2):
            # Softer at edges
            edge_dist = min(x - x1, x2 - x) / max(1, (x2 - x1) / 2)
            edge_factor = min(1, edge_dist * 2)
            shadow[y, x] = max(shadow[y, x], intensity * edge_factor)
    
    # 2. Spread shadow (elliptical)
    shadow_center_x = (left_x + right_x) // 2
    shadow_center_y = bottom_y + contact_shadow_height + spread_shadow_height // 3
    
    for y in range(bottom_y, min(height, bottom_y + spread_shadow_height)):
        for x in range(max(0, left_x - 50), min(width, right_x + 50)):
            # Ellipse equation
            dx = (x - shadow_center_x) / (vehicle_width * 0.5)
            dy = (y - shadow_center_y) / (spread_shadow_height * 0.5)
            dist = dx**2 + dy**2
            
            if dist < 1:
                intensity = (1 - dist) ** 1.5 * 0.4
                shadow[y, x] = max(shadow[y, x], intensity)
    
    # Blur for softness
    shadow = gaussian_filter(shadow, sigma=8)
    
    # Remove shadow from vehicle
    shadow[mask] = 0
    
    # Create shadow alpha
    shadow_alpha = (shadow * 255 * 0.7).astype(np.uint8)
    
    # Create result image
    result = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    result_array = np.array(result)
    
    # Shadow layer (dark gray/blue)
    result_array[:, :, 0] = 30
    result_array[:, :, 1] = 30
    result_array[:, :, 2] = 40
    result_array[:, :, 3] = shadow_alpha
    
    # Convert to PIL
    shadow_image = Image.fromarray(result_array)
    
    # Create vehicle layer
    vehicle_rgba = vehicle_image.convert('RGBA')
    vehicle_rgba.putalpha(Image.fromarray(vehicle_alpha))
    
    # Composite shadow + vehicle
    result = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    result.paste(shadow_image, (0, 0), shadow_image)
    result.paste(vehicle_rgba, (0, 0), vehicle_rgba)
    
    return result


# =============================================================================
# DEBUG & OUTPUT
# =============================================================================

def save_debug(image_path: Path, vehicle_box: list, mask: np.ndarray):
    """Save debug visualization."""
    img = Image.open(image_path).convert('RGB')
    draw = ImageDraw.Draw(img)
    
    x1, y1, x2, y2 = vehicle_box
    draw.rectangle([x1, y1, x2, y2], outline='lime', width=3)
    
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
    """Process single image with vehicle segmentation and shadow generation."""
    results = {}
    
    try:
        img = Image.open(image_path)
        image_size = img.size
        
        # Step 1: Detect vehicles
        print(f"  → Detecting vehicles...")
        vehicles = detect_with_yolo(image_path)
        
        if not vehicles:
            results['success'] = False
            results['error'] = "No vehicles detected"
            return results
        
        main_vehicle = vehicles[0]
        print(f"    ✓ {main_vehicle['class_name']} (conf={main_vehicle['confidence']:.2f})")
        
        # Step 2: Expand box and segment
        print(f"  → Segmenting with SAM...")
        expanded_box = expand_box_for_vehicle(main_vehicle['box'], image_size)
        mask = segment_vehicle_with_sam(image_path, expanded_box, sam_predictor)
        mask = refine_mask(mask)
        
        # Save debug
        save_debug(image_path, expanded_box, mask)
        
        # Step 3: Create transparent vehicle (no shadow)
        print(f"  → Creating transparent output...")
        original = Image.open(image_path).convert('RGB')
        vehicle_alpha = create_soft_alpha(mask)
        
        transparent = original.convert('RGBA')
        transparent.putalpha(Image.fromarray(vehicle_alpha))
        
        stem = image_path.stem
        transparent_path = OUTPUT_DIR / "transparent" / f"{stem}_transparent.png"
        transparent.save(transparent_path, 'PNG', optimize=True)
        results['transparent'] = transparent_path
        
        # Step 4: Generate shadow with IC-Light (or fallback)
        print(f"  → Generating realistic shadow...")
        
        if shadow_generator is not None:
            with_shadow = shadow_generator.generate_shadow(original, mask)
            method = "IC-Light" if shadow_generator.pipe is not None else "synthetic"
        else:
            with_shadow = create_simple_professional_shadow(original, mask)
            method = "simple"
        
        shadow_path = OUTPUT_DIR / "iclight_shadow" / f"{stem}_shadow.png"
        with_shadow.save(shadow_path, 'PNG', optimize=True)
        results['shadow'] = shadow_path
        results['shadow_method'] = method
        
        # Step 5: White background version
        white_bg = create_white_background(with_shadow)
        white_path = OUTPUT_DIR / "white_shadow" / f"{stem}_white.jpg"
        white_bg.save(white_path, 'JPEG', quality=95)
        results['white_bg'] = white_path
        
        # Step 6: Create composite with padding
        padding = 60
        composite_size = (image_size[0] + padding * 2, image_size[1] + padding)
        composite = Image.new('RGB', composite_size, (255, 255, 255))
        composite.paste(with_shadow, (padding, padding), with_shadow)
        
        composite_path = OUTPUT_DIR / "composite" / f"{stem}_composite.jpg"
        composite.save(composite_path, 'JPEG', quality=95)
        results['composite'] = composite_path
        
        results['success'] = True
        
    except Exception as e:
        results['success'] = False
        results['error'] = str(e)
        import traceback
        traceback.print_exc()
    
    return results


def main():
    print("=" * 70)
    print("🚗 Background Removal V10 - IC-Light Realistic Shadows")
    print("=" * 70)
    print()
    print("Features:")
    print("  ✓ YOLO vehicle detection")
    print("  ✓ SAM precise segmentation")
    print("  ✓ IC-Light AI-generated shadows (if model available)")
    print("  ✓ Professional synthetic shadow fallback")
    print()
    print("Outputs:")
    print("  📁 transparent/    - Vehicle only (no shadow)")
    print("  📁 iclight_shadow/ - Vehicle + AI shadow (PNG)")
    print("  📁 white_shadow/   - White BG + shadow (JPG)")
    print("  📁 composite/      - Ready for use (JPG)")
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
    
    # Load IC-Light shadow generator
    print("\n📦 Loading shadow generator...")
    shadow_generator = ICLightShadowGenerator()
    
    try:
        shadow_generator.load_model()
    except Exception as e:
        print(f"   ⚠️ IC-Light not available: {e}")
        print("   Will use synthetic shadows")
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
    shadow_methods = {'IC-Light': 0, 'synthetic': 0, 'simple': 0}
    
    for i, image_path in enumerate(sorted(images), 1):
        print(f"[{i}/{len(images)}] {image_path.name}")
        img_start = time.time()
        
        results = process_image(image_path, sam_predictor, shadow_generator)
        
        img_time = time.time() - img_start
        
        if results['success']:
            method = results.get('shadow_method', 'unknown')
            shadow_methods[method] = shadow_methods.get(method, 0) + 1
            print(f"  ✅ Done ({method} shadow) in {img_time:.1f}s\n")
            successful += 1
        else:
            print(f"  ❌ Failed: {results.get('error', 'Unknown')}\n")
    
    total_time = time.time() - start_time
    
    print("=" * 70)
    print("📊 SUMMARY")
    print("=" * 70)
    print(f"✅ Successful: {successful}/{len(images)}")
    print(f"\nShadow methods used:")
    for method, count in shadow_methods.items():
        if count > 0:
            print(f"   {method}: {count}")
    print(f"\n⏱️  Total time: {total_time:.1f}s ({total_time/len(images):.1f}s avg)")
    print(f"\n📂 Output: {OUTPUT_DIR}/")
    print("=" * 70)
    
    # Print IC-Light setup instructions if not available
    if shadow_generator is None or shadow_generator.pipe is None:
        print("\n💡 To enable IC-Light AI shadows:")
        print("   1. pip install diffusers transformers accelerate safetensors")
        print("   2. Download model from: https://huggingface.co/lllyasviel/ic-light")
        print("   3. Place 'iclight_sd15_fc.safetensors' in the workers folder")
        print("=" * 70)


if __name__ == "__main__":
    main()
