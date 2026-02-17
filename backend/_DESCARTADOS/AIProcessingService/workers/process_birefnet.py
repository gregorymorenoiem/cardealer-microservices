#!/usr/bin/env python3
"""
🚗 BiRefNet Vehicle Background Removal
======================================

Uses BiRefNet (Bilateral Reference Network) for high-quality vehicle segmentation.
This is one of the best open-source models for background removal.

Usage:
    python process_birefnet.py
    python process_birefnet.py --input ./input --output ./output_birefnet
"""

import os
import sys
import logging
import argparse
from pathlib import Path
import numpy as np
from PIL import Image
import torch
import cv2

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# Default directories
INPUT_DIR = os.getenv("INPUT_DIR", "./input")
OUTPUT_DIR = os.getenv("OUTPUT_DIR", "./output_birefnet")


class BiRefNetProcessor:
    """BiRefNet-based background removal"""
    
    def __init__(self, device: str = "cpu"):
        self.device = device
        self.model = None
        self.processor = None
        self._load_model()
    
    def _load_model(self):
        """Load BiRefNet model from Hugging Face"""
        try:
            from transformers import AutoModelForImageSegmentation, AutoProcessor
            
            logger.info("📥 Loading BiRefNet model from Hugging Face...")
            logger.info("   (This may take a few minutes on first run)")
            
            # BiRefNet is the current SOTA for image matting/segmentation
            model_name = "ZhengPeng7/BiRefNet"
            
            self.model = AutoModelForImageSegmentation.from_pretrained(
                model_name,
                trust_remote_code=True
            )
            self.model.to(self.device)
            self.model.eval()
            
            logger.info("✅ BiRefNet model loaded successfully")
            
        except Exception as e:
            logger.error(f"❌ Failed to load BiRefNet: {e}")
            logger.info("Trying alternative: BRIA RMBG model...")
            self._load_fallback_model()
    
    def _load_fallback_model(self):
        """Load BRIA RMBG as fallback"""
        try:
            from transformers import pipeline
            
            # BRIA RMBG is another excellent option
            self.pipe = pipeline(
                "image-segmentation",
                model="briaai/RMBG-1.4",
                trust_remote_code=True,
                device=0 if self.device == "cuda" else -1
            )
            self.model = "pipeline"
            logger.info("✅ BRIA RMBG model loaded as fallback")
            
        except Exception as e:
            logger.error(f"❌ Failed to load fallback model: {e}")
            self.model = None
    
    def remove_background(self, image: Image.Image) -> tuple:
        """
        Remove background from image
        
        Returns:
            tuple: (result_image_with_white_bg, mask, rgba_image)
        """
        if self.model is None:
            raise RuntimeError("No model loaded")
        
        # Convert to RGB if needed
        if image.mode != "RGB":
            image = image.convert("RGB")
        
        original_size = image.size
        
        try:
            if self.model == "pipeline":
                # Using BRIA RMBG pipeline
                result = self.pipe(image)
                mask = result  # Pipeline returns the mask directly
                
            else:
                # Using BiRefNet directly
                from torchvision import transforms
                
                # Prepare input
                transform = transforms.Compose([
                    transforms.Resize((1024, 1024)),
                    transforms.ToTensor(),
                    transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])
                ])
                
                input_tensor = transform(image).unsqueeze(0).to(self.device)
                
                # Get prediction
                with torch.no_grad():
                    output = self.model(input_tensor)
                    
                    # Handle different output formats
                    if isinstance(output, tuple):
                        pred = output[-1]  # Usually last element is the refined output
                    elif isinstance(output, dict):
                        pred = output.get('pred', output.get('mask', list(output.values())[0]))
                    else:
                        pred = output
                    
                    # Ensure pred is 2D or 3D
                    if len(pred.shape) == 4:
                        pred = pred.squeeze(0)
                    if len(pred.shape) == 3:
                        pred = pred.squeeze(0)
                    
                    # Apply sigmoid if needed
                    if pred.min() < 0 or pred.max() > 1:
                        pred = torch.sigmoid(pred)
                    
                    # Convert to numpy and resize to original
                    mask_np = pred.cpu().numpy()
                    mask_np = (mask_np * 255).astype(np.uint8)
                    
                    # Resize to original size
                    mask_pil = Image.fromarray(mask_np)
                    mask_pil = mask_pil.resize(original_size, Image.LANCZOS)
                    mask = np.array(mask_pil)
            
            # Post-process mask
            mask = self._refine_mask(mask)
            
            # Create outputs
            image_array = np.array(image)
            
            # RGBA with transparency
            rgba = np.dstack([image_array, mask])
            rgba_image = Image.fromarray(rgba, 'RGBA')
            
            # White background version
            white_bg = np.full_like(image_array, 255)
            alpha = mask.astype(np.float32) / 255.0
            alpha_3ch = np.dstack([alpha, alpha, alpha])
            result_array = (image_array * alpha_3ch + white_bg * (1 - alpha_3ch)).astype(np.uint8)
            result_image = Image.fromarray(result_array)
            
            return result_image, Image.fromarray(mask), rgba_image
            
        except Exception as e:
            logger.error(f"Error during segmentation: {e}")
            import traceback
            traceback.print_exc()
            raise
    
    def _refine_mask(self, mask: np.ndarray) -> np.ndarray:
        """Refine the segmentation mask"""
        if mask.dtype != np.uint8:
            mask = (mask * 255).astype(np.uint8)
        
        # Ensure binary-ish mask
        _, binary = cv2.threshold(mask, 127, 255, cv2.THRESH_BINARY)
        
        # Fill small holes
        kernel = np.ones((5, 5), np.uint8)
        binary = cv2.morphologyEx(binary, cv2.MORPH_CLOSE, kernel)
        
        # Remove small artifacts
        binary = cv2.morphologyEx(binary, cv2.MORPH_OPEN, kernel)
        
        # Keep only largest contour
        contours, _ = cv2.findContours(binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        if contours:
            largest = max(contours, key=cv2.contourArea)
            refined = np.zeros_like(binary)
            cv2.drawContours(refined, [largest], -1, 255, -1)
            binary = refined
        
        # Smooth edges with Gaussian blur
        smoothed = cv2.GaussianBlur(binary.astype(np.float32), (5, 5), 0)
        
        return smoothed.astype(np.uint8)


def process_batch(input_dir: str, output_dir: str, device: str = "cpu"):
    """Process all images in input directory"""
    
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
    logger.info("🚗 BiRefNet Vehicle Background Removal")
    logger.info("=" * 60)
    logger.info(f"📁 Input:  {input_dir}")
    logger.info(f"📁 Output: {output_dir}")
    logger.info(f"📷 Images: {len(images)}")
    logger.info(f"🔧 Device: {device}")
    logger.info("=" * 60)
    
    # Initialize processor
    processor = BiRefNetProcessor(device)
    
    if processor.model is None:
        logger.error("❌ No model available, exiting")
        return
    
    success = 0
    for img_path in images:
        try:
            logger.info(f"\n🔄 Processing: {img_path.name}")
            
            # Load image
            image = Image.open(img_path).convert("RGB")
            logger.info(f"   Size: {image.size[0]}x{image.size[1]}")
            
            # Process
            result, mask, rgba = processor.remove_background(image)
            
            # Save outputs
            base_name = img_path.stem
            
            # 1. White background version
            bg_removed_path = output_path / f"{base_name}_bg_removed.png"
            result.save(bg_removed_path, quality=95)
            logger.info(f"   ✅ {bg_removed_path.name}")
            
            # 2. Transparent PNG (RGBA)
            transparent_path = output_path / f"{base_name}_transparent.png"
            rgba.save(transparent_path)
            logger.info(f"   ✅ {transparent_path.name}")
            
            # 3. Mask
            mask_path = output_path / f"{base_name}_mask.png"
            mask.save(mask_path)
            logger.info(f"   ✅ {mask_path.name}")
            
            success += 1
            
        except Exception as e:
            logger.error(f"   ❌ Failed: {e}")
    
    logger.info("\n" + "=" * 60)
    logger.info(f"✅ Processed {success}/{len(images)} images")
    logger.info(f"📁 Output saved to: {output_dir}")
    logger.info("=" * 60)


def main():
    parser = argparse.ArgumentParser(description="BiRefNet Vehicle Background Removal")
    parser.add_argument("--input", default=INPUT_DIR, help="Input directory")
    parser.add_argument("--output", default=OUTPUT_DIR, help="Output directory")
    parser.add_argument("--device", default="cpu", choices=["cpu", "cuda", "mps"], help="Device")
    args = parser.parse_args()
    
    process_batch(args.input, args.output, args.device)


if __name__ == "__main__":
    main()
