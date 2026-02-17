#!/usr/bin/env python3
"""
🎯 Remove.bg Quality Vehicle Background Removal
================================================

Esta implementación busca igualar la calidad de remove.bg enfocándose en:

1. BORDES PERFECTOS - Sin halos, sin cortes, transiciones suaves
2. PRESERVAR DETALLES - Espejos, antenas, ruedas con rayos
3. SIN ARTEFACTOS - No líneas verticales, no manchas
4. TRANSPARENCIA CORRECTA - Alpha limpio para compositing

Estrategia:
- Usar YOLO solo para detección (bbox), NO para máscara
- SAM2 con múltiples iteraciones de refinamiento
- Trimap-based matting para bordes suaves
- Post-procesamiento avanzado

Author: OKLA Team
Date: January 2026
"""

import os
import sys
import logging
import numpy as np
from PIL import Image
from pathlib import Path
import torch
import cv2
from typing import Optional, Tuple, List
from dataclasses import dataclass
from scipy import ndimage

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# Paths
SCRIPT_DIR = Path(__file__).parent
MODELS_DIR = SCRIPT_DIR / "models"
INPUT_DIR = Path(os.getenv("INPUT_DIR", SCRIPT_DIR / "input"))
OUTPUT_DIR = Path(os.getenv("OUTPUT_DIR", SCRIPT_DIR / "output_removebg"))
DEVICE = os.getenv("DEVICE", "cuda" if torch.cuda.is_available() else "cpu")


@dataclass
class ProcessingConfig:
    """Configuration for processing quality"""
    # SAM2 settings
    sam2_points_per_side: int = 12  # More points = better coverage
    sam2_box_padding: float = 0.25  # 25% padding around bbox
    
    # Edge refinement
    edge_feather_radius: int = 2  # Suave pero no borroso
    edge_erosion: int = 1  # Ligera erosión para eliminar halos
    
    # Artifact removal
    min_component_ratio: float = 0.01  # Componentes < 1% se eliminan
    fill_holes: bool = True
    
    # Alpha matting
    trimap_width: int = 15  # Ancho de zona de transición
    
    # Quality
    output_quality: int = 95


class VehicleDetector:
    """
    YOLOv8 SOLO para detección de bbox, NO para segmentación.
    La máscara de YOLO tiene bordes pobres.
    """
    
    VEHICLE_CLASSES = {2: 'car', 3: 'motorcycle', 5: 'bus', 7: 'truck'}
    
    def __init__(self, device: str = "cpu"):
        self.device = device
        self.model = None
        self._load_model()
    
    def _load_model(self):
        model_path = MODELS_DIR / "yolov8x-seg.pt"
        if not model_path.exists():
            model_path = "yolov8n-seg.pt"
        
        try:
            from ultralytics import YOLO
            original_load = torch.load
            def patched_load(*args, **kwargs):
                kwargs['weights_only'] = False
                return original_load(*args, **kwargs)
            torch.load = patched_load
            
            self.model = YOLO(str(model_path))
            torch.load = original_load
            logger.info(f"✅ YOLO detector loaded")
        except Exception as e:
            logger.error(f"❌ YOLO load failed: {e}")
    
    def detect(self, image: np.ndarray) -> Optional[np.ndarray]:
        """Return ONLY bounding box, not mask"""
        if self.model is None:
            return None
        
        try:
            results = self.model(image, device=self.device, verbose=False)
            
            best_box = None
            best_score = 0
            
            for result in results:
                for box in result.boxes:
                    class_id = int(box.cls[0])
                    if class_id in self.VEHICLE_CLASSES:
                        x1, y1, x2, y2 = box.xyxy[0].tolist()
                        conf = float(box.conf[0])
                        area = (x2 - x1) * (y2 - y1)
                        score = area * conf
                        
                        if score > best_score and conf > 0.3:
                            best_score = score
                            best_box = np.array([x1, y1, x2, y2])
            
            if best_box is not None:
                logger.info(f"  🚗 Vehicle detected: conf={best_score/(best_box[2]-best_box[0])/(best_box[3]-best_box[1]):.2f}")
            
            return best_box
        except Exception as e:
            logger.error(f"Detection failed: {e}")
            return None


