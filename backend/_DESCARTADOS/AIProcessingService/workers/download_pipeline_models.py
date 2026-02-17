#!/usr/bin/env python3
"""
🚗 Professional Pipeline Model Downloader
==========================================

Downloads all required models for the 7-stage vehicle segmentation pipeline.

Models Downloaded:
- Stage 1: YOLOv8x-seg (Ultralytics)
- Stage 1: GroundingDINO (HuggingFace)
- Stage 2: ShadowFormer (PINTO_model_zoo ONNX)
- Stage 3: SAM2-Large (Meta)
- Stage 4: CarParts-Seg (HuggingFace)
- Stage 5: SegFormer-B5 (HuggingFace/NVIDIA)
- Stage 6: ISNet-DIS (xuebinqin/DIS)
- Stage 7: RealESRGAN (xinntao)

Usage:
    python download_pipeline_models.py --all
    python download_pipeline_models.py --stage 1 3 6  # Download specific stages
    python download_pipeline_models.py --check  # Verify all models
"""

import os
import sys
import argparse
import logging
import hashlib
import shutil
from pathlib import Path
from typing import Dict, Optional, List
from dataclasses import dataclass

import requests
from tqdm import tqdm

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


# =============================================================================
# CONFIGURATION
# =============================================================================

MODELS_DIR = Path(os.getenv("MODELS_DIR", "/models"))

@dataclass
class ModelInfo:
    """Information about a model to download"""
    name: str
    stage: int
    filename: str
    url: str
    size_mb: float
    sha256: Optional[str] = None
    download_type: str = "direct"  # direct, huggingface, gdown


# Model registry
MODELS: Dict[str, ModelInfo] = {
    # Stage 1: Detection
    "yolov8x-seg": ModelInfo(
        name="YOLOv8x-seg",
        stage=1,
        filename="yolov8x-seg.pt",
        url="https://github.com/ultralytics/assets/releases/download/v8.1.0/yolov8x-seg.pt",
        size_mb=137.0,
        download_type="direct"
    ),
    "grounding-dino": ModelInfo(
        name="GroundingDINO",
        stage=1,
        filename="grounding_dino/",  # Directory - downloaded via HF
        url="IDEA-Research/grounding-dino-base",
        size_mb=900.0,
        download_type="huggingface"
    ),
    
    # Stage 2: Shadow Detection
    "shadowformer": ModelInfo(
        name="ShadowFormer",
        stage=2,
        filename="shadowformer.onnx",
        url="https://github.com/PINTO0309/PINTO_model_zoo/raw/main/353_ShadowFormer/shadowformer_istd_256x256.onnx",
        size_mb=180.0,
        download_type="direct"
    ),
    
    # Stage 3: SAM2
    "sam2-large": ModelInfo(
        name="SAM2-Large",
        stage=3,
        filename="sam2_hiera_large.pt",
        url="https://dl.fbaipublicfiles.com/segment_anything_2/072824/sam2_hiera_large.pt",
        size_mb=897.0,
        download_type="direct"
    ),
    "sam2-base": ModelInfo(
        name="SAM2-Base-Plus",
        stage=3,
        filename="sam2_hiera_base_plus.pt",
        url="https://dl.fbaipublicfiles.com/segment_anything_2/072824/sam2_hiera_base_plus.pt",
        size_mb=323.0,
        download_type="direct"
    ),
    
    # Stage 4: CarParts
    "carparts-seg": ModelInfo(
        name="CarParts-Seg",
        stage=4,
        filename="carparts_seg/",  # Directory - downloaded via HF
        url="Armandoliv/cars-parts-segmentation-unet-resnet18",
        size_mb=100.0,
        download_type="huggingface"
    ),
    
    # Stage 5: SegFormer
    "segformer-b5": ModelInfo(
        name="SegFormer-B5",
        stage=5,
        filename="segformer_b5/",  # Directory - downloaded via HF
        url="nvidia/segformer-b5-finetuned-ade-640-640",
        size_mb=380.0,
        download_type="huggingface"
    ),
    
    # Stage 6: ISNet
    "isnet": ModelInfo(
        name="ISNet-General",
        stage=6,
        filename="isnet-general-use.pth",
        url="https://github.com/xuebinqin/DIS/releases/download/v1.0/isnet-general-use.pth",
        size_mb=175.0,
        sha256="8e4ebc0a4dc1b6e4f3a2a53d9f7b8a9c0e1d2f3a4b5c6d7e8f9a0b1c2d3e4f5a6",
        download_type="direct"
    ),
    
    # Stage 7: Enhancement
    "realesrgan": ModelInfo(
        name="RealESRGAN-x4plus",
        stage=7,
        filename="RealESRGAN_x4plus.pth",
        url="https://github.com/xinntao/Real-ESRGAN/releases/download/v0.1.0/RealESRGAN_x4plus.pth",
        size_mb=64.0,
        download_type="direct"
    ),
    "codeformer": ModelInfo(
        name="CodeFormer",
        stage=7,
        filename="codeformer.pth",
        url="https://github.com/sczhou/CodeFormer/releases/download/v0.1.0/codeformer.pth",
        size_mb=376.0,
        download_type="direct"
    ),
}

