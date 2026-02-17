#!/usr/bin/env python3
"""
🚗 Professional Vehicle Batch Processor
========================================

Production-ready batch processor using the 7-stage professional pipeline.

This replaces the simpler process_local_batch.py with the full professional
pipeline for automotive dealership-quality vehicle cutouts.

Usage:
    python process_professional_batch.py
    python process_professional_batch.py --input /path/to/images --output /path/to/results
    python process_professional_batch.py --gpu --save-intermediates

Features:
    - 7-stage professional segmentation pipeline
    - Shadow-aware processing
    - Wheel protection via CarParts analysis
    - Sub-pixel edge refinement with ISNet
    - Professional alpha matting
    - Batch processing with progress tracking
    - Memory-efficient model management
"""

import os
import sys
import logging
import argparse
import time
from pathlib import Path
from typing import List, Optional
import numpy as np
from PIL import Image
import torch

# Add current directory to path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from professional_pipeline import ProfessionalVehiclePipeline, PipelineConfig

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - [%(name)s] %(message)s'
)
logger = logging.getLogger("BatchProcessor")


# =============================================================================
# CONFIGURATION
# =============================================================================

INPUT_DIR = os.getenv("INPUT_DIR", "/app/input")
OUTPUT_DIR = os.getenv("OUTPUT_DIR", "/app/output")
DEVICE = os.getenv("DEVICE", "cuda" if torch.cuda.is_available() else "cpu")


# =============================================================================
# BATCH PROCESSOR
# =============================================================================

