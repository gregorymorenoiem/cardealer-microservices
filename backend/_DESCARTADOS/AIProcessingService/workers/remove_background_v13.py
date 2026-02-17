#!/usr/bin/env python3
"""
Remove Background V13 - Natural Shadow Detection
=================================================
Detecta la sombra NATURAL de la imagen original y la usa como base
para crear una sombra realista que toque directamente las ruedas.

Estrategia:
  1. Detectar vehículo con YOLO
  2. Segmentar con SAM
  3. DETECTAR SOMBRA NATURAL en la imagen original
     - Buscar áreas oscuras debajo del vehículo
     - Identificar la forma y posición de la sombra
  4. Crear sombra sintética basada en la natural
     - Sombra en CONTACTO con las ruedas (no flotando)
     - Forma similar a la sombra original

Usage:
    python remove_background_v13.py
"""

import os
import sys
import time
from pathlib import Path
from PIL import Image, ImageFilter, ImageDraw
import numpy as np
from scipy.ndimage import binary_fill_holes, binary_dilation, binary_erosion, gaussian_filter
from scipy.ndimage import distance_transform_edt, label
import torch

# Directories
INPUT_DIR = Path("./input")
OUTPUT_DIR = Path("./output_v13")

# Model paths
SAM_CHECKPOINT = Path("./sam_vit_h_4b8939.pth")


def setup_directories():
    """Create output directories."""
    OUTPUT_DIR.mkdir(exist_ok=True)
    (OUTPUT_DIR / "transparent").mkdir(exist_ok=True)
    (OUTPUT_DIR / "with_shadow").mkdir(exist_ok=True)
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
# NATURAL SHADOW DETECTION
# =============================================================================

def detect_natural_shadow(image: np.ndarray, vehicle_mask: np.ndarray) -> dict:
    """
    Detecta la sombra natural en la imagen original.
    
    Busca áreas oscuras debajo del vehículo que representan la sombra natural.
    
    Returns:
        dict con información de la sombra:
        - 'shadow_mask': máscara de la sombra detectada
        - 'ground_line': línea del suelo (y donde las ruedas tocan)
        - 'shadow_intensity': intensidad promedio de la sombra
        - 'wheel_positions': posiciones X estimadas de las ruedas
    """
    height, width = vehicle_mask.shape
    
    # Convertir a escala de grises para detectar oscuridad
    if len(image.shape) == 3:
        gray = np.mean(image, axis=2)
    else:
        gray = image.copy()
    
    # Encontrar el borde inferior del vehículo (ground line)
    rows_with_vehicle = np.where(vehicle_mask.any(axis=1))[0]
    cols_with_vehicle = np.where(vehicle_mask.any(axis=0))[0]
    
    if len(rows_with_vehicle) == 0:
        return None
    
    vehicle_bottom = rows_with_vehicle[-1]
    vehicle_top = rows_with_vehicle[0]
    vehicle_left = cols_with_vehicle[0]
    vehicle_right = cols_with_vehicle[-1]
    vehicle_width = vehicle_right - vehicle_left
    vehicle_height = vehicle_bottom - vehicle_top
    
    # =========================================================================
    # PASO 1: Encontrar la línea del suelo (donde las ruedas tocan)
    # =========================================================================
    
    # Analizar los últimos píxeles del vehículo para encontrar las ruedas
    # Las ruedas son los puntos más bajos del vehículo
    ground_points = []
    
    for x in range(vehicle_left, vehicle_right):
        # Encontrar el punto más bajo del vehículo en esta columna
        col_mask = vehicle_mask[:, x]
        col_rows = np.where(col_mask)[0]
        if len(col_rows) > 0:
            ground_points.append((x, col_rows[-1]))
    
    if not ground_points:
        return None
    
    ground_points = np.array(ground_points)
    ground_line = int(np.max(ground_points[:, 1]))  # El punto más bajo
    
    # =========================================================================
    # PASO 2: Detectar posiciones de las ruedas
    # =========================================================================
    
    # Las ruedas están donde el vehículo toca el suelo (puntos más bajos)
    # Buscar picos en ground_points
    bottom_threshold = ground_line - 5  # 5 pixels de tolerancia
    
    wheel_candidates = ground_points[ground_points[:, 1] >= bottom_threshold]
    
    # Agrupar puntos cercanos para encontrar centros de ruedas
    wheel_positions = []
    if len(wheel_candidates) > 0:
        # Dividir en grupos (ruedas)
        x_coords = wheel_candidates[:, 0]
        
        # Encontrar gaps grandes para separar ruedas
        sorted_x = np.sort(x_coords)
        gaps = np.diff(sorted_x)
        gap_threshold = vehicle_width * 0.15  # Gap mínimo entre ruedas
        
        gap_indices = np.where(gaps > gap_threshold)[0]
        
        # Crear grupos
        groups = []
        start = 0
        for gap_idx in gap_indices:
            groups.append(sorted_x[start:gap_idx + 1])
            start = gap_idx + 1
        groups.append(sorted_x[start:])
        
        # Centro de cada grupo = posición de rueda
        for group in groups:
            if len(group) > 5:  # Mínimo de puntos para considerar rueda
                wheel_positions.append(int(np.mean(group)))
    
    # Si no detectamos ruedas, estimar posiciones típicas
    if len(wheel_positions) < 2:
        wheel_positions = [
            vehicle_left + int(vehicle_width * 0.2),  # Rueda delantera
            vehicle_left + int(vehicle_width * 0.8),  # Rueda trasera
        ]
    
    # =========================================================================
    # PASO 3: Detectar sombra natural debajo del vehículo
    # =========================================================================
    
    # Área de búsqueda de sombra: justo debajo del vehículo
    shadow_search_height = min(100, int(vehicle_height * 0.2))
    shadow_top = ground_line
    shadow_bottom = min(height, ground_line + shadow_search_height)
    
    # Crear máscara de búsqueda (debajo del vehículo)
    search_mask = np.zeros_like(vehicle_mask)
    search_mask[shadow_top:shadow_bottom, vehicle_left:vehicle_right] = True
    
    # NO incluir el vehículo mismo
    search_mask = search_mask & ~vehicle_mask
    
    # Detectar píxeles oscuros en el área de búsqueda
    if np.any(search_mask):
        search_values = gray[search_mask]
        
        # Calcular umbral adaptativo para "oscuro"
        # Comparar con el brillo promedio de la imagen
        overall_brightness = np.mean(gray)
        shadow_threshold = overall_brightness * 0.7  # 70% del brillo promedio
        
        # También considerar el brillo del área de búsqueda
        area_brightness = np.mean(search_values)
        
        # Máscara de sombra: píxeles más oscuros que el umbral
        shadow_mask = np.zeros_like(vehicle_mask)
        dark_pixels = gray < shadow_threshold
        shadow_mask = dark_pixels & search_mask
        
        # Calcular intensidad de la sombra
        if np.any(shadow_mask):
            shadow_intensity = 1 - (np.mean(gray[shadow_mask]) / 255)
        else:
            shadow_intensity = 0.3  # Default
    else:
        shadow_mask = np.zeros_like(vehicle_mask)
        shadow_intensity = 0.3
    
    return {
        'ground_line': ground_line,
        'vehicle_bottom': vehicle_bottom,
        'vehicle_left': vehicle_left,
        'vehicle_right': vehicle_right,
        'vehicle_width': vehicle_width,
        'wheel_positions': wheel_positions,
        'shadow_mask': shadow_mask,
        'shadow_intensity': min(0.5, shadow_intensity),  # Cap at 0.5
        'shadow_search_height': shadow_search_height,
    }


