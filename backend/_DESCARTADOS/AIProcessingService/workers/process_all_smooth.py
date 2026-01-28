#!/usr/bin/env python3
"""
Procesar TODAS las imágenes con silueta suave y continua
- CORREGIDO: Sin líneas/púas en el contorno
"""

import numpy as np
from PIL import Image
import cv2
import torch
import sys
import os
from pathlib import Path

# Patch torch.load una sola vez
original_load = torch.load
def patched_load(*args, **kwargs):
    kwargs['weights_only'] = False
    return original_load(*args, **kwargs)
torch.load = patched_load

def smooth_contour_circular(contour, kernel_size=15):
    """Suavizar contorno de forma CIRCULAR (sin artefactos en los extremos)"""
    points = contour.reshape(-1, 2).astype(np.float64)
    n = len(points)
    
    if n < kernel_size * 2:
        return contour
    
    # Padding circular para evitar artefactos en los extremos
    half_k = kernel_size // 2
    
    # Extender el array de forma circular
    extended_x = np.concatenate([points[-half_k:, 0], points[:, 0], points[:half_k, 0]])
    extended_y = np.concatenate([points[-half_k:, 1], points[:, 1], points[:half_k, 1]])
    
    # Kernel de suavizado
    kernel = np.ones(kernel_size) / kernel_size
    
    # Suavizar
    x_smooth = np.convolve(extended_x, kernel, mode='valid')
    y_smooth = np.convolve(extended_y, kernel, mode='valid')
    
    # Reconstruir contorno
    smooth_points = np.column_stack([x_smooth, y_smooth]).astype(np.int32)
    return smooth_points.reshape(-1, 1, 2)

def process_image(image_path, output_dir, yolo_model, sam_predictor):
    """Procesar una imagen con silueta suave"""
    
    filename = Path(image_path).stem
    print(f"\n{'='*60}")
    print(f"📷 Procesando: {filename}")
    print(f"{'='*60}")
    
    # Cargar imagen
    original = np.array(Image.open(image_path).convert('RGB'))
    h, w = original.shape[:2]

    # === PASO 1: YOLO ===
    print("  1️⃣ Detectando vehículo con YOLO...")
    results = yolo_model(original, device='cpu', verbose=False)

    yolo_mask = None
    bbox = None
    VEHICLE_CLASSES = {2: 'car', 3: 'motorcycle', 5: 'bus', 7: 'truck'}

    for result in results:
        if result.masks is None:
            continue
        for i, box in enumerate(result.boxes):
            class_id = int(box.cls[0])
            if class_id in VEHICLE_CLASSES and float(box.conf[0]) > 0.5:
                if i < len(result.masks.data):
                    mask = result.masks.data[i].cpu().numpy()
                    yolo_mask = cv2.resize(mask, (w, h))
                    yolo_mask = (yolo_mask > 0.5).astype(np.uint8) * 255
                    bbox = box.xyxy[0].tolist()
                    print(f"     ✓ Vehículo detectado: conf={float(box.conf[0]):.2f}")
                    break

    if yolo_mask is None:
        print(f"  ❌ No se detectó vehículo en {filename}")
        return False

    # === PASO 2: SAM2 ===
    print("  2️⃣ Refinando con SAM2...")
    sam_predictor.set_image(original)

    # Puntos seguros dentro de YOLO
    kernel = np.ones((30, 30), np.uint8)
    safe_zone = cv2.erode(yolo_mask, kernel, iterations=1)

    safe_coords = np.where(safe_zone > 127)
    if len(safe_coords[0]) > 0:
        n_points = 20
        indices = np.linspace(0, len(safe_coords[0]) - 1, n_points, dtype=int)
        
        point_coords = np.array([
            [safe_coords[1][i], safe_coords[0][i]] for i in indices
        ])
        point_labels = np.ones(len(point_coords), dtype=np.int32)
        
        x1, y1, x2, y2 = bbox
        pad = 0.05
        bw, bh = x2 - x1, y2 - y1
        padded_bbox = np.array([
            max(0, x1 - bw * pad),
            max(0, y1 - bh * pad),
            min(w, x2 + bw * pad),
            min(h, y2 + bh * pad)
        ])
        
        masks, scores, _ = sam_predictor.predict(
            point_coords=point_coords,
            point_labels=point_labels,
            box=padded_bbox,
            multimask_output=True
        )
        
        best_idx = np.argmax(scores)
        sam_mask = (masks[best_idx] * 255).astype(np.uint8)
        print(f"     ✓ SAM2 score: {scores[best_idx]:.3f}")
    else:
        sam_mask = yolo_mask

    # === PASO 3: Intersección ===
    print("  3️⃣ Combinando máscaras...")
    yolo_dilated = cv2.dilate(yolo_mask, np.ones((5, 5), np.uint8), iterations=1)
    combined_mask = cv2.bitwise_and(sam_mask, yolo_dilated)

    # === PASO 4: Suavizar contornos (SIN PÚAS) ===
    print("  4️⃣ Suavizando silueta...")
    contours, _ = cv2.findContours(combined_mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)

    smooth_mask = np.zeros((h, w), dtype=np.uint8)
    
    if contours:
        main_contour = max(contours, key=cv2.contourArea)
        
        # Usar suavizado CIRCULAR (sin artefactos en extremos)
        smooth_contour = smooth_contour_circular(main_contour, kernel_size=15)
        
        cv2.fillPoly(smooth_mask, [smooth_contour], 255)

    # Morfología final
    kernel_ellipse = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
    smooth_mask = cv2.morphologyEx(smooth_mask, cv2.MORPH_CLOSE, kernel_ellipse)

    # === PASO 5: Limpiar ===
    print("  5️⃣ Limpiando...")
    num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(smooth_mask, connectivity=8)
    if num_labels > 1:
        largest = 1 + np.argmax(stats[1:, cv2.CC_STAT_AREA])
        smooth_mask = ((labels == largest) * 255).astype(np.uint8)

    smooth_mask = cv2.morphologyEx(smooth_mask, cv2.MORPH_CLOSE, np.ones((20, 20), np.uint8))

    # === PASO 6: Alpha suave ===
    alpha = smooth_mask.astype(np.float32)
    alpha = cv2.GaussianBlur(alpha, (5, 5), 0)
    alpha = np.clip(alpha, 0, 255).astype(np.uint8)

    # === PASO 7: Composite ===
    print("  6️⃣ Generando salidas...")
    alpha_norm = alpha.astype(np.float32) / 255.0
    white_bg = np.full((h, w, 3), 255, dtype=np.uint8)

    composite = np.zeros((h, w, 3), dtype=np.uint8)
    for c in range(3):
        composite[:, :, c] = (
            original[:, :, c] * alpha_norm + 
            white_bg[:, :, c] * (1 - alpha_norm)
        ).astype(np.uint8)

    # Guardar
    Image.fromarray(composite).save(f'{output_dir}/{filename}_white.png', quality=95)
    Image.fromarray(smooth_mask).save(f'{output_dir}/{filename}_mask.png')
    
    rgba = np.dstack([original, alpha])
    Image.fromarray(rgba).save(f'{output_dir}/{filename}_transparent.png')
    
    print(f"  ✅ {filename} completado!")
    return True