class ProfessionalBatchProcessor:
    """
    Batch processor using the 7-stage professional pipeline
    """
    
    def __init__(
        self,
        input_dir: str = INPUT_DIR,
        output_dir: str = OUTPUT_DIR,
        device: str = DEVICE,
        save_intermediates: bool = False,
        config: Optional[PipelineConfig] = None
    ):
        self.input_dir = Path(input_dir)
        self.output_dir = Path(output_dir)
        self.device = device
        self.save_intermediates = save_intermediates
        
        # Initialize config
        if config is None:
            self.config = PipelineConfig(device=device)
        else:
            self.config = config
            self.config.device = device
        
        # Initialize pipeline (lazy loading)
        self.pipeline = None
        
        # Statistics
        self.stats = {
            "total": 0,
            "success": 0,
            "failed": 0,
            "total_time": 0.0,
            "images": []
        }
    
    def _load_pipeline(self):
        """Load the professional pipeline"""
        if self.pipeline is None:
            logger.info("Initializing Professional Vehicle Pipeline...")
            self.pipeline = ProfessionalVehiclePipeline(self.config)
            self.pipeline.load_models()
    
    def _find_images(self) -> List[Path]:
        """Find all images in input directory"""
        extensions = [".jpg", ".jpeg", ".png", ".webp", ".bmp"]
        images = []
        
        for ext in extensions:
            images.extend(self.input_dir.glob(f"*{ext}"))
            images.extend(self.input_dir.glob(f"*{ext.upper()}"))
        
        return sorted(images)
    
    def process_single_image(
        self,
        image_path: Path,
        output_subdir: Optional[Path] = None
    ) -> bool:
        """Process a single image"""
        start_time = time.time()
        
        try:
            # Determine output directory
            if output_subdir is None:
                output_subdir = self.output_dir / image_path.stem
            output_subdir.mkdir(parents=True, exist_ok=True)
            
            # Load image
            pil_image = Image.open(image_path).convert("RGB")
            image_array = np.array(pil_image)
            
            logger.info(f"\n{'='*60}")
            logger.info(f"📷 Processing: {image_path.name}")
            logger.info(f"   Size: {image_array.shape[1]}x{image_array.shape[0]}")
            logger.info(f"{'='*60}")
            
            # Process through pipeline
            result = self.pipeline.process_image(
                image_array,
                output_path=output_subdir,
                return_intermediates=self.save_intermediates
            )
            
            elapsed = time.time() - start_time
            
            if result["success"]:
                # Save additional outputs
                self._save_outputs(result, output_subdir, image_path.stem)
                
                self.stats["success"] += 1
                logger.info(f"✅ Completed in {elapsed:.2f}s: {image_path.name}")
                
                self.stats["images"].append({
                    "name": image_path.name,
                    "success": True,
                    "time": elapsed
                })
                return True
            else:
                self.stats["failed"] += 1
                logger.error(f"❌ Failed: {image_path.name}")
                
                self.stats["images"].append({
                    "name": image_path.name,
                    "success": False,
                    "error": "Pipeline processing failed"
                })
                return False
                
        except Exception as e:
            elapsed = time.time() - start_time
            self.stats["failed"] += 1
            logger.error(f"❌ Error processing {image_path.name}: {e}")
            
            self.stats["images"].append({
                "name": image_path.name,
                "success": False,
                "error": str(e)
            })
            return False
        
        finally:
            self.stats["total_time"] += time.time() - start_time
    
    def _save_outputs(self, result: dict, output_dir: Path, base_name: str):
        """Save all output files"""
        # Main result with background removed (already saved by pipeline)
        # Save additional formats if needed
        
        # Save PNG with transparency (RGBA)
        if result.get("final_image") is not None and result.get("alpha_matte") is not None:
            final_image = result["final_image"]
            alpha = result["alpha_matte"]
            
            # Create RGBA image
            rgba = np.dstack([final_image, alpha])
            rgba_path = output_dir / f"{base_name}_transparent.png"
            Image.fromarray(rgba).save(rgba_path)
            logger.info(f"   📄 Saved: {rgba_path.name} (with transparency)")
        
        # Save summary JSON
        import json
        summary = {
            "source": base_name,
            "stages_completed": 7,
            "success": result.get("success", False),
            "outputs": {
                "bg_removed": f"{base_name}_bg_removed.png",
                "transparent": f"{base_name}_transparent.png",
                "mask": f"{base_name}_mask.png",
                "alpha": f"{base_name}_alpha.png"
            }
        }
        
        # Add stage metadata
        if "context" in result:
            for i in range(1, 8):
                meta_key = f"stage{i}_metadata"
                if meta_key in result["context"]:
                    summary[f"stage{i}"] = result["context"][meta_key]
        
        summary_path = output_dir / f"{base_name}_summary.json"
        with open(summary_path, 'w') as f:
            json.dump(summary, f, indent=2, default=str)
    
    def process_batch(self) -> dict:
        """Process all images in input directory"""
        # Create output directory
        self.output_dir.mkdir(parents=True, exist_ok=True)
        
        # Find images
        images = self._find_images()
        
        if not images:
            logger.warning(f"No images found in {self.input_dir}")
            return self.stats
        
        self.stats["total"] = len(images)
        
        logger.info("\n" + "=" * 60)
        logger.info("🚗 Professional Vehicle Batch Processor")
        logger.info("=" * 60)
        logger.info(f"📁 Input:  {self.input_dir}")
        logger.info(f"📁 Output: {self.output_dir}")
        logger.info(f"🔧 Device: {self.device}")
        logger.info(f"📷 Images: {len(images)}")
        logger.info(f"💾 Save intermediates: {self.save_intermediates}")
        logger.info("=" * 60)
        
        # Load pipeline
        self._load_pipeline()
        
        # Process each image
        for i, image_path in enumerate(images, 1):
            logger.info(f"\n[{i}/{len(images)}] Processing...")
            self.process_single_image(image_path)
            
            # Memory cleanup between images
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
        
        # Print summary
        self._print_summary()
        
        return self.stats
    
    def _print_summary(self):
        """Print processing summary"""
        logger.info("\n" + "=" * 60)
        logger.info("📊 BATCH PROCESSING SUMMARY")
        logger.info("=" * 60)
        logger.info(f"✅ Success: {self.stats['success']}/{self.stats['total']}")
        logger.info(f"❌ Failed:  {self.stats['failed']}/{self.stats['total']}")
        
        if self.stats['success'] > 0:
            avg_time = self.stats['total_time'] / self.stats['success']
            logger.info(f"⏱️  Average time per image: {avg_time:.2f}s")
        
        logger.info(f"⏱️  Total time: {self.stats['total_time']:.2f}s")
        logger.info(f"📁 Output saved to: {self.output_dir}")
        logger.info("=" * 60)
    
    def unload(self):
        """Unload pipeline to free memory"""
        if self.pipeline is not None:
            self.pipeline.unload_models()
            self.pipeline = None