class SAM2Segmenter:
    """
    SAM2 para segmentación de alta calidad.
    Usa múltiples estrategias de prompts para mejor cobertura.
    """
    
    def __init__(self, device: str = "cpu"):
        self.device = device
        self.predictor = None
        self._load_model()
    
    def _load_model(self):
        model_path = MODELS_DIR / "sam2_hiera_large.pt"
        if not model_path.exists():
            logger.warning(f"⚠️ SAM2 not found")
            return
        
        try:
            from sam2.sam2_image_predictor import SAM2ImagePredictor
            from sam2.build_sam import build_sam2
            
            model = build_sam2(
                config_file="sam2_hiera_l.yaml",
                ckpt_path=str(model_path),
                device=self.device
            )
            self.predictor = SAM2ImagePredictor(model)
            logger.info("✅ SAM2 Large loaded for segmentation")
        except Exception as e:
            logger.error(f"❌ SAM2 load failed: {e}")
    
    def segment_vehicle(
        self, 
        image: np.ndarray, 
        bbox: np.ndarray,
        config: ProcessingConfig
    ) -> Optional[np.ndarray]:
        """
        Segmentación de alta calidad usando múltiples prompts.
        
        Estrategia:
        1. Usar bbox con padding generoso
        2. Múltiples puntos en grid dentro del vehículo
        3. Puntos específicos en ruedas y extremos
        4. Tomar la mejor máscara
        """
        if self.predictor is None:
            return None
        
        try:
            h, w = image.shape[:2]
            x1, y1, x2, y2 = bbox
            bw, bh = x2 - x1, y2 - y1
            
            # Padding generoso para no cortar
            pad = config.sam2_box_padding
            padded_bbox = np.array([
                max(0, x1 - bw * pad),
                max(0, y1 - bh * pad * 0.5),  # Menos arriba
                min(w, x2 + bw * pad),
                min(h, y2 + bh * pad * 1.2),  # Más abajo para ruedas
            ])
            
            self.predictor.set_image(image)
            
            # === ESTRATEGIA 1: Grid de puntos ===
            grid_points = self._generate_grid_points(bbox, config.sam2_points_per_side)
            
            # === ESTRATEGIA 2: Puntos específicos ===
            specific_points = self._generate_specific_points(bbox)
            
            # === ESTRATEGIA 3: Puntos en ruedas ===
            wheel_points = self._generate_wheel_points(bbox)
            
            # Combinar todos los puntos
            all_points = np.vstack([grid_points, specific_points, wheel_points])
            all_labels = np.ones(len(all_points), dtype=np.int32)
            
            # Predecir con todos los puntos
            masks, scores, _ = self.predictor.predict(
                point_coords=all_points,
                point_labels=all_labels,
                box=padded_bbox,
                multimask_output=True
            )
            
            # Tomar la mejor máscara
            best_idx = np.argmax(scores)
            best_mask = masks[best_idx]
            best_score = scores[best_idx]
            
            logger.info(f"  🎯 SAM2 segmentation: score={best_score:.3f}, points={len(all_points)}")
            
            return (best_mask * 255).astype(np.uint8)
            
        except Exception as e:
            logger.error(f"SAM2 segmentation failed: {e}")
            import traceback
            traceback.print_exc()
            return None
    
    def _generate_grid_points(self, bbox: np.ndarray, n: int) -> np.ndarray:
        """Generar grid de puntos dentro del bbox"""
        x1, y1, x2, y2 = bbox
        # Reducir área para evitar bordes
        margin_x = (x2 - x1) * 0.1
        margin_y = (y2 - y1) * 0.1
        
        xs = np.linspace(x1 + margin_x, x2 - margin_x, n)
        ys = np.linspace(y1 + margin_y, y2 - margin_y, n)
        
        points = []
        for x in xs:
            for y in ys:
                points.append([x, y])
        
        return np.array(points)
    
    def _generate_specific_points(self, bbox: np.ndarray) -> np.ndarray:
        """Puntos en ubicaciones específicas del vehículo"""
        x1, y1, x2, y2 = bbox
        cx, cy = (x1 + x2) / 2, (y1 + y2) / 2
        bw, bh = x2 - x1, y2 - y1
        
        return np.array([
            # Centro
            [cx, cy],
            # Línea central horizontal
            [x1 + bw * 0.2, cy],
            [x1 + bw * 0.4, cy],
            [x1 + bw * 0.6, cy],
            [x1 + bw * 0.8, cy],
            # Parte superior (techo)
            [cx, y1 + bh * 0.15],
            [x1 + bw * 0.3, y1 + bh * 0.2],
            [x2 - bw * 0.3, y1 + bh * 0.2],
            # Parte inferior (carrocería baja)
            [cx, y2 - bh * 0.15],
            [x1 + bw * 0.25, y2 - bh * 0.2],
            [x2 - bw * 0.25, y2 - bh * 0.2],
            # Frente y trasera
            [x1 + bw * 0.1, cy],
            [x2 - bw * 0.1, cy],
        ])
    
    def _generate_wheel_points(self, bbox: np.ndarray) -> np.ndarray:
        """Puntos específicamente en las zonas de ruedas"""
        x1, y1, x2, y2 = bbox
        bw, bh = x2 - x1, y2 - y1
        
        return np.array([
            # Rueda izquierda (delantera o trasera dependiendo del ángulo)
            [x1 + bw * 0.18, y2 - bh * 0.12],
            [x1 + bw * 0.15, y2 - bh * 0.18],
            [x1 + bw * 0.22, y2 - bh * 0.08],
            # Rueda derecha
            [x2 - bw * 0.18, y2 - bh * 0.12],
            [x2 - bw * 0.15, y2 - bh * 0.18],
            [x2 - bw * 0.22, y2 - bh * 0.08],
            # Centro inferior (entre ruedas)
            [cx, y2 - bh * 0.1] if (cx := (x1 + x2) / 2) else [cx, y2 - bh * 0.1],
        ])