# =============================================================================
# SHADOW GENERATION (Based on Natural Shadow)
# =============================================================================

def create_natural_shadow(vehicle_mask: np.ndarray, 
                          shadow_info: dict,
                          output_height: int) -> np.ndarray:
    """
    Crea una sombra sintética basada en la información de la sombra natural.
    
    La sombra:
    - Toca directamente las ruedas (no flota)
    - Tiene forma elíptica difusa
    - Es más intensa debajo de las ruedas
    - Se desvanece gradualmente hacia afuera
    """
    orig_height, orig_width = vehicle_mask.shape
    
    ground_line = shadow_info['ground_line']
    vehicle_left = shadow_info['vehicle_left']
    vehicle_right = shadow_info['vehicle_right']
    vehicle_width = shadow_info['vehicle_width']
    wheel_positions = shadow_info['wheel_positions']
    base_intensity = shadow_info['shadow_intensity']
    
    # El canvas de salida puede ser más grande
    shadow = np.zeros((output_height, orig_width), dtype=np.float32)
    
    vehicle_center_x = (vehicle_left + vehicle_right) // 2
    
    # =========================================================================
    # CAPA 1: Sombra de contacto (línea fina bajo las ruedas)
    # =========================================================================
    
    contact_y = ground_line  # JUSTO en la línea del suelo
    contact_height = max(3, int(vehicle_width * 0.008))  # Línea muy fina
    contact_intensity = base_intensity * 1.2  # Más oscura en el contacto
    
    for dy in range(contact_height):
        y = contact_y + dy
        if y >= output_height:
            break
        
        # Intensidad decrece rápido
        row_intensity = contact_intensity * (1 - dy / contact_height)
        
        # Ancho basado en el vehículo
        contact_width = int(vehicle_width * 0.9)
        half_w = contact_width // 2
        
        for x in range(vehicle_center_x - half_w, vehicle_center_x + half_w):
            if 0 <= x < orig_width:
                # Falloff horizontal suave
                dist = abs(x - vehicle_center_x) / half_w
                h_falloff = 1 - dist ** 3
                shadow[y, x] = max(shadow[y, x], row_intensity * h_falloff)
    
    # =========================================================================
    # CAPA 2: Sombras intensas bajo las ruedas
    # =========================================================================
    
    wheel_shadow_intensity = base_intensity * 1.0
    wheel_shadow_radius_x = int(vehicle_width * 0.08)
    wheel_shadow_radius_y = int(vehicle_width * 0.025)
    
    for wx in wheel_positions:
        wy = ground_line  # Justo en el suelo
        
        for dy in range(wheel_shadow_radius_y * 2):
            for dx in range(-wheel_shadow_radius_x, wheel_shadow_radius_x):
                x = wx + dx
                y = wy + dy
                
                if 0 <= x < orig_width and 0 <= y < output_height:
                    # Distancia normalizada (elipse)
                    dist_x = abs(dx) / wheel_shadow_radius_x
                    dist_y = dy / (wheel_shadow_radius_y * 2)
                    dist = (dist_x ** 2 + dist_y ** 2) ** 0.5
                    
                    if dist < 1:
                        intensity = wheel_shadow_intensity * (1 - dist) ** 1.5
                        shadow[y, x] = max(shadow[y, x], intensity)
    
    # =========================================================================
    # CAPA 3: Sombra difusa general (elipse grande)
    # =========================================================================
    
    spread_intensity = base_intensity * 0.4
    spread_width = vehicle_width * 0.95
    spread_height = max(20, int(vehicle_width * 0.05))
    spread_center_y = ground_line + spread_height * 0.3
    
    a = spread_width / 2  # Semi-eje X
    b = spread_height    # Semi-eje Y
    
    if a > 0 and b > 0:
        for y in range(ground_line, min(output_height, ground_line + int(spread_height * 2))):
            for x in range(vehicle_left - 20, vehicle_right + 20):
                if 0 <= x < orig_width:
                    # Distancia a la elipse
                    ellipse_dist = ((x - vehicle_center_x) / a) ** 2 + \
                                   ((y - spread_center_y) / b) ** 2
                    
                    if ellipse_dist < 1:
                        intensity = spread_intensity * (1 - ellipse_dist) ** 2
                        shadow[y, x] = max(shadow[y, x], intensity)
    
    # =========================================================================
    # APLICAR BLUR GAUSSIANO
    # =========================================================================
    
    # Blur moderado para suavizar
    shadow = gaussian_filter(shadow, sigma=8)
    
    # Segundo paso de blur más suave para los bordes
    shadow_soft = gaussian_filter(shadow, sigma=15)
    
    # Combinar: más definida en el centro, más difusa en los bordes
    center_mask = shadow > 0.1
    shadow = np.where(center_mask, shadow * 0.7 + shadow_soft * 0.3, shadow_soft)
    
    return shadow