# Essential models for basic functionality
ESSENTIAL_MODELS = ["yolov8x-seg", "sam2-base", "isnet"]

# Full professional models
PROFESSIONAL_MODELS = list(MODELS.keys())


# =============================================================================
# DOWNLOAD FUNCTIONS
# =============================================================================

def download_with_progress(url: str, dest_path: Path, desc: str = "Downloading") -> bool:
    """Download file with progress bar"""
    try:
        response = requests.get(url, stream=True, allow_redirects=True)
        response.raise_for_status()
        
        total_size = int(response.headers.get('content-length', 0))
        
        dest_path.parent.mkdir(parents=True, exist_ok=True)
        
        with open(dest_path, 'wb') as f:
            with tqdm(
                total=total_size,
                unit='B',
                unit_scale=True,
                desc=desc,
                ncols=80
            ) as pbar:
                for chunk in response.iter_content(chunk_size=8192):
                    if chunk:
                        f.write(chunk)
                        pbar.update(len(chunk))
        
        return True
        
    except Exception as e:
        logger.error(f"Download failed: {e}")
        if dest_path.exists():
            dest_path.unlink()
        return False


def download_from_huggingface(model_id: str, dest_dir: Path) -> bool:
    """Download model from HuggingFace Hub"""
    try:
        from huggingface_hub import snapshot_download
        
        dest_dir.mkdir(parents=True, exist_ok=True)
        
        logger.info(f"Downloading {model_id} from HuggingFace...")
        
        snapshot_download(
            repo_id=model_id,
            local_dir=dest_dir,
            local_dir_use_symlinks=False
        )
        
        return True
        
    except ImportError:
        logger.error("huggingface_hub not installed. Run: pip install huggingface_hub")
        return False
    except Exception as e:
        logger.error(f"HuggingFace download failed: {e}")
        return False


def download_from_gdrive(file_id: str, dest_path: Path) -> bool:
    """Download file from Google Drive"""
    try:
        import gdown
        
        dest_path.parent.mkdir(parents=True, exist_ok=True)
        
        url = f"https://drive.google.com/uc?id={file_id}"
        gdown.download(url, str(dest_path), quiet=False)
        
        return dest_path.exists()
        
    except ImportError:
        logger.error("gdown not installed. Run: pip install gdown")
        return False
    except Exception as e:
        logger.error(f"Google Drive download failed: {e}")
        return False