class EdgeRefiner:
    """
    Refinamiento de bordes para calidad remove.bg.
    """
    
    @staticmethod
    def refine_mask(
        mask: np.ndarray, 
        image: np.ndarray,
        config: ProcessingConfig
    ) -> np.ndarray:
        """
        Pipeline de refinamiento de máscara.
        """
        h, w = mask.shape[:2]
        
        # 1. Binarizar
        binary = (mask > 127).astype(np.uint8) * 255
        
        # 2. Eliminar componentes pequeños (ruido)
        binary = EdgeRefiner._remove_small_components(binary, config.min_component_ratio)
        
        # 3. Mantener solo el componente más grande (el vehículo)
        binary = EdgeRefiner._keep_largest_component(binary)
        
        # 4. Rellenar agujeros internos
        if config.fill_holes:
            binary = EdgeRefiner._fill_holes(binary)
        
        # 5. Suavizar bordes (sin perder detalle)
        binary = EdgeRefiner._smooth_edges(binary)
        
        # 6. Erosión mínima para eliminar halos del fondo
        if config.edge_erosion > 0:
            kernel = np.ones((config.edge_erosion * 2 + 1, config.edge_erosion * 2 + 1), np.uint8)
            binary = cv2.erode(binary, kernel, iterations=1)
        
        return binary
    
    @staticmethod
    def _remove_small_components(mask: np.ndarray, min_ratio: float) -> np.ndarray:
        """Eliminar componentes pequeños (ruido)"""
        h, w = mask.shape[:2]
        min_size = int(h * w * min_ratio)
        
        num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(mask, connectivity=8)
        
        result = np.zeros_like(mask)
        for i in range(1, num_labels):
            if stats[i, cv2.CC_STAT_AREA] >= min_size:
                result[labels == i] = 255
        
        return result
    
    @staticmethod
    def _keep_largest_component(mask: np.ndarray) -> np.ndarray:
        """Mantener solo el componente más grande"""
        num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(mask, connectivity=8)
        
        if num_labels <= 1:
            return mask
        
        # Encontrar el más grande (excluyendo fondo 0)
        largest = 1 + np.argmax(stats[1:, cv2.CC_STAT_AREA])
        
        return ((labels == largest) * 255).astype(np.uint8)
    
    @staticmethod
    def _fill_holes(mask: np.ndarray) -> np.ndarray:
        """Rellenar agujeros internos"""
        # Invertir: agujeros se vuelven foreground
        inverted = cv2.bitwise_not(mask)
        
        # Encontrar componentes conectados en invertida
        num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(inverted, connectivity=8)
        
        # El fondo exterior es el componente más grande que toca el borde
        h, w = mask.shape
        border_mask = np.zeros((h, w), dtype=bool)
        border_mask[0, :] = True
        border_mask[-1, :] = True
        border_mask[:, 0] = True
        border_mask[:, -1] = True
        
        # Encontrar componentes que tocan el borde
        exterior_components = set()
        for i in range(1, num_labels):
            if np.any(labels[border_mask] == i):
                exterior_components.add(i)
        
        # Todo lo que no es exterior es agujero interno
        filled = mask.copy()
        for i in range(1, num_labels):
            if i not in exterior_components:
                filled[labels == i] = 255
        
        return filled
    
    @staticmethod
    def _smooth_edges(mask: np.ndarray, iterations: int = 2) -> np.ndarray:
        """Suavizar bordes sin perder detalle"""
        # Closing seguido de opening para suavizar
        kernel = np.ones((3, 3), np.uint8)
        
        result = mask.copy()
        for _ in range(iterations):
            result = cv2.morphologyEx(result, cv2.MORPH_CLOSE, kernel)
            result = cv2.morphologyEx(result, cv2.MORPH_OPEN, kernel)
        
        return result