# =============================================================================
# OUTPUT GENERATION
# =============================================================================

def create_image_with_shadow(vehicle_image: Image.Image,
                             vehicle_mask: np.ndarray,
                             shadow_info: dict) -> Image.Image:
    """
    Crea la imagen final con vehículo + sombra natural.
    """
    orig_height, orig_width = vehicle_mask.shape
    
    # Calcular espacio extra para la sombra
    shadow_space = max(50, int(shadow_info['vehicle_width'] * 0.08))
    new_height = orig_height + shadow_space
    
    # Generar sombra
    shadow_array = create_natural_shadow(vehicle_mask, shadow_info, new_height)
    
    # Crear canvas RGBA
    result = Image.new('RGBA', (orig_width, new_height), (0, 0, 0, 0))
    result_array = np.array(result)
    
    # Aplicar sombra (gris oscuro semi-transparente)
    shadow_alpha = (shadow_array * 255).astype(np.uint8)
    result_array[:, :, 0] = 30   # R
    result_array[:, :, 1] = 30   # G  
    result_array[:, :, 2] = 35   # B
    result_array[:, :, 3] = shadow_alpha
    
    result = Image.fromarray(result_array)
    
    # Crear vehículo con alpha
    vehicle_alpha = create_soft_alpha(vehicle_mask)
    vehicle_rgba = vehicle_image.convert('RGBA')
    vehicle_rgba.putalpha(Image.fromarray(vehicle_alpha))
    
    # Pegar vehículo (en la parte superior)
    result.paste(vehicle_rgba, (0, 0), vehicle_rgba)
    
    return result


