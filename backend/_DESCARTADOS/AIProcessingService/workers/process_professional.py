#!/usr/bin/env python3
"""
Professional Vehicle Background Removal Pipeline
Uses multiple specialized models for best quality results:

1. YOLOv8x-seg - High accuracy vehicle detection + segmentation
2. IS-Net (DIS) - Superior edge detection for clean cutouts
3. SAM2 Large - Fallback precision segmentation
4. Shadow Generation - Natural ground shadows

Usage: python3 process_professional.py

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
from typing import Optional, Tuple

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# Paths
SCRIPT_DIR = Path(__file__).parent
MODELS_DIR = SCRIPT_DIR / "models"
INPUT_DIR = Path(os.getenv("INPUT_DIR", SCRIPT_DIR / "input"))
OUTPUT_DIR = Path(os.getenv("OUTPUT_DIR", SCRIPT_DIR / "output"))
DEVICE = os.getenv("DEVICE", "cuda" if torch.cuda.is_available() else "cpu")


class ISNetProcessor:
    """
    IS-Net (Dichotomous Image Segmentation) for ultra-clean edges.
    Best for: High-contrast foreground/background separation
    """
    
    def __init__(self, device: str = "cpu"):
        self.device = device
        self.model = None
        self.input_size = (1024, 1024)
        self._load_model()
    
    def _load_model(self):
        """Load IS-Net model"""
        model_path = MODELS_DIR / "isnet-general-use.pth"
        
        if not model_path.exists():
            logger.warning(f"⚠️ IS-Net model not found at {model_path}")
            return
        
        try:
            # Try to import IS-Net
            from isnet import ISNetDIS
            
            self.model = ISNetDIS()
            self.model.load_state_dict(torch.load(str(model_path), map_location=self.device, weights_only=False))
            self.model.to(self.device)
            self.model.eval()
            logger.info("✅ IS-Net (DIS) loaded - Superior edge detection")
        except ImportError:
            logger.warning("⚠️ IS-Net module not available, trying alternative...")
            self._load_model_alternative()
        except Exception as e:
            logger.warning(f"⚠️ Failed to load IS-Net: {e}")
            self._load_model_alternative()
    
    def _load_model_alternative(self):
        """Alternative loading using rembg library"""
        try:
            from rembg import new_session
            self.session = new_session("isnet-general-use")
            self.model = "rembg"
            logger.info("✅ IS-Net loaded via rembg")
        except Exception as e:
            logger.warning(f"⚠️ Could not load IS-Net via rembg: {e}")
            self.model = None
    
    def segment(self, image: np.ndarray) -> Optional[np.ndarray]:
        """
        Segment image using IS-Net for clean edges.
        
        Returns:
            Alpha mask (0-255) or None if failed
        """
        if self.model is None:
            return None
        
        if self.model == "rembg":
            return self._segment_rembg(image)
        
        return self._segment_native(image)
    
    def _segment_rembg(self, image: np.ndarray) -> Optional[np.ndarray]:
        """Use rembg for segmentation"""
        try:
            from rembg import remove
            
            # Convert to PIL
            pil_image = Image.fromarray(image)
            
            # Remove background (returns RGBA)
            result = remove(pil_image, session=self.session)
            
            # Extract alpha channel
            alpha = np.array(result)[:, :, 3]
            
            logger.info(f"  🎯 IS-Net segmentation complete (rembg)")
            return alpha
        except Exception as e:
            logger.error(f"IS-Net rembg failed: {e}")
            return None
    
    def _segment_native(self, image: np.ndarray) -> Optional[np.ndarray]:
        """Native IS-Net segmentation"""
        try:
            h, w = image.shape[:2]
            
            # Preprocess
            img = cv2.resize(image, self.input_size)
            img = img.astype(np.float32) / 255.0
            img = (img - 0.5) / 0.5  # Normalize to [-1, 1]
            img = torch.from_numpy(img.transpose(2, 0, 1)).unsqueeze(0).to(self.device)
            
            # Inference
            with torch.no_grad():
                result = self.model(img)
                mask = result[0][0].squeeze().cpu().numpy()
            
            # Post-process
            mask = (mask * 255).astype(np.uint8)
            mask = cv2.resize(mask, (w, h))
            
            logger.info(f"  🎯 IS-Net segmentation complete (native)")
            return mask
        except Exception as e:
            logger.error(f"IS-Net native failed: {e}")
            return None


class YOLOv8SegProcessor:
    """
    YOLOv8x-seg for high accuracy vehicle detection and segmentation.
    Uses the large model for better precision.
    """
    
    VEHICLE_CLASSES = {2: 'car', 3: 'motorcycle', 5: 'bus', 7: 'truck'}
    
    def __init__(self, device: str = "cpu"):
        self.device = device
        self.model = None
        self._load_model()
    
    def _load_model(self):
        """Load YOLOv8x-seg model"""
        model_path = MODELS_DIR / "yolov8x-seg.pt"
        
        # Fallback to smaller model if large not available
        if not model_path.exists():
            model_path = SCRIPT_DIR / "yolov8m-seg.pt"
        if not model_path.exists():
            model_path = "yolov8n-seg.pt"  # Will download
        
        try:
            from ultralytics import YOLO
            
            # Patch torch.load for compatibility
            original_load = torch.load
            def patched_load(*args, **kwargs):
                kwargs['weights_only'] = False
                return original_load(*args, **kwargs)
            torch.load = patched_load
            
            self.model = YOLO(str(model_path))
            
            torch.load = original_load
            logger.info(f"✅ YOLOv8-seg loaded from {model_path.name}")
        except Exception as e:
            logger.warning(f"⚠️ Failed to load YOLOv8-seg: {e}")
            self.model = None
    
    def detect_and_segment(self, image: np.ndarray) -> Tuple[Optional[np.ndarray], Optional[np.ndarray]]:
        """
        Detect vehicle and return both bbox and segmentation mask.
        
        Returns:
            Tuple of (bbox [x1,y1,x2,y2], mask 0-255) or (None, None)
        """
        if self.model is None:
            return None, None
        
        try:
            results = self.model(image, device=self.device, verbose=False)
            
            best_box = None
            best_mask = None
            best_area = 0
            best_conf = 0
            
            for result in results:
                if result.masks is None:
                    continue
                    
                for i, box in enumerate(result.boxes):
                    class_id = int(box.cls[0])
                    
                    if class_id in self.VEHICLE_CLASSES:
                        x1, y1, x2, y2 = box.xyxy[0].tolist()
                        conf = float(box.conf[0])
                        area = (x2 - x1) * (y2 - y1)
                        
                        # Prefer larger vehicles with higher confidence
                        score = area * conf
                        if score > best_area * best_conf and conf > 0.3:
                            best_area = area
                            best_conf = conf
                            best_box = np.array([x1, y1, x2, y2])
                            
                            # Get segmentation mask
                            if i < len(result.masks.data):
                                mask = result.masks.data[i].cpu().numpy()
                                # Resize to image size
                                h, w = image.shape[:2]
                                mask = cv2.resize(mask, (w, h))
                                best_mask = (mask * 255).astype(np.uint8)
            
            if best_box is not None:
                logger.info(f"  🚗 YOLO detected vehicle: conf={best_conf:.2f}, area={best_area:.0f}px²")
            
            return best_box, best_mask
            
        except Exception as e:
            logger.error(f"YOLO detection failed: {e}")
            return None, None


class SAM2Processor:
    """SAM2 Large for precision segmentation refinement"""
    
    def __init__(self, device: str = "cpu"):
        self.device = device
        self.model = None
        self.predictor = None
        self._load_model()
    
    def _load_model(self):
        """Load SAM2 model"""
        model_path = MODELS_DIR / "sam2_hiera_large.pt"
        
        if not model_path.exists():
            logger.warning(f"⚠️ SAM2 model not found at {model_path}")
            return
        
        try:
            from sam2.sam2_image_predictor import SAM2ImagePredictor
            from sam2.build_sam import build_sam2
            
            logger.info(f"Loading SAM2 Large from {model_path}...")
            self.model = build_sam2(
                config_file="sam2_hiera_l.yaml",
                ckpt_path=str(model_path),
                device=self.device
            )
            self.predictor = SAM2ImagePredictor(self.model)
            logger.info("✅ SAM2 Large loaded")
        except Exception as e:
            logger.warning(f"⚠️ Failed to load SAM2: {e}")
    
    def refine_mask(self, image: np.ndarray, bbox: np.ndarray, initial_mask: Optional[np.ndarray] = None) -> Optional[np.ndarray]:
        """
        Use SAM2 to refine segmentation with bounding box prompt.
        
        Args:
            image: RGB image
            bbox: Vehicle bounding box [x1, y1, x2, y2]
            initial_mask: Optional initial mask to refine
            
        Returns:
            Refined mask (0-255) or None
        """
        if self.predictor is None:
            return initial_mask
        
        try:
            h, w = image.shape[:2]
            x1, y1, x2, y2 = bbox
            box_width = x2 - x1
            box_height = y2 - y1
            
            # Generous padding to not cut edges
            pad_left = box_width * 0.20
            pad_right = box_width * 0.20
            pad_top = box_height * 0.15
            pad_bottom = box_height * 0.30  # More for wheels
            
            padded_bbox = np.array([
                max(0, x1 - pad_left),
                max(0, y1 - pad_top),
                min(w, x2 + pad_right),
                min(h, y2 + pad_bottom)
            ])
            
            self.predictor.set_image(image)
            
            # Create prompt points for better coverage
            center_x = (x1 + x2) / 2
            center_y = (y1 + y2) / 2
            
            point_coords = np.array([
                [center_x, center_y],
                [center_x, y1 + box_height * 0.25],  # Upper
                [center_x, y1 + box_height * 0.75],  # Lower
                [x1 + box_width * 0.20, center_y],   # Left
                [x2 - box_width * 0.20, center_y],   # Right
                [x1 + box_width * 0.15, y2 - box_height * 0.15],  # Left wheel
                [x2 - box_width * 0.15, y2 - box_height * 0.15],  # Right wheel
            ])
            point_labels = np.array([1, 1, 1, 1, 1, 1, 1])
            
            masks, scores, _ = self.predictor.predict(
                point_coords=point_coords,
                point_labels=point_labels,
                box=padded_bbox,
                multimask_output=True
            )
            
            best_idx = np.argmax(scores)
            mask = (masks[best_idx] * 255).astype(np.uint8)
            
            logger.info(f"  🎯 SAM2 refinement complete, score: {scores[best_idx]:.3f}")
            return mask
            
        except Exception as e:
            logger.error(f"SAM2 refinement failed: {e}")
            return initial_mask


class ShadowGenerator:
    """Generate realistic ground shadows for vehicles"""
    
    def __init__(self):
        self.shadow_color = (180, 180, 180)
        self.shadow_blur = 35
        self.shadow_opacity = 0.5
    
    def add_ground_shadow(
        self, 
        image: np.ndarray, 
        mask: np.ndarray,
        shadow_offset_y: int = 10,
        shadow_stretch: float = 1.2
    ) -> np.ndarray:
        """
        Add a natural-looking ground shadow below the vehicle.
        
        Args:
            image: RGB image with white/transparent background
            mask: Vehicle mask (0-255)
            shadow_offset_y: Vertical offset in pixels
            shadow_stretch: Horizontal stretch factor
            
        Returns:
            Image with shadow added
        """
        h, w = image.shape[:2]
        result = image.copy()
        
        # Normalize mask
        mask_norm = mask.astype(np.float32) / 255.0
        
        # Find vehicle bottom
        mask_binary = mask_norm > 0.5
        rows_with_vehicle = np.any(mask_binary, axis=1)
        if not np.any(rows_with_vehicle):
            return image
        
        vehicle_bottom = np.max(np.where(rows_with_vehicle)[0])
        vehicle_top = np.min(np.where(rows_with_vehicle)[0])
        vehicle_height = vehicle_bottom - vehicle_top
        
        # Create shadow from bottom portion of vehicle
        shadow_mask = np.zeros((h, w), dtype=np.float32)
        shadow_source_height = int(vehicle_height * 0.25)
        
        for row in range(vehicle_bottom - shadow_source_height, vehicle_bottom):
            if row < 0 or row >= h:
                continue
            
            progress = (row - (vehicle_bottom - shadow_source_height)) / shadow_source_height
            target_row = min(h - 1, vehicle_bottom + int(shadow_offset_y * (0.5 + progress * 0.5)))
            
            # Copy row with fade
            source_row = mask_norm[row, :]
            shadow_mask[target_row, :] = np.maximum(
                shadow_mask[target_row, :],
                source_row * progress * self.shadow_opacity
            )
        
        # Stretch shadow horizontally
        cols_with_shadow = np.any(shadow_mask > 0, axis=0)
        if np.any(cols_with_shadow):
            left_col = np.min(np.where(cols_with_shadow)[0])
            right_col = np.max(np.where(cols_with_shadow)[0])
            center = (left_col + right_col) // 2
            shadow_width = right_col - left_col
            
            new_left = max(0, int(center - shadow_width * shadow_stretch / 2))
            new_right = min(w - 1, int(center + shadow_width * shadow_stretch / 2))
            
            # Resize shadow horizontally
            shadow_region = shadow_mask[:, left_col:right_col + 1]
            if shadow_region.shape[1] > 0:
                stretched = cv2.resize(shadow_region, (new_right - new_left + 1, h))
                shadow_mask[:, :] = 0
                shadow_mask[:, new_left:new_left + stretched.shape[1]] = stretched
        
        # Apply Gaussian blur
        shadow_mask = cv2.GaussianBlur(shadow_mask, (self.shadow_blur * 2 + 1, self.shadow_blur * 2 + 1), 0)
        
        # Don't overlap with vehicle
        shadow_mask[mask_norm > 0.5] = 0
        
        # Apply shadow to background
        for c in range(3):
            shadow_effect = (255 - self.shadow_color[c]) * shadow_mask
            result[:, :, c] = np.clip(
                result[:, :, c].astype(np.float32) - shadow_effect,
                0, 255
            ).astype(np.uint8)
        
        logger.info(f"  🌑 Added ground shadow")
        return result


class MaskRefiner:
    """Post-processing for clean masks"""
    
    @staticmethod
    def refine(mask: np.ndarray, image: Optional[np.ndarray] = None) -> np.ndarray:
        """
        Apply refinement pipeline to mask.
        
        Steps:
        1. Remove small artifacts
        2. Fill holes
        3. Smooth edges
        4. Feather for alpha matting
        """
        if mask.max() <= 1:
            mask = (mask * 255).astype(np.uint8)
        
        h, w = mask.shape[:2]
        
        # 1. Remove small components
        num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(mask, connectivity=8)
        min_size = int(h * w * 0.005)  # 0.5% of image
        
        refined = np.zeros_like(mask)
        for i in range(1, num_labels):
            if stats[i, cv2.CC_STAT_AREA] >= min_size:
                refined[labels == i] = 255
        
        # 2. Keep largest component
        if refined.sum() > 0:
            num_labels, labels, stats, _ = cv2.connectedComponentsWithStats(refined, connectivity=8)
            if num_labels > 1:
                largest = 1 + np.argmax(stats[1:, cv2.CC_STAT_AREA])
                refined = ((labels == largest) * 255).astype(np.uint8)
        
        # 3. Fill holes
        refined = cv2.morphologyEx(refined, cv2.MORPH_CLOSE, np.ones((15, 15), np.uint8))
        
        # 4. Smooth edges
        refined = cv2.GaussianBlur(refined, (5, 5), 0)
        _, refined = cv2.threshold(refined, 127, 255, cv2.THRESH_BINARY)
        
        # 5. Slight dilation to prevent edge cutting
        refined = cv2.dilate(refined, np.ones((3, 3), np.uint8), iterations=1)
        
        return refined
    
    @staticmethod
    def create_alpha_matte(mask: np.ndarray, feather_radius: int = 4) -> np.ndarray:
        """Create smooth alpha matte for compositing"""
        if mask.max() <= 1:
            mask = (mask * 255).astype(np.uint8)
        
        # Blur edges for smooth transition
        alpha = cv2.GaussianBlur(mask, (feather_radius * 2 + 1, feather_radius * 2 + 1), 0)
        
        return alpha


class ProfessionalProcessor:
    """
    Main processing pipeline combining all models.
    
    Pipeline:
    1. YOLO detection + initial segmentation
    2. IS-Net for clean edges (if available)
    3. SAM2 refinement (if available)
    4. Mask post-processing
    5. Alpha compositing
    6. Shadow generation
    """
    
    def __init__(self, device: str = "cpu"):
        self.device = device
        
        logger.info("=" * 60)
        logger.info("🚀 Initializing Professional Vehicle Processor")
        logger.info("=" * 60)
        
        # Initialize models
        self.yolo = YOLOv8SegProcessor(device)
        self.isnet = ISNetProcessor(device)
        self.sam2 = SAM2Processor(device)
        self.shadow_gen = ShadowGenerator()
        
        logger.info("=" * 60)
    
    def process_image(self, image_path: Path, output_dir: Path) -> bool:
        """Process a single image with full pipeline"""
        logger.info(f"\n🔄 Processing: {image_path.name}")
        
        # Load image
        pil_image = Image.open(image_path).convert("RGB")
        image = np.array(pil_image)
        h, w = image.shape[:2]
        
        base_name = image_path.stem
        final_mask = None
        bbox = None
        
        # Step 1: YOLO detection + segmentation
        bbox, yolo_mask = self.yolo.detect_and_segment(image)
        
        if yolo_mask is not None:
            final_mask = yolo_mask
            logger.info("  ✓ Using YOLO segmentation as base")
        
        # Step 2: Try IS-Net for cleaner edges
        isnet_mask = self.isnet.segment(image)
        if isnet_mask is not None:
            if final_mask is not None:
                # Combine: use IS-Net edges but YOLO coverage
                # IS-Net is better at edges, YOLO better at vehicle recognition
                final_mask = self._combine_masks(final_mask, isnet_mask, bbox)
            else:
                final_mask = isnet_mask
            logger.info("  ✓ Enhanced with IS-Net edges")
        
        # Step 3: SAM2 refinement if we have bbox
        if bbox is not None and self.sam2.predictor is not None:
            sam_mask = self.sam2.refine_mask(image, bbox, final_mask)
            if sam_mask is not None:
                if final_mask is not None:
                    # Use SAM2 to fill gaps, especially wheels
                    final_mask = np.maximum(final_mask, sam_mask)
                else:
                    final_mask = sam_mask
                logger.info("  ✓ Refined with SAM2")
        
        # Fallback: center region
        if final_mask is None:
            logger.warning("  ⚠️ No segmentation available, using center fallback")
            final_mask = np.zeros((h, w), dtype=np.uint8)
            margin_x = int(w * 0.1)
            margin_y = int(h * 0.1)
            final_mask[margin_y:h-margin_y, margin_x:w-margin_x] = 255
        
        # Step 4: Refine mask
        final_mask = MaskRefiner.refine(final_mask, image)
        
        # Step 5: Create alpha matte
        alpha_matte = MaskRefiner.create_alpha_matte(final_mask, feather_radius=4)
        
        # Step 6: Composite onto white background
        white_bg = np.full((h, w, 3), 255, dtype=np.uint8)
        alpha_norm = alpha_matte.astype(np.float32) / 255.0
        
        composite = np.zeros((h, w, 3), dtype=np.uint8)
        for c in range(3):
            composite[:, :, c] = (
                image[:, :, c] * alpha_norm + 
                white_bg[:, :, c] * (1 - alpha_norm)
            ).astype(np.uint8)
        
        # Step 7: Add shadow
        composite_with_shadow = self.shadow_gen.add_ground_shadow(composite, final_mask)
        
        # Save outputs
        # 1. Main result with shadow
        output_path = output_dir / f"{base_name}_professional.png"
        Image.fromarray(composite_with_shadow).save(output_path, quality=95)
        logger.info(f"  ✅ Saved: {output_path.name}")
        
        # 2. Clean version without shadow
        clean_path = output_dir / f"{base_name}_clean.png"
        Image.fromarray(composite).save(clean_path, quality=95)
        logger.info(f"  ✅ Saved: {clean_path.name}")
        
        # 3. Mask for debugging
        mask_path = output_dir / f"{base_name}_mask.png"
        Image.fromarray(final_mask).save(mask_path)
        logger.info(f"  ✅ Saved: {mask_path.name}")
        
        # 4. RGBA version (transparent background)
        rgba = np.dstack([image, alpha_matte])
        rgba_path = output_dir / f"{base_name}_transparent.png"
        Image.fromarray(rgba).save(rgba_path)
        logger.info(f"  ✅ Saved: {rgba_path.name}")
        
        return True
    
    def _combine_masks(
        self, 
        yolo_mask: np.ndarray, 
        isnet_mask: np.ndarray,
        bbox: Optional[np.ndarray]
    ) -> np.ndarray:
        """
        Intelligently combine YOLO and IS-Net masks.
        YOLO is better at vehicle recognition, IS-Net at edge precision.
        """
        # Normalize both masks
        yolo = yolo_mask.astype(np.float32) / 255.0 if yolo_mask.max() > 1 else yolo_mask.astype(np.float32)
        isnet = isnet_mask.astype(np.float32) / 255.0 if isnet_mask.max() > 1 else isnet_mask.astype(np.float32)
        
        # Within bbox: trust YOLO more for coverage
        # Outside bbox: trust IS-Net more for edge precision
        
        if bbox is not None:
            x1, y1, x2, y2 = map(int, bbox)
            h, w = yolo_mask.shape[:2]
            
            # Create blend weights
            weight_yolo = np.zeros((h, w), dtype=np.float32)
            weight_yolo[y1:y2, x1:x2] = 0.7  # Trust YOLO in bbox
            
            # Blend
            combined = yolo * weight_yolo + isnet * (1 - weight_yolo)
        else:
            # Simple blend
            combined = np.maximum(yolo * 0.5, isnet * 0.5)
        
        return (combined * 255).astype(np.uint8)


def main():
    print("=" * 70)
    print("🚀 Professional Vehicle Background Removal Pipeline")
    print("=" * 70)
    print(f"📁 Input:  {INPUT_DIR}")
    print(f"📁 Output: {OUTPUT_DIR}")
    print(f"🔧 Device: {DEVICE}")
    print(f"📦 Models: {MODELS_DIR}")
    print("=" * 70)
    
    # Create output directory
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    
    # Find images
    images = (
        list(INPUT_DIR.glob("*.jpg")) + 
        list(INPUT_DIR.glob("*.jpeg")) + 
        list(INPUT_DIR.glob("*.png")) +
        list(INPUT_DIR.glob("*.webp")) +
        list(INPUT_DIR.glob("*.JPG")) +
        list(INPUT_DIR.glob("*.JPEG")) +
        list(INPUT_DIR.glob("*.PNG")) +
        list(INPUT_DIR.glob("*.WEBP"))
    )
    
    if not images:
        logger.error(f"❌ No images found in {INPUT_DIR}")
        return 1
    
    logger.info(f"📷 Found {len(images)} images")
    
    # Initialize processor
    processor = ProfessionalProcessor(DEVICE)
    
    # Process each image
    success = 0
    for img_path in images:
        try:
            if processor.process_image(img_path, OUTPUT_DIR):
                success += 1
        except Exception as e:
            logger.error(f"❌ Failed to process {img_path.name}: {e}")
            import traceback
            traceback.print_exc()
    
    print("\n" + "=" * 70)
    print(f"✅ Processed {success}/{len(images)} images")
    print(f"📁 Output saved to: {OUTPUT_DIR}")
    print("=" * 70)
    print("\n📄 Output files per image:")
    print("   - *_professional.png  (white background + shadow)")
    print("   - *_clean.png         (white background, no shadow)")
    print("   - *_transparent.png   (transparent background RGBA)")
    print("   - *_mask.png          (binary mask)")
    
    return 0


if __name__ == "__main__":
    sys.exit(main())