class AlphaMatting:
    """
    Alpha matting para bordes suaves tipo remove.bg.
    Crea transiciones suaves en los bordes sin halos.
    """
    
    @staticmethod
    def create_matte(
        mask: np.ndarray, 
        config: ProcessingConfig
    ) -> np.ndarray:
        """
        Crear alpha matte con bordes suaves.
        
        Usa enfoque de trimap:
        - Definido foreground: alpha = 255
        - Definido background: alpha = 0  
        - Zona de transición: gradiente suave
        """
        # Crear trimap
        foreground = cv2.erode(mask, np.ones((config.trimap_width, config.trimap_width), np.uint8))
        background = cv2.dilate(mask, np.ones((config.trimap_width, config.trimap_width), np.uint8))
        background = cv2.bitwise_not(background)
        
        # Zona de transición = ni foreground ni background definidos
        unknown = cv2.bitwise_not(cv2.bitwise_or(foreground, background))
        
        # Crear alpha inicial
        alpha = np.zeros_like(mask, dtype=np.float32)
        alpha[foreground > 127] = 255.0
        alpha[background > 127] = 0.0
        
        # En zona de transición: gradiente basado en distancia
        if np.any(unknown > 127):
            # Distancia desde foreground
            dist_fg = cv2.distanceTransform(cv2.bitwise_not(foreground), cv2.DIST_L2, 5)
            # Distancia desde background
            dist_bg = cv2.distanceTransform(cv2.bitwise_not(background), cv2.DIST_L2, 5)
            
            # Alpha proporcional a distancias
            total_dist = dist_fg + dist_bg
            total_dist[total_dist == 0] = 1  # Evitar división por cero
            
            transition_alpha = (dist_bg / total_dist) * 255.0
            
            # Aplicar solo en zona de transición
            alpha[unknown > 127] = transition_alpha[unknown > 127]
        
        # Suavizado final muy ligero
        alpha = cv2.GaussianBlur(alpha, (3, 3), 0)
        
        return np.clip(alpha, 0, 255).astype(np.uint8)


class ArtifactRemover:
    """
    Detectar y eliminar artefactos como la franja vertical.
    """
    
    @staticmethod
    def detect_vertical_artifacts(mask: np.ndarray, threshold: float = 0.3) -> List[int]:
        """Detectar columnas con artefactos verticales"""
        h, w = mask.shape
        
        artifacts = []
        
        # Buscar columnas con cambios abruptos
        for x in range(1, w - 1):
            col = mask[:, x].astype(float)
            col_left = mask[:, x - 1].astype(float)
            col_right = mask[:, x + 1].astype(float)
            
            # Diferencia con columnas vecinas
            diff_left = np.abs(col - col_left).mean()
            diff_right = np.abs(col - col_right).mean()
            
            # Si la columna es muy diferente a ambos lados, es artefacto
            if diff_left > 255 * threshold and diff_right > 255 * threshold:
                artifacts.append(x)
        
        return artifacts
    
    @staticmethod
    def remove_artifacts(mask: np.ndarray) -> np.ndarray:
        """Eliminar artefactos detectados"""
        result = mask.copy()
        artifacts = ArtifactRemover.detect_vertical_artifacts(mask)
        
        if artifacts:
            logger.info(f"  🔧 Removing {len(artifacts)} vertical artifacts")
            for x in artifacts:
                # Interpolar de columnas vecinas
                if x > 0 and x < mask.shape[1] - 1:
                    result[:, x] = (mask[:, x - 1].astype(float) + mask[:, x + 1].astype(float)) / 2
        
        return result.astype(np.uint8)


