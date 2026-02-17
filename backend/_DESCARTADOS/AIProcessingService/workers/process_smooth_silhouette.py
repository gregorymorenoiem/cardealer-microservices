#!/usr/bin/env python3
"""
SAM2 con silueta suave y continua
- YOLO para guiar los puntos
- SAM2 para segmentación precisa
- Suavizado de contornos para curvas continuas
"""

import numpy as np
from PIL import Image
import cv2
import torch
import sys

def main():
    print("🎯 SAM2 con silueta suave y continua...")

    # Cargar imagen
    original = np.array(Image.open('input/car_front_2.jpg').convert('RGB'))
    h, w = original.shape[:2]

    # === PASO 1: Obtener máscara de YOLO como guía ===
    print("\n1️⃣ Obteniendo guía de YOLO...")
    from ultralytics import YOLO

    original_load = torch.load
    def patched_load(*args, **kwargs):
        kwargs['weights_only'] = False
        return original_load(*args, **kwargs)
    torch.load = patched_load

    yolo_model = YOLO('models/yolov8x-seg.pt')
    torch.load = original_load

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
                    print(f"   Vehículo detectado: conf={float(box.conf[0]):.2f}")
                    break

    if yolo_mask is None:
        print("❌ No se detectó vehículo")
        return 1

    # === PASO 2: SAM2 con puntos DENTRO de la máscara YOLO ===
    print("\n2️⃣ Refinando con SAM2 (puntos dentro del vehículo)...")

    from sam2.sam2_image_predictor import SAM2ImagePredictor
    from sam2.build_sam import build_sam2

    sam_model = build_sam2(
        config_file="sam2_hiera_l.yaml",
        ckpt_path="models/sam2_hiera_large.pt",
        device="cpu"
    )
    predictor = SAM2ImagePredictor(sam_model)
    predictor.set_image(original)

    # Generar puntos SOLO donde YOLO dice que hay vehículo
    # Erosionar la máscara YOLO para puntos seguros (no en bordes)
    kernel = np.ones((30, 30), np.uint8)
    safe_zone = cv2.erode(yolo_mask, kernel, iterations=1)

    # Obtener coordenadas de píxeles seguros
    safe_coords = np.where(safe_zone > 127)
    if len(safe_coords[0]) > 0:
        # Seleccionar puntos distribuidos
        n_points = 20
        indices = np.linspace(0, len(safe_coords[0]) - 1, n_points, dtype=int)
        
        point_coords = np.array([
            [safe_coords[1][i], safe_coords[0][i]] for i in indices
        ])
        point_labels = np.ones(len(point_coords), dtype=np.int32)
        
        print(f"   Usando {len(point_coords)} puntos dentro del vehículo")
        
        # Bbox con padding mínimo
        x1, y1, x2, y2 = bbox
        pad = 0.05
        bw, bh = x2 - x1, y2 - y1
        padded_bbox = np.array([
            max(0, x1 - bw * pad),
            max(0, y1 - bh * pad),
            min(w, x2 + bw * pad),
            min(h, y2 + bh * pad)
        ])
        
        # Predecir con SAM2
        masks, scores, _ = predictor.predict(
            point_coords=point_coords,
            point_labels=point_labels,
            box=padded_bbox,
            multimask_output=True
        )
        
        best_idx = np.argmax(scores)
        sam_mask = (masks[best_idx] * 255).astype(np.uint8)
        print(f"   SAM2 score: {scores[best_idx]:.3f}")
    else:
        sam_mask = yolo_mask

    # === PASO 3: Usar INTERSECCIÓN de SAM2 y YOLO ===
    print("\n3️⃣ Combinando máscaras (SAM2 ∩ YOLO)...")

    # Dilatar YOLO solo ligeramente para bordes
    yolo_dilated = cv2.dilate(yolo_mask, np.ones((5, 5), np.uint8), iterations=1)

    # Intersección: SAM2 solo donde YOLO confirma vehículo
    combined_mask = cv2.bitwise_and(sam_mask, yolo_dilated)

    # === PASO 4: Suavizar contornos con spline ===
    print("\n4️⃣ Suavizando contornos...")

    # Encontrar contornos
    contours, _ = cv2.findContours(combined_mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)

    smooth_mask = np.zeros((h, w), dtype=np.uint8)
    
    if contours:
        # Tomar el contorno más grande
        main_contour = max(contours, key=cv2.contourArea)
        
        # Suavizar el contorno usando convolución
        contour_points = main_contour.reshape(-1, 2).astype(np.float64)
        
        # Kernel de suavizado (moving average)
        kernel_size = 15
        kernel = np.ones(kernel_size) / kernel_size
        
        # Suavizar x e y por separado
        x_smooth = np.convolve(contour_points[:, 0], kernel, mode='same')
        y_smooth = np.convolve(contour_points[:, 1], kernel, mode='same')
        
        # Reconstruir contorno suavizado
        smooth_contour = np.column_stack([x_smooth, y_smooth]).astype(np.int32)
        smooth_contour = smooth_contour.reshape(-1, 1, 2)
        
        # Dibujar contorno suavizado
        cv2.fillPoly(smooth_mask, [smooth_contour], 255)
        
        print(f"   Contorno suavizado: {len(contour_points)} -> {len(smooth_contour)} puntos")

    # Operaciones morfológicas finales
    kernel_ellipse = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
    smooth_mask = cv2.morphologyEx(smooth_mask, cv2.MORPH_CLOSE, kernel_ellipse)

    # === PASO 5: Mantener solo el componente más grande ===
    print("\n5️⃣ Limpiando componentes pequeños...")
    
    num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(smooth_mask, connectivity=8)
    if num_labels > 1:
        largest = 1 + np.argmax(stats[1:, cv2.CC_STAT_AREA])
        smooth_mask = ((labels == largest) * 255).astype(np.uint8)

    # Rellenar huecos internos
    smooth_mask = cv2.morphologyEx(smooth_mask, cv2.MORPH_CLOSE, np.ones((20, 20), np.uint8))

    # === PASO 6: Crear alpha con antialiasing ===
    print("\n6️⃣ Creando alpha suave...")

    # Distance transform para bordes suaves
    dist_inside = cv2.distanceTransform(smooth_mask, cv2.DIST_L2, 5)
    
    # Alpha con gradiente en los bordes (2 píxeles de transición)
    alpha = np.zeros((h, w), dtype=np.float32)
    alpha[smooth_mask > 127] = 255
    
    # Suavizar bordes
    alpha = cv2.GaussianBlur(alpha, (5, 5), 0)
    alpha = np.clip(alpha, 0, 255).astype(np.uint8)

    # === PASO 7: Composite ===
    print("\n7️⃣ Creando imagen final...")

    alpha_norm = alpha.astype(np.float32) / 255.0
    white_bg = np.full((h, w, 3), 255, dtype=np.uint8)

    composite = np.zeros((h, w, 3), dtype=np.uint8)
    for c in range(3):
        composite[:, :, c] = (
            original[:, :, c] * alpha_norm + 
            white_bg[:, :, c] * (1 - alpha_norm)
        ).astype(np.uint8)

    # Guardar
    Image.fromarray(composite).save('output_removebg/car_front_2_final.png', quality=95)
    print("\n✅ Guardado: car_front_2_final.png")

    Image.fromarray(smooth_mask).save('output_removebg/car_front_2_mask_final.png')
    print("✅ Guardado: car_front_2_mask_final.png")

    rgba = np.dstack([original, alpha])
    Image.fromarray(rgba).save('output_removebg/car_front_2_transparent_final.png')
    print("✅ Guardado: car_front_2_transparent_final.png")

    print("\n🎉 Proceso completado!")
    return 0

if __name__ == "__main__":
    sys.exit(main())