def verify_checksum(file_path: Path, expected_sha256: str) -> bool:
    """Verify file SHA256 checksum"""
    sha256_hash = hashlib.sha256()
    
    with open(file_path, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            sha256_hash.update(chunk)
    
    actual = sha256_hash.hexdigest()
    return actual.lower() == expected_sha256.lower()


# =============================================================================
# MAIN DOWNLOAD LOGIC
# =============================================================================

def download_model(model_key: str, force: bool = False) -> bool:
    """Download a single model"""
    if model_key not in MODELS:
        logger.error(f"Unknown model: {model_key}")
        return False
    
    model = MODELS[model_key]
    dest_path = MODELS_DIR / model.filename
    
    # Check if already exists
    if dest_path.exists() and not force:
        logger.info(f"✅ {model.name} already exists at {dest_path}")
        return True
    
    logger.info(f"\n📥 Downloading {model.name} (Stage {model.stage})")
    logger.info(f"   Size: ~{model.size_mb:.0f} MB")
    logger.info(f"   Destination: {dest_path}")
    
    success = False
    
    if model.download_type == "direct":
        success = download_with_progress(model.url, dest_path, model.name)
        
    elif model.download_type == "huggingface":
        success = download_from_huggingface(model.url, dest_path)
        
    elif model.download_type == "gdown":
        success = download_from_gdrive(model.url, dest_path)
    
    if success and model.sha256:
        logger.info("Verifying checksum...")
        if verify_checksum(dest_path, model.sha256):
            logger.info("✅ Checksum verified")
        else:
            logger.warning("⚠️ Checksum mismatch! File may be corrupted.")
    
    if success:
        logger.info(f"✅ {model.name} downloaded successfully")
    else:
        logger.error(f"❌ Failed to download {model.name}")
    
    return success


def download_by_stage(stage: int, force: bool = False) -> bool:
    """Download all models for a specific stage"""
    stage_models = [k for k, v in MODELS.items() if v.stage == stage]
    
    if not stage_models:
        logger.warning(f"No models found for stage {stage}")
        return True
    
    logger.info(f"\n{'='*60}")
    logger.info(f"📦 Downloading Stage {stage} models")
    logger.info(f"{'='*60}")
    
    success = True
    for model_key in stage_models:
        if not download_model(model_key, force):
            success = False
    
    return success


def download_all(force: bool = False) -> bool:
    """Download all models"""
    logger.info("\n" + "=" * 60)
    logger.info("🚀 Professional Pipeline Model Downloader")
    logger.info("=" * 60)
    logger.info(f"Target directory: {MODELS_DIR}")
    
    total_size = sum(m.size_mb for m in MODELS.values())
    logger.info(f"Total download size: ~{total_size / 1024:.1f} GB")
    
    success = True
    for stage in range(1, 8):
        if not download_by_stage(stage, force):
            success = False
    
    return success


def download_essential(force: bool = False) -> bool:
    """Download essential models only"""
    logger.info("\n" + "=" * 60)
    logger.info("📦 Downloading Essential Models")
    logger.info("=" * 60)
    
    success = True
    for model_key in ESSENTIAL_MODELS:
        if not download_model(model_key, force):
            success = False
    
    return success


def check_models() -> Dict[str, bool]:
    """Check which models are available"""
    logger.info("\n" + "=" * 60)
    logger.info("🔍 Checking Model Availability")
    logger.info("=" * 60)
    
    status = {}
    
    for model_key, model in MODELS.items():
        dest_path = MODELS_DIR / model.filename
        exists = dest_path.exists()
        status[model_key] = exists
        
        icon = "✅" if exists else "❌"
        size_info = ""
        if exists and dest_path.is_file():
            actual_size = dest_path.stat().st_size / (1024 * 1024)
            size_info = f" ({actual_size:.1f} MB)"
        
        logger.info(f"  {icon} Stage {model.stage} - {model.name}: {dest_path}{size_info}")
    
    # Summary
    available = sum(status.values())
    total = len(status)
    logger.info(f"\n📊 Summary: {available}/{total} models available")
    
    if available < total:
        missing = [k for k, v in status.items() if not v]
        logger.info(f"   Missing: {', '.join(missing)}")
    
    return status


# =============================================================================
# CLI
# =============================================================================

def main():
    parser = argparse.ArgumentParser(
        description="Download models for Professional Vehicle Segmentation Pipeline",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  %(prog)s --all                  Download all models (~3.5 GB)
  %(prog)s --essential            Download essential models only (~600 MB)
  %(prog)s --stage 1 3 6          Download models for stages 1, 3, and 6
  %(prog)s --model isnet sam2-large  Download specific models
  %(prog)s --check                Check which models are installed
  %(prog)s --list                 List all available models
        """
    )
    
    parser.add_argument(
        "--all", "-a",
        action="store_true",
        help="Download all models"
    )
    parser.add_argument(
        "--essential", "-e",
        action="store_true",
        help="Download essential models only (YOLOv8x-seg, SAM2-Base, ISNet)"
    )
    parser.add_argument(
        "--stage", "-s",
        type=int,
        nargs="+",
        choices=[1, 2, 3, 4, 5, 6, 7],
        help="Download models for specific stages"
    )
    parser.add_argument(
        "--model", "-m",
        type=str,
        nargs="+",
        choices=list(MODELS.keys()),
        help="Download specific models"
    )
    parser.add_argument(
        "--check", "-c",
        action="store_true",
        help="Check which models are installed"
    )
    parser.add_argument(
        "--list", "-l",
        action="store_true",
        help="List all available models"
    )
    parser.add_argument(
        "--force", "-f",
        action="store_true",
        help="Force re-download even if model exists"
    )
    parser.add_argument(
        "--models-dir",
        type=str,
        default=str(MODELS_DIR),
        help=f"Models directory (default: {MODELS_DIR})"
    )
    
    args = parser.parse_args()
    
    # Update models directory - use a local variable to avoid global issues
    models_path = Path(args.models_dir)
    models_path.mkdir(parents=True, exist_ok=True)
    
    # Handle commands
    if args.list:
        print("\n📋 Available Models:")
        print("-" * 70)
        for key, model in MODELS.items():
            print(f"  Stage {model.stage}: {key:20s} - {model.name} ({model.size_mb:.0f} MB)")
        return 0
    
    if args.check:
        status = check_models_in_dir(models_path)
        return 0 if all(status.values()) else 1
    
    if args.all:
        success = download_all_to_dir(models_path, args.force)
        return 0 if success else 1
    
    if args.essential:
        success = download_essential(args.force)
        return 0 if success else 1
    
    if args.stage:
        success = True
        for stage in args.stage:
            if not download_by_stage(stage, args.force):
                success = False
        return 0 if success else 1
    
    if args.model:
        success = True
        for model_key in args.model:
            if not download_model(model_key, args.force):
                success = False
        return 0 if success else 1
    
    # No action specified - show help
    parser.print_help()
    return 0


if __name__ == "__main__":
    sys.exit(main())
