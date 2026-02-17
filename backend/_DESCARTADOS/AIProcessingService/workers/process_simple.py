#!/usr/bin/env python3
"""
Enfoque SIMPLE y LIMPIO
1. Mejorar imagen (opcional upscale)
2. SAM2 directo con mínimo post-procesamiento
3. Sin sobre-ingeniería
"""

import numpy as np
from PIL import Image, ImageEnhance, ImageFilter
import cv2
import torch
import sys
import os
from pathlib import Path

# Patch torch.load
original_load = torch.load
def patched_load(*args, **kwargs):
    kwargs['weights_only'] = False
    return original_load(*args, **kwargs)
torch.load = patched_load


def enhance_image(image_array):
    """Mejorar calidad de imagen antes de procesar"""
    img = Image.fromarray(image_array)
    
    # 1. Aumentar nitidez
    enhancer = ImageEnhance.Sharpness(img)
    img = enhancer.enhance(1.3)
    
    # 2. Mejorar contraste ligeramente
    enhancer = ImageEnhance.Contrast(img)
    img = enhancer.enhance(1.1)
    
    return np.array(img)


def process_simple(image_path, output_dir, yolo_model, sam_predictor):
    """Procesamiento SIMPLE sin sobre-ingeniería"""
    
    filename = Path(image_path).stem
    print(f"\n📷 {filename}")
    
    # Cargar imagen
    original = np.array(Image.open(image_path).convert('RGB'))
    h, w = original.shape[:2]
    
    # Mejorar imagen
    enhanced = enhance_image(original)
    
    # --- YOLO: Detectar vehículo ---
    VEHICLE_CLASSES = {2: 'car', 3: 'motorcycle', 5: 'bus', 7: 'truck'}
    results = yolo_model(enhanced, device='cpu', verbose=False)
    
    yolo_mask = None
    bbox = None
    
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
                    print(f"   YOLO: {float(box.conf[0]):.2f}")
                    break
    
    if yolo_mask is None:
        print(f"   ❌ No detectado")
        return False
    
    # --- SAM2: Segmentación precisa ---
    sam_predictor.set_image(enhanced)
    
    # Puntos seguros dentro del vehículo (erosión conservadora)
    kernel = np.ones((20, 20), np.uint8)
    safe_zone = cv2.erode(yolo_mask, kernel, iterations=1)
    
    safe_coords = np.where(safe_zone > 127)
    if len(safe_coords[0]) > 20:
        # 25 puntos distribuidos
        n_points = 25
        indices = np.linspace(0, len(safe_coords[0]) - 1, n_points, dtype=int)
        
        point_coords = np.array([
            [safe_coords[1][i], safe_coords[0][i]] for i in indices
        ])
        point_labels = np.ones(len(point_coords), dtype=np.int32)
        
        # Bbox con mínimo padding
        x1, y1, x2, y2 = bbox
        padded_bbox = np.array([
            max(0, x1 - 10),
            max(0, y1 - 10),
            min(w, x2 + 10),
            min(h, y2 + 10)
        ])
        
        masks, scores, _ = sam_predictor.predict(
            point_coords=point_coords,
            point_labels=point_labels,
            box=padded_bbox,
            multimask_output=True
        )
        
        best_idx = np.argmax(scores)
        sam_mask = (masks[best_idx] * 255).astype(np.uint8)
        print(f"   SAM2: {scores[best_idx]:.3f}")
    else:
        sam_mask = yolo_mask
    
    # --- Combinar: SAM2 ∩ YOLO (con pequeña dilatación) ---
    yolo_dilated = cv2.dilate(yolo_mask, np.ones((3, 3), np.uint8), iterations=1)
    final_mask = cv2.bitwise_and(sam_mask, yolo_dilated)
    
    # --- Limpieza mínima ---
    # Solo mantener componente más grande
    num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(final_mask, connectivity=8)
    if num_labels > 1:
        largest = 1 + np.argmax(stats[1:, cv2.CC_STAT_AREA])
        final_mask = ((labels == largest) * 255).astype(np.uint8)
    
    # Cerrar pequeños huecos
    final_mask = cv2.morphologyEx(final_mask, cv2.MORPH_CLOSE, np.ones((5, 5), np.uint8))
    
    # --- Suavizar contorno para líneas continuas ---
    contours, _ = cv2.findContours(final_mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
    if contours:
        main_contour = max(contours, key=cv2.contourArea)
        points = main_contour.reshape(-1, 2).astype(np.float64)
        n = len(points)
        
        if n > 30:
            # Suavizado circular con kernel de 11
            kernel_size = 11
            half_k = kernel_size // 2
            
            # Extender circularmente
            ext_x = np.concatenate([points[-half_k:, 0], points[:, 0], points[:half_k, 0]])
            ext_y = np.concatenate([points[-half_k:, 1], points[:, 1], points[:half_k, 1]])
            
            kernel = np.ones(kernel_size) / kernel_size
            x_smooth = np.convolve(ext_x, kernel, mode='valid')
            y_smooth = np.convolve(ext_y, kernel, mode='valid')
            
            smooth_points = np.column_stack([x_smooth, y_smooth]).astype(np.int32)
            smooth_contour = smooth_points.reshape(-1, 1, 2)
            
            final_mask = np.zeros((h, w), dtype=np.uint8)
            cv2.fillPoly(final_mask, [smooth_contour], 255)
    
    # --- Alpha con blur mínimo (solo 3px) ---
    alpha = cv2.GaussianBlur(final_mask.astype(np.float32), (3, 3), 0)
    alpha = np.clip(alpha, 0, 255).astype(np.uint8)
    
    # --- Composites ---
    alpha_norm = alpha.astype(np.float32) / 255.0
    
    # Fondo blanco
    white_bg = np.full((h, w, 3), 255, dtype=np.uint8)
    composite = np.zeros((h, w, 3), dtype=np.uint8)
    for c in range(3):
        composite[:, :, c] = (
            original[:, :, c] * alpha_norm + 
            white_bg[:, :, c] * (1 - alpha_norm)
        ).astype(np.uint8)
    
    # Guardar
    Image.fromarray(composite).save(f'{output_dir}/{filename}_white.png')
    Image.fromarray(final_mask).save(f'{output_dir}/{filename}_mask.png')
    
    rgba = np.dstack([original, alpha])
    Image.fromarray(rgba).save(f'{output_dir}/{filename}_transparent.png')
    
    print(f"   ✅ OK")
    return True


def main():
    print("🎯 Procesador SIMPLE - Sin sobre-ingeniería")
    print("="*50)
    
    # Cargar modelos
    print("\n📦 Cargando modelos...")
    
    from ultralytics import YOLO
    yolo_model = YOLO('models/yolov8x-seg.pt')
    print("   ✓ YOLO")
    
    from sam2.sam2_image_predictor import SAM2ImagePredictor
    from sam2.build_sam import build_sam2
    
    sam_model = build_sam2(
        config_file="sam2_hiera_l.yaml",
        ckpt_path="models/sam2_hiera_large.pt",
        device="cpu"
    )
    sam_predictor = SAM2ImagePredictor(sam_model)
    print("   ✓ SAM2")
    
    # Output
    output_dir = 'output_simple'
    os.makedirs(output_dir, exist_ok=True)
    
    # Procesar
    input_dir = 'input'
    images = sorted([f for f in os.listdir(input_dir) 
                    if f.endswith(('.jpg', '.jpeg', '.png', '.webp')) and not f.startswith('.')])
    
    print(f"\n📷 {len(images)} imágenes")
    
    success = 0
    for img_name in images:
        try:
            if process_simple(f'{input_dir}/{img_name}', output_dir, yolo_model, sam_predictor):
                success += 1
        except Exception as e:
            print(f"   ❌ Error: {e}")
    
    print(f"\n✅ {success}/{len(images)} completadas")
    print(f"📁 {output_dir}/")


if __name__ == "__main__":
    main()