def main():
    print("🚀 Procesando TODAS las imágenes con silueta suave...")
    print("="*60)
    
    # Directorio de salida
    output_dir = 'output_smooth'
    os.makedirs(output_dir, exist_ok=True)
    
    # Cargar modelos UNA sola vez
    print("\n📦 Cargando modelos...")
    
    from ultralytics import YOLO
    yolo_model = YOLO('models/yolov8x-seg.pt')
    print("   ✓ YOLO cargado")
    
    from sam2.sam2_image_predictor import SAM2ImagePredictor
    from sam2.build_sam import build_sam2
    
    sam_model = build_sam2(
        config_file="sam2_hiera_l.yaml",
        ckpt_path="models/sam2_hiera_large.pt",
        device="cpu"
    )
    sam_predictor = SAM2ImagePredictor(sam_model)
    print("   ✓ SAM2 cargado")
    
    # Listar imágenes
    input_dir = 'input'
    images = [f for f in os.listdir(input_dir) if f.endswith(('.jpg', '.jpeg', '.png')) and not f.startswith('.')]
    images.sort()
    
    print(f"\n📷 Encontradas {len(images)} imágenes para procesar")
    
    # Procesar cada imagen
    success = 0
    failed = 0
    
    for img_name in images:
        img_path = os.path.join(input_dir, img_name)
        try:
            if process_image(img_path, output_dir, yolo_model, sam_predictor):
                success += 1
            else:
                failed += 1
        except Exception as e:
            print(f"  ❌ Error procesando {img_name}: {e}")
            failed += 1
    
    # Resumen
    print("\n" + "="*60)
    print("📊 RESUMEN")
    print("="*60)
    print(f"   ✅ Exitosas: {success}")
    print(f"   ❌ Fallidas: {failed}")
    print(f"   📁 Salida: {output_dir}/")
    print("\n🎉 ¡Proceso completado!")
    
    return 0 if failed == 0 else 1

if __name__ == "__main__":
    sys.exit(main())
