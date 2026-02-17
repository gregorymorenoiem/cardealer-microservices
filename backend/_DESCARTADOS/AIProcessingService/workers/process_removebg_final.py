#!/usr/bin/env python3
"""
REMOVE.BG QUALITY - Versión Final
Para imágenes de alta calidad
"""

import numpy as np
from PIL import Image
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


def get_edge_points(mask, n_points=15):
    """Obtener puntos a lo largo del borde de la máscara"""
    contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
    if not contours:
        return []
    
    main_contour = max(contours, key=cv2.contourArea)
    points = main_contour.reshape(-1, 2)
    
    if len(points) < n_points:
        return points.tolist()
    
    indices = np.linspace(0, len(points) - 1, n_points, dtype=int)
    return [points[i].tolist() for i in indices]


def process_removebg(image_path, output_dir, yolo_model, sam_predictor):
    """Procesamiento calidad Remove.bg"""
    
    filename = Path(image_path).stem
    # Acortar nombre para display
    short_name = filename[:40] + "..." if len(filename) > 40 else filename
    print(f"\n📷 {short_name}")
    
    # Cargar imagen
    original = np.array(Image.open(image_path).convert('RGB'))
    h, w = original.shape[:2]
    print(f"   Resolución: {w}x{h}")
    
    # --- YOLO: Detectar vehículo ---
    VEHICLE_CLASSES = {2: 'car', 3: 'motorcycle', 5: 'bus', 7: 'truck'}
    results = yolo_model(original, device='cpu', verbose=False)
    
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
    
    # --- SAM2: Segmentación de alta precisión ---
    sam_predictor.set_image(original)
    
    # Estrategia: puntos en grid + puntos en bordes
    all_points = []
    all_labels = []
    
    # 1. Puntos interiores en grid (muy dentro del vehículo)
    kernel_large = np.ones((40, 40), np.uint8)
    inner_zone = cv2.erode(yolo_mask, kernel_large, iterations=1)
    inner_coords = np.where(inner_zone > 127)
    
    if len(inner_coords[0]) > 0:
        # Grid de puntos interiores
        n_inner = min(40, len(inner_coords[0]))
        indices = np.linspace(0, len(inner_coords[0]) - 1, n_inner, dtype=int)
        for i in indices:
            all_points.append([inner_coords[1][i], inner_coords[0][i]])
            all_labels.append(1)
    
    # 2. Puntos cerca del borde (pero dentro)
    kernel_small = np.ones((15, 15), np.uint8)
    border_zone = cv2.erode(yolo_mask, kernel_small) - cv2.erode(yolo_mask, kernel_large)
    edge_points = get_edge_points(border_zone, n_points=20)
    for pt in edge_points:
        all_points.append(pt)
        all_labels.append(1)
    
    # 3. Puntos NEGATIVOS fuera del vehículo (para mejor separación)
    outside = 255 - cv2.dilate(yolo_mask, np.ones((30, 30), np.uint8))
    outside_coords = np.where(outside > 127)
    
    if len(outside_coords[0]) > 0:
        n_outside = min(15, len(outside_coords[0]))
        indices = np.linspace(0, len(outside_coords[0]) - 1, n_outside, dtype=int)
        for i in indices:
            all_points.append([outside_coords[1][i], outside_coords[0][i]])
            all_labels.append(0)
    
    if not all_points:
        print("   ❌ No hay puntos")
        return False
    
    point_coords = np.array(all_points)
    point_labels = np.array(all_labels, dtype=np.int32)
    
    print(f"   Puntos: {sum(all_labels)} positivos, {len(all_labels) - sum(all_labels)} negativos")
    
    # Bbox ajustado
    x1, y1, x2, y2 = bbox
    padded_bbox = np.array([
        max(0, x1 - 5),
        max(0, y1 - 5),
        min(w, x2 + 5),
        min(h, y2 + 5)
    ])
    
    # Predecir con SAM2
    masks, scores, _ = sam_predictor.predict(
        point_coords=point_coords,
        point_labels=point_labels,
        box=padded_bbox,
        multimask_output=True
    )
    
    best_idx = np.argmax(scores)
    sam_mask = (masks[best_idx] * 255).astype(np.uint8)
    print(f"   SAM2: {scores[best_idx]:.3f}")
    
    # --- Combinar SAM2 ∩ YOLO ---
    yolo_dilated = cv2.dilate(yolo_mask, np.ones((3, 3), np.uint8), iterations=1)
    final_mask = cv2.bitwise_and(sam_mask, yolo_dilated)
    
    # --- Post-procesamiento mínimo ---
    # 1. Solo componente más grande
    num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(final_mask, connectivity=8)
    if num_labels > 1:
        largest = 1 + np.argmax(stats[1:, cv2.CC_STAT_AREA])
        final_mask = ((labels == largest) * 255).astype(np.uint8)
    
    # 2. Cerrar huecos pequeños
    final_mask = cv2.morphologyEx(final_mask, cv2.MORPH_CLOSE, np.ones((7, 7), np.uint8))
    
    # 3. Suavizar contorno (líneas continuas)
    contours, _ = cv2.findContours(final_mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
    if contours:
        main_contour = max(contours, key=cv2.contourArea)
        points = main_contour.reshape(-1, 2).astype(np.float64)
        n = len(points)
        
        if n > 30:
            # Suavizado circular
            kernel_size = 9
            half_k = kernel_size // 2
            
            ext_x = np.concatenate([points[-half_k:, 0], points[:, 0], points[:half_k, 0]])
            ext_y = np.concatenate([points[-half_k:, 1], points[:, 1], points[:half_k, 1]])
            
            kernel = np.ones(kernel_size) / kernel_size
            x_smooth = np.convolve(ext_x, kernel, mode='valid')
            y_smooth = np.convolve(ext_y, kernel, mode='valid')
            
            smooth_points = np.column_stack([x_smooth, y_smooth]).astype(np.int32)
            smooth_contour = smooth_points.reshape(-1, 1, 2)
            
            final_mask = np.zeros((h, w), dtype=np.uint8)
            cv2.fillPoly(final_mask, [smooth_contour], 255)
    
    # --- Alpha profesional con feathering ---
    # Distance transform para bordes suaves
    dist = cv2.distanceTransform(final_mask, cv2.DIST_L2, 5)
    
    # Feathering de 2-3 pixels
    feather_width = 2.5
    alpha = np.zeros((h, w), dtype=np.float32)
    alpha[final_mask > 127] = 1.0
    
    # Transición suave en el borde
    border = (dist > 0) & (dist < feather_width)
    alpha[border] = dist[border] / feather_width
    
    # Blur muy ligero
    alpha = cv2.GaussianBlur(alpha, (3, 3), 0)
    alpha = np.clip(alpha * 255, 0, 255).astype(np.uint8)
    
    # --- Guardar resultados ---
    # 1. Con fondo blanco
    alpha_norm = alpha.astype(np.float32) / 255.0
    white_bg = np.full((h, w, 3), 255, dtype=np.uint8)
    
    composite = np.zeros((h, w, 3), dtype=np.uint8)
    for c in range(3):
        composite[:, :, c] = (
            original[:, :, c] * alpha_norm + 
            white_bg[:, :, c] * (1 - alpha_norm)
        ).astype(np.uint8)
    
    Image.fromarray(composite).save(f'{output_dir}/{filename}_white.png', optimize=True)
    
    # 2. Máscara
    Image.fromarray(final_mask).save(f'{output_dir}/{filename}_mask.png')
    
    # 3. Transparente (RGBA)
    rgba = np.dstack([original, alpha])
    Image.fromarray(rgba).save(f'{output_dir}/{filename}_transparent.png', optimize=True)
    
    print(f"   ✅ OK")
    return True


def main():
    print("🎯 REMOVE.BG QUALITY - Para imágenes de alta calidad")
    print("="*55)
    
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
    print("   ✓ SAM2 Large")
    
    # Output
    output_dir = 'output_removebg'
    os.makedirs(output_dir, exist_ok=True)
    
    # Procesar
    input_dir = 'input'
    images = sorted([f for f in os.listdir(input_dir) 
                    if f.lower().endswith(('.jpg', '.jpeg', '.png', '.webp')) 
                    and not f.startswith('.')])
    
    print(f"\n📷 {len(images)} imágenes a procesar")
    
    success = 0
    for img_name in images:
        try:
            if process_removebg(f'{input_dir}/{img_name}', output_dir, yolo_model, sam_predictor):
                success += 1
        except Exception as e:
            print(f"   ❌ Error: {e}")
    
    print(f"\n{'='*55}")
    print(f"✅ {success}/{len(images)} completadas")
    print(f"📁 {output_dir}/")


if __name__ == "__main__":
    main()