# =============================================================================
# SIMPLIFIED PIPELINE (Fallback)
# =============================================================================

class SimplifiedBatchProcessor:
    """
    Simplified processor using only YOLO + SAM2 (original approach)
    Use this if you don't have all professional models installed
    """
    
    def __init__(
        self,
        input_dir: str = INPUT_DIR,
        output_dir: str = OUTPUT_DIR,
        device: str = DEVICE
    ):
        # Import original processor
        from process_local_batch import SAM2Processor
        
        self.input_dir = Path(input_dir)
        self.output_dir = Path(output_dir)
        self.processor = SAM2Processor(device)
    
    def process_batch(self):
        """Process all images using simplified pipeline"""
        from process_local_batch import main
        return main()


# =============================================================================
# CLI
# =============================================================================

def main():
    parser = argparse.ArgumentParser(
        description="Professional Vehicle Batch Processor",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  %(prog)s                                    # Use default dirs (/app/input, /app/output)
  %(prog)s --input ./images --output ./results
  %(prog)s --gpu --save-intermediates
  %(prog)s --simplified                      # Use YOLO+SAM2 only (faster, less quality)
        """
    )
    
    parser.add_argument(
        "--input", "-i",
        type=str,
        default=INPUT_DIR,
        help=f"Input directory (default: {INPUT_DIR})"
    )
    parser.add_argument(
        "--output", "-o",
        type=str,
        default=OUTPUT_DIR,
        help=f"Output directory (default: {OUTPUT_DIR})"
    )
    parser.add_argument(
        "--device", "-d",
        type=str,
        default=DEVICE,
        choices=["cuda", "cpu"],
        help=f"Device to use (default: {DEVICE})"
    )
    parser.add_argument(
        "--gpu",
        action="store_true",
        help="Force GPU usage"
    )
    parser.add_argument(
        "--cpu",
        action="store_true",
        help="Force CPU usage"
    )
    parser.add_argument(
        "--save-intermediates",
        action="store_true",
        help="Save intermediate masks from each stage"
    )
    parser.add_argument(
        "--simplified",
        action="store_true",
        help="Use simplified YOLO+SAM2 pipeline (faster, original quality)"
    )
    parser.add_argument(
        "--single", "-s",
        type=str,
        default=None,
        help="Process a single image instead of batch"
    )
    
    args = parser.parse_args()
    
    # Determine device
    device = args.device
    if args.gpu:
        device = "cuda"
    elif args.cpu:
        device = "cpu"
    
    # Check CUDA availability
    if device == "cuda" and not torch.cuda.is_available():
        logger.warning("CUDA not available, falling back to CPU")
        device = "cpu"
    
    # Use simplified or professional pipeline
    if args.simplified:
        logger.info("Using simplified YOLO+SAM2 pipeline")
        processor = SimplifiedBatchProcessor(args.input, args.output, device)
        processor.process_batch()
    else:
        logger.info("Using professional 7-stage pipeline")
        processor = ProfessionalBatchProcessor(
            input_dir=args.input,
            output_dir=args.output,
            device=device,
            save_intermediates=args.save_intermediates
        )
        
        if args.single:
            # Single image mode
            processor._load_pipeline()
            processor.process_single_image(Path(args.single))
        else:
            # Batch mode
            stats = processor.process_batch()
        
        processor.unload()
    
    return 0


if __name__ == "__main__":
    sys.exit(main())
