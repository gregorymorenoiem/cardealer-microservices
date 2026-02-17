#!/usr/bin/env python3
"""
🚗 Simple Background Removal for Vehicles
==========================================

Uses a simpler approach with pretrained models from Hugging Face.
Works with Python 3.13 without kornia dependency issues.

Usage:
    python process_simple_bg.py
"""

import os
import sys
import logging
from pathlib import Path
import numpy as np
from PIL import Image
import cv2
import torch

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

INPUT_DIR = os.getenv("INPUT_DIR", "./input")
OUTPUT_DIR = os.getenv("OUTPUT_DIR", "./output_simple")


class U2NetProcessor:
    """U2-Net based background removal - simpler than BiRefNet"""
    
    def __init__(self, device: str = "cpu"):
        self.device = device
        self.model = None
        self._load_model()
    
    def _load_model(self):
        """Load segmentation model"""
        try:
            # Try loading DIS (Highly Accurate Dichotomous Image Segmentation)
            from transformers import pipeline
            
            logger.info("📥 Loading IS-Net-DIS model...")
            
            # IS-Net is specifically trained for high-quality image segmentation
            self.pipe = pipeline(
                "image-segmentation",
                model="ZhengPeng7/BiRefNet",  # Updated to use BiRefNet which is available
                trust_remote_code=True,
                device=-1  # CPU
            )
            logger.info("✅ Model loaded successfully")
            
        except Exception as e:
            logger.warning(f"⚠️ Failed to load IS-Net: {e}")
            self._load_simple_model()
    
    def _load_simple_model(self):
        """Use a simpler approach with OpenCV + YOLO"""
        try:
            from ultralytics import YOLO
            
            logger.info("📥 Loading YOLO + GrabCut approach...")
            
            # Patch for weights_only
            original_load = torch.load
            def patched_load(*args, **kwargs):
                kwargs['weights_only'] = False
                return original_load(*args, **kwargs)
            torch.load = patched_load
            
            self.yolo = YOLO('yolov8m-seg.pt')  # Segmentation model
            
            torch.load = original_load
            self.pipe = None  # Mark as using YOLO
            logger.info("✅ YOLO-Seg model loaded")
            
        except Exception as e:
            logger.error(f"❌ Failed to load any model: {e}")
            self.yolo = None
            self.pipe = None
    
    def remove_background(self, image: Image.Image) -> tuple:
        """Remove background from image"""
        
        # Convert to array
        img_array = np.array(image.convert("RGB"))
        h, w = img_array.shape[:2]
        
        if self.pipe is not None:
            # Use pipeline
            try:
                result = self.pipe(image)
                if isinstance(result, list) and len(result) > 0:
                    mask = np.array(result[0]['mask'])
                else:
                    mask = np.array(result)
                
                # Ensure mask is right size
                if mask.shape[:2] != (h, w):
                    mask = cv2.resize(mask.astype(np.uint8), (w, h))
                    
            except Exception as e:
                logger.warning(f"Pipeline failed: {e}, using YOLO fallback")
                mask = self._yolo_segment(img_array)
                
        elif hasattr(self, 'yolo') and self.yolo is not None:
            mask = self._yolo_segment(img_array)
        else:
            # Ultimate fallback: GrabCut
            mask = self._grabcut_segment(img_array)
        
        # Refine mask
        mask = self._refine_mask(mask)
        
        # Create outputs
        return self._create_outputs(img_array, mask)
    
    def _yolo_segment(self, img_array: np.ndarray) -> np.ndarray:
        """Use YOLO segmentation"""
        h, w = img_array.shape[:2]
        
        try:
            results = self.yolo(img_array, verbose=False, conf=0.4)
            
            # Vehicle class IDs: car=2, motorcycle=3, bus=5, truck=7
            vehicle_classes = {2, 3, 5, 7}
            
            combined_mask = np.zeros((h, w), dtype=np.uint8)
            
            for result in results:
                if result.masks is None:
                    continue
                    
                for i, (mask, box) in enumerate(zip(result.masks.data, result.boxes)):
                    class_id = int(box.cls[0])
                    if class_id in vehicle_classes:
                        # Get mask and resize
                        m = mask.cpu().numpy()
                        m = cv2.resize(m, (w, h))
                        combined_mask = np.maximum(combined_mask, (m * 255).astype(np.uint8))
            
            if combined_mask.max() > 0:
                logger.info(f"   🚗 YOLO detected vehicle, mask coverage: {(combined_mask > 127).sum() / (h*w) * 100:.1f}%")
                return combined_mask
            else:
                logger.warning("   ⚠️ No vehicle detected by YOLO, using GrabCut")
                return self._grabcut_segment(img_array)
                
        except Exception as e:
            logger.error(f"YOLO segmentation failed: {e}")
            return self._grabcut_segment(img_array)
    
    def _grabcut_segment(self, img_array: np.ndarray) -> np.ndarray:
        """Use GrabCut as ultimate fallback"""
        h, w = img_array.shape[:2]
        
        # Initialize mask
        mask = np.zeros((h, w), np.uint8)
        
        # Define rectangle (assume vehicle is in center 80%)
        margin_x = int(w * 0.1)
        margin_y = int(h * 0.1)
        rect = (margin_x, margin_y, w - 2*margin_x, h - 2*margin_y)
        
        # Models
        bgd_model = np.zeros((1, 65), np.float64)
        fgd_model = np.zeros((1, 65), np.float64)
        
        try:
            cv2.grabCut(img_array, mask, rect, bgd_model, fgd_model, 5, cv2.GC_INIT_WITH_RECT)
            mask = np.where((mask == 2) | (mask == 0), 0, 255).astype(np.uint8)
            logger.info("   📐 Used GrabCut fallback")
        except:
            # If GrabCut fails, use simple rectangle
            mask[margin_y:h-margin_y, margin_x:w-margin_x] = 255
            logger.info("   📐 Used rectangle fallback")
        
        return mask
    
    def _refine_mask(self, mask: np.ndarray) -> np.ndarray:
        """Refine segmentation mask"""
        if mask.dtype != np.uint8:
            if mask.max() <= 1:
                mask = (mask * 255).astype(np.uint8)
            else:
                mask = mask.astype(np.uint8)
        
        # Threshold
        _, binary = cv2.threshold(mask, 127, 255, cv2.THRESH_BINARY)
        
        # Fill holes
        kernel = np.ones((7, 7), np.uint8)
        binary = cv2.morphologyEx(binary, cv2.MORPH_CLOSE, kernel, iterations=2)
        
        # Remove noise
        binary = cv2.morphologyEx(binary, cv2.MORPH_OPEN, kernel)
        
        # Keep largest contour
        contours, _ = cv2.findContours(binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        if contours:
            largest = max(contours, key=cv2.contourArea)
            refined = np.zeros_like(binary)
            cv2.drawContours(refined, [largest], -1, 255, -1)
            binary = refined
        
        # Smooth edges
        binary = cv2.GaussianBlur(binary.astype(np.float32), (5, 5), 0)
        binary = (binary > 127).astype(np.uint8) * 255
        
        # Edge feathering for smooth transitions
        edge_blur = cv2.GaussianBlur(binary.astype(np.float32), (11, 11), 0)
        
        return edge_blur.astype(np.uint8)
    
    def _create_outputs(self, img_array: np.ndarray, mask: np.ndarray) -> tuple:
        """Create output images"""
        h, w = img_array.shape[:2]
        
        # Normalize mask
        alpha = mask.astype(np.float32) / 255.0
        alpha_3ch = np.dstack([alpha, alpha, alpha])
        
        # White background
        white_bg = np.full_like(img_array, 255)
        result_array = (img_array * alpha_3ch + white_bg * (1 - alpha_3ch)).astype(np.uint8)
        result_image = Image.fromarray(result_array)
        
        # RGBA with transparency
        rgba = np.dstack([img_array, mask])
        rgba_image = Image.fromarray(rgba, 'RGBA')
        
        # Mask as image
        mask_image = Image.fromarray(mask)
        
        return result_image, mask_image, rgba_image


def process_batch(input_dir: str, output_dir: str):
    """Process all images"""
    
    input_path = Path(input_dir)
    output_path = Path(output_dir)
    output_path.mkdir(parents=True, exist_ok=True)
    
    # Find images
    extensions = [".jpg", ".jpeg", ".png", ".webp"]
    images = []
    for ext in extensions:
        images.extend(input_path.glob(f"*{ext}"))
        images.extend(input_path.glob(f"*{ext.upper()}"))
    
    if not images:
        logger.error(f"❌ No images found in {input_dir}")
        return
    
    logger.info("=" * 60)
    logger.info("🚗 Simple Vehicle Background Removal")
    logger.info("=" * 60)
    logger.info(f"📁 Input:  {input_dir}")
    logger.info(f"📁 Output: {output_dir}")
    logger.info(f"📷 Images: {len(images)}")
    logger.info("=" * 60)
    
    # Initialize
    processor = U2NetProcessor("cpu")
    
    if processor.pipe is None and (not hasattr(processor, 'yolo') or processor.yolo is None):
        logger.error("❌ No model available, exiting")
        return
    
    success = 0
    for img_path in images:
        try:
            logger.info(f"\n🔄 Processing: {img_path.name}")
            
            image = Image.open(img_path).convert("RGB")
            logger.info(f"   Size: {image.size[0]}x{image.size[1]}")
            
            result, mask, rgba = processor.remove_background(image)
            
            base_name = img_path.stem
            
            # Save
            bg_path = output_path / f"{base_name}_bg_removed.png"
            result.save(bg_path)
            logger.info(f"   ✅ {bg_path.name}")
            
            trans_path = output_path / f"{base_name}_transparent.png"
            rgba.save(trans_path)
            logger.info(f"   ✅ {trans_path.name}")
            
            mask_path = output_path / f"{base_name}_mask.png"
            mask.save(mask_path)
            logger.info(f"   ✅ {mask_path.name}")
            
            success += 1
            
        except Exception as e:
            logger.error(f"   ❌ Failed: {e}")
            import traceback
            traceback.print_exc()
    
    logger.info("\n" + "=" * 60)
    logger.info(f"✅ Processed {success}/{len(images)} images")
    logger.info(f"📁 Output: {output_dir}")
    logger.info("=" * 60)


def main():
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", default=INPUT_DIR)
    parser.add_argument("--output", default=OUTPUT_DIR)
    args = parser.parse_args()
    
    process_batch(args.input, args.output)


if __name__ == "__main__":
    main()