class RemoveBGProcessor:
    """
    Procesador principal que busca calidad remove.bg.
    """
    
    def __init__(self, device: str = "cpu"):
        self.device = device
        self.config = ProcessingConfig()
        
        logger.info("=" * 60)
        logger.info("🎯 Remove.bg Quality Processor")
        logger.info("=" * 60)
        
        self.detector = VehicleDetector(device)
        self.segmenter = SAM2Segmenter(device)
        
        logger.info("=" * 60)
    
    def process_image(self, image_path: Path, output_dir: Path) -> bool:
        """Procesar una imagen con calidad remove.bg"""
        logger.info(f"\n🔄 Processing: {image_path.name}")
        
        # Cargar imagen
        pil_image = Image.open(image_path).convert("RGB")
        image = np.array(pil_image)
        h, w = image.shape[:2]
        
        base_name = image_path.stem
        
        # === PASO 1: Detectar vehículo ===
        bbox = self.detector.detect(image)
        
        if bbox is None:
            logger.warning("  ⚠️ No vehicle detected, using full image")
            bbox = np.array([0, 0, w, h])
        
        # === PASO 2: Segmentar con SAM2 ===
        raw_mask = self.segmenter.segment_vehicle(image, bbox, self.config)
        
        if raw_mask is None:
            logger.error("  ❌ Segmentation failed")
            return False
        
        # === PASO 3: Detectar y eliminar artefactos ===
        clean_mask = ArtifactRemover.remove_artifacts(raw_mask)
        
        # === PASO 4: Refinar bordes ===
        refined_mask = EdgeRefiner.refine_mask(clean_mask, image, self.config)
        
        # === PASO 5: Crear alpha matte ===
        alpha = AlphaMatting.create_matte(refined_mask, self.config)
        
        # === PASO 6: Compositar ===
        # Fondo blanco
        alpha_norm = alpha.astype(np.float32) / 255.0
        white_bg = np.full((h, w, 3), 255, dtype=np.uint8)
        
        composite = np.zeros((h, w, 3), dtype=np.uint8)
        for c in range(3):
            composite[:, :, c] = (
                image[:, :, c] * alpha_norm + 
                white_bg[:, :, c] * (1 - alpha_norm)
            ).astype(np.uint8)
        
        # === GUARDAR OUTPUTS ===
        
        # 1. Versión con fondo blanco (como remove.bg default)
        white_path = output_dir / f"{base_name}_white.png"
        Image.fromarray(composite).save(white_path, quality=self.config.output_quality)
        logger.info(f"  ✅ Saved: {white_path.name}")
        
        # 2. Versión transparente (RGBA)
        rgba = np.dstack([image, alpha])
        transparent_path = output_dir / f"{base_name}_transparent.png"
        Image.fromarray(rgba).save(transparent_path)
        logger.info(f"  ✅ Saved: {transparent_path.name}")
        
        # 3. Máscara binaria
        mask_path = output_dir / f"{base_name}_mask.png"
        Image.fromarray(refined_mask).save(mask_path)
        logger.info(f"  ✅ Saved: {mask_path.name}")
        
        # 4. Alpha matte (para debug)
        alpha_path = output_dir / f"{base_name}_alpha.png"
        Image.fromarray(alpha).save(alpha_path)
        logger.info(f"  ✅ Saved: {alpha_path.name}")
        
        return True


def main():
    print("=" * 70)
    print("🎯 Remove.bg Quality Vehicle Background Removal")
    print("=" * 70)
    print(f"📁 Input:  {INPUT_DIR}")
    print(f"📁 Output: {OUTPUT_DIR}")
    print(f"🔧 Device: {DEVICE}")
    print("=" * 70)
    
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    
    # Buscar imágenes
    images = (
        list(INPUT_DIR.glob("*.jpg")) + 
        list(INPUT_DIR.glob("*.jpeg")) + 
        list(INPUT_DIR.glob("*.png")) +
        list(INPUT_DIR.glob("*.JPG")) +
        list(INPUT_DIR.glob("*.JPEG"))
    )
    
    if not images:
        logger.error(f"❌ No images found in {INPUT_DIR}")
        return 1
    
    logger.info(f"📷 Found {len(images)} images")
    
    processor = RemoveBGProcessor(DEVICE)
    
    success = 0
    for img_path in images:
        try:
            if processor.process_image(img_path, OUTPUT_DIR):
                success += 1
        except Exception as e:
            logger.error(f"❌ Failed: {img_path.name}: {e}")
            import traceback
            traceback.print_exc()
    
    print("\n" + "=" * 70)
    print(f"✅ Processed {success}/{len(images)} images")
    print(f"📁 Output: {OUTPUT_DIR}")
    print("=" * 70)
    print("\n📄 Output files:")
    print("   - *_white.png       (white background, remove.bg style)")
    print("   - *_transparent.png (transparent RGBA)")
    print("   - *_mask.png        (binary mask)")
    print("   - *_alpha.png       (alpha matte)")
    
    return 0


if __name__ == "__main__":
    sys.exit(main())
