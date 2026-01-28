#!/usr/bin/env python3
"""
Remove Background V12 - Studio Shadow (FIXED)
==============================================
Genera sombras estilo catálogo/estudio como las de dealers profesionales.

FIXES sobre versión anterior:
  ✓ Expande el canvas PRIMERO para dar espacio a la sombra
  ✓ Sombra se genera en el espacio expandido
  ✓ Intensidad y opacidad corregidas
  ✓ Orden correcto de operaciones

Outputs:
  📁 transparent/    - Vehículo sin fondo (PNG)
  📁 studio_shadow/  - Vehículo + sombra de estudio (PNG)
  📁 white_bg/       - Fondo blanco + sombra (JPG)
  📁 catalog/        - Listo para catálogo (JPG)

Usage:
    python remove_background_v12_fixed.py
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
OUTPUT_DIR = Path("./output_v12_fixed")

# Model paths
SAM_CHECKPOINT = Path("./sam_vit_h_4b8939.pth")


def setup_directories():
    """Create output directories."""
    OUTPUT_DIR.mkdir(exist_ok=True)
    (OUTPUT_DIR / "transparent").mkdir(exist_ok=True)
    (OUTPUT_DIR / "studio_shadow").mkdir(exist_ok=True)
    (OUTPUT_DIR / "white_bg").mkdir(exist_ok=True)
    (OUTPUT_DIR / "catalog").mkdir(exist_ok=True)
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


# =============================================================================
# SAM SEGMENTATION
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
    """Refine the mask."""
    mask = binary_fill_holes(mask)
    mask = binary_dilation(mask, iterations=1)
    mask = binary_erosion(mask, iterations=1)
    return mask


def create_soft_alpha(mask: np.ndarray, edge_width: int = 2) -> np.ndarray:
    """Create alpha with soft edges."""
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
# STUDIO SHADOW GENERATION (FIXED VERSION)
# =============================================================================

def create_studio_shadow_image(vehicle_image: Image.Image, 
                                mask: np.ndarray) -> Image.Image:
    """
    Crea imagen con vehículo + sombra de estudio.
    
    ESTRATEGIA CORREGIDA:
    1. Crear canvas expandido (con espacio para sombra)
    2. Generar sombra en el canvas expandido
    3. Poner vehículo encima de la sombra
    
    Returns:
        PIL Image RGBA con vehículo + sombra sobre fondo transparente
    """
    orig_height, orig_width = mask.shape
    
    # Encontrar límites del vehículo
    rows = np.where(mask.any(axis=1))[0]
    cols = np.where(mask.any(axis=0))[0]
    
    if len(rows) == 0 or len(cols) == 0:
        # No vehicle found, return original with alpha
        vehicle_rgba = vehicle_image.convert('RGBA')
        alpha = create_soft_alpha(mask)
        vehicle_rgba.putalpha(Image.fromarray(alpha))
        return vehicle_rgba
    
    vehicle_top = rows[0]
    vehicle_bottom = rows[-1]
    vehicle_left = cols[0]
    vehicle_right = cols[-1]
    vehicle_width = vehicle_right - vehicle_left
    vehicle_height = vehicle_bottom - vehicle_top
    
    # =========================================================================
    # PASO 1: Calcular dimensiones del canvas expandido
    # =========================================================================
    
    # Espacio para sombra proporcional al tamaño del vehículo
    shadow_space_bottom = max(80, int(vehicle_width * 0.15))
    shadow_space_sides = max(40, int(vehicle_width * 0.08))
    
    new_width = orig_width + shadow_space_sides * 2
    new_height = orig_height + shadow_space_bottom
    
    # Offset donde pondremos el vehículo original
    offset_x = shadow_space_sides
    offset_y = 0  # El vehículo va arriba
    
    # =========================================================================
    # PASO 2: Crear la sombra en el canvas expandido
    # =========================================================================
    
    shadow_array = np.zeros((new_height, new_width), dtype=np.float32)
    
    # Posición del bottom del vehículo en el nuevo canvas
    vehicle_bottom_new = vehicle_bottom + offset_y
    vehicle_center_x_new = (vehicle_left + vehicle_right) // 2 + offset_x
    
    # --- CONTACT SHADOW (línea oscura de contacto) ---
    contact_intensity = 0.45  # Intensidad del centro
    contact_height = max(8, int(vehicle_width * 0.02))
    contact_width = int(vehicle_width * 0.92)
    
    for dy in range(contact_height):
        y = vehicle_bottom_new + dy + 2  # +2 de offset
        if y >= new_height:
            break
        
        # Intensidad decrece con distancia vertical
        intensity = contact_intensity * (1 - (dy / contact_height) ** 0.5)
        
        # Ancho decrece ligeramente
        current_width = int(contact_width * (1 - dy / contact_height * 0.1))
        half_w = current_width // 2
        
        x_start = max(0, vehicle_center_x_new - half_w)
        x_end = min(new_width, vehicle_center_x_new + half_w)
        
        for x in range(x_start, x_end):
            # Falloff horizontal suave
            dist_from_center = abs(x - vehicle_center_x_new) / half_w if half_w > 0 else 0
            h_falloff = 1 - dist_from_center ** 2
            shadow_array[y, x] = max(shadow_array[y, x], intensity * h_falloff)
    
    # --- SPREAD SHADOW (elipse difusa) ---
    spread_width = vehicle_width * 1.1
    spread_height = max(50, int(vehicle_width * 0.12))
    spread_center_y = vehicle_bottom_new + contact_height + spread_height * 0.4
    
    # Crear elipse
    y_coords = np.arange(new_height).reshape(-1, 1)
    x_coords = np.arange(new_width).reshape(1, -1)
    
    a = spread_width / 2  # semi-eje X
    b = spread_height / 2  # semi-eje Y
    
    if a > 0 and b > 0:
        # Distancia normalizada a la elipse
        ellipse_dist = ((x_coords - vehicle_center_x_new) / a) ** 2 + \
                       ((y_coords - spread_center_y) / b) ** 2
        
        # Gradiente suave dentro de la elipse
        spread_mask = ellipse_dist < 1
        spread_intensity = np.zeros_like(shadow_array)
        spread_intensity[spread_mask] = (1 - ellipse_dist[spread_mask]) ** 1.5 * 0.25
        
        shadow_array = np.maximum(shadow_array, spread_intensity)
    
    # --- WHEEL SHADOWS (sombras bajo las ruedas) ---
    # Estimamos posición de ruedas
    wheel_positions = [
        vehicle_left + int(vehicle_width * 0.18) + offset_x,  # Delantera
        vehicle_left + int(vehicle_width * 0.82) + offset_x,  # Trasera
    ]
    
    wheel_shadow_width = int(vehicle_width * 0.12)
    wheel_shadow_height = int(vehicle_width * 0.04)
    
    for wx in wheel_positions:
        wy = vehicle_bottom_new + 3
        
        for dy in range(wheel_shadow_height):
            for dx in range(-wheel_shadow_width//2, wheel_shadow_width//2):
                x = wx + dx
                y = wy + dy
                
                if 0 <= x < new_width and 0 <= y < new_height:
                    # Gradiente radial
                    dist_x = abs(dx) / (wheel_shadow_width / 2)
                    dist_y = dy / wheel_shadow_height
                    dist = (dist_x ** 2 + dist_y ** 2) ** 0.5
                    
                    if dist < 1:
                        intensity = (1 - dist) ** 1.5 * 0.35
                        shadow_array[y, x] = max(shadow_array[y, x], intensity)
    
    # --- APLICAR BLUR GAUSSIANO ---
    shadow_array = gaussian_filter(shadow_array, sigma=18)
    
    # =========================================================================
    # PASO 3: Crear imagen final con sombra + vehículo
    # =========================================================================
    
    # Canvas RGBA final
    result = Image.new('RGBA', (new_width, new_height), (0, 0, 0, 0))
    result_array = np.array(result)
    
    # Capa de sombra (negro/azul oscuro con alpha)
    shadow_alpha = (shadow_array * 255).astype(np.uint8)
    
    result_array[:, :, 0] = 25   # R - toque de color
    result_array[:, :, 1] = 25   # G
    result_array[:, :, 2] = 30   # B - ligeramente azulado
    result_array[:, :, 3] = shadow_alpha
    
    result = Image.fromarray(result_array)
    
    # Crear vehículo con alpha
    vehicle_alpha = create_soft_alpha(mask)
    vehicle_rgba = vehicle_image.convert('RGBA')
    vehicle_rgba.putalpha(Image.fromarray(vehicle_alpha))
    
    # Pegar vehículo sobre la sombra
    result.paste(vehicle_rgba, (offset_x, offset_y), vehicle_rgba)
    
    return result


# =============================================================================
# OUTPUT GENERATION
# =============================================================================

def create_white_background(rgba_image: Image.Image) -> Image.Image:
    """Composite on white background."""
    white = Image.new('RGBA', rgba_image.size, (255, 255, 255, 255))
    white.paste(rgba_image, (0, 0), rgba_image)
    return white.convert('RGB')


def create_catalog_image(rgba_image: Image.Image, 
                         padding: int = 60) -> Image.Image:
    """Create catalog-ready image with padding."""
    width, height = rgba_image.size
    
    new_width = width + padding * 2
    new_height = height + padding * 2
    
    catalog = Image.new('RGB', (new_width, new_height), (255, 255, 255))
    catalog.paste(rgba_image, (padding, padding), rgba_image)
    
    return catalog


def save_debug_image(image_path: Path, main_vehicle: dict, 
                     background_vehicles: list, mask: np.ndarray):
    """Save debug visualization."""
    img = Image.open(image_path).convert('RGB')
    draw = ImageDraw.Draw(img)
    
    for v in background_vehicles:
        x1, y1, x2, y2 = v['box']
        draw.rectangle([x1, y1, x2, y2], outline='red', width=3)
    
    if main_vehicle:
        x1, y1, x2, y2 = main_vehicle['box']
        draw.rectangle([x1, y1, x2, y2], outline='lime', width=4)
    
    debug_path = OUTPUT_DIR / "debug" / f"{image_path.stem}_detection.jpg"
    img.save(debug_path, quality=90)
    
    # También guardar preview de la sombra
    mask_img = Image.fromarray((mask * 255).astype(np.uint8))
    mask_path = OUTPUT_DIR / "debug" / f"{image_path.stem}_mask.png"
    mask_img.save(mask_path)


# =============================================================================
# MAIN PROCESSING
# =============================================================================

def process_image(image_path: Path, sam_predictor) -> dict:
    """Process image with studio shadow."""
    results = {}
    
    try:
        img = Image.open(image_path)
        
        # Detect vehicles
        print(f"  → Detecting vehicles...")
        vehicles = detect_with_yolo(image_path)
        
        if not vehicles:
            results['success'] = False
            results['error'] = "No vehicles detected"
            return results
        
        main_vehicle = vehicles[0]
        background_vehicles = vehicles[1:]
        
        print(f"    ✓ {main_vehicle.get('class_name', 'vehicle')} (conf={main_vehicle['confidence']:.2f})")
        
        if background_vehicles:
            print(f"    🔴 Excluding {len(background_vehicles)} background vehicles")
        
        # Segment with SAM
        print(f"  → Segmenting with SAM...")
        mask = segment_with_sam(image_path, main_vehicle['box'], sam_predictor)
        mask = refine_mask(mask)
        
        # Save debug
        save_debug_image(image_path, main_vehicle, background_vehicles, mask)
        
        # Create outputs
        print(f"  → Creating studio shadow...")
        original = Image.open(image_path).convert('RGB')
        stem = image_path.stem
        
        # 1. Transparent (no shadow, original size)
        vehicle_alpha = create_soft_alpha(mask)
        transparent = original.convert('RGBA')
        transparent.putalpha(Image.fromarray(vehicle_alpha))
        
        transparent_path = OUTPUT_DIR / "transparent" / f"{stem}_transparent.png"
        transparent.save(transparent_path, 'PNG', optimize=True)
        results['transparent'] = transparent_path
        
        # 2. Studio shadow (vehicle + shadow, expanded canvas)
        with_shadow = create_studio_shadow_image(original, mask)
        
        shadow_path = OUTPUT_DIR / "studio_shadow" / f"{stem}_studio.png"
        with_shadow.save(shadow_path, 'PNG', optimize=True)
        results['studio_shadow'] = shadow_path
        
        # 3. White background
        white_bg = create_white_background(with_shadow)
        white_path = OUTPUT_DIR / "white_bg" / f"{stem}_white.jpg"
        white_bg.save(white_path, 'JPEG', quality=95)
        results['white_bg'] = white_path
        
        # 4. Catalog ready
        catalog = create_catalog_image(with_shadow)
        catalog_path = OUTPUT_DIR / "catalog" / f"{stem}_catalog.jpg"
        catalog.save(catalog_path, 'JPEG', quality=95)
        results['catalog'] = catalog_path
        
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
    print("🚗 Background Removal V12 - Studio Shadow (FIXED)")
    print("=" * 70)
    print()
    print("Genera sombras profesionales estilo catálogo.")
    print()
    print("FIXES aplicados:")
    print("  ✓ Canvas expandido para dar espacio a la sombra")
    print("  ✓ Sombra generada DEBAJO del vehículo")
    print("  ✓ Intensidad y blur corregidos")
    print("  ✓ Orden correcto: sombra primero, vehículo encima")
    print()
    print("Outputs:")
    print("  📁 transparent/   - Sin fondo (PNG)")
    print("  📁 studio_shadow/ - Con sombra de estudio (PNG)")
    print("  📁 white_bg/      - Fondo blanco (JPG)")
    print("  📁 catalog/       - Listo para catálogo (JPG)")
    print()
    print("=" * 70)
    
    setup_directories()
    
    if not SAM_CHECKPOINT.exists():
        print(f"\n❌ SAM model not found at {SAM_CHECKPOINT}")
        sys.exit(1)
    
    print("\n📦 Loading SAM model...")
    sam_predictor = load_sam_model()
    print("   ✓ SAM loaded\n")
    
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
    
    for i, image_path in enumerate(sorted(images), 1):
        print(f"[{i}/{len(images)}] {image_path.name}")
        img_start = time.time()
        
        results = process_image(image_path, sam_predictor)
        
        img_time = time.time() - img_start
        
        if results['success']:
            bg = results.get('background_count', 0)
            status = "✅ Done"
            if bg > 0:
                status += f" (excluded {bg} bg)"
            print(f"  {status} in {img_time:.1f}s\n")
            successful += 1
        else:
            print(f"  ❌ Failed: {results.get('error')}\n")
    
    total_time = time.time() - start_time
    
    print("=" * 70)
    print("📊 SUMMARY")
    print("=" * 70)
    print(f"✅ Successful: {successful}/{len(images)}")
    print(f"⏱️  Total time: {total_time:.1f}s ({total_time/len(images):.1f}s avg)")
    print(f"\n📂 Output: {OUTPUT_DIR}/")
    print("=" * 70)


if __name__ == "__main__":
    main()
