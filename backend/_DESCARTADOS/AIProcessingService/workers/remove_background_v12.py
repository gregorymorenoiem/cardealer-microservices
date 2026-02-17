#!/usr/bin/env python3
"""
Remove Background V12 - Studio Shadow (Catalog Style)
======================================================
Genera sombras estilo catálogo/estudio como las de dealers profesionales.

Este tipo de sombra NO es la sombra natural del vehículo - es una sombra
sintética que simula iluminación de estudio profesional.

Características de la sombra de estudio:
  ✓ Contact shadow - Línea sutil debajo del vehículo
  ✓ Difusa/Elíptica - Se expande hacia afuera
  ✓ Gradiente - Más oscura en el centro
  ✓ Bordes suaves - Alto blur gaussiano
  ✓ Semi-transparente - No es negra sólida

Esta es la sombra típica de:
  - Catálogos de concesionarios
  - Fotos de stock de vehículos
  - Marketplaces de autos
  - Publicidad automotriz

Outputs:
  📁 transparent/    - Vehículo sin fondo (PNG)
  📁 studio_shadow/  - Vehículo + sombra de estudio (PNG)
  📁 white_bg/       - Fondo blanco + sombra (JPG)
  📁 catalog/        - Listo para catálogo con padding (JPG)

Usage:
    python remove_background_v12.py
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
OUTPUT_DIR = Path("./output_v12")

# Model paths
SAM_CHECKPOINT = Path("./sam_vit_h_4b8939.pth")

# Studio shadow settings (ajustables)
SHADOW_OPACITY = 0.25          # Opacidad general de la sombra (0.0-1.0)
SHADOW_BLUR = 25               # Radio de blur (más alto = más difusa)
SHADOW_SPREAD = 0.12           # Qué tanto se expande (% del ancho del vehículo)
SHADOW_OFFSET_Y = 5            # Offset vertical en pixels
SHADOW_COLOR = (20, 20, 25)    # Color de la sombra (casi negro con tinte azul)


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
# STUDIO SHADOW GENERATION (El estilo de la foto que mostraste)
# =============================================================================

def create_studio_shadow(mask: np.ndarray, 
                         opacity: float = SHADOW_OPACITY,
                         blur_radius: float = SHADOW_BLUR,
                         spread: float = SHADOW_SPREAD) -> np.ndarray:
    """
    Crea una sombra estilo estudio/catálogo.
    
    Este es el tipo de sombra que se ve en:
    - Catálogos de concesionarios
    - Fotos de stock
    - Marketplaces de autos
    
    Características:
    - Elíptica y difusa
    - Más oscura en el centro
    - Bordes muy suaves
    - Posicionada justo debajo del vehículo
    """
    height, width = mask.shape
    
    # Encontrar los límites del vehículo
    rows = np.where(mask.any(axis=1))[0]
    cols = np.where(mask.any(axis=0))[0]
    
    if len(rows) == 0 or len(cols) == 0:
        return np.zeros((height, width), dtype=np.float32)
    
    vehicle_bottom = rows[-1]
    vehicle_left = cols[0]
    vehicle_right = cols[-1]
    vehicle_width = vehicle_right - vehicle_left
    vehicle_center_x = (vehicle_left + vehicle_right) // 2
    
    # =========================================================================
    # COMPONENTE 1: Contact Shadow (línea de contacto)
    # =========================================================================
    # Esta es la parte más oscura, justo donde el vehículo "toca" el suelo
    
    contact_shadow = np.zeros((height, width), dtype=np.float32)
    contact_height = max(6, int(vehicle_width * 0.015))
    
    for dy in range(contact_height):
        y = vehicle_bottom + dy + SHADOW_OFFSET_Y
        if y >= height:
            break
        
        # Intensidad disminuye con la distancia
        intensity = 0.8 * (1 - (dy / contact_height) ** 0.7)
        
        # El ancho de la sombra de contacto sigue el contorno inferior del vehículo
        # Buscamos los píxeles del vehículo en la fila inferior
        bottom_row = min(vehicle_bottom, height - 1)
        vehicle_cols = np.where(mask[bottom_row, :])[0]
        
        if len(vehicle_cols) > 0:
            left = vehicle_cols[0]
            right = vehicle_cols[-1]
            
            # Pequeño shrink para que no se vea exactamente igual
            shrink = 1 - (dy / contact_height) * 0.05
            center = (left + right) // 2
            half_width = int((right - left) / 2 * shrink)
            
            x1 = max(0, center - half_width)
            x2 = min(width, center + half_width)
            
            for x in range(x1, x2):
                # Falloff en los bordes
                edge_dist = min(x - x1, x2 - x) / max(1, (x2 - x1) / 2)
                edge_factor = min(1.0, edge_dist * 3)
                contact_shadow[y, x] = intensity * edge_factor
    
    # =========================================================================
    # COMPONENTE 2: Spread Shadow (sombra difusa elíptica)
    # =========================================================================
    # Esta es la sombra que se expande hacia afuera
    
    spread_shadow = np.zeros((height, width), dtype=np.float32)
    
    # Dimensiones de la elipse de sombra
    shadow_width = vehicle_width * (1 + spread * 2)
    shadow_height = vehicle_width * spread * 1.5  # Más ancha que alta
    
    # Centro de la elipse (justo debajo del centro del vehículo)
    shadow_center_x = vehicle_center_x
    shadow_center_y = vehicle_bottom + SHADOW_OFFSET_Y + shadow_height * 0.3
    
    # Crear gradiente elíptico
    y_coords, x_coords = np.ogrid[:height, :width]
    
    # Ecuación de la elipse normalizada
    a = shadow_width / 2  # semi-eje horizontal
    b = shadow_height / 2  # semi-eje vertical
    
    if a > 0 and b > 0:
        # Distancia normalizada al centro de la elipse
        ellipse_dist = ((x_coords - shadow_center_x) / a) ** 2 + \
                       ((y_coords - shadow_center_y) / b) ** 2
        
        # Gradiente: 1 en el centro, 0 en el borde
        # Usamos una curva suave para transición natural
        spread_gradient = np.maximum(0, 1 - ellipse_dist)
        spread_gradient = np.power(spread_gradient, 1.2)  # Curva más suave
        
        spread_shadow = spread_gradient * 0.5  # Intensidad base
    
    # =========================================================================
    # COMPONENTE 3: Wheel Shadows (sombras extra debajo de las ruedas)
    # =========================================================================
    # Las ruedas proyectan sombras más intensas
    
    wheel_shadow = np.zeros((height, width), dtype=np.float32)
    
    # Estimar posición de las ruedas (aproximadamente 15% y 85% del ancho)
    wheel_positions = [
        vehicle_left + int(vehicle_width * 0.15),  # Rueda delantera
        vehicle_left + int(vehicle_width * 0.85),  # Rueda trasera
    ]
    
    wheel_radius = int(vehicle_width * 0.06)  # Radio de la sombra de rueda
    
    for wheel_x in wheel_positions:
        wheel_y = vehicle_bottom + SHADOW_OFFSET_Y + 3
        
        for dy in range(-2, wheel_radius):
            for dx in range(-wheel_radius, wheel_radius + 1):
                x = wheel_x + dx
                y = wheel_y + dy
                
                if 0 <= x < width and 0 <= y < height:
                    dist = np.sqrt(dx**2 + (dy * 2)**2) / wheel_radius
                    if dist < 1:
                        intensity = (1 - dist) ** 2 * 0.3
                        wheel_shadow[y, x] = max(wheel_shadow[y, x], intensity)
    
    # =========================================================================
    # COMBINAR TODOS LOS COMPONENTES
    # =========================================================================
    
    combined_shadow = np.maximum(contact_shadow, spread_shadow)
    combined_shadow = np.maximum(combined_shadow, wheel_shadow)
    
    # Aplicar blur gaussiano para suavizar
    combined_shadow = gaussian_filter(combined_shadow, sigma=blur_radius)
    
    # Asegurar que la sombra no entre en el área del vehículo
    combined_shadow[mask] = 0
    
    # Aplicar opacidad global
    combined_shadow = combined_shadow * opacity
    
    return combined_shadow


def apply_studio_shadow(vehicle_image: Image.Image, 
                        mask: np.ndarray) -> Image.Image:
    """
    Aplica la sombra de estudio al vehículo.
    Retorna imagen RGBA con vehículo + sombra sobre fondo transparente.
    """
    height, width = mask.shape
    
    # Crear alpha del vehículo
    vehicle_alpha = create_soft_alpha(mask)
    
    # Generar sombra de estudio
    shadow_intensity = create_studio_shadow(mask)
    
    # Crear imagen RGBA del resultado
    result = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    result_array = np.array(result)
    
    # Capa de sombra (color oscuro con alpha variable)
    shadow_alpha = (shadow_intensity * 255).astype(np.uint8)
    
    # Crear capa de sombra
    shadow_layer = np.zeros((height, width, 4), dtype=np.uint8)
    shadow_layer[:, :, 0] = SHADOW_COLOR[0]  # R
    shadow_layer[:, :, 1] = SHADOW_COLOR[1]  # G
    shadow_layer[:, :, 2] = SHADOW_COLOR[2]  # B
    shadow_layer[:, :, 3] = shadow_alpha      # A
    
    # Capa del vehículo
    vehicle_rgba = vehicle_image.convert('RGBA')
    vehicle_rgba.putalpha(Image.fromarray(vehicle_alpha))
    vehicle_array = np.array(vehicle_rgba)
    
    # Alpha compositing: shadow debajo, vehicle encima
    # Formula: result = vehicle + shadow * (1 - vehicle_alpha)
    
    vehicle_a = vehicle_array[:, :, 3:4].astype(np.float32) / 255
    shadow_a = shadow_layer[:, :, 3:4].astype(np.float32) / 255
    
    # Combined alpha
    out_a = vehicle_a + shadow_a * (1 - vehicle_a)
    out_a_safe = np.maximum(out_a, 0.001)
    
    # Combined RGB
    out_rgb = (vehicle_array[:, :, :3].astype(np.float32) * vehicle_a +
               shadow_layer[:, :, :3].astype(np.float32) * shadow_a * (1 - vehicle_a)) / out_a_safe
    
    result_array[:, :, :3] = np.clip(out_rgb, 0, 255).astype(np.uint8)
    result_array[:, :, 3] = (out_a[:, :, 0] * 255).astype(np.uint8)
    
    return Image.fromarray(result_array)


# =============================================================================
# OUTPUT GENERATION
# =============================================================================

def create_white_background(rgba_image: Image.Image) -> Image.Image:
    """Composite on white background."""
    white = Image.new('RGBA', rgba_image.size, (255, 255, 255, 255))
    white.paste(rgba_image, (0, 0), rgba_image)
    return white.convert('RGB')


def create_catalog_image(rgba_image: Image.Image, 
                         padding: int = 80) -> Image.Image:
    """
    Crea imagen estilo catálogo con padding y fondo blanco.
    Lista para usar en marketplace/catálogo.
    """
    width, height = rgba_image.size
    
    # Nuevo tamaño con padding
    new_width = width + padding * 2
    new_height = height + padding + int(padding * 0.7)  # Más padding abajo para la sombra
    
    # Crear canvas blanco
    catalog = Image.new('RGB', (new_width, new_height), (255, 255, 255))
    
    # Pegar vehículo con sombra
    catalog.paste(rgba_image, (padding, padding), rgba_image)
    
    return catalog


def save_debug_image(image_path: Path, main_vehicle: dict, 
                     background_vehicles: list, mask: np.ndarray):
    """Save debug visualization."""
    img = Image.open(image_path).convert('RGB')
    draw = ImageDraw.Draw(img)
    
    # Background vehicles in red
    for v in background_vehicles:
        x1, y1, x2, y2 = v['box']
        draw.rectangle([x1, y1, x2, y2], outline='red', width=3)
    
    # Main vehicle in green
    if main_vehicle:
        x1, y1, x2, y2 = main_vehicle['box']
        draw.rectangle([x1, y1, x2, y2], outline='lime', width=4)
    
    # Save
    debug_path = OUTPUT_DIR / "debug" / f"{image_path.stem}_detection.jpg"
    img.save(debug_path, quality=90)
    
    # Save mask preview
    mask_preview = Image.fromarray((mask * 255).astype(np.uint8))
    mask_path = OUTPUT_DIR / "debug" / f"{image_path.stem}_mask.png"
    mask_preview.save(mask_path)
    
    # Save shadow preview
    shadow = create_studio_shadow(mask)
    shadow_preview = Image.fromarray((shadow * 255).astype(np.uint8))
    shadow_path = OUTPUT_DIR / "debug" / f"{image_path.stem}_shadow.png"
    shadow_preview.save(shadow_path)


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
        
        # 1. Transparent (no shadow)
        vehicle_alpha = create_soft_alpha(mask)
        transparent = original.convert('RGBA')
        transparent.putalpha(Image.fromarray(vehicle_alpha))
        
        transparent_path = OUTPUT_DIR / "transparent" / f"{stem}_transparent.png"
        transparent.save(transparent_path, 'PNG', optimize=True)
        results['transparent'] = transparent_path
        
        # 2. Studio shadow (vehicle + shadow on transparent)
        with_shadow = apply_studio_shadow(original, mask)
        
        shadow_path = OUTPUT_DIR / "studio_shadow" / f"{stem}_studio.png"
        with_shadow.save(shadow_path, 'PNG', optimize=True)
        results['studio_shadow'] = shadow_path
        
        # 3. White background
        white_bg = create_white_background(with_shadow)
        white_path = OUTPUT_DIR / "white_bg" / f"{stem}_white.jpg"
        white_bg.save(white_path, 'JPEG', quality=95)
        results['white_bg'] = white_path
        
        # 4. Catalog ready (with padding)
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
    print("🚗 Background Removal V12 - Studio Shadow (Catalog Style)")
    print("=" * 70)
    print()
    print("Genera sombras profesionales estilo catálogo de concesionario.")
    print()
    print("Características de la sombra:")
    print("  ✓ Contact shadow - Línea sutil de contacto")
    print("  ✓ Spread shadow  - Elipse difusa debajo")
    print("  ✓ Wheel shadows  - Énfasis en las ruedas")
    print("  ✓ Soft edges     - Bordes muy suaves")
    print()
    print("Outputs:")
    print("  📁 transparent/   - Sin fondo (PNG)")
    print("  📁 studio_shadow/ - Con sombra de estudio (PNG)")
    print("  📁 white_bg/      - Fondo blanco (JPG)")
    print("  📁 catalog/       - Listo para catálogo (JPG)")
    print()
    print("=" * 70)
    
    setup_directories()
    
    # Check SAM
    if not SAM_CHECKPOINT.exists():
        print(f"\n❌ SAM model not found at {SAM_CHECKPOINT}")
        sys.exit(1)
    
    # Load SAM
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
    print()
    print("💡 Ajustar parámetros de sombra en el script:")
    print(f"   SHADOW_OPACITY = {SHADOW_OPACITY}  # 0.0-1.0")
    print(f"   SHADOW_BLUR = {SHADOW_BLUR}     # Más alto = más difusa")
    print(f"   SHADOW_SPREAD = {SHADOW_SPREAD}   # Expansión lateral")
    print("=" * 70)


if __name__ == "__main__":
    main()