def create_white_background(rgba_image: Image.Image) -> Image.Image:
    """Composite on white background."""
    white = Image.new('RGBA', rgba_image.size, (255, 255, 255, 255))
    white.paste(rgba_image, (0, 0), rgba_image)
    return white.convert('RGB')


def create_catalog_image(rgba_image: Image.Image, padding: int = 40) -> Image.Image:
    """Create catalog-ready image with padding."""
    width, height = rgba_image.size
    
    new_width = width + padding * 2
    new_height = height + padding * 2
    
    catalog = Image.new('RGB', (new_width, new_height), (255, 255, 255))
    catalog.paste(rgba_image, (padding, padding), rgba_image)
    
    return catalog


def save_debug_images(image_path: Path, 
                      main_vehicle: dict,
                      vehicle_mask: np.ndarray,
                      shadow_info: dict,
                      original: np.ndarray):
    """Save debug visualizations."""
    stem = image_path.stem
    
    # 1. Detection boxes
    img = Image.open(image_path).convert('RGB')
    draw = ImageDraw.Draw(img)
    x1, y1, x2, y2 = main_vehicle['box']
    draw.rectangle([x1, y1, x2, y2], outline='lime', width=4)
    
    # Draw ground line
    ground_line = shadow_info['ground_line']
    draw.line([(0, ground_line), (img.width, ground_line)], fill='cyan', width=2)
    
    # Draw wheel positions
    for wx in shadow_info['wheel_positions']:
        draw.ellipse([wx-10, ground_line-10, wx+10, ground_line+10], 
                     outline='yellow', width=3)
    
    img.save(OUTPUT_DIR / "debug" / f"{stem}_detection.jpg", quality=90)
    
    # 2. Vehicle mask
    mask_img = Image.fromarray((vehicle_mask * 255).astype(np.uint8))
    mask_img.save(OUTPUT_DIR / "debug" / f"{stem}_mask.png")
    
    # 3. Shadow detection
    if shadow_info.get('shadow_mask') is not None:
        shadow_debug = Image.fromarray((shadow_info['shadow_mask'] * 255).astype(np.uint8))
        shadow_debug.save(OUTPUT_DIR / "debug" / f"{stem}_shadow_detected.png")


# =============================================================================
# MAIN PROCESSING
# =============================================================================

def process_image(image_path: Path, sam_predictor) -> dict:
    """Process image with natural shadow detection."""
    results = {}
    
    try:
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
        
        # Load original image
        original = Image.open(image_path).convert('RGB')
        original_array = np.array(original)
        
        # Detect natural shadow
        print(f"  → Detecting natural shadow...")
        shadow_info = detect_natural_shadow(original_array, mask)
        
        if shadow_info is None:
            results['success'] = False
            results['error'] = "Could not detect shadow information"
            return results
        
        print(f"    ✓ Ground line: y={shadow_info['ground_line']}")
        print(f"    ✓ Wheels at x={shadow_info['wheel_positions']}")
        print(f"    ✓ Shadow intensity: {shadow_info['shadow_intensity']:.2f}")
        
        # Save debug images
        save_debug_images(image_path, main_vehicle, mask, shadow_info, original_array)
        
        stem = image_path.stem
        
        # 1. Transparent (no shadow)
        print(f"  → Creating outputs...")
        vehicle_alpha = create_soft_alpha(mask)
        transparent = original.convert('RGBA')
        transparent.putalpha(Image.fromarray(vehicle_alpha))
        
        transparent_path = OUTPUT_DIR / "transparent" / f"{stem}_transparent.png"
        transparent.save(transparent_path, 'PNG', optimize=True)
        results['transparent'] = transparent_path
        
        # 2. With shadow
        with_shadow = create_image_with_shadow(original, mask, shadow_info)
        
        shadow_path = OUTPUT_DIR / "with_shadow" / f"{stem}_shadow.png"
        with_shadow.save(shadow_path, 'PNG', optimize=True)
        results['with_shadow'] = shadow_path
        
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
    print("🚗 Background Removal V13 - Natural Shadow Detection")
    print("=" * 70)
    print()
    print("Detecta la sombra NATURAL de la imagen y la recrea.")
    print()
    print("Características:")
    print("  ✓ Detecta línea del suelo (donde tocan las ruedas)")
    print("  ✓ Identifica posición de las ruedas")
    print("  ✓ Sombra en CONTACTO con el vehículo (no flotando)")
    print("  ✓ Intensidad basada en la sombra original")
    print()
    print("Outputs:")
    print("  📁 transparent/  - Sin fondo (PNG)")
    print("  📁 with_shadow/  - Con sombra natural (PNG)")
    print("  📁 white_bg/     - Fondo blanco (JPG)")
    print("  📁 catalog/      - Para catálogo (JPG)")
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
